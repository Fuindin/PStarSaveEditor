using System.Drawing.Drawing2D;
using System.Drawing.Text;
using PStarSaveEditor.Game;

namespace PStarSaveEditor;

/// <summary>
/// Palette taken from the Phantasy Star menus: near-black windows with a white outer
/// frame and a blue inner rim, white text, and the amber of the "Phantasy Star" logo
/// for headings and meseta, all set against the starfield of the title screens. Each
/// game adds its own accent colour (name plate, title plate, highlights).
/// </summary>
internal static class Theme
{
    public static readonly Color Void       = Color.FromArgb(0x03, 0x04, 0x0B); // deep space backdrop
    public static readonly Color Window     = Color.FromArgb(0x09, 0x0C, 0x1E); // menu windows
    public static readonly Color WindowHi   = Color.FromArgb(0x10, 0x15, 0x2E); // cards / tab pages
    public static readonly Color Field      = Color.FromArgb(0x19, 0x21, 0x4A); // numeric fields
    public static readonly Color FieldRO    = Color.FromArgb(0x0E, 0x13, 0x30); // read-only fields
    public static readonly Color Silver     = Color.FromArgb(0xF0, 0xF2, 0xFA); // white outer window frame
    public static readonly Color Steel      = Color.FromArgb(0x4A, 0x62, 0xB8); // blue inner rim
    public static readonly Color SteelDeep  = Color.FromArgb(0x26, 0x36, 0x72);
    public static readonly Color Gold       = Color.FromArgb(0xFF, 0xC4, 0x48); // logo amber / meseta
    public static readonly Color GoldDeep   = Color.FromArgb(0xA8, 0x6A, 0x0E);
    public static readonly Color Text       = Color.FromArgb(0xF4, 0xF6, 0xFF);
    public static readonly Color TextMuted  = Color.FromArgb(0x8A, 0x96, 0xC2);
    public static readonly Color HpBar      = Color.FromArgb(0x60, 0xDC, 0x58);
    public static readonly Color MpBar      = Color.FromArgb(0x50, 0xC8, 0xF8);
    public static readonly Color BarEmpty   = Color.FromArgb(0x22, 0x2C, 0x5A);
    public static readonly Color Edited     = Color.FromArgb(0xFF, 0x8C, 0x42); // unsaved-change marker

    /// <summary>Accent of the game currently being edited.</summary>
    public static GameAccent Accent { get; set; } =
        new(Color.FromArgb(0xC2, 0x60, 0x1C), Color.FromArgb(0x3E, 0x16, 0x06), Color.FromArgb(0xFF, 0xC8, 0x74));

    public static readonly string Family = "Segoe UI";

    public static Font Body(float size, FontStyle style = FontStyle.Regular) => new(Family, size, style);

    public static GraphicsPath Rounded(Rectangle r, int radius)
    {
        var p = new GraphicsPath();
        int d = Math.Max(2, radius * 2);
        p.AddArc(r.Left, r.Top, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Top, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }

    /// <summary>
    /// Paints a Phantasy Star style menu window: dark gradient fill, a white outer
    /// frame and a coloured inner rim, with softly rounded corners.
    /// </summary>
    public static void PaintWindow(Graphics g, Rectangle r, Color top, Color bottom, Color outer, Color inner, int radius = 8)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        r.Width -= 1;
        r.Height -= 1;
        if (r.Width < 8 || r.Height < 8)
        {
            return;
        }

        using (var path = Rounded(r, radius))
        using (var fill = new LinearGradientBrush(r, top, bottom, LinearGradientMode.Vertical))
        {
            g.FillPath(fill, path);
            using var pen = new Pen(outer, 2f);
            g.DrawPath(pen, path);
        }

        var ir = Rectangle.Inflate(r, -4, -4);
        using (var ipath = Rounded(ir, Math.Max(2, radius - 3)))
        using (var ipen = new Pen(inner, 1.5f))
        {
            g.DrawPath(ipen, ipath);
        }
    }

    public static void PaintWindow(Graphics g, Rectangle r, int radius = 8) =>
        PaintWindow(g, r, WindowHi, Window, Silver, Steel, radius);

    /// <summary>Scatters a fixed field of stars over <paramref name="r"/>, like the title screens.</summary>
    public static void PaintStars(Graphics g, Rectangle r, int seed, int pixelsPerStar = 2200)
    {
        if (r.Width <= 0 || r.Height <= 0)
        {
            return;
        }

        var rnd = new Random(seed);
        int count = Math.Max(8, r.Width * r.Height / pixelsPerStar);
        var old = g.SmoothingMode;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var brush = new SolidBrush(Color.White);
        for (int i = 0; i < count; i++)
        {
            float x = r.X + (float)rnd.NextDouble() * r.Width;
            float y = r.Y + (float)rnd.NextDouble() * r.Height;
            double roll = rnd.NextDouble();
            float size = roll > 0.97 ? 2.6f : roll > 0.85 ? 1.8f : 1.1f;
            int alpha = 70 + rnd.Next(150);
            brush.Color = roll > 0.93
                ? Color.FromArgb(alpha, 0xFF, 0xE0, 0xA8)   // a few warm stars
                : Color.FromArgb(alpha, 0xD8, 0xE4, 0xFF);
            g.FillEllipse(brush, x, y, size, size);
        }

        g.SmoothingMode = old;
    }

