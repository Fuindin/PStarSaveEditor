namespace PStarSaveEditor.Game;

/// <summary>
/// Phantasy Star (Master System). Four 0x10-byte party records from 0x44BD.
/// A Z80 game, so two-byte values are little-endian.
/// </summary>
internal sealed class PhantasyStar1Game : GameDefinition
{
    private const int TableBase = 0x44BD;
    private const int RecordSize = 0x10;

    private static readonly (string Name, string Role)[] Party =
    {
        ("Alis Landale", "Swordswoman of Camineet  ·  Palma"),
        ("Myau", "Musk cat  ·  Motavia"),
        ("Odin", "Warrior  ·  Palma"),
        ("Noah", "Esper  ·  Motavia"),
    };

    public override string Title => "Phantasy Star";
    public override string ShortTitle => "Phantasy Star";
    public override string MenuText => "Phantasy Star  (Master System)";
    public override bool IsMasterSystem => true;
    public override bool LittleEndian => true;
    public override string PoolLabel => "MP";

    public override int MesetaOffset => 0x459C;
    public override int MesetaWidth => 2;
    public override long MesetaMax => 65535;

    public override string CoverResource => "covers.ps1";
    public override AlgoWorld World => AlgoWorld.Palma;

    // The amber of the Phantasy Star logo.
    public override GameAccent Accent { get; } =
        new(Color.FromArgb(0xC2, 0x60, 0x1C), Color.FromArgb(0x3E, 0x16, 0x06), Color.FromArgb(0xFF, 0xC8, 0x74));

    public override IReadOnlyList<FieldDef> Fields { get; } = new FieldDef[]
    {
        new("level",   "LV",      "Level",                    1, editable: false),
        new("exp",     "EXP",     "Experience points",        2),
        new("hpCur",   "HP",      "Current hit points",       1),
        new("hpMax",   "MAX HP",  "Maximum hit points",       1),
        new("mpCur",   "MP",      "Current magic points",     1),
        new("mpMax",   "MAX MP",  "Maximum magic points",     1),
        new("attack",  "ATTACK",  "Attack power",             1),
        new("defense", "DEFENSE", "Defense power",            1),
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
                ["hpCur"] = b + 0x00, ["exp"] = b + 0x02, ["level"] = b + 0x04, ["hpMax"] = b + 0x05,
                ["attack"] = b + 0x07, ["defense"] = b + 0x08,
            };

            // Odin can't use magic, so he has no MP to edit.
            if (Party[i].Name != "Odin")
            {
                o["mpCur"] = b + 0x01;
                o["mpMax"] = b + 0x06;
            }

            var equip = new (string, int)[] { ("WEAPON", b + 0x09), ("ARMOR", b + 0x0A), ("SHIELD", b + 0x0B) };
            list.Add(new CharacterDef($"ps1.{i}", Party[i].Name, Party[i].Role, o, equip));
        }

        return list;
    }

    public override IReadOnlyList<EquipEntry> Equipment(SaveState save, CharacterDef c) =>
        FromTable(save, c, ItemTables.PhantasyStar1);
}
