namespace PStarSaveEditor.Game;

internal enum FieldKind
{
    /// <summary>Unsigned integer of <see cref="FieldDef.Width"/> bytes.</summary>
    Number,

    /// <summary>On / off bit(s) within one byte, shown as a check box.</summary>
    Flag,
}

/// <summary>One stat row on the STATUS tab.</summary>
internal sealed class FieldDef
{
    /// <param name="key">Lookup key into a character's offsets (e.g. "hpMax").</param>
    /// <param name="label">Short in-game style label (e.g. "MAX HP").</param>
    /// <param name="description">Plain-English description shown beside the editor.</param>
    /// <param name="width">Size in bytes.</param>
    /// <param name="editable">False = shown for reference only.</param>
    /// <param name="max">Largest value the editor accepts; defaults to what the width can hold.</param>
    /// <param name="kind">Number or a flag bit.</param>
    /// <param name="mask">For flags: the bit(s) that mean "on".</param>
    public FieldDef(string key, string label, string description, int width,
        bool editable = true, long max = -1, FieldKind kind = FieldKind.Number, byte mask = 0)
    {
        Key = key;
        Label = label;
        Description = description;
        Width = width;
        Editable = editable;
        Kind = kind;
        Mask = mask;
        Max = kind == FieldKind.Flag ? 1 : max >= 0 ? Math.Min(max, SaveState.MaxFor(width)) : SaveState.MaxFor(width);
    }

    public string Key { get; }
    public string Label { get; }
    public string Description { get; }
    public int Width { get; }
    public bool Editable { get; }
    public long Max { get; }
    public FieldKind Kind { get; }
    public byte Mask { get; }
}

/// <summary>A party member: a name plus the absolute file offset of each field.</summary>
internal sealed class CharacterDef
{
    public CharacterDef(string id, string name, string role, Dictionary<string, int> offsets,
        IReadOnlyList<(string Slot, int Offset)>? equipment = null)
    {
        Id = id;
        Name = name;
        Role = role;
        Offsets = offsets;
        Equipment = equipment ?? Array.Empty<(string, int)>();
    }

    /// <summary>Stable key for tracking unsaved edits.</summary>
    public string Id { get; }

    /// <summary>Position in the game's party table (0-based).</summary>
    public int Index { get; init; }

    public string Name { get; }

    /// <summary>Short description shown under the name (class, home world, …).</summary>
    public string Role { get; }
    public Dictionary<string, int> Offsets { get; }
    public IReadOnlyList<(string Slot, int Offset)> Equipment { get; }
}

/// <summary>An equipment slot resolved to a display name.</summary>
internal readonly record struct EquipEntry(string Slot, int Raw, string Name, bool Empty, bool Unknown);

internal enum RowKind
{
    /// <summary>Section heading inside a list (e.g. "BATTLE").</summary>
    Header,

    /// <summary>An item / technique the character has.</summary>
    Entry,

    /// <summary>Something shown for reference only (not learned yet, unknown id, …).</summary>
    Muted,
}

/// <summary>One row on the ITEMS or TECH tab.</summary>
/// <param name="Detail">Right-aligned note (raw id, "equipped", uses left, …).</param>
/// <param name="Highlight">Drawn in amber (equipped items).</param>
internal readonly record struct InfoRow(RowKind Kind, string Text, string? Detail = null, bool Highlight = false)
{
    public static InfoRow Header(string text) => new(RowKind.Header, text);
}

/// <summary>Colours that give each game its own identity on top of the shared theme.</summary>
/// <param name="PlateTop">Top of the name / title plate gradient.</param>
/// <param name="PlateBottom">Bottom of the plate gradient.</param>
/// <param name="Glow">Inner frame and highlights.</param>
internal sealed record GameAccent(Color PlateTop, Color PlateBottom, Color Glow);