    /// <summary>
    /// Draws the Algo star system — the setting of all four games — with the world the
    /// game is set on picked out in its accent colour. Used when no box art is embedded.
    /// </summary>
    public static Bitmap RenderAlgoEmblem(int width, int height, AlgoWorld highlight, GameAccent accent, string title)
    {
        var bmp = new Bitmap(width, height);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        var r = new Rectangle(0, 0, width, height);

        using (var bg = new LinearGradientBrush(r, Color.FromArgb(0x0A, 0x0E, 0x26), Void, LinearGradientMode.Vertical))
        {
            g.FillRectangle(bg, r);
        }

        PaintStars(g, r, 1987, 900);

        float cx = width / 2f, cy = height * 0.50f;
        float unit = Math.Min(width, height * 0.9f);

        // Orbits (drawn first so worlds sit on top of them).
        float[] orbit = { unit * 0.20f, unit * 0.31f, unit * 0.42f, unit * 0.485f };
        const float tilt = 0.40f;
        using (var orbitPen = new Pen(Color.FromArgb(110, Steel), Math.Max(1.5f, unit / 360f)))
        {
            for (int i = 0; i < 3; i++)
            {
                g.DrawEllipse(orbitPen, cx - orbit[i], cy - orbit[i] * tilt, orbit[i] * 2, orbit[i] * 2 * tilt);
            }

            if (highlight == AlgoWorld.Alisa)
            {
                orbitPen.DashStyle = DashStyle.Dash;
                orbitPen.Color = Color.FromArgb(150, accent.Glow);
                g.DrawEllipse(orbitPen, cx - orbit[3], cy - orbit[3] * tilt, orbit[3] * 2, orbit[3] * 2 * tilt);
            }
        }

        // Algol, the sun.
        for (int i = 6; i >= 1; i--)
        {
            float rad = unit * 0.022f * i;
            using var glow = new SolidBrush(Color.FromArgb(18 + (6 - i) * 10, Gold));
            g.FillEllipse(glow, cx - rad, cy - rad, rad * 2, rad * 2);
        }

        float core = unit * 0.045f;
        using (var sun = new LinearGradientBrush(new RectangleF(cx - core, cy - core, core * 2, core * 2),
                   Color.FromArgb(0xFF, 0xFA, 0xE0), Gold, 45f))
        {
            g.FillEllipse(sun, cx - core, cy - core, core * 2, core * 2);
        }

        using var labelFont = new Font(Family, Math.Max(9f, unit / 34f), FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel);

        void World(AlgoWorld w, string name, float orbitR, float angleDeg, float size, Color light, Color dark)
        {
            double a = angleDeg * Math.PI / 180;
            float x = cx + orbitR * (float)Math.Cos(a);
            float y = cy + orbitR * tilt * (float)Math.Sin(a);
            bool on = w == highlight;
            float rad = size * (on ? 1.18f : 1f);

            if (on)
            {
                for (int i = 4; i >= 1; i--)
                {
                    float gr = rad + i * unit * 0.012f;
                    using var halo = new SolidBrush(Color.FromArgb(22 + (4 - i) * 14, accent.Glow));
                    g.FillEllipse(halo, x - gr, y - gr, gr * 2, gr * 2);
                }
            }

            var box = new RectangleF(x - rad, y - rad, rad * 2, rad * 2);
            using (var fill = new LinearGradientBrush(box, light, dark, 45f))
            {
                g.FillEllipse(fill, box);
            }

            using (var rim = new Pen(on ? accent.Glow : Color.FromArgb(120, Silver), on ? 2.4f : 1.2f))
            {
                g.DrawEllipse(rim, box);
            }

            using var tb = new SolidBrush(on ? accent.Glow : Color.FromArgb(150, TextMuted));
            var sz = g.MeasureString(name, labelFont);
            g.DrawString(name, labelFont, tb, x - sz.Width / 2, y + rad + unit * 0.012f);
        }

        float ps = unit * 0.034f;
        World(AlgoWorld.Palma, "PALMA", orbit[0], 205, ps * 1.05f, Color.FromArgb(0x8C, 0xE0, 0xA8), Color.FromArgb(0x1C, 0x5C, 0xB8));
        World(AlgoWorld.Motavia, "MOTAVIA", orbit[1], 335, ps * 1.25f, Color.FromArgb(0xF6, 0xD8, 0x96), Color.FromArgb(0xA8, 0x5C, 0x20));
        World(AlgoWorld.Dezolis, "DEZOLIS", orbit[2], 70, ps * 0.95f, Color.FromArgb(0xFF, 0xFF, 0xFF), Color.FromArgb(0x7C, 0x98, 0xC4));

        if (highlight == AlgoWorld.Alisa)
        {
            // Alisa III, the generation ship: a small ringed station on the outer path.
            World(AlgoWorld.Alisa, "ALISA III", orbit[3], 140, ps * 0.6f, Color.FromArgb(0xE8, 0xE0, 0xFF), accent.PlateTop);
            double a = 140 * Math.PI / 180;
            float x = cx + orbit[3] * (float)Math.Cos(a);
            float y = cy + orbit[3] * tilt * (float)Math.Sin(a);
            using var ring = new Pen(accent.Glow, 2f);
            g.DrawEllipse(ring, x - ps * 1.5f, y - ps * 0.45f, ps * 3f, ps * 0.9f);
        }

        // Heading.
        using var head = new Font(Family, Math.Max(10f, unit / 22f), FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel);
        using var sub = new Font(Family, Math.Max(8f, unit / 36f), FontStyle.Regular, GraphicsUnit.Pixel);
        using var fmt = new StringFormat { Alignment = StringAlignment.Center };
        using (var shadow = new SolidBrush(GoldDeep))
        using (var gold = new SolidBrush(Gold))
        {
            var hr = new RectangleF(0, height * 0.06f, width, unit / 12f);
            g.DrawString(title.ToUpperInvariant(), head, shadow, new RectangleF(hr.X + 2, hr.Y + 2, hr.Width, hr.Height), fmt);
            g.DrawString(title.ToUpperInvariant(), head, gold, hr, fmt);
        }

        using (var muted = new SolidBrush(TextMuted))
        {
            g.DrawString("THE ALGO STAR SYSTEM", sub, muted, new RectangleF(0, height * 0.88f, width, unit / 16f), fmt);
        }

        // White frame with an accent rim, like a menu window.
        using (var frame = new Pen(Silver, Math.Max(3f, unit / 120f)))
        {
            g.DrawRectangle(frame, 2, 2, width - 5, height - 5);
        }

        using (var rim = new Pen(accent.Glow, Math.Max(1.5f, unit / 260f)))
        {
            int inset = (int)Math.Max(8f, unit / 45f);
            g.DrawRectangle(rim, inset, inset, width - 1 - inset * 2, height - 1 - inset * 2);
        }

        return bmp;
    }

