using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PStarSaveEditor
{
    public partial class MainForm : Form
    {
        #region - Enums -
        private enum AppPanel
        {
            All,
            PStar1,
            PStar2,
            PStar3,
            PStar4,
            None
        }
        #endregion

        #region - Class Fields -
        private const ushort PS1_SHORT_MAX = 65535;
        private const short PS1_BYTE_MAX = 255;
        private const short PS4_BYTE_MAX = 255;
        private const string PS1_MESETA_LOC = "459C";
        private const string PS2_MESETA_LOC = "EA98";
        private const string PS3_MESETA_LOC = "E4B8";
        private const string PS4_MESETA_LOC = "118B0";
        private AppPanel activePanel;
        private List<PSItem> ps4ItemsList;
        private List<PSItem> ps1ItemsList;
        // Files backed up (once) this session, so we don't re-snapshot on every edit.
        private readonly HashSet<string> backedUpThisSession =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        #endregion

        #region - Class Properties -
        /// <summary>
        /// Derived from the selected path rather than tracked in a field. The game menu
        /// handlers clear the path when switching games, and a file can be moved or
        /// deleted while the application is open, so a cached flag drifts out of step
        /// with what is actually readable.
        /// </summary>
        private bool FileLoaded
        {
            get { return saveStateFileTb.Text != string.Empty && File.Exists(saveStateFileTb.Text); }
        }
        private AppPanel ActivePanel
        {
            get { return activePanel; }
            set { activePanel = value; }
        }
        #endregion

        #region - Class Constructor -
        public MainForm()
        {
            InitializeComponent();
        }
        #endregion

        #region - Class Event Handlers -
        private void MainForm_Load(object sender, EventArgs e)
        {
            ActivePanel = AppPanel.None;
            ShowPanel(AppPanel.All, false);
        }

        private void browseBtn_Click(object sender, EventArgs e)
        {
            string game = GetSelectedGameTitle();
            if (game != string.Empty)
            {
                // Set the open file dialog properties
                openFD.Title = "Select a " + game + " save state file";
                openFD.InitialDirectory = string.Empty;
                openFD.FileName = "";

                // Show the open file dialog and capture the selected file
                if (openFD.ShowDialog() != DialogResult.Cancel)
                {
                    saveStateFileTb.Text = openFD.FileName;
                    switch (ActivePanel)
                    {
                        case AppPanel.PStar1:
                            PopulatePS1CurrentMeseta();
                            break;

                        case AppPanel.PStar2:
                            PopulatePS2CurrentMeseta();
                            break;

                        case AppPanel.PStar3:
                            PopulatePS3CurrentMeseta();
                            break;

                        case AppPanel.PStar4:
                            PopulatePS4CurrentMeseta();
                            break;
                    }
                }
                else
                {
                    saveStateFileTb.Text = "";
                }
            }
            else
            {
                MessageBox.Show("You must first select a Phantasy Star game from the menu to ensure the correct game data is loaded.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void clearErrorLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearErrorLog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void phantasyStarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActivePanel = AppPanel.PStar1;
            saveStateFileTb.Text = string.Empty;
            ps1CharacterCmb.SelectedIndex = -1;
            ResetPS1Controls();
            PopulatePS1CharacterList();
            PopulatePS1ItemsList();
            ShowPanel(AppPanel.PStar1, true);
        }

        private void phantasyStar2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActivePanel = AppPanel.PStar2;
            saveStateFileTb.Text = string.Empty;
            ps2CharacterCmb.SelectedIndex = -1;
            ResetPS2Controls();
            PopulatePS2CharacterList();
            ShowPanel(AppPanel.PStar2, true);
        }

        private void phantasyStar3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActivePanel = AppPanel.PStar3;
            saveStateFileTb.Text = string.Empty;
            ResetPS3Controls();
            PopulatePS3CharacterList();
            ShowPanel(AppPanel.PStar3, true);
        }

        private void phantasyStar4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActivePanel = AppPanel.PStar4;
            saveStateFileTb.Text = string.Empty;
            ps4CharacterCmb.SelectedIndex = -1;
            ResetPS4Controls();
            PopulatePS4CharacterList();
            PopulatePS4ItemsList();
            ShowPanel(AppPanel.PStar4, true);
        }

        private void errorLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErrorLogView errLogView = new ErrorLogView();
            errLogView.ShowDialog();
        }

        private void ps1CharacterCmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ps1CharacterCmb.SelectedIndex >= 0)
            {
                if (FileLoaded)
                {
                    PopulatePS1CharacterDetails(ps1CharacterCmb.SelectedItem as PS1CharacterItem);
                }
                else
                {
                    MessageBox.Show("You must load a save state file before you can view a character from it.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void ps2CharacterCmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ps2CharacterCmb.SelectedIndex >= 0)
            {
                if (FileLoaded)
                {
                    PS2CharacterItem selItem = ps2CharacterCmb.SelectedItem as PS2CharacterItem;
                    if (selItem.Name == "Rudo Steiner")
                    {
                        ShowControl(ps2NewCurTPTb, false);
                        ShowControl(ps2NewMaxTPTb, false);
                    }
                    else
                    {
                        ShowControl(ps2NewCurTPTb, true);
                        ShowControl(ps2NewMaxTPTb, true);
                    }
                    PopulatePS2CharacterDetails(selItem);
                }
                else
                {
                    MessageBox.Show("You must load a save state file before you can view a character from it.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void ps3CharacterCmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ps3CharacterCmb.SelectedIndex >= 0)
            {
                if (FileLoaded)
                {
                    PopulatePS3CharacterDetails(ps3CharacterCmb.SelectedItem as PS3CharacterItem);
                }
                else
                {
                    MessageBox.Show("You must load a save state file before you can view a character from it.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void ps4CharacterCmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ps4CharacterCmb.SelectedIndex >= 0)
            {
                if (FileLoaded)
                {
                    PopulatePS4CharacterDetails(ps4CharacterCmb.SelectedItem as PS4CharacterItem);
                }
                else
                {
                    MessageBox.Show("You must load a save state file before you can view a character from it.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void upsPS1SaveStateBtn_Click(object sender, EventArgs e)
        {
            if (PrepareForUpdate())
            {
                UpdatePS1SaveState();
            }
        }

        private void ps2UpdSavStateBtn_Click(object sender, EventArgs e)
        {
            if (PrepareForUpdate())
            {
                UpdatePS2SaveState();
            }
        }

        private void ps3UpdSavStateBtn_Click(object sender, EventArgs e)
        {
            if (PrepareForUpdate())
            {
                UpdatePS3SaveState();
            }
        }

        private void ps4UpdSavStateBtn_Click(object sender, EventArgs e)
        {
            if (PrepareForUpdate())
            {
                UpdatePS4SaveState();
            }
        }
        #endregion

        #region - Class Methods -
        /// <summary>
        /// Copies the save state to "&lt;path&gt;.bak" once per session before the first
        /// edit, so the pre-edit state can always be recovered. A failure here is logged
        /// rather than fatal, so it cannot block the edit the user asked for.
        /// </summary>
        private void EnsureBackup(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path) || backedUpThisSession.Contains(path))
                {
                    return;
                }

                File.Copy(path, path + ".bak", true);
                backedUpThisSession.Add(path);
            }
            catch (Exception e)
            {
                LogError(e.Message + " Occurred while creating a backup (.bak) of the save state file.");
            }
        }

        /// <summary>
        /// Runs before any save state is written. Confirms a file is actually loaded and
        /// takes the session's backup of it. The README asks the user to make a backup by
        /// hand before editing, which is easy to forget and impossible to act on once a
        /// bad write has already happened. Returns false to abort the update.
        /// </summary>
        private bool PrepareForUpdate()
        {
            if (!FileLoaded)
            {
                MessageBox.Show("You must load a save state file before it can be updated.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Pull the file into memory before backing it up, so a save state that
            // cannot be read is rejected before anything else happens.
            if (!EnsureBufferLoaded())
            {
                MessageBox.Show("The save state could not be read. See the error log for details.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            EnsureBackup(saveStateFileTb.Text);
            return true;
        }

        private void LogError(string errMsg)
        {
            // Create a write and open the file
            string filePath = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            filePath += @"\errorlog.txt";

            // This is called from inside the catch blocks of the file read and write
            // methods, so it must never throw. If the log cannot be written there is
            // nowhere left to report that, and taking down the application over it
            // would lose the edit the user was making.
            try
            {
                using (TextWriter writer = new StreamWriter(filePath, true))
                {
                    // Write the error message to the error log
                    writer.WriteLine(errMsg + " Added: " + DateTime.Now.ToString());
                }
            }
            catch (Exception)
            {
            }
        }

        private void ClearErrorLog()
        {
            // Create the file path
            string filePath = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            filePath += @"\errorlog.txt";

            string errMessage = string.Empty;

            // Delete the file
            try
            {
                File.Delete(filePath);
                MessageBox.Show("The error log has been cleared.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (IOException ioE)
            {
                errMessage = ioE.Message + " Occurred during call to ClearErroLog().";
                MessageBox.Show(errMessage, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (UnauthorizedAccessException uaE)
            {
                errMessage = uaE.Message + " Occurred during call to ClearErroLog().";
                MessageBox.Show(errMessage, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception e)
            {
                errMessage = e.Message + " Occurred during call to ClearErroLog().";
                MessageBox.Show(errMessage, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetSelectedGameTitle()
        {
            string game = string.Empty;
            switch (ActivePanel)
            {
                case AppPanel.PStar1:
                    game = "Phantasy Star";
                    break;

                case AppPanel.PStar2:
                    game = "Phantasy Star 2";
                    break;

                case AppPanel.PStar3:
                    game = "Phantasy Star 3";
                    break;

                case AppPanel.PStar4:
                    game = "Phantasy Star 4";
                    break;
            }

            return game;
        }

        private void ShowPanel(AppPanel panel, bool show)
        {
            switch (panel)
            {
                case AppPanel.All:
                    pstar1Panel.Visible = show;
                    pstar2Panel.Visible = show;
                    pstar3Panel.Visible = show;
                    pstar4Panel.Visible = show;
                    break;

                case AppPanel.PStar1:
                    pstar1Panel.Visible = show;
                    if (show)
                    {
                        pstar2Panel.Visible = !show;
                        pstar3Panel.Visible = !show;
                        pstar4Panel.Visible = !show;
                    }
                    break;

                case AppPanel.PStar2:
                    pstar2Panel.Visible = show;
                    if (show)
                    {
                        pstar1Panel.Visible = !show;
                        pstar3Panel.Visible = !show;
                        pstar4Panel.Visible = !show;
                    }
                    break;

                case AppPanel.PStar3:
                    pstar3Panel.Visible = show;
                    if (show)
                    {
                        pstar1Panel.Visible = !show;
                        pstar2Panel.Visible = !show;
                        pstar4Panel.Visible = !show;
                    }
                    break;

                case AppPanel.PStar4:
                    pstar4Panel.Visible = show;
                    if (show)
                    {
                        pstar1Panel.Visible = !show;
                        pstar2Panel.Visible = !show;
                        pstar3Panel.Visible = !show;
                    }
                    break;
            }
        }

        private void ShowControl(TextBox control, bool show)
        {
            control.Visible = show;
        }

        private void ShowControl(Label control, bool show)
        {
            control.Visible = show;
        }

        private static string Hex(int value)
        {
            return value.ToString("X");
        }

        // Each Phantasy Star 3 character record is 0x80 bytes. The poison flag sits
        // just below the record rather than inside it, and the item count lives in a
        // separate table whose spacing is irregular, so it is passed in explicitly.
        private PS3CharacterItem MakePS3Char(string name, int b, string itemCntLoc)
        {
            return new PS3CharacterItem(name,
                Hex(b + 0x00),    // speed       (1 byte)
                Hex(b + 0x03),    // name        (4 ASCII bytes)
                Hex(b + 0x08),    // level
                Hex(b + 0x0A),    // max HP
                Hex(b + 0x0C),    // max TP
                Hex(b + 0x0E),    // current HP
                Hex(b + 0x10),    // current TP
                Hex(b + 0x12),    // damage
                Hex(b + 0x14),    // defense
                Hex(b + 0x16),    // experience  (4 bytes)
                Hex(b + 0x2E),    // luck        (1 byte)
                Hex(b + 0x2F),    // skill       (1 byte)
                Hex(b - 0x23),    // poison flag, below the record
                itemCntLoc);
        }

        private void PopulatePS3CharacterList()
        {
            ps3CharacterCmb.Items.Clear();

            // Phantasy Star 3 spans three generations that reuse the same five party
            // records, so several names read from one record. Each row lists the names
            // sharing a record, in the order they appear in the drop down.
            var slots = new[]
            {
                new { Names = new[] { "Rhys", "Ayn", "Nial" },  Base = 0xE51C, ItemCnt = "102F8" },
                new { Names = new[] { "Mieu" },                 Base = 0xE59C, ItemCnt = "1031B" },
                new { Names = new[] { "Wren" },                 Base = 0xE61C, ItemCnt = "10338" },
                new { Names = new[] { "Lyle", "Thea", "Ryan" }, Base = 0xE69C, ItemCnt = "10358" },
                new { Names = new[] { "Lena", "Sari", "Laya" }, Base = 0xE71C, ItemCnt = "10378" }
            };

            foreach (var slot in slots)
            {
                foreach (string name in slot.Names)
                {
                    ps3CharacterCmb.Items.Add(MakePS3Char(name, slot.Base, slot.ItemCnt));
                }
            }

            ps3CharacterCmb.DisplayMember = "Name";
        }

        private void PopulatePS3CharacterDetails(PS3CharacterItem charItem)
        {
            string value = string.Empty;
            value = GetValueByOffset(charItem.NameLoc, 4);
            byte[] bytes = HexStringToBytes(value);
            ps3CurNameTb.Text = Encoding.ASCII.GetString(bytes);
            value = GetValueByOffset(charItem.SpeedLoc, 1);
            long val = ParseHexOrZero(value);
            ps3CurSpeedTb.Text = val.ToString();
            value = GetValueByOffset(charItem.LevelLoc, 2);
            val = ParseHexOrZero(value);
            ps3CurLevelTb.Text = val.ToString();
            value = GetValueByOffset(charItem.MaxHPLoc, 2);
            val = ParseHexOrZero(value);
            ps3CurMaxHPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.MaxTPLoc, 2);
            val = ParseHexOrZero(value);
            ps3CurMaxTPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.CurHPLoc, 2);
            val = ParseHexOrZero(value);
            ps3CurCurHPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.CurTPLoc, 2);
            val = ParseHexOrZero(value);
            ps3CurCurTPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.DmgLoc, 2);
            val = ParseHexOrZero(value);
            ps3CurDmgTb.Text = val.ToString();
            value = GetValueByOffset(charItem.DefLoc, 2);
            val = ParseHexOrZero(value);
            ps3CurDefTb.Text = val.ToString();
            value = GetValueByOffset(charItem.ExpLoc, 4);
            val = ParseHexOrZero(value);
            ps3CurExpTb.Text = val.ToString();
            value = GetValueByOffset(charItem.LuckLoc, 1);
            val = ParseHexOrZero(value);
            ps3CurLuckTb.Text = val.ToString();
            value = GetValueByOffset(charItem.SkillLoc, 1);
            val = ParseHexOrZero(value);
            ps3CurSkillTb.Text = val.ToString();
            value = GetValueByOffset(charItem.PoisonLoc, 1);
            if (value == "40")
            {
                ps3CurPoisonChk.Checked = true;
            }
            else
            {
                ps3CurPoisonChk.Checked = false;
            }
            //value = GetValueByOffset(charItem.ItemCntLoc, 2);
            //val = ParseHexOrZero(value);
            //curItemCountTb.Text = val.ToString();
        }

        // The whole file is loaded once into fileBytes; all reads and writes go through
        // it, and each "Update Save State" flushes it to disk in a single write instead
        // of re-opening the file for every field.
        private byte[] fileBytes;
        private string loadedPath;
        private DateTime loadedWriteTime;
        private long loadedLength;

        private static int ParseOffset(string offset)
        {
            return int.Parse(offset, System.Globalization.NumberStyles.HexNumber);
        }

        /// <summary>
        /// Ensures fileBytes holds the contents of the file named in the path box. The
        /// buffer is re-read when the path changes, and also when the file's size or
        /// timestamp has moved underneath us: the emulator may well have written a new
        /// save state to the same path while this window sat open, and flushing a stale
        /// buffer over it would silently discard that.
        /// </summary>
        private bool EnsureBufferLoaded()
        {
            string path = saveStateFileTb.Text;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return false;
            }

            try
            {
                FileInfo info = new FileInfo(path);
                bool current = fileBytes != null
                    && string.Equals(loadedPath, path, StringComparison.OrdinalIgnoreCase)
                    && loadedLength == info.Length
                    && loadedWriteTime == info.LastWriteTimeUtc;

                if (current)
                {
                    return true;
                }

                fileBytes = File.ReadAllBytes(path);
                loadedPath = path;
                loadedLength = info.Length;
                loadedWriteTime = info.LastWriteTimeUtc;
                return true;
            }
            catch (Exception e)
            {
                LogError(e.Message + " Occurred while loading the save state into memory.");
                fileBytes = null;
                loadedPath = null;
                return false;
            }
        }

        /// <summary>Writes the in-memory buffer back to disk in a single operation.</summary>
        private bool SaveBufferToDisk()
        {
            if (fileBytes == null || string.IsNullOrEmpty(loadedPath))
            {
                return false;
            }

            try
            {
                File.WriteAllBytes(loadedPath, fileBytes);

                // Adopt the timestamp we just created, so the staleness check above does
                // not mistake our own write for someone else's.
                FileInfo info = new FileInfo(loadedPath);
                loadedLength = info.Length;
                loadedWriteTime = info.LastWriteTimeUtc;
                return true;
            }
            catch (Exception e)
            {
                LogError(e.Message + " Occurred while saving the save state to disk.");
                MessageBox.Show("The save state could not be written to disk. See the error log for details.",
                    "Save failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Copies count bytes from source[index..] into the buffer at offset. Every write
        /// funnels through here so the bounds check cannot be skipped: a bad offset used
        /// to be caught by the file stream, and now has to be caught by us.
        /// </summary>
        private bool WriteBytes(int offset, byte[] source, int index, int count)
        {
            if (!EnsureBufferLoaded())
            {
                return false;
            }

            if (offset < 0 || count < 0 || index < 0
                || offset + count > fileBytes.Length || index + count > source.Length)
            {
                LogError("Refused out-of-range write at offset 0x" + offset.ToString("X") + " (" + count.ToString() + " bytes).");
                return false;
            }

            Buffer.BlockCopy(source, index, fileBytes, offset, count);
            return true;
        }

        private string GetValueByOffset(string offset, int bytesToRead)
        {
            if (!EnsureBufferLoaded())
            {
                return string.Empty;
            }

            try
            {
                return BitConverter.ToString(fileBytes, ParseOffset(offset), bytesToRead).Replace("-", null);
            }
            catch (Exception e)
            {
                LogError(e.Message + " Occurred when attempting to read a value by its offset.");
                return string.Empty;
            }
        }

        /// <summary>
        /// Parses a hex string read out of the save state. GetValueByOffset returns an
        /// empty string when the read fails, so this must tolerate that rather than
        /// throwing a FormatException out of the display code.
        /// </summary>
        private long ParseHexOrZero(string hexValue)
        {
            long result = 0;

            if (!string.IsNullOrEmpty(hexValue))
            {
                long.TryParse(hexValue, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out result);
            }

            return result;
        }

        private string ReverseHexPairs(string hexString)
        {
            string newHex = string.Empty;

            // Only a two byte value can be swapped here. Anything else, including the
            // empty string a failed read returns, is passed through untouched.
            if (hexString.Length != 4)
            {
                newHex = hexString;
            }
            else
            {
                newHex = hexString.Substring(2, 2);
                newHex += hexString.Substring(0, 2);
            }

            return newHex;
        }

        // The Genesis / Mega Drive saves are big-endian, so multi-byte values are
        // reversed before writing. Phantasy Star is a Master System game and stores its
        // two-byte values little-endian, which is what the ushort overload's reverse
        // flag selects. Each overload builds its bytes and routes through WriteBytes.
        private bool SetValueByOffset(int value, string offset)
        {
            byte[] bytes = BitConverter.GetBytes(value).Reverse().ToArray();
            return WriteBytes(ParseOffset(offset), bytes, 0, 4);
        }

        private bool SetValueByOffset(short value, string offset)
        {
            byte[] bytes = BitConverter.GetBytes(value).Reverse().ToArray();
            return WriteBytes(ParseOffset(offset), bytes, 0, 2);
        }

        private bool SetValueByOffset(ushort value, string offset, bool reverse)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (reverse)
            {
                bytes = bytes.Reverse().ToArray();
            }

            return WriteBytes(ParseOffset(offset), bytes, 0, 2);
        }

        private bool SetValueByOffset(byte value, string offset)
        {
            return WriteBytes(ParseOffset(offset), new byte[] { value }, 0, 1);
        }

        private string GetCurrentPS4Meseta()
        {
            string hexVal = GetValueByOffset(PS4_MESETA_LOC, 4);
            long meseta = ParseHexOrZero(hexVal);
            return meseta.ToString();
        }

        private void PopulatePS4CurrentMeseta()
        {
            ps4CurrentMesetaTb.Text = GetCurrentPS4Meseta();
        }

        private void ResetPS4Controls()
        {
            ClearPanelFields(pstar4Panel);
        }

        private void PopulatePS4ItemsList()
        {
            ps4ItemsList = new List<PSItem>();
            ps4ItemsList.Add(new PSItem("00", "Empty"));
            ps4ItemsList.Add(new PSItem("01", "Dagger"));
            ps4ItemsList.Add(new PSItem("02", "Hunting Knife"));
            ps4ItemsList.Add(new PSItem("03", "Boomerang"));
            ps4ItemsList.Add(new PSItem("04", "Leather Cloth"));
            ps4ItemsList.Add(new PSItem("05", "Leather Helm"));
            ps4ItemsList.Add(new PSItem("06", "Leather Crown"));
            ps4ItemsList.Add(new PSItem("07", "Leather Band"));
            ps4ItemsList.Add(new PSItem("08", "Steel Sword"));
            ps4ItemsList.Add(new PSItem("09", "Slasher"));
            ps4ItemsList.Add(new PSItem("0A", "Leather Shield"));
            ps4ItemsList.Add(new PSItem("0B", "Carbon Suit"));
            ps4ItemsList.Add(new PSItem("0C", "Carbon Shield"));
            ps4ItemsList.Add(new PSItem("0D", "Carbon Helm"));
            ps4ItemsList.Add(new PSItem("0E", "Carbon Crown"));
            ps4ItemsList.Add(new PSItem("0F", "Circlet"));
            ps4ItemsList.Add(new PSItem("10", "Wooden Cane"));
            ps4ItemsList.Add(new PSItem("11", "White Mantle"));
            ps4ItemsList.Add(new PSItem("12", "Titanium Sword"));
            ps4ItemsList.Add(new PSItem("13", "Titanium Dagger"));
            ps4ItemsList.Add(new PSItem("14", "Titanium Slasher"));
            ps4ItemsList.Add(new PSItem("15", "Titanium Axe"));
            ps4ItemsList.Add(new PSItem("16", "Broad Axe"));
            ps4ItemsList.Add(new PSItem("17", "Titanium Mail"));
            ps4ItemsList.Add(new PSItem("18", "Titanium Shield"));
            ps4ItemsList.Add(new PSItem("19", "Titanium Helm"));
            ps4ItemsList.Add(new PSItem("1A", "Titanium Crown"));
            ps4ItemsList.Add(new PSItem("1B", "Nothing*"));
            ps4ItemsList.Add(new PSItem("1C", "Graphite Suit"));
            ps4ItemsList.Add(new PSItem("1D", "Graphite Shield"));
            ps4ItemsList.Add(new PSItem("1E", "Ceramic Sword"));
            ps4ItemsList.Add(new PSItem("1F", "Graphite Crown"));
            ps4ItemsList.Add(new PSItem("20", "Claw"));
            ps4ItemsList.Add(new PSItem("21", "Ceramic Knife"));
            ps4ItemsList.Add(new PSItem("22", "Ceramic Shield"));
            ps4ItemsList.Add(new PSItem("23", "Laser Slasher"));
            ps4ItemsList.Add(new PSItem("24", "Saber Claw"));
            ps4ItemsList.Add(new PSItem("25", "Struggle Axe"));
            ps4ItemsList.Add(new PSItem("26", "Ceramic Mail"));
            ps4ItemsList.Add(new PSItem("27", "Ceramic Helm"));
            ps4ItemsList.Add(new PSItem("28", "Laser Sword"));
            ps4ItemsList.Add(new PSItem("29", "Laser Claw"));
            ps4ItemsList.Add(new PSItem("2A", "Laser Barrier"));
            ps4ItemsList.Add(new PSItem("2B", "Impacter"));
            ps4ItemsList.Add(new PSItem("2C", "Titanium Armor"));
            ps4ItemsList.Add(new PSItem("2D", "Head Gear"));
            ps4ItemsList.Add(new PSItem("2E", "Stun Shot"));
            ps4ItemsList.Add(new PSItem("2F", "Laser Axe"));
            ps4ItemsList.Add(new PSItem("30", "Laser Knife"));
            ps4ItemsList.Add(new PSItem("31", "Ceramic Armor"));
            ps4ItemsList.Add(new PSItem("32", "Titanium Gear"));
            ps4ItemsList.Add(new PSItem("33", "Psychic Mail"));
            ps4ItemsList.Add(new PSItem("34", "Psychic Shield"));
            ps4ItemsList.Add(new PSItem("35", "Psychic Crown"));
            ps4ItemsList.Add(new PSItem("36", "Psychic Circlet"));
            ps4ItemsList.Add(new PSItem("37", "Force Cane"));
            ps4ItemsList.Add(new PSItem("38", "Psychic Robe"));
            ps4ItemsList.Add(new PSItem("39", "Psycho Wand"));
            ps4ItemsList.Add(new PSItem("3A", "Frade Mantle"));
            ps4ItemsList.Add(new PSItem("3B", "Wave Shot"));
            ps4ItemsList.Add(new PSItem("3C", "Space Armor"));
            ps4ItemsList.Add(new PSItem("3D", "Ceramic Gear"));
            ps4ItemsList.Add(new PSItem("3E", "Plasma Rifle"));
            ps4ItemsList.Add(new PSItem("3F", "Pulse Laser"));
            ps4ItemsList.Add(new PSItem("40", "Plasma Sword"));
            ps4ItemsList.Add(new PSItem("41", "Plasma Claw"));
            ps4ItemsList.Add(new PSItem("42", "Plasma Dagger"));
            ps4ItemsList.Add(new PSItem("43", "Plasma Field"));
            ps4ItemsList.Add(new PSItem("44", "Silver Rod"));
            ps4ItemsList.Add(new PSItem("45", "Silver Mantle"));
            ps4ItemsList.Add(new PSItem("46", "Silver Circlet"));
            ps4ItemsList.Add(new PSItem("47", "Silver Mail"));
            ps4ItemsList.Add(new PSItem("48", "Silver Shield"));
            ps4ItemsList.Add(new PSItem("49", "Silver Helm"));
            ps4ItemsList.Add(new PSItem("4A", "Silver Crown"));
            ps4ItemsList.Add(new PSItem("4B", "Zirco Gear"));
            ps4ItemsList.Add(new PSItem("4C", "Napalm Shot"));
            ps4ItemsList.Add(new PSItem("4D", "Zirco Armor"));
            ps4ItemsList.Add(new PSItem("4E", "Flame Sword"));
            ps4ItemsList.Add(new PSItem("4F", "Thunder Claw"));
            ps4ItemsList.Add(new PSItem("50", "Tornado Dagger"));
            ps4ItemsList.Add(new PSItem("51", "Dream Rod"));
            ps4ItemsList.Add(new PSItem("52", "Phantasm Robe"));
            ps4ItemsList.Add(new PSItem("53", "Silver Tusk"));
            ps4ItemsList.Add(new PSItem("54", "Pulse Vulcan"));
            ps4ItemsList.Add(new PSItem("55", "Compound Armor"));
            ps4ItemsList.Add(new PSItem("56", "Compound Gear"));
            ps4ItemsList.Add(new PSItem("57", "Reflect Mail"));
            ps4ItemsList.Add(new PSItem("58", "Reflect Shield"));
            ps4ItemsList.Add(new PSItem("59", "Reflect Robe"));
            ps4ItemsList.Add(new PSItem("5A", "Laconian Sword"));
            ps4ItemsList.Add(new PSItem("5B", "Laconian Dagger"));
            ps4ItemsList.Add(new PSItem("5C", "Laconian Claw"));
            ps4ItemsList.Add(new PSItem("5D", "Laconian Slasher"));
            ps4ItemsList.Add(new PSItem("5E", "Guard Rod"));
            ps4ItemsList.Add(new PSItem("5F", "Plasma Launcher"));
            ps4ItemsList.Add(new PSItem("60", "Elastic Armor"));
            ps4ItemsList.Add(new PSItem("61", "Elastic Gear"));
            ps4ItemsList.Add(new PSItem("62", "Laconian Rod"));
            ps4ItemsList.Add(new PSItem("63", "Genocycle Claw"));
            ps4ItemsList.Add(new PSItem("64", "Swift Helm"));
            ps4ItemsList.Add(new PSItem("65", "Moon Slasher"));
            ps4ItemsList.Add(new PSItem("66", "Power Shield"));
            ps4ItemsList.Add(new PSItem("67", "Laconian Mail"));
            ps4ItemsList.Add(new PSItem("68", "Laconian Helm"));
            ps4ItemsList.Add(new PSItem("69", "Laconian Crown"));
            ps4ItemsList.Add(new PSItem("6A", "Laconian Circlet"));
            ps4ItemsList.Add(new PSItem("6B", "Laconian Shield"));
            ps4ItemsList.Add(new PSItem("6C", "Cyber Suit"));
            ps4ItemsList.Add(new PSItem("6D", "Guard Sword"));
            ps4ItemsList.Add(new PSItem("6E", "Photon Eraser"));
            ps4ItemsList.Add(new PSItem("6F", "Laconian Armor"));
            ps4ItemsList.Add(new PSItem("70", "Laconian Gear"));
            ps4ItemsList.Add(new PSItem("71", "Mahlay Dagger"));
            ps4ItemsList.Add(new PSItem("72", "Guard Claw"));
            ps4ItemsList.Add(new PSItem("73", "Guard Armor"));
            ps4ItemsList.Add(new PSItem("74", "Guard Robe"));
            ps4ItemsList.Add(new PSItem("75", "Guard Mail"));
            ps4ItemsList.Add(new PSItem("76", "Nothing*"));
            ps4ItemsList.Add(new PSItem("77", "Elsydeon"));
            ps4ItemsList.Add(new PSItem("78", "Laconian Axe"));
            ps4ItemsList.Add(new PSItem("79", "Sonic Buster"));
            ps4ItemsList.Add(new PSItem("7A", "Defeat Axe"));
            ps4ItemsList.Add(new PSItem("7B", "Nothing"));
            ps4ItemsList.Add(new PSItem("7C", "Mahlay Mail"));
            ps4ItemsList.Add(new PSItem("7D", "Monomate"));
            ps4ItemsList.Add(new PSItem("7E", "Dimate"));
            ps4ItemsList.Add(new PSItem("7F", "Trimate"));
            ps4ItemsList.Add(new PSItem("80", "Antidote"));
            ps4ItemsList.Add(new PSItem("81", "Cure Paralysis"));
            ps4ItemsList.Add(new PSItem("82", "Moon Dew"));
            ps4ItemsList.Add(new PSItem("83", "Star Dew"));
            ps4ItemsList.Add(new PSItem("84", "Telepipe"));
            ps4ItemsList.Add(new PSItem("85", "Escapipe"));
            ps4ItemsList.Add(new PSItem("86", "Sole Dew"));
            ps4ItemsList.Add(new PSItem("87", "Guard Shield"));
            ps4ItemsList.Add(new PSItem("88", "Mahlay Shield"));
            ps4ItemsList.Add(new PSItem("89", "Shadow Blade"));
            ps4ItemsList.Add(new PSItem("8A", "Alis Sword"));
            ps4ItemsList.Add(new PSItem("8B", "Dynamite"));
            ps4ItemsList.Add(new PSItem("8C", "Nothing*"));
            ps4ItemsList.Add(new PSItem("8D", "Alshline"));
            ps4ItemsList.Add(new PSItem("8E", "Eclipse Torch"));
            ps4ItemsList.Add(new PSItem("8F", "Aero Prism"));
            ps4ItemsList.Add(new PSItem("90", "Repair Kit"));
            ps4ItemsList.Add(new PSItem("91", "Shortcake"));
            ps4ItemsList.Add(new PSItem("92", "Penguin Feed"));
            ps4ItemsList.Add(new PSItem("93", "Perolymate"));
            ps4ItemsList.Add(new PSItem("94", "Pennant"));
            ps4ItemsList.Add(new PSItem("95", "Wood Carving"));
            ps4ItemsList.Add(new PSItem("96", "Land Rover"));
            ps4ItemsList.Add(new PSItem("97", "Ice Digger"));
            ps4ItemsList.Add(new PSItem("98", "Hydrofoil"));
            ps4ItemsList.Add(new PSItem("99", "Control Key"));
            ps4ItemsList.Add(new PSItem("9A", "Canceller"));
            ps4ItemsList.Add(new PSItem("9B", "Palma Ring"));
            ps4ItemsList.Add(new PSItem("9C", "Motavia Ring"));
            ps4ItemsList.Add(new PSItem("9D", "Dezolis Ring"));
            ps4ItemsList.Add(new PSItem("9E", "Rykros Ring"));
            ps4ItemsList.Add(new PSItem("9F", "Algo Ring"));
            ps4ItemsList.Add(new PSItem("A0", "Mahlay ring"));
        }

        // Each Phantasy Star 4 character record is 0x80 bytes; fields sit at fixed
        // sub-offsets within it. The four stats and both combat values are single
        // bytes, which is why they are written through UpdatePS4ByteStat.
        private PS4CharacterItem MakePS4Char(string name, int b)
        {
            return new PS4CharacterItem(name,
                Hex(b + 0x01),    // level
                Hex(b + 0x02),    // experience  (4 bytes)
                Hex(b + 0x06),    // current HP
                Hex(b + 0x08),    // max HP
                Hex(b + 0x0A),    // current TP
                Hex(b + 0x0C),    // max TP
                Hex(b + 0x10),    // strength    (1 byte)
                Hex(b + 0x13),    // mental      (1 byte)
                Hex(b + 0x16),    // agility     (1 byte)
                Hex(b + 0x19),    // dexterity   (1 byte)
                Hex(b + 0x44),    // weapon slot 1
                Hex(b + 0x45),    // weapon slot 2
                Hex(b + 0x46),    // helmet
                Hex(b + 0x47),    // armor
                Hex(b + 0x1D),    // attack      (1 byte)
                Hex(b + 0x21));   // defense     (1 byte)
        }

        private void PopulatePS4CharacterList()
        {
            ps4CharacterCmb.Items.Clear();

            string[] names =
            {
                "Chaz", "Alys", "Hahn", "Rune", "Gryz", "Rika",
                "Demi", "Wren", "Raja", "Kyra", "Seth"
            };

            for (int i = 0; i < names.Length; i++)
            {
                ps4CharacterCmb.Items.Add(MakePS4Char(names[i], 0x11980 + i * 0x80));
            }

            ps4CharacterCmb.DisplayMember = "Name";
        }

        private void PopulatePS4CharacterDetails(PS4CharacterItem charItem)
        {
            string value = GetValueByOffset(charItem.LevelLoc, 1);
            long val = ParseHexOrZero(value);
            ps4CurrentLevelTb.Text = val.ToString();
            value = GetValueByOffset(charItem.ExpLoc, 4);
            val = ParseHexOrZero(value);
            ps4CurExpTb.Text = val.ToString();
            value = GetValueByOffset(charItem.CurrentHPLoc, 2);
            val = ParseHexOrZero(value);
            ps4CurHPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.MaxHPLoc, 2);
            val = ParseHexOrZero(value);
            ps4MaxHPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.CurrentTPLoc, 2);
            val = ParseHexOrZero(value);
            ps4CurTPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.MaxTPLoc, 2);
            val = ParseHexOrZero(value);
            ps4MaxTPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.StrengthLoc, 1);
            val = ParseHexOrZero(value);
            ps4StrTb.Text = val.ToString();
            value = GetValueByOffset(charItem.MentalLoc, 1);
            val = ParseHexOrZero(value);
            ps4MentalTb.Text = val.ToString();
            value = GetValueByOffset(charItem.AgilityLoc, 1);
            val = ParseHexOrZero(value);
            ps4AgilityTb.Text = val.ToString();
            value = GetValueByOffset(charItem.DexterityLoc, 1);
            val = ParseHexOrZero(value);
            ps4DexTb.Text = val.ToString();
            value = GetValueByOffset(charItem.WeaponSlot1Loc, 1);
            ps4WeaponSlot1Tb.Text = GetItemNameByID(value);
            value = GetValueByOffset(charItem.WeaponSlot2Loc, 1);
            ps4WeaponSlot2Tb.Text = GetItemNameByID(value);
            value = GetValueByOffset(charItem.HelmetLoc, 1);
            ps4HelmetTb.Text = GetItemNameByID(value);
            value = GetValueByOffset(charItem.ArmorLoc, 1);
            ps4ArmorTb.Text = GetItemNameByID(value);
            value = GetValueByOffset(charItem.AttackLoc, 1);
            val = ParseHexOrZero(value);
            ps4AtkPowTb.Text = val.ToString();
            value = GetValueByOffset(charItem.DefenseLoc, 1);
            val = ParseHexOrZero(value);
            ps4DefPowTb.Text = val.ToString();
        }

        private string GetItemNameByID(string id)
        {
            PSItem item = ps4ItemsList.FirstOrDefault(i => i.ItemID == id);
            string name = string.Empty;
            if (item != null)
            {
                name = item.ItemName;
            }

            return name;
        }

        /// <summary>
        /// Blanks every field on a game panel. Walking the panel is deliberate: the
        /// hand written reset methods this replaces had each drifted out of step with
        /// their panel, leaving the previous character's values on screen.
        /// </summary>
        private void ClearPanelFields(Control panel)
        {
            foreach (Control control in panel.Controls)
            {
                TextBox textBox = control as TextBox;
                if (textBox != null)
                {
                    textBox.Text = string.Empty;
                    continue;
                }

                CheckBox checkBox = control as CheckBox;
                if (checkBox != null)
                {
                    checkBox.Checked = false;
                    continue;
                }

                // Recurse so fields nested in a container are not missed
                if (control.HasChildren)
                {
                    ClearPanelFields(control);
                }
            }
        }

        private void ResetPS1Controls()
        {
            ClearPanelFields(pstar1Panel);
        }

        private void PopulatePS1ItemsList()
        {
            ps1ItemsList = new List<PSItem>();
            ps1ItemsList.Add(new PSItem("00", "Nothing"));
            ps1ItemsList.Add(new PSItem("01", "Wood Cane"));
            ps1ItemsList.Add(new PSItem("02", "Short Sword"));
            ps1ItemsList.Add(new PSItem("03", "Iron Sword"));
            ps1ItemsList.Add(new PSItem("04", "Wand"));
            ps1ItemsList.Add(new PSItem("05", "Iron Fang"));
            ps1ItemsList.Add(new PSItem("06", "Iron Axe"));
            ps1ItemsList.Add(new PSItem("07", "Titanium Sword"));
            ps1ItemsList.Add(new PSItem("08", "Ceramic Sword"));
            ps1ItemsList.Add(new PSItem("09", "Needle Gun"));
            ps1ItemsList.Add(new PSItem("0A", "Silver Fang"));
            ps1ItemsList.Add(new PSItem("0B", "Heat Gun"));
            ps1ItemsList.Add(new PSItem("0C", "Light Sabre"));
            ps1ItemsList.Add(new PSItem("0D", "Laser Gun"));
            ps1ItemsList.Add(new PSItem("0E", "Laconia Sword"));
            ps1ItemsList.Add(new PSItem("0F", "Loconia Axe"));
            ps1ItemsList.Add(new PSItem("10", "Leather Armor"));
            ps1ItemsList.Add(new PSItem("11", "White Mantle"));
            ps1ItemsList.Add(new PSItem("12", "Light Suit"));
            ps1ItemsList.Add(new PSItem("13", "Iron Armor"));
            ps1ItemsList.Add(new PSItem("14", "Thick Fur"));
            ps1ItemsList.Add(new PSItem("15", "Zirconia Armor"));
            ps1ItemsList.Add(new PSItem("16", "Diamond Armor"));
            ps1ItemsList.Add(new PSItem("17", "Laconia Armor"));
            ps1ItemsList.Add(new PSItem("18", "Frade Mantle"));
            ps1ItemsList.Add(new PSItem("19", "Leather Shield"));
            ps1ItemsList.Add(new PSItem("1A", "Bronze Shield"));
            ps1ItemsList.Add(new PSItem("1B", "Iron Shield"));
            ps1ItemsList.Add(new PSItem("1C", "Ceramic Shield"));
            ps1ItemsList.Add(new PSItem("1D", "Gloves"));
            ps1ItemsList.Add(new PSItem("1E", "Laser Shield"));
            ps1ItemsList.Add(new PSItem("1F", "Mirror Shield"));
            ps1ItemsList.Add(new PSItem("20", "Laconia Shield"));
            ps1ItemsList.Add(new PSItem("21", "Land Rover"));
            ps1ItemsList.Add(new PSItem("22", "Hovercraft"));
            ps1ItemsList.Add(new PSItem("23", "Ice Digger"));
            ps1ItemsList.Add(new PSItem("24", "Cola"));
            ps1ItemsList.Add(new PSItem("25", "Burger"));
            ps1ItemsList.Add(new PSItem("26", "Flute"));
            ps1ItemsList.Add(new PSItem("27", "Flash"));
            ps1ItemsList.Add(new PSItem("28", "Escaper"));
            ps1ItemsList.Add(new PSItem("29", "Transfer"));
            ps1ItemsList.Add(new PSItem("2A", "Magic Hat"));
            ps1ItemsList.Add(new PSItem("2B", "Alsulin"));
            ps1ItemsList.Add(new PSItem("2C", "Polymeteral"));
            ps1ItemsList.Add(new PSItem("2D", "Dungeon Key"));
            ps1ItemsList.Add(new PSItem("2E", "Sphere"));
            ps1ItemsList.Add(new PSItem("2F", "Eclipse Torch"));
            ps1ItemsList.Add(new PSItem("30", "Aero Prism"));
            ps1ItemsList.Add(new PSItem("31", "Nuts"));
            ps1ItemsList.Add(new PSItem("32", "Hapsby the Robot"));
            ps1ItemsList.Add(new PSItem("33", "Road Pass"));
            ps1ItemsList.Add(new PSItem("34", "Passport"));
            ps1ItemsList.Add(new PSItem("35", "Compass"));
            ps1ItemsList.Add(new PSItem("36", "Cake"));
            ps1ItemsList.Add(new PSItem("37", "Letter"));
            ps1ItemsList.Add(new PSItem("38", "Laconia Pot"));
            ps1ItemsList.Add(new PSItem("39", "Magic Lamp"));
            ps1ItemsList.Add(new PSItem("3A", "Amber Eye"));
            ps1ItemsList.Add(new PSItem("3B", "Gas Shield"));
            ps1ItemsList.Add(new PSItem("3C", "Crystal"));
            ps1ItemsList.Add(new PSItem("3D", "M System"));
            ps1ItemsList.Add(new PSItem("3E", "Miracle Key"));
            ps1ItemsList.Add(new PSItem("3F", "Debug"));
        }

        // Each Phantasy Star character record is 0x10 bytes; fields sit at fixed
        // sub-offsets within it. This is a Master System game, so its two byte values
        // are little-endian, unlike the three Genesis titles.
        private PS1CharacterItem MakePS1Char(string name, int b)
        {
            return new PS1CharacterItem(name,
                Hex(b + 0x02),    // experience  (2 bytes, little-endian)
                Hex(b + 0x04),    // level
                Hex(b + 0x00),    // current HP
                Hex(b + 0x05),    // max HP
                Hex(b + 0x01),    // current MP
                Hex(b + 0x06),    // max MP
                Hex(b + 0x07),    // attack
                Hex(b + 0x08),    // defense
                Hex(b + 0x09),    // equipped weapon
                Hex(b + 0x0A),    // equipped armor
                Hex(b + 0x0B));   // equipped shield
        }

        private void PopulatePS1CharacterList()
        {
            ps1CharacterCmb.Items.Clear();

            string[] names = { "Alis Landale", "Myau", "Odin", "Noah" };

            for (int i = 0; i < names.Length; i++)
            {
                ps1CharacterCmb.Items.Add(MakePS1Char(names[i], 0x44BD + i * 0x10));
            }

            ps1CharacterCmb.DisplayMember = "Name";
        }

        private string GetPS1CurrentMeseta()
        {
            string hexVal = GetValueByOffset(PS1_MESETA_LOC, 2);
            hexVal = ReverseHexPairs(hexVal);
            long meseta = ParseHexOrZero(hexVal);
            return meseta.ToString();
        }

        private void PopulatePS1CurrentMeseta()
        {
            ps1CurrentMesetaTb.Text = GetPS1CurrentMeseta();
        }

        private void PopulatePS1CharacterDetails(PS1CharacterItem charItem)
        {
            string value = GetValueByOffset(charItem.LevelLoc, 1);
            long val = ParseHexOrZero(value);
            ps1LevelTb.Text = val.ToString();

            value = GetValueByOffset(charItem.ExperienceLoc, 2);
            value = ReverseHexPairs(value);
            val = ParseHexOrZero(value);
            ps1ExpTb.Text = val.ToString();

            value = GetValueByOffset(charItem.CurrentHPLoc, 1);
            val = ParseHexOrZero(value);
            ps1CurrentHPTb.Text = val.ToString();

            value = GetValueByOffset(charItem.MaxHPLoc, 1);
            val = ParseHexOrZero(value);
            ps1MaxHPTb.Text = val.ToString();

            value = GetValueByOffset(charItem.CurrentMPLoc, 1);
            val = ParseHexOrZero(value);
            ps1CurrentMPTb.Text = val.ToString();

            value = GetValueByOffset(charItem.MaxMPLoc, 1);
            val = ParseHexOrZero(value);
            ps1MaxMPTb.Text = val.ToString();
            if (charItem.Name == "Odin")
            {
                ps1NewMaxMPTb.Enabled = false;
            }
            else
            {
                ps1NewMaxMPTb.Enabled = true;
            }

            value = GetValueByOffset(charItem.AttackLoc, 1);
            val = ParseHexOrZero(value);
            ps1AttackTb.Text = val.ToString();

            value = GetValueByOffset(charItem.DefenseLoc, 1);
            val = ParseHexOrZero(value);
            ps1DefenseTb.Text = val.ToString();

            value = GetValueByOffset(charItem.EquippedWeaponLoc, 1);
            ps1EquipedWeaponTb.Text = GetPS1ItemNameByID(value);

            value = GetValueByOffset(charItem.EquippedArmorLoc, 1);
            ps1EquipedArmorTb.Text = GetPS1ItemNameByID(value);

            value = GetValueByOffset(charItem.EquippedShieldLoc, 1);
            ps1EquipedShieldTb.Text = GetPS1ItemNameByID(value);
        }

        private string GetPS1ItemNameByID(string id)
        {
            PSItem item = ps1ItemsList.FirstOrDefault(i => i.ItemID == id);
            string name = string.Empty;
            if (item != null)
            {
                name = item.ItemName;
            }
            else
            {
                name = "Debug";
            }

            return name;
        }

        private void ResetPS2Controls()
        {
            ClearPanelFields(pstar2Panel);
        }

        // Each Phantasy Star 2 character record is 0x40 bytes; fields sit at fixed
        // sub-offsets within it.
        private PS2CharacterItem MakePS2Char(string name, int b)
        {
            return new PS2CharacterItem(name,
                Hex(b + 0x00),    // current HP
                Hex(b + 0x02),    // max HP
                Hex(b + 0x04),    // current TP
                Hex(b + 0x06),    // max TP
                Hex(b + 0x09),    // level       (1 byte)
                Hex(b + 0x0A),    // experience  (4 bytes)
                Hex(b + 0x0E),    // strength
                Hex(b + 0x10),    // mental
                Hex(b + 0x12),    // agility
                Hex(b + 0x14),    // luck
                Hex(b + 0x16),    // dexterity
                Hex(b + 0x1A),    // attack
                Hex(b + 0x1C));   // defense
        }

        private void PopulatePS2CharacterList()
        {
            ps2CharacterCmb.Items.Clear();

            string[] names =
            {
                "Rolf Landale", "Nei", "Rudo Steiner", "Amy Sage",
                "Hugh Tompson", "Anna Zirski", "Josh Kain", "Shir Gold"
            };

            for (int i = 0; i < names.Length; i++)
            {
                ps2CharacterCmb.Items.Add(MakePS2Char(names[i], 0xE47A + i * 0x40));
            }

            ps2CharacterCmb.DisplayMember = "Name";
        }

        private string GetPS2CurrentMeseta()
        {
            string hexVal = GetValueByOffset(PS2_MESETA_LOC, 4);
            long meseta = ParseHexOrZero(hexVal);
            return meseta.ToString();
        }

        private void PopulatePS2CurrentMeseta()
        {
            ps2CurMesetaTb.Text = GetPS2CurrentMeseta();
        }

        private void PopulatePS2CharacterDetails(PS2CharacterItem charItem)
        {
            string value = GetValueByOffset(charItem.CurrentHPLoc, 2);
            long val = ParseHexOrZero(value);
            ps2CurHPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.MaxHPLoc, 2);
            val = ParseHexOrZero(value);
            ps2MaxHPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.CurrentTPLoc, 2);
            val = ParseHexOrZero(value);
            ps2CurTPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.MaxTPLoc, 2);
            val = ParseHexOrZero(value);
            ps2MaxTPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.LevelLoc, 1);
            val = ParseHexOrZero(value);
            ps2LevelTb.Text = val.ToString();
            value = GetValueByOffset(charItem.ExperienceLoc, 4);
            val = ParseHexOrZero(value);
            ps2ExpTb.Text = val.ToString();
            value = GetValueByOffset(charItem.StrengthLoc, 2);
            val = ParseHexOrZero(value);
            ps2StrTb.Text = val.ToString();
            value = GetValueByOffset(charItem.MentalLoc, 2);
            val = ParseHexOrZero(value);
            ps2MentalTb.Text = val.ToString();
            value = GetValueByOffset(charItem.AgilityLoc, 2);
            val = ParseHexOrZero(value);
            ps2AgilityTb.Text = val.ToString();
            value = GetValueByOffset(charItem.LuckLoc, 2);
            val = ParseHexOrZero(value);
            ps2LuckTb.Text = val.ToString();
            value = GetValueByOffset(charItem.DexterityLoc, 2);
            val = ParseHexOrZero(value);
            ps2DexTb.Text = val.ToString();
            value = GetValueByOffset(charItem.AttackLoc, 2);
            val = ParseHexOrZero(value);
            ps2AttackTb.Text = val.ToString();
            value = GetValueByOffset(charItem.DefenseLoc, 2);
            val = ParseHexOrZero(value);
            ps2DefTb.Text = val.ToString();
        }

        private string GetPS3CurrentMeseta()
        {
            string hexVal = GetValueByOffset(PS3_MESETA_LOC, 4);
            long meseta = ParseHexOrZero(hexVal);
            return meseta.ToString();
        }

        private void PopulatePS3CurrentMeseta()
        {
            ps3CurrentMesetaTb.Text = GetPS3CurrentMeseta();
        }

        private byte[] HexStringToBytes(string hexString)
        {
            if (hexString == null)
            {
                throw new ArgumentNullException("hexString");
            }

            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("hexString must have an even length", "hexString");
            }

            var bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                string currentHex = hexString.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(currentHex, 16);
            }
            return bytes;
        }

        private void ResetPS3Controls()
        {
            ClearPanelFields(pstar3Panel);
        }

        private void UpdatePS1SaveState()
        {
            int errorCount = 0;
            PS1CharacterItem charItem = null;

            if (ps1NewMesetaTb.Text != string.Empty)
            {
                ushort meseta = 0;
                if (ushort.TryParse(ps1NewMesetaTb.Text, out meseta))
                {
                    if (meseta <= PS1_SHORT_MAX)
                    {
                        if (!SetValueByOffset(meseta, PS1_MESETA_LOC, false))
                        {
                            errorCount++;
                        }
                    }
                    else
                    {
                        errorCount++;
                        LogError("You must enter a value less than or equal to " + PS1_SHORT_MAX.ToString() + " for the new meseta value.");
                    }
                }
                else
                {
                    errorCount++;
                    LogError("You must enter a numeric value for the new mesta value.");
                }
            }

            if (ps1CharacterCmb.SelectedIndex >= 0)
            {
                charItem = ps1CharacterCmb.SelectedItem as PS1CharacterItem;
                if (ps1NewExpTb.Text != string.Empty)
                {
                    ushort exp = 0;
                    if (ushort.TryParse(ps1NewExpTb.Text, out exp))
                    {
                        if (exp <= PS1_SHORT_MAX)
                        {
                            if (!SetValueByOffset(exp, charItem.ExperienceLoc, false))
                            {
                                errorCount++;
                            }
                        }
                        else
                        {
                            errorCount++;
                            LogError("You must enter a value less than or equal to " + PS1_SHORT_MAX.ToString() + " for the new experience points value.");
                        }
                    }
                    else
                    {
                        errorCount++;
                        LogError("You must enter a numeric value for the new experience points value.");
                    }
                }

                if (ps1NewCurrentHPTb.Text != string.Empty)
                {
                    short curHP = 0;
                    if (short.TryParse(ps1NewCurrentHPTb.Text, out curHP))
                    {
                        if (curHP <= PS1_BYTE_MAX)
                        {
                            if (!SetValueByOffset(Convert.ToByte(curHP), charItem.CurrentHPLoc))
                            {
                                errorCount++;
                            }
                        }
                        else
                        {
                            errorCount++;
                            LogError("You must enter a value less than or equal to " + PS1_BYTE_MAX.ToString() + " for the new current HP value.");
                        }
                    }
                    else
                    {
                        errorCount++;
                        LogError("You must enter a numeric value for the new current HP value.");
                    }
                }

                if (ps1NewMaxHPTb.Text != string.Empty)
                {
                    short maxHP = 0;
                    if (short.TryParse(ps1NewMaxHPTb.Text, out maxHP))
                    {
                        if (maxHP <= PS1_BYTE_MAX)
                        {
                            if (!SetValueByOffset(Convert.ToByte(maxHP), charItem.MaxHPLoc))
                            {
                                errorCount++;
                            }
                        }
                        else
                        {
                            errorCount++;
                            LogError("You must enter a value less than or equal to " + PS1_BYTE_MAX.ToString() + " for the new max HP value.");
                        }
                    }
                    else
                    {
                        errorCount++;
                        LogError("You must enter a numeric value for the new max HP value.");
                    }
                }

                if (ps1NewCurrentMPTb.Text != string.Empty)
                {
                    short curMP = 0;
                    if (short.TryParse(ps1NewCurrentMPTb.Text, out curMP))
                    {
                        if (curMP <= PS1_BYTE_MAX)
                        {
                            if (!SetValueByOffset(Convert.ToByte(curMP), charItem.CurrentMPLoc))
                            {
                                errorCount++;
                            }
                        }
                        else
                        {
                            errorCount++;
                            LogError("You must enter a value less than or equal to " + PS1_BYTE_MAX.ToString() + " for the new current MP value.");
                        }
                    }
                    else
                    {
                        errorCount++;
                        LogError("You must enter a numeric value for the new current MP value.");
                    }
                }

                if (ps1NewMaxMPTb.Text != string.Empty)
                {
                    short maxMP = 0;
                    if (short.TryParse(ps1NewMaxMPTb.Text, out maxMP))
                    {
                        if (maxMP <= PS1_BYTE_MAX)
                        {
                            if (!SetValueByOffset(Convert.ToByte(maxMP), charItem.MaxMPLoc))
                            {
                                errorCount++;
                            }
                        }
                        else
                        {
                            errorCount++;
                            LogError("You must enter a value less than or equal to " + PS1_BYTE_MAX.ToString() + " for the new max MP value.");
                        }
                    }
                    else
                    {
                        errorCount++;
                        LogError("You must enter a numeric value for the new max MP value.");
                    }
                }

                if (ps1NewAttackTb.Text != string.Empty)
                {
                    short attack = 0;
                    if (short.TryParse(ps1NewAttackTb.Text, out attack))
                    {
                        if (attack <= PS1_BYTE_MAX)
                        {
                            if (!SetValueByOffset(Convert.ToByte(attack), charItem.AttackLoc))
                            {
                                errorCount++;
                            }
                        }
                        else
                        {
                            errorCount++;
                            LogError("You must enter a value less than or equal to " + PS1_BYTE_MAX.ToString() + " for the new attack value.");
                        }
                    }
                    else
                    {
                        errorCount++;
                        LogError("You must enter a numeric value for the new attack value.");
                    }
                }

                if (ps1NewDefenseTb.Text != string.Empty)
                {
                    short defense = 0;
                    if (short.TryParse(ps1NewDefenseTb.Text, out defense))
                    {
                        if (defense <= PS1_BYTE_MAX)
                        {
                            if (!SetValueByOffset(Convert.ToByte(defense), charItem.DefenseLoc))
                            {
                                errorCount++;
                            }
                        }
                        else
                        {
                            errorCount++;
                            LogError("You must enter a value less than or equal to " + PS1_BYTE_MAX.ToString() + " for the new defense value.");
                        }
                    }
                    else
                    {
                        errorCount++;
                        LogError("You must enter a numeric value for the new defense value.");
                    }
                }
            }

            if (!SaveBufferToDisk())
            {
                return;   // write failed; SaveBufferToDisk already reported it
            }

            string completionMessage = string.Empty;
            if (errorCount > 0)
            {
                completionMessage = "The save state update process has completed with errors.";
            }
            else
            {
                completionMessage = "The save state update process has completed.";
            }
            MessageBox.Show(completionMessage, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

            ResetPS1Controls();
            PopulatePS1CurrentMeseta();
            if (charItem != null)
            {
                PopulatePS1CharacterDetails(charItem);
            }
        }

        private void UpdatePS2SaveState()
        {
            PS2CharacterItem charItem = ps2CharacterCmb.SelectedItem as PS2CharacterItem;

            if (ps2NewMesetaTb.Text != string.Empty)
            {
                int meseta = 0;
                if (int.TryParse(ps2NewMesetaTb.Text, out meseta))
                {
                    SetValueByOffset(meseta, PS2_MESETA_LOC);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the new meseta value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            // Meseta is not tied to a character, so it is written above either way. Every
            // remaining field is, so there is nothing further to do without a selection.
            if (charItem == null)
            {
                MessageBox.Show("You must select a character before the character values can be updated.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ResetPS2Controls();
                PopulatePS2CurrentMeseta();
                return;
            }

            if (ps2NewCurHPTb.Text != string.Empty)
            {
                short hp = 0;
                if (short.TryParse(ps2NewCurHPTb.Text, out hp))
                {
                    SetValueByOffset(hp, charItem.CurrentHPLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the current HP value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps2NewMaxHPTb.Text != string.Empty)
            {
                short hp = 0;
                if (short.TryParse(ps2NewMaxHPTb.Text, out hp))
                {
                    SetValueByOffset(hp, charItem.MaxHPLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the max HP value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps2NewCurTPTb.Text != string.Empty)
            {
                short tp = 0;
                if (short.TryParse(ps2NewCurTPTb.Text, out tp))
                {
                    SetValueByOffset(tp, charItem.CurrentTPLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the current TP value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps2NewMaxTPTb.Text != string.Empty)
            {
                short tp = 0;
                if (short.TryParse(ps2NewMaxTPTb.Text, out tp))
                {
                    SetValueByOffset(tp, charItem.MaxTPLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the max TP value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps2NewExpTb.Text != string.Empty)
            {
                int exp = 0;
                if (int.TryParse(ps2NewExpTb.Text, out exp))
                {
                    SetValueByOffset(exp, charItem.ExperienceLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the experience value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps2NewStrTb.Text != string.Empty)
            {
                short str = 0;
                if (short.TryParse(ps2NewStrTb.Text, out str))
                {
                    SetValueByOffset(str, charItem.StrengthLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the strength value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps2NewMentalTb.Text != string.Empty)
            {
                short mental = 0;
                if (short.TryParse(ps2NewMentalTb.Text, out mental))
                {
                    SetValueByOffset(mental, charItem.MentalLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the mental value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps2NewAgilityTb.Text != string.Empty)
            {
                short agility = 0;
                if (short.TryParse(ps2NewAgilityTb.Text, out agility))
                {
                    SetValueByOffset(agility, charItem.AgilityLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the agility value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps2NewLuckTb.Text != string.Empty)
            {
                short luck = 0;
                if (short.TryParse(ps2NewLuckTb.Text, out luck))
                {
                    SetValueByOffset(luck, charItem.LuckLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the luck value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps2NewDexTb.Text != string.Empty)
            {
                short dex = 0;
                if (short.TryParse(ps2NewDexTb.Text, out dex))
                {
                    SetValueByOffset(dex, charItem.DexterityLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the dexterity value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps2NewAttackTb.Text != string.Empty)
            {
                short attack = 0;
                if (short.TryParse(ps2NewAttackTb.Text, out attack))
                {
                    SetValueByOffset(attack, charItem.AttackLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the attack value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps2NewDefTb.Text != string.Empty)
            {
                short def = 0;
                if (short.TryParse(ps2NewDefTb.Text, out def))
                {
                    SetValueByOffset(def, charItem.DefenseLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the defense value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (!SaveBufferToDisk())
            {
                return;   // write failed; SaveBufferToDisk already reported it
            }

            MessageBox.Show("The save state update process has completed.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ResetPS2Controls();
            PopulatePS2CurrentMeseta();
            PopulatePS2CharacterDetails(charItem);
        }

        private void UpdatePS3SaveState()
        {
            PS3CharacterItem charItem = ps3CharacterCmb.SelectedItem as PS3CharacterItem;
            if (ps3NewMesetaTb.Text != string.Empty)
            {
                int meseta = 0;
                if (int.TryParse(ps3NewMesetaTb.Text, out meseta))
                {
                    SetValueByOffset(meseta, PS3_MESETA_LOC);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the new meseta value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            // Meseta is not tied to a character, so it is written above either way. Every
            // remaining field is, so there is nothing further to do without a selection.
            if (charItem == null)
            {
                MessageBox.Show("You must select a character before the character values can be updated.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ResetPS3Controls();
                PopulatePS3CurrentMeseta();
                return;
            }

            if (ps3NewSpeedTb.Text != string.Empty)
            {
                short speed = 0;
                if (short.TryParse(ps3NewSpeedTb.Text, out speed))
                {
                    SetValueByOffset((byte)speed, charItem.SpeedLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the speed value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps3NewMaxHPTb.Text != string.Empty)
            {
                short hp = 0;
                if (short.TryParse(ps3NewMaxHPTb.Text, out hp))
                {
                    SetValueByOffset(hp, charItem.MaxHPLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the max HP value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps3NewMaxTPTb.Text != string.Empty)
            {
                short tp = 0;
                if (short.TryParse(ps3NewMaxTPTb.Text, out tp))
                {
                    SetValueByOffset(tp, charItem.MaxTPLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the max TP value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps3NewCurHPTb.Text != string.Empty)
            {
                short hp = 0;
                if (short.TryParse(ps3NewCurHPTb.Text, out hp))
                {
                    SetValueByOffset(hp, charItem.CurHPLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the current HP value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps3NewCurTPTb.Text != string.Empty)
            {
                short tp = 0;
                if (short.TryParse(ps3NewCurTPTb.Text, out tp))
                {
                    SetValueByOffset(tp, charItem.CurTPLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the current TP value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps3NewDmgTb.Text != string.Empty)
            {
                short dmg = 0;
                if (short.TryParse(ps3NewDmgTb.Text, out dmg))
                {
                    SetValueByOffset(dmg, charItem.DmgLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the damage value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps3NewDefTb.Text != string.Empty)
            {
                short def = 0;
                if (short.TryParse(ps3NewDefTb.Text, out def))
                {
                    SetValueByOffset(def, charItem.DefLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the defense value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps3NewExpTb.Text != string.Empty)
            {
                int exp = 0;
                if (int.TryParse(ps3NewExpTb.Text, out exp))
                {
                    SetValueByOffset(exp, charItem.ExpLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the experience value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps3NewLuckTb.Text != string.Empty)
            {
                short luck = 0;
                if (short.TryParse(ps3NewLuckTb.Text, out luck))
                {
                    SetValueByOffset((byte)luck, charItem.LuckLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the luck value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps3NewSkillTb.Text != string.Empty)
            {
                short skill = 0;
                if (short.TryParse(ps3NewSkillTb.Text, out skill))
                {
                    SetValueByOffset((byte)skill, charItem.SkillLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the skill value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            short poisoned = 0;
            if (ps3CurPoisonChk.Checked)
            {
                poisoned = 64;
            }

            SetValueByOffset((byte)poisoned, charItem.PoisonLoc);

            if (!SaveBufferToDisk())
            {
                return;   // write failed; SaveBufferToDisk already reported it
            }

            MessageBox.Show("The save state update process has completed.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ResetPS3Controls();
            PopulatePS3CurrentMeseta();
            PopulatePS3CharacterDetails(charItem);
        }        

        /// <summary>
        /// Writes a single byte stat to the save state from the given text box. Does
        /// nothing when the text box is empty. The stat is read back as one byte by
        /// PopulatePS4CharacterDetails, so it must be written as one byte as well.
        /// </summary>
        private void UpdatePS4ByteStat(TextBox newValueTb, string offset, string fieldName)
        {
            if (newValueTb.Text == string.Empty)
            {
                return;
            }

            short value = 0;
            if (!short.TryParse(newValueTb.Text, out value))
            {
                MessageBox.Show("You must enter a numeric value for the new " + fieldName + " value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (value < 0 || value > PS4_BYTE_MAX)
            {
                MessageBox.Show("You must enter a value between 0 and " + PS4_BYTE_MAX.ToString() + " for the new " + fieldName + " value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetValueByOffset((byte)value, offset);
        }

        private void UpdatePS4SaveState()
        {
            PS4CharacterItem charItem = ps4CharacterCmb.SelectedItem as PS4CharacterItem;
            if (ps4NewMesetaTb.Text != string.Empty)
            {
                int meseta = 0;
                if (int.TryParse(ps4NewMesetaTb.Text, out meseta))
                {
                    SetValueByOffset(meseta, PS4_MESETA_LOC);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the new meseta value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            // Meseta is not tied to a character, so it is written above either way. Every
            // remaining field is, so there is nothing further to do without a selection.
            if (charItem == null)
            {
                MessageBox.Show("You must select a character before the character values can be updated.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ResetPS4Controls();
                PopulatePS4CurrentMeseta();
                return;
            }

            if (ps4NewExpTb.Text != string.Empty)
            {
                int exp = 0;
                if (int.TryParse(ps4NewExpTb.Text, out exp))
                {
                    SetValueByOffset(exp, charItem.ExpLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the new experience value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps4NewCurHPTb.Text != string.Empty)
            {
                short hp = 0;
                if (short.TryParse(ps4NewCurHPTb.Text, out hp))
                {
                    SetValueByOffset(hp, charItem.CurrentHPLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the new current HP value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps4NewMaxHPTb.Text != string.Empty)
            {
                short hp = 0;
                if (short.TryParse(ps4NewMaxHPTb.Text, out hp))
                {
                    SetValueByOffset(hp, charItem.MaxHPLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the new max HP value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps4NewCurTPTb.Text != string.Empty)
            {
                short tp = 0;
                if (short.TryParse(ps4NewCurTPTb.Text, out tp))
                {
                    SetValueByOffset(tp, charItem.CurrentTPLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the new current TP value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps4NewMaxTPTb.Text != string.Empty)
            {
                short tp = 0;
                if (short.TryParse(ps4NewMaxTPTb.Text, out tp))
                {
                    SetValueByOffset(tp, charItem.MaxTPLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the new max TP value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            // These six stats are single byte fields, so they must be written one byte
            // at a time. Writing them as a short would zero the stat and clobber the
            // neighbouring byte.
            UpdatePS4ByteStat(ps4NewStrTb, charItem.StrengthLoc, "strength");
            UpdatePS4ByteStat(ps4NewMentalTb, charItem.MentalLoc, "mental");
            UpdatePS4ByteStat(ps4NewAgilityTb, charItem.AgilityLoc, "agility");
            UpdatePS4ByteStat(ps4NewDexTb, charItem.DexterityLoc, "dexterity");
            UpdatePS4ByteStat(ps4NewAtkPowTb, charItem.AttackLoc, "attack power");
            UpdatePS4ByteStat(ps4NewDefPowTb, charItem.DefenseLoc, "defense power");

            if (!SaveBufferToDisk())
            {
                return;   // write failed; SaveBufferToDisk already reported it
            }

            MessageBox.Show("The save state update process has completed.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ResetPS4Controls();
            PopulatePS4CurrentMeseta();
            PopulatePS4CharacterDetails(charItem);
        }
        #endregion        
    }
}