/// <summary>Which world of the Algo star system the cover emblem highlights.</summary>
internal enum AlgoWorld
{
    Palma,
    Motavia,
    Dezolis,
    Alisa,
}

/// <summary>Everything the editor needs to know about one game's save-state layout.</summary>
internal abstract class GameDefinition
{
    public abstract string Title { get; }
    public abstract string ShortTitle { get; }
    public abstract string MenuText { get; }
    public virtual bool IsMasterSystem => false;

    /// <summary>Multi-byte values are little-endian (the Master System game).</summary>
    public virtual bool LittleEndian => false;

    public abstract int MesetaOffset { get; }
    public abstract int MesetaWidth { get; }
    public abstract long MesetaMax { get; }

    /// <summary>"MP" in the first game, "TP" (technique points) after.</summary>
    public virtual string PoolLabel => "TP";

    public abstract IReadOnlyList<FieldDef> Fields { get; }

    /// <summary>True when equipment slots are mapped (shows the EQUIP tab).</summary>
    public virtual bool HasEquipment => false;

    /// <summary>Logical base name of the optional embedded box art (".jpg" / ".png" is appended).</summary>
    public abstract string CoverResource { get; }
    public abstract GameAccent Accent { get; }
    public abstract AlgoWorld World { get; }

    /// <summary>Builds the party for a loaded save (PS III reads the names from the file).</summary>
    public abstract IReadOnlyList<CharacterDef> BuildCharacters(SaveState? save);

    private int? _requiredLength;

    /// <summary>Smallest file that contains every offset this game touches.</summary>
    public int RequiredLength => _requiredLength ??= ComputeRequiredLength();

    public virtual IReadOnlyList<EquipEntry> Equipment(SaveState save, CharacterDef c) => Array.Empty<EquipEntry>();

    /// <summary>True when the whole party shares one inventory (PS I, PS IV).</summary>
    public virtual bool InventoryIsShared => false;

    /// <summary>Items carried (by this character, or by the party when shared).</summary>
    public virtual IReadOnlyList<InfoRow> Inventory(SaveState save, CharacterDef c) => Array.Empty<InfoRow>();

    /// <summary>Magic / techniques / skills the character knows.</summary>
    public virtual IReadOnlyList<InfoRow> Techniques(SaveState save, CharacterDef c) => Array.Empty<InfoRow>();

    /// <summary>Tab caption for <see cref="Techniques"/>.</summary>
    public virtual string TechTabTitle => "TECH";

    /// <summary>Heading above the technique list.</summary>
    public virtual string TechHeading => "TECHNIQUES";

    /// <summary>Optional footnote under the technique list.</summary>
    public virtual string? TechNote => null;

    /// <summary>Extra line in the header.</summary>
    public virtual string? InfoLine => null;

    /// <summary>Note under the party list.</summary>
    public virtual string? PartyNote => null;

    public FieldDef? Field(string key) => Fields.FirstOrDefault(f => f.Key == key);

    /// <summary>Reads a stat for a character, or null when that character doesn't have it.</summary>
    public long? Get(SaveState save, CharacterDef c, string key)
    {
        if (!c.Offsets.TryGetValue(key, out int off) || Field(key) is not { } f)
        {
            return null;
        }

        return f.Kind == FieldKind.Flag
            ? (save.Byte(off) & f.Mask) != 0 ? 1 : 0
            : save.Read(off, f.Width);
    }

    /// <summary>Writes a stat. Returns true if the save changed.</summary>
    public bool Set(SaveState save, CharacterDef c, FieldDef f, long value)
    {
        if (!f.Editable || !c.Offsets.TryGetValue(f.Key, out int off))
        {
            return false;
        }

        return f.Kind == FieldKind.Flag
            ? save.SetFlag(off, f.Mask, value != 0)
            : save.Write(off, f.Width, Math.Clamp(value, 0, f.Max));
    }

    // ---- Helpers for subclasses -------------------------------------------------

