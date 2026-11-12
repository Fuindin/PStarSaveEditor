namespace PStarSaveEditor.Game;

/// <summary>
/// Phantasy Star III: Generations of Doom (Genesis). Five 0x80-byte party records.
/// The game spans three generations that reuse the same records, so each card is a
/// party slot named after whoever currently holds it (read from the save).
/// </summary>
internal sealed class PhantasyStar3Game : GameDefinition
{
    private const long ShortMax = short.MaxValue;
    private const long IntMax = int.MaxValue;

    private static readonly (int Base, string[] Holders)[] Slots =
    {
        (0xE51C, new[] { "Rhys", "Ayn", "Nial" }),
        (0xE59C, new[] { "Mieu" }),
        (0xE61C, new[] { "Wren" }),
        (0xE69C, new[] { "Lyle", "Thea", "Ryan" }),
        (0xE71C, new[] { "Lena", "Sari", "Laya" }),
    };

    public override string Title => "Phantasy Star III";
    public override string ShortTitle => "Phantasy Star III";
    public override string MenuText => "Phantasy Star II&I  (Genesis)";

    public override int MesetaOffset => 0xE4B8;
    public override int MesetaWidth => 4;
    public override long MesetaMax => IntMax;

    public override string CoverResource => "covers.ps3";
    public override AlgoWorld World => AlgoWorld.Alisa;

    // Violet of the Orakio / Laya war banners.
    public override GameAccent Accent { get; } =
        new(Color.FromArgb(0x74, 0x38, 0xC4), Color.FromArgb(0x22, 0x0A, 0x46), Color.FromArgb(0xD4, 0xAC, 0xFF));

    public override IReadOnlyList<FieldDef> Fields { get; } = new FieldDef[]
    {
        new("level",   "LV",      "Level",                          2, editable: false),
        new("exp",     "EXP",     "Experience points",              4, max: IntMax),
        new("hpCur",   "HP",      "Current hit points",             2, max: ShortMax),
        new("hpMax",   "MAX HP",  "Maximum hit points",             2, max: ShortMax),
        new("mpCur",   "TP",      "Current technique points",       2, max: ShortMax),
        new("mpMax",   "MAX TP",  "Maximum technique points",       2, max: ShortMax),
        new("damage",  "DAMAGE",  "Attack damage",                  2, max: ShortMax),
        new("defense", "DEFENSE", "Defense",                        2, max: ShortMax),
        new("speed",   "SPEED",   "Speed (turn order)",             1),
        new("luck",    "LUCK",    "Luck",                           1),
        new("skill",   "SKILL",   "Skill",                          1),
        new("poison",  "POISON",  "Poisoned (untick to cure)",      1, kind: FieldKind.Flag, mask: 0x40),
    };

    public override string? PartyNote =>
        "Each card is a party slot shared across the three generations; the name comes from the save.";

    public override IReadOnlyList<CharacterDef> BuildCharacters(SaveState? save)
    {
        var list = new List<CharacterDef>(Slots.Length);
        for (int i = 0; i < Slots.Length; i++)
        {
            var (b, holders) = Slots[i];
            var o = new Dictionary<string, int>
            {
                ["speed"] = b + 0x00, ["level"] = b + 0x08,
                ["hpMax"] = b + 0x0A, ["mpMax"] = b + 0x0C, ["hpCur"] = b + 0x0E, ["mpCur"] = b + 0x10,
                ["damage"] = b + 0x12, ["defense"] = b + 0x14, ["exp"] = b + 0x16,
                ["luck"] = b + 0x2E, ["skill"] = b + 0x2F,
                ["poison"] = b - 0x23,   // the status byte sits just below the record
            };

            string name = NameFromSave(save, b + 0x03) ?? holders[0];
            string role = holders.Length > 1
                ? $"Party slot {i + 1}  ·  {string.Join(" / ", holders)}"
                : $"Party slot {i + 1}  ·  {(holders[0] == "Mieu" ? "Combat cyborg" : "Android")}";
            list.Add(new CharacterDef($"ps3.{i}", name, role, o));
        }

        return list;
    }

    /// <summary>Reads the four-letter name stored in the record, tidied for display.</summary>
    private static string? NameFromSave(SaveState? save, int offset)
    {
        if (save is null)
        {
            return null;
        }

        string raw = save.Ascii(offset, 4);
        if (raw.Length == 0 || !raw.Any(char.IsLetter))
        {
            return null;
        }

        // "RHYS" → "Rhys"; leave mixed case alone.
        return raw.Any(char.IsLower) ? raw : raw[0] + raw[1..].ToLowerInvariant();
    }
}
