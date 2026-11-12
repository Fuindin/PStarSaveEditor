namespace PStarSaveEditor.Game;

/// <summary>
/// A Kega Fusion save state held entirely in memory. Every edit made in the UI is
/// written into <see cref="Data"/> straight away, so changes to any number of
/// characters (and meseta) accumulate here until <see cref="Save"/> flushes them to
/// disk in a single write.
///
/// Byte order follows the game: the Genesis titles store multi-byte values
/// big-endian (68000), while Phantasy Star on the Master System stores them
/// little-endian (Z80).
/// </summary>
internal sealed class SaveState
{
    // Kega Fusion Genesis save states begin with the ASCII signature "GST" and are
    // this size. Master System states are much smaller and have a different layout.
    private static readonly byte[] GstMagic = { 0x47, 0x53, 0x54 };
    public const long GenesisSize = 140408;

    // Files backed up this session. Shared across loads, so reloading a file we have
    // already saved does not overwrite the pristine .bak with the edited version.
    private static readonly HashSet<string> BackedUp = new(StringComparer.OrdinalIgnoreCase);

    private long _diskLength;
    private DateTime _diskWriteTimeUtc;

    public string Path { get; }
    public byte[] Data { get; }
    public GameDefinition Game { get; }
    public bool LittleEndian => Game.LittleEndian;

    /// <summary>Path of the .bak written before the first save this session, if any.</summary>
    public string? BackupPath { get; private set; }

    private SaveState(string path, byte[] data, GameDefinition game)
    {
        Path = path;
        Data = data;
        Game = game;
        var info = new FileInfo(path);
        _diskLength = info.Length;
        _diskWriteTimeUtc = info.LastWriteTimeUtc;
        if (BackedUp.Contains(path))
        {
            BackupPath = path + ".bak";
        }
    }

    /// <summary>Loads and validates a save state for <paramref name="game"/>.</summary>
    /// <exception cref="InvalidDataException">The file is not a usable save state for the game.</exception>
    public static SaveState Open(string path, GameDefinition game)
    {
        byte[] data = File.ReadAllBytes(path);
        bool gst = data.Length >= GstMagic.Length && data.AsSpan(0, GstMagic.Length).SequenceEqual(GstMagic);

        if (game.IsMasterSystem)
        {
            if (gst && data.Length == GenesisSize)
            {
                throw new InvalidDataException(
                    $"This looks like a Genesis save state, not a Master System one.\n\n" +
                    $"{game.Title} is a Master System game — pick its Kega Fusion state (usually .ss0 – .ss9).");
            }
        }
        else if (!gst)
        {
            throw new InvalidDataException(
                "This is not a Kega Fusion Genesis save state (its header is missing the \"GST\" signature).\n\n" +
                $"Make sure you picked a save state for {game.Title} (usually .gs0 – .gs9).");
        }

        if (data.Length < game.RequiredLength)
        {
            throw new InvalidDataException(
                $"This file is too small to be a {game.Title} save state.\n\n" +
                $"Needed at least: {game.RequiredLength:N0} bytes\n" +
                $"Selected file: {data.Length:N0} bytes");
        }

        return new SaveState(path, data, game);
    }

    // ---- Reads / writes ---------------------------------------------------------

    public bool InRange(int offset, int width) => offset >= 0 && width > 0 && offset + width <= Data.Length;

    /// <summary>Reads an unsigned value of 1–4 bytes in the game's byte order.</summary>
    public long Read(int offset, int width)
    {
        if (!InRange(offset, width))
        {
            return 0;
        }

        long v = 0;
        for (int i = 0; i < width; i++)
        {
            int idx = LittleEndian ? offset + width - 1 - i : offset + i;
            v = (v << 8) | Data[idx];
        }

        return v;
    }

    /// <summary>Writes an unsigned value of 1–4 bytes in the game's byte order. Returns true if any byte changed.</summary>
    public bool Write(int offset, int width, long value)
    {
        if (!InRange(offset, width))
        {
            Log.Write($"Refused out-of-range write at offset 0x{offset:X} ({width} bytes).");
            return false;
        }

        value = Math.Clamp(value, 0, MaxFor(width));
        bool changed = false;
        for (int i = 0; i < width; i++)
        {
            // i = 0 is the least significant byte.
            int idx = LittleEndian ? offset + i : offset + width - 1 - i;
            byte b = (byte)(value & 0xFF);
            if (Data[idx] != b)
            {
                Data[idx] = b;
                changed = true;
            }

            value >>= 8;
        }

        return changed;
    }

    public byte Byte(int offset) => offset >= 0 && offset < Data.Length ? Data[offset] : (byte)0;

    /// <summary>Sets or clears the bits in <paramref name="mask"/>, leaving the rest of the byte alone.</summary>
    public bool SetFlag(int offset, byte mask, bool on)
    {
        if (!InRange(offset, 1))
        {
            Log.Write($"Refused out-of-range flag write at offset 0x{offset:X}.");
            return false;
        }

        byte before = Data[offset];
        byte after = on ? (byte)(before | mask) : (byte)(before & ~mask);
        Data[offset] = after;
        return before != after;
    }

    /// <summary>Reads printable ASCII of at most <paramref name="length"/> bytes (stops at 0).</summary>
    public string Ascii(int offset, int length)
    {
        if (!InRange(offset, length))
        {
            return "";
        }

        var chars = new List<char>(length);
        foreach (byte b in Data.AsSpan(offset, length))
        {
            if (b == 0)
            {
                break;
            }

            chars.Add(b is >= 0x20 and < 0x7F ? (char)b : ' ');
        }

        return new string(chars.ToArray()).Trim();
    }

    public static long MaxFor(int width) => width >= 8 ? long.MaxValue : (1L << (8 * width)) - 1;

    // ---- Disk -------------------------------------------------------------------

    /// <summary>
    /// True when the file on disk has changed since it was loaded or last saved —
    /// e.g. the emulator wrote a new state to the same slot while the editor was open.
    /// </summary>
    public bool ChangedOnDisk()
    {
        try
        {
            var info = new FileInfo(Path);
            return !info.Exists || info.Length != _diskLength || info.LastWriteTimeUtc != _diskWriteTimeUtc;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Writes every pending edit back to the original file in one go. Before the first
    /// save of the session the untouched file is copied to <c>&lt;file&gt;.bak</c>.
    /// </summary>
    public void Save()
    {
        if (!BackedUp.Contains(Path) && File.Exists(Path))
        {
            string bak = Path + ".bak";
            File.Copy(Path, bak, overwrite: true);
            BackedUp.Add(Path);
            BackupPath = bak;
        }

        File.WriteAllBytes(Path, Data);

        var info = new FileInfo(Path);
        _diskLength = info.Length;
        _diskWriteTimeUtc = info.LastWriteTimeUtc;
    }
}

/// <summary>Best-effort error log next to the executable (viewable from Help ▸ View Error Log).</summary>
internal static class Log
{
    public static string FilePath => System.IO.Path.Combine(AppContext.BaseDirectory, "errorlog.txt");

    public static void Write(string message)
    {
        try
        {
            File.AppendAllText(FilePath, $"{message} Added: {DateTime.Now}{Environment.NewLine}");
        }
        catch
        {
            // Logging must never throw.
        }
    }
}