    /// <summary>Looks each equipment slot up in an item table.</summary>
    protected static IReadOnlyList<EquipEntry> FromTable(SaveState save, CharacterDef c, IReadOnlyDictionary<int, string> table)
    {
        var list = new List<EquipEntry>(c.Equipment.Count);
        foreach (var (slot, off) in c.Equipment)
        {
            int raw = save.Byte(off);
            if (raw == 0)
            {
                list.Add(new EquipEntry(slot, raw, "Nothing", Empty: true, Unknown: false));
            }
            else if (table.TryGetValue(raw, out string? name))
            {
                list.Add(new EquipEntry(slot, raw, name, Empty: false, Unknown: false));
            }
            else
            {
                list.Add(new EquipEntry(slot, raw, "Unknown item", Empty: false, Unknown: true));
            }
        }

        return list;
    }

    protected static string Hex(int raw) => $"0x{raw:X2}";

    /// <summary>
    /// Reads a list of one-byte item ids from <paramref name="start"/>. Zero bytes are
    /// empty slots; with <paramref name="stopAtZero"/> the first zero ends the list.
    /// With <paramref name="equipBit"/> bit 7 marks an equipped item.
    /// </summary>
    protected static List<InfoRow> ByteItems(SaveState save, int start, int slots,
        IReadOnlyDictionary<int, string> table, bool stopAtZero, bool equipBit = false)
    {
        var rows = new List<InfoRow>();
        for (int i = 0; i < slots; i++)
        {
            int raw = save.Byte(start + i);
            if (raw == 0)
            {
                if (stopAtZero)
                {
                    break;
                }

                continue;
            }

            bool equipped = equipBit && (raw & 0x80) != 0;
            int id = equipBit ? raw & 0x7F : raw;
            rows.Add(ItemRow(id, raw, equipped, table));
        }

        return rows;
    }

    protected static InfoRow ItemRow(int id, int raw, bool equipped, IReadOnlyDictionary<int, string> table) =>
        table.TryGetValue(id, out var name)
            ? new InfoRow(RowKind.Entry, name, equipped ? $"equipped   {Hex(raw)}" : Hex(raw), equipped)
            : new InfoRow(RowKind.Muted, "Unknown item", Hex(raw));

    /// <summary>
    /// Lists a learning order, showing the first <paramref name="learned"/> entries as
    /// known and the rest as still to come.
    /// </summary>
    protected static void AddLearned(List<InfoRow> rows, string heading, int[] order, int learned,
        IReadOnlyDictionary<int, string> table)
    {
        if (order.Length == 0)
        {
            return;
        }

        learned = Math.Clamp(learned, 0, order.Length);
        rows.Add(InfoRow.Header($"{heading}   ({learned} of {order.Length})"));
        for (int i = 0; i < order.Length; i++)
        {
            string name = table.TryGetValue(order[i], out var n) ? n : $"Unknown ({Hex(order[i])})";
            rows.Add(i < learned
                ? new InfoRow(RowKind.Entry, name)
                : new InfoRow(RowKind.Muted, name, "not learned yet"));
        }
    }

    /// <summary>Largest offset + width over every character field and meseta.</summary>
    private int ComputeRequiredLength()
    {
        int max = MesetaOffset + MesetaWidth;
        foreach (var c in BuildCharacters(null))
        {
            foreach (var (key, off) in c.Offsets)
            {
                max = Math.Max(max, off + (Field(key)?.Width ?? 1));
            }

            foreach (var (_, off) in c.Equipment)
            {
                max = Math.Max(max, off + 1);
            }
        }

        return max;
    }
}

/// <summary>Registry of supported games, in menu order.</summary>
internal static class Games
{
    public static readonly IReadOnlyList<GameDefinition> All = new GameDefinition[]
    {
        new PhantasyStar1Game(),
        new PhantasyStar2Game(),
        new PhantasyStar3Game(),
        new PhantasyStar4Game(),
    };
}