    /// <summary>Applies the palette to a simple dialog (Error Log).</summary>
    public static void ApplyDialog(Control root)
    {
        root.BackColor = Window;
        root.ForeColor = Text;
        foreach (Control c in root.Controls)
        {
            switch (c)
            {
                case TextBoxBase tb:
                    tb.BackColor = Void;
                    tb.ForeColor = Text;
                    tb.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case Button b:
                    StyleButton(b);
                    break;
                case PictureBox:
                    break;
                default:
                    c.BackColor = Window;
                    c.ForeColor = Text;
                    break;
            }

            if (c.HasChildren && c is not TextBoxBase)
            {
                ApplyDialog(c);
            }
        }
    }

    public static void StyleButton(Button b)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.BackColor = Accent.PlateBottom;
        b.ForeColor = Text;
        b.Font = Body(10f, FontStyle.Bold);
        b.FlatAppearance.BorderColor = Silver;
        b.FlatAppearance.BorderSize = 2;
        b.FlatAppearance.MouseOverBackColor = Accent.PlateTop;
        b.FlatAppearance.MouseDownBackColor = SteelDeep;
        b.Cursor = Cursors.Hand;
    }
}

/// <summary>Themed colours for the menu strip (dark popups, blue highlight).</summary>
internal sealed class PhantasyMenuColors : ProfessionalColorTable
{
    public override Color MenuStripGradientBegin => Theme.Void;
    public override Color MenuStripGradientEnd => Theme.Void;
    public override Color ToolStripDropDownBackground => Theme.Window;
    public override Color ImageMarginGradientBegin => Theme.Window;
    public override Color ImageMarginGradientMiddle => Theme.Window;
    public override Color ImageMarginGradientEnd => Theme.Window;
    public override Color MenuItemSelected => Theme.SteelDeep;
    public override Color MenuItemSelectedGradientBegin => Theme.SteelDeep;
    public override Color MenuItemSelectedGradientEnd => Theme.SteelDeep;
    public override Color MenuItemBorder => Theme.Steel;
    public override Color MenuItemPressedGradientBegin => Theme.Field;
    public override Color MenuItemPressedGradientEnd => Theme.Window;
    public override Color MenuBorder => Theme.Steel;
    public override Color SeparatorDark => Theme.Steel;
    public override Color CheckBackground => Theme.SteelDeep;
    public override Color CheckSelectedBackground => Theme.Steel;
    public override Color CheckPressedBackground => Theme.Steel;
}
