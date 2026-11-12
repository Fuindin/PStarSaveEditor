namespace PStarSaveEditor.Game;

/// <summary>Phantasy Star II (Genesis). Eight 0x40-byte party records from 0xE47A.</summary>
internal sealed class PhantasyStar2Game : GameDefinition
{
    private const int TableBase = 0xE47A;
    private const int RecordSize = 0x40;
    private const long ShortMax = short.MaxValue;
    private const long IntMax = int.MaxValue;

    private static readonly (string Name, string Role)[] Party =
    {
        ("Rolf Landale", "Agent of Paseo"),
        ("Nei", "Numan"),
        ("Rudo Steiner", "Hunter"),
        ("Amy Sage", "Doctor"),
        ("Hugh Tompson", "Biologist"),
        ("Anna Zirski", "Guardian"),
        ("Josh Kain", "Wrecker"),
        ("Shir Gold", "Thief"),
    };

    public override string Title => "Phantasy Star II";
    public override string ShortTitle => "Phantasy Star II";
    public override string MenuText => "Phantasy Star &II  (Genesis)";

    public override int MesetaOffset => 0xEA98;
    public override int MesetaWidth => 4;
    public override long MesetaMax => IntMax;

    public override string CoverResource => "covers.ps2";
    public override AlgoWorld World => AlgoWorld.Motavia;

    // The teal of Mother Brain's Motavia — Paseo's domes and biosystems.
    public override GameAccent Accent { get; } =
        new(Color.FromArgb(0x13, 0x8A, 0x9A), Color.FromArgb(0x05, 0x2E, 0x36), Color.FromArgb(0x7C, 0xF2, 0xEE));

    public override IReadOnlyList<FieldDef> Fields { get; } = new FieldDef[]
    {
        new("level",     "LV",        "Level",                         1, editable: false),
        new("exp",       "EXP",       "Experience points",             4, max: IntMax),
        new("hpCur",     "HP",        "Current hit points",            2, max: ShortMax),
        new("hpMax",     "MAX HP",    "Maximum hit points",            2, max: ShortMax),
        new("mpCur",     "TP",        "Current technique points",      2, max: ShortMax),
        new("mpMax",     "MAX TP",    "Maximum technique points",      2, max: ShortMax),
        new("strength",  "STRENGTH",  "Strength",                      2, max: ShortMax),
        new("mental",    "MENTAL",    "Mental (technique power)",      2, max: ShortMax),
        new("agility",   "AGILITY",   "Agility",                       2, max: ShortMax),
        new("luck",      "LUCK",      "Luck",                          2, max: ShortMax),
        new("dexterity", "DEXTERITY", "Dexterity",                     2, max: ShortMax),
        new("attack",    "ATTACK",    "Attack power",                  2, max: ShortMax),
        new("defense",   "DEFENSE",   "Defense power",                 2, max: ShortMax),
    };

    public override IReadOnlyList<CharacterDef> BuildCharacters(SaveState? save)
    {
        var list = new List<CharacterDef>(Party.Length);
        for (int i = 0; i < Party.Length; i++)
        {
            int b = TableBase + i * RecordSize;
            var o = new Dictionary<string, int>
            {
                ["hpCur"] = b + 0x00, ["hpMax"] = b + 0x02,
                ["level"] = b + 0x09, ["exp"] = b + 0x0A,
                ["strength"] = b + 0x0E, ["mental"] = b + 0x10, ["agility"] = b + 0x12,
                ["luck"] = b + 0x14, ["dexterity"] = b + 0x16,
                ["attack"] = b + 0x1A, ["defense"] = b + 0x1C,
            };

            // Rudo learns no techniques, so he has no TP.
            if (Party[i].Name != "Rudo Steiner")
            {
                o["mpCur"] = b + 0x04;
                o["mpMax"] = b + 0x06;
            }

            list.Add(new CharacterDef($"ps2.{i}", Party[i].Name, Party[i].Role, o));
        }

        return list;
    }
}
