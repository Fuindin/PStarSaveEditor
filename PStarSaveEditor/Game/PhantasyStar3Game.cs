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
            string role = name switch
            {
                "Mieu" => $"Party slot {i + 1}  ·  Combat cyborg",
                "Wren" => $"Party slot {i + 1}  ·  Android",
                _ => $"Party slot {i + 1}",
            };
            list.Add(new CharacterDef($"ps3.{i}", name, role, o) { Index = i });
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

    // ---- Inventory, equipment and techniques ----------------------------------------

    // Each party slot has a 0x20-byte inventory block: a word holding the number of
    // bytes in use (two per item), then up to 15 item words. In each word bits 4–11
    // are the item id and bit 15 marks it as equipped.
    private const int InventoryBase = 0x102F8;
    private const int InventoryStride = 0x20;
    private const int InventorySlots = 15;

    private static IEnumerable<(int Word, int Id, bool Equipped)> ReadInventory(SaveState save, int slot)
    {
        int b = InventoryBase + slot * InventoryStride;
        int count = Math.Clamp((int)save.Read(b, 2) / 2, 0, InventorySlots);
        for (int k = 0; k < count; k++)
        {
            int w = (int)save.Read(b + 2 + k * 2, 2);
            int id = (w >> 4) & 0xFF;
            if (id != 0)
            {
                yield return (w, id, (w & 0x8000) != 0);
            }
        }
    }

    public override bool HasEquipment => true;

    public override IReadOnlyList<EquipEntry> Equipment(SaveState save, CharacterDef c) =>
        ReadInventory(save, c.Index)
            .Where(x => x.Equipped)
            .Select(x => NameTables.PhantasyStar3Items.TryGetValue(x.Id, out var n)
                ? new EquipEntry("EQUIPPED", x.Id, n, Empty: false, Unknown: false)
                : new EquipEntry("EQUIPPED", x.Id, "Unknown item", Empty: false, Unknown: true))
            .ToList();

    public override IReadOnlyList<InfoRow> Inventory(SaveState save, CharacterDef c) =>
        ReadInventory(save, c.Index)
            .Select(x => ItemRow(x.Id, x.Id, x.Equipped, NameTables.PhantasyStar3Items))
            .ToList();

    // Characters don't learn techniques in Phantasy Star III: each one has every
    // technique in its groups from the start, and shares a power grid between them.
    private static readonly (string Group, int Tp, string[] Techs)[] Groups =
    {
        ("HEAL", 5, new[] { "Res", "Gires", "Rever", "Anti" }),
        ("MELEE", 2, new[] { "Foi", "Gra", "Tsu", "Zan" }),
        ("ORDER", 1, new[] { "Fanbi", "Forsa", "Nasak", "Shu" }),
        ("TIME", 1, new[] { "Deban", "Ner", "Rimit", "Shiza" }),
    };

    private static readonly Dictionary<string, string[]> GroupsByName = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Sean"] = new[] { "HEAL", "MELEE", "TIME" },
        ["Mieu"] = new[] { "HEAL", "ORDER" },
        ["Laya"] = new[] { "HEAL", "ORDER" },
        ["Gwyn"] = new[] { "HEAL", "ORDER" },
        ["Ayn"] = new[] { "HEAL", "TIME" },
        ["Thea"] = new[] { "HEAL", "MELEE" },
        ["Adan"] = new[] { "HEAL" },
        ["Aron"] = new[] { "HEAL" },
        ["Wren"] = new[] { "MELEE", "TIME" },
        ["Lyle"] = new[] { "MELEE" },
        ["Ryan"] = new[] { "TIME" },
        ["Crys"] = new[] { "TIME" },
        // There are two Karas: the princess (Heal + Order) and the warrior (Melee + Order).
        ["Kara"] = new[] { "ORDER", "HEAL", "MELEE" },
    };

    public override string TechHeading => "TECHNIQUES";

    public override string? TechNote =>
        "Characters start with every technique in their groups and share power between them (levels aren't shown).";

    public override IReadOnlyList<InfoRow> Techniques(SaveState save, CharacterDef c)
    {
        var rows = new List<InfoRow>();
        if (!GroupsByName.TryGetValue(c.Name, out var groups))
        {
            return rows;
        }

        bool kara = c.Name.Equals("Kara", StringComparison.OrdinalIgnoreCase);
        foreach (var g in Groups.Where(g => groups.Contains(g.Group)))
        {
            string suffix = kara && g.Group == "HEAL" ? "   (Kara the princess)"
                : kara && g.Group == "MELEE" ? "   (Kara the warrior)"
                : "";
            rows.Add(InfoRow.Header($"{g.Group}   ·   {g.Tp} TP{suffix}"));
            foreach (var t in g.Techs)
            {
                rows.Add(new InfoRow(RowKind.Entry, t, $"{g.Tp} TP"));
            }
        }

        return rows;
    }
}
