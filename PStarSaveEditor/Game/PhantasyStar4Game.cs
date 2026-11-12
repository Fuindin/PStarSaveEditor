namespace PStarSaveEditor.Game;

/// <summary>Phantasy Star IV: The End of the Millennium (Genesis). Eleven 0x80-byte records from 0x11980.</summary>
internal sealed class PhantasyStar4Game : GameDefinition
{
    private const int TableBase = 0x11980;
    private const int RecordSize = 0x80;
    private const long ShortMax = short.MaxValue;
    private const long IntMax = int.MaxValue;

    private static readonly (string Name, string Role)[] Party =
    {
        ("Chaz", "Hunter"),
        ("Alys", "Hunter"),
        ("Hahn", "Scholar"),
        ("Rune", "Esper"),
        ("Gryz", "Motavian warrior"),
        ("Rika", "Numan"),
        ("Demi", "Android"),
        ("Wren", "Android"),
        ("Raja", "Dezorian priest"),
        ("Kyra", "Esper"),
        ("Seth", "Scholar"),
    };

    public override string Title => "Phantasy Star IV";
    public override string ShortTitle => "Phantasy Star IV";
    public override string MenuText => "Phantasy Star I&V  (Genesis)";

    public override int MesetaOffset => 0x118B0;
    public override int MesetaWidth => 4;
    public override long MesetaMax => IntMax;

    public override string CoverResource => "covers.ps4";
    public override AlgoWorld World => AlgoWorld.Motavia;

    // Motavian sunset red — the box art and the Millennium's end.
    public override GameAccent Accent { get; } =
        new(Color.FromArgb(0xC0, 0x40, 0x2A), Color.FromArgb(0x3E, 0x0E, 0x0A), Color.FromArgb(0xFF, 0xA8, 0x84));

    // The four stats and both combat values are single bytes; writing them as two
    // bytes would zero the stat and clobber its neighbour.
    public override IReadOnlyList<FieldDef> Fields { get; } = new FieldDef[]
    {
        new("level",     "LV",        "Level",                      1, editable: false),
        new("exp",       "EXP",       "Experience points",          4, max: IntMax),
        new("hpCur",     "HP",        "Current hit points",         2, max: ShortMax),
        new("hpMax",     "MAX HP",    "Maximum hit points",         2, max: ShortMax),
        new("mpCur",     "TP",        "Current technique points",   2, max: ShortMax),
        new("mpMax",     "MAX TP",    "Maximum technique points",   2, max: ShortMax),
        new("strength",  "STRENGTH",  "Strength",                   1),
        new("mental",    "MENTAL",    "Mental (technique power)",   1),
        new("agility",   "AGILITY",   "Agility",                    1),
        new("dexterity", "DEXTERITY", "Dexterity",                  1),
        new("attack",    "ATTACK",    "Attack power",               1),
        new("defense",   "DEFENSE",   "Defense power",              1),
    };

    public override bool HasEquipment => true;

    public override IReadOnlyList<CharacterDef> BuildCharacters(SaveState? save)
    {
        var list = new List<CharacterDef>(Party.Length);
        for (int i = 0; i < Party.Length; i++)
        {
            int b = TableBase + i * RecordSize;
            var o = new Dictionary<string, int>
            {
                ["level"] = b + 0x01, ["exp"] = b + 0x02,
                ["hpCur"] = b + 0x06, ["hpMax"] = b + 0x08, ["mpCur"] = b + 0x0A, ["mpMax"] = b + 0x0C,
                ["strength"] = b + 0x10, ["mental"] = b + 0x13, ["agility"] = b + 0x16, ["dexterity"] = b + 0x19,
                ["attack"] = b + 0x1D, ["defense"] = b + 0x21,
            };
            var equip = new (string, int)[]
            {
                ("WEAPON 1", b + 0x44), ("WEAPON 2", b + 0x45), ("HEAD", b + 0x46), ("BODY", b + 0x47),
            };
            list.Add(new CharacterDef($"ps4.{i}", Party[i].Name, Party[i].Role, o, equip));
        }

        return list;
    }

    public override IReadOnlyList<EquipEntry> Equipment(SaveState save, CharacterDef c) =>
        FromTable(save, c, ItemTables.PhantasyStar4);
}
