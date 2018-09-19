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
        public enum AppPanel
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
        private bool fileLoaded;
        private const int PS3_TOTAL_UPDATE_FIELDS = 12;
        private const ushort PS1_SHORT_MAX = 65535;
        private const short PS1_BYTE_MAX = 255;
        private const string PS1_MESETA_LOC = "459C";
        private const string PS2_MESETA_LOC = "EA98";
        private const string PS3_MESETA_LOC = "E4B8";
        private const string PS4_MESETA_LOC = "118B0";
        private AppPanel activePanel;
        private List<PSItem> ps4ItemsList;
        #endregion

        #region - Class Properties -
        public bool FileLoaded
        {
            get { return fileLoaded; }
            set { fileLoaded = value; }
        }
        public AppPanel ActivePanel
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
            FileLoaded = false;
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
                    FileLoaded = true;
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
            UpdatePS1SaveState();
        }        

        private void ps2UpdSavStateBtn_Click(object sender, EventArgs e)
        {
            UpdatePS2SaveState();
        }

        private void ps3UpdSavStateBtn_Click(object sender, EventArgs e)
        {
            UpdatePS3SaveState();
        }

        private void ps4UpdSavStateBtn_Click(object sender, EventArgs e)
        {
            UpdatePS4SaveState();
        }
        #endregion

        #region - Class Methods -
        private void LogError(string errMsg)
        {
            // Create a write and open the file
            string filePath = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            filePath += @"\errorlog.txt";
            TextWriter writer = new StreamWriter(filePath, true);

            // Write the error message to the error log
            writer.WriteLine(errMsg + " Added: " + DateTime.Now.ToString());

            // Close the stream
            writer.Close();
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

        private void PopulatePS3CharacterList()
        {
            ps3CharacterCmb.Items.Clear();

            // Rhys
            PS3CharacterItem rhysItem = new PS3CharacterItem("Rhys",
                "E51C",
                "E51F",
                "E524",
                "E526",
                "E528",
                "E52A",
                "E52C",
                "E52E",
                "E530",
                "E532",
                "E54A",
                "E54B",
                "E4F9",
                "102F8");
            ps3CharacterCmb.Items.Add(rhysItem);

            // Ayn
            PS3CharacterItem aynItem = new PS3CharacterItem(rhysItem);
            aynItem.Name = "Ayn";
            ps3CharacterCmb.Items.Add(aynItem);

            // Nial
            PS3CharacterItem nialItem = new PS3CharacterItem(aynItem);
            nialItem.Name = "Nial";
            ps3CharacterCmb.Items.Add(nialItem);

            // Mieu
            PS3CharacterItem chrItem = new PS3CharacterItem("Mieu",
                "E59C",
                "E59F",
                "E5A4",
                "E5A6",
                "E5A8",
                "E5AA",
                "E5AC",
                "E5AE",
                "E5B0",
                "E5B2",
                "E5CA",
                "E5CB",
                "E579",
                "1031B");
            ps3CharacterCmb.Items.Add(chrItem);

            // Wren
            chrItem = new PS3CharacterItem("Wren",
                "E61C",
                "E61F",
                "E624",
                "E626",
                "E628",
                "E62A",
                "E62C",
                "E62E",
                "E630",
                "E632",
                "E64A",
                "E64B",
                "E5F9",
                "10338");
            ps3CharacterCmb.Items.Add(chrItem);

            // Lyle
            PS3CharacterItem lyleItem = new PS3CharacterItem("Lyle",
                "E69C",
                "E69F",
                "E6A4",
                "E6A6",
                "E6A8",
                "E6AA",
                "E6AC",
                "E6AE",
                "E6B0",
                "E6B2",
                "E6CA",
                "E6CB",
                "E679",
                "10358");
            ps3CharacterCmb.Items.Add(lyleItem);

            // Thea
            PS3CharacterItem theaItem = new PS3CharacterItem(lyleItem);
            theaItem.Name = "Thea";
            ps3CharacterCmb.Items.Add(theaItem);

            // Ryan
            PS3CharacterItem ryanItem = new PS3CharacterItem(theaItem);
            ryanItem.Name = "Ryan";
            ps3CharacterCmb.Items.Add(ryanItem);

            // Lena
            PS3CharacterItem lenaItem = new PS3CharacterItem("Lena",
                "E71C",
                "E71F",
                "E724",
                "E726",
                "E728",
                "E72A",
                "E72C",
                "E72E",
                "E730",
                "E732",
                "E74A",
                "E74B",
                "E6F9",
                "10378");
            ps3CharacterCmb.Items.Add(lenaItem);

            // Sari
            PS3CharacterItem sariItem = new PS3CharacterItem(lenaItem);
            sariItem.Name = "Sari";
            ps3CharacterCmb.Items.Add(sariItem);

            // Laya
            PS3CharacterItem layaItem = new PS3CharacterItem(sariItem);
            layaItem.Name = "Laya";
            ps3CharacterCmb.Items.Add(layaItem);

            ps3CharacterCmb.DisplayMember = "Name";
        }

        private void PopulatePS3CharacterDetails(PS3CharacterItem charItem)
        {
            string value = string.Empty;
            value = GetValueByOffset(charItem.NameLoc, 4);
            byte[] bytes = HexStringToBytes(value);
            ps3CurNameTb.Text = Encoding.ASCII.GetString(bytes);
            value = GetValueByOffset(charItem.SpeedLoc, 1);
            long val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps3CurSpeedTb.Text = val.ToString();
            value = GetValueByOffset(charItem.LevelLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps3CurLevelTb.Text = val.ToString();
            value = GetValueByOffset(charItem.MaxHPLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps3CurMaxHPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.MaxTPLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps3CurMaxTPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.CurHPLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps3CurCurHPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.CurTPLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps3CurCurTPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.DmgLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps3CurDmgTb.Text = val.ToString();
            value = GetValueByOffset(charItem.DefLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps3CurDefTb.Text = val.ToString();
            value = GetValueByOffset(charItem.ExpLoc, 4);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps3CurExpTb.Text = val.ToString();
            value = GetValueByOffset(charItem.LuckLoc, 1);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps3CurLuckTb.Text = val.ToString();
            value = GetValueByOffset(charItem.SkillLoc, 1);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
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
            //val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            //curItemCountTb.Text = val.ToString();
        }

        private string GetValueByOffset(string offset, int bytesToRead)
        {
            string value = string.Empty;
            BinaryReader reader = null;

            try
            {
                reader = new BinaryReader(new FileStream(saveStateFileTb.Text, FileMode.Open));
                // Set the position of the reader by the offset
                reader.BaseStream.Position = long.Parse(offset, System.Globalization.NumberStyles.HexNumber);
                // Read the offset
                value = BitConverter.ToString(reader.ReadBytes(bytesToRead)).Replace("-", null);
            }
            catch (IOException ioe)
            {
                LogError(ioe.Message + " Occurred when attempting to read a value by its offset.");
            }
            catch (ArgumentException aue)
            {
                LogError(aue.Message + " Occurred when attempting to read a value by its offset.");
            }
            catch (Exception e)
            {
                LogError(e.Message + " Occurred when attempting to read a value by its offset.");
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }

            return value;
        }

        private string ReverseHexPairs(string hexString)
        {
            string newHex = string.Empty;

            if (hexString.Length % 2 != 0)
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

        private bool SetValueByOffset(string value, string offset)
        {
            bool success = false;
            BinaryWriter writer = null;

            try
            {
                writer = new BinaryWriter(new FileStream(saveStateFileTb.Text, FileMode.Open));
                writer.BaseStream.Position = long.Parse(offset, System.Globalization.NumberStyles.HexNumber);
                int valNum = Convert.ToInt32(value);
                byte[] bytes = BitConverter.GetBytes(valNum).Reverse().ToArray();
                writer.Write(bytes);
                success = true;
            }
            catch (IOException ioe)
            {
                LogError(ioe.Message + " Occurred while attempting to write a value to the save state file.");
                success = false;
            }
            catch (ArgumentException aue)
            {
                LogError(aue.Message + " Occurred while attempting to write a value to the save state file.");
                success = false;
            }
            catch (Exception e)
            {
                LogError(e.Message + " Occurred while attempting to write a value to the save state file.");
                success = false;
            }
            finally
            {
                writer.Close();
                writer.Dispose();
            }

            return success;
        }

        private bool SetValueByOffset(long value, string offset)
        {
            bool success = false;
            BinaryWriter writer = null;

            try
            {
                writer = new BinaryWriter(new FileStream(saveStateFileTb.Text, FileMode.Open));
                writer.BaseStream.Position = long.Parse(offset, System.Globalization.NumberStyles.HexNumber);
                byte[] bytes = BitConverter.GetBytes(value).Reverse().ToArray();
                writer.Write(bytes);

                success = true;
            }
            catch (IOException ioe)
            {
                LogError(ioe.Message + " Occurred while attempting to write a value to the save state file.");
                success = false;
            }
            catch (ArgumentException aue)
            {
                LogError(aue.Message + " Occurred while attempting to write a value to the save state file.");
                success = false;
            }
            catch (Exception e)
            {
                LogError(e.Message + " Occurred while attempting to write a value to the save state file.");
                success = false;
            }
            finally
            {
                writer.Close();
                writer.Dispose();
            }

            return success;
        }

        private bool SetValueByOffset(int value, string offset)
        {
            bool success = false;
            BinaryWriter writer = null;

            try
            {
                writer = new BinaryWriter(new FileStream(saveStateFileTb.Text, FileMode.Open));
                writer.BaseStream.Position = long.Parse(offset, System.Globalization.NumberStyles.HexNumber);
                byte[] bytes = BitConverter.GetBytes(value).Reverse().ToArray();
                writer.Write(bytes);

                success = true;
            }
            catch (IOException ioe)
            {
                LogError(ioe.Message + " Occurred while attempting to write a value to the save state file.");
                success = false;
            }
            catch (ArgumentException aue)
            {
                LogError(aue.Message + " Occurred while attempting to write a value to the save state file.");
                success = false;
            }
            catch (Exception e)
            {
                LogError(e.Message + " Occurred while attempting to write a value to the save state file.");
                success = false;
            }
            finally
            {
                writer.Close();
                writer.Dispose();
            }

            return success;
        }

        private bool SetValueByOffset(short value, string offset)
        {
            bool success = false;
            BinaryWriter writer = null;

            try
            {
                writer = new BinaryWriter(new FileStream(saveStateFileTb.Text, FileMode.Open));
                writer.BaseStream.Position = long.Parse(offset, System.Globalization.NumberStyles.HexNumber);
                byte[] bytes = BitConverter.GetBytes(value).Reverse().ToArray();
                writer.Write(bytes);

                success = true;
            }
            catch (IOException ioe)
            {
                LogError(ioe.Message + " Occurred while attempting to write a value to the save state file.");
                success = false;
            }
            catch (ArgumentException aue)
            {
                LogError(aue.Message + " Occurred while attempting to write a value to the save state file.");
                success = false;
            }
            catch (Exception e)
            {
                LogError(e.Message + " Occurred while attempting to write a value to the save state file.");
                success = false;
            }
            finally
            {
                writer.Close();
                writer.Dispose();
            }

            return success;
        }

        private bool SetValueByOffset(ushort value, string offset, bool reverse)
        {
            bool success = false;
            BinaryWriter writer = null;

            try
            {
                writer = new BinaryWriter(new FileStream(saveStateFileTb.Text, FileMode.Open));
                writer.BaseStream.Position = long.Parse(offset, System.Globalization.NumberStyles.HexNumber);
                byte[] bytes = new byte[]{ };
                if (reverse)
                {
                    bytes = BitConverter.GetBytes(value).Reverse().ToArray();
                }
                else
                {
                    bytes = BitConverter.GetBytes(value).ToArray();
                }
                writer.Write(bytes);

                success = true;
            }
            catch (IOException ioe)
            {
                LogError(ioe.Message + " Occurred while attempting to write a value to the save state file.");
                success = false;
            }
            catch (ArgumentException aue)
            {
                LogError(aue.Message + " Occurred while attempting to write a value to the save state file.");
                success = false;
            }
            catch (Exception e)
            {
                LogError(e.Message + " Occurred while attempting to write a value to the save state file.");
                success = false;
            }
            finally
            {
                writer.Close();
                writer.Dispose();
            }

            return success;
        }

        private bool SetValueByOffset(byte value, string offset)
        {
            bool success = false;
            BinaryWriter writer = null;

            try
            {
                writer = new BinaryWriter(new FileStream(saveStateFileTb.Text, FileMode.Open));
                writer.BaseStream.Position = long.Parse(offset, System.Globalization.NumberStyles.HexNumber);
                writer.Write(value);

                success = true;
            }
            catch (IOException ioe)
            {
                LogError(ioe.Message + " Occurred while attempting to write a value to the save state file.");
                success = false;
            }
            catch (ArgumentException aue)
            {
                LogError(aue.Message + " Occurred while attempting to write a value to the save state file.");
                success = false;
            }
            catch (Exception e)
            {
                LogError(e.Message + " Occurred while attempting to write a value to the save state file.");
                success = false;
            }
            finally
            {
                writer.Close();
                writer.Dispose();
            }

            return success;
        }

        private string GetCurrentPS4Meseta()
        {
            string hexVal = GetValueByOffset(PS4_MESETA_LOC, 4);
            long meseta = long.Parse(hexVal, System.Globalization.NumberStyles.HexNumber);
            return meseta.ToString();
        }

        private void PopulatePS4CurrentMeseta()
        {
            ps4CurrentMesetaTb.Text = GetCurrentPS4Meseta();
        }

        private void ResetPS4Controls()
        {
            ps4CurrentLevelTb.Text = string.Empty;
            ps4CurExpTb.Text = string.Empty;
            ps4NewExpTb.Text = string.Empty;
            ps4CurrentMesetaTb.Text = string.Empty;
            ps4NewMesetaTb.Text = string.Empty;
            ps4CurrentLevelTb.Text = string.Empty;
            ps4CurHPTb.Text = string.Empty;
            ps4NewCurHPTb.Text = string.Empty;
            ps4MaxHPTb.Text = string.Empty;
            ps4NewMaxHPTb.Text = string.Empty;
            ps4CurTPTb.Text = string.Empty;
            ps4NewCurTPTb.Text = string.Empty;
            ps4MaxTPTb.Text = string.Empty;
            ps4NewMaxTPTb.Text = string.Empty;
            ps4StrTb.Text = string.Empty;
            ps4NewStrTb.Text = string.Empty;
            ps4MentalTb.Text = string.Empty;
            ps4NewMaxHPTb.Text = string.Empty;
            ps4AgilityTb.Text = string.Empty;
            ps4NewAgilityTb.Text = string.Empty;
            ps4DexTb.Text = string.Empty;
            ps4NewDexTb.Text = string.Empty;
            ps4WeaponSlot1Tb.Text = string.Empty;
            ps4WeaponSlot2Tb.Text = string.Empty;
            ps4HelmetTb.Text = string.Empty;
            ps4ArmorTb.Text = string.Empty;
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

        private void PopulatePS4CharacterList()
        {
            ps4CharacterCmb.Items.Clear();

            PS4CharacterItem chazItem = new PS4CharacterItem("Chaz",
                "11981",
                "11982",
                "11986",
                "11988",
                "1198A",
                "1198C",
                "11990",
                "11993",
                "11996",
                "11999",
                "119C4",
                "119C5",
                "119C6",
                "119C7",
                "1199D",
                "119A1");
            ps4CharacterCmb.Items.Add(chazItem);

            PS4CharacterItem alysItem = new PS4CharacterItem("Alys",
                "11A01",
                "11A02",
                "11A06",
                "11A08",
                "11A0A",
                "11A0C",
                "11A10",
                "11A13",
                "11A16",
                "11A19",
                "11A44",
                "11A45",
                "11A46",
                "11A47",
                "11A1D",
                "11A21");
            ps4CharacterCmb.Items.Add(alysItem);

            PS4CharacterItem hahnItem = new PS4CharacterItem("Hahn",
                "11A81",
                "11A82",
                "11A86",
                "11A88",
                "11A8A",
                "11A8C",
                "11A90",
                "11A93",
                "11A96",
                "11A99",
                "11AC4",
                "11AC5",
                "11AC6",
                "11AC7",
                "11A9D",
                "11AA1");
            ps4CharacterCmb.Items.Add(hahnItem);

            PS4CharacterItem runeItem = new PS4CharacterItem("Rune",
                "11B01",
                "11B02",
                "11B06",
                "11B08",
                "11B0A",
                "11B0C",
                "11B10",
                "11B13",
                "11B16",
                "11B19",
                "11B44",
                "11B45",
                "11B46",
                "11B47",
                "11B1D",
                "11B21");
            ps4CharacterCmb.Items.Add(runeItem);

            PS4CharacterItem gryzItem = new PS4CharacterItem("Gryz",
                "11B81",
                "11B82",
                "11B86",
                "11B88",
                "11B8A",
                "11B8C",
                "11B90",
                "11B93",
                "11B96",
                "11B99",
                "11BC4",
                "11BC5",
                "11BC6",
                "11BC7",
                "11B9D",
                "11BA1");
            ps4CharacterCmb.Items.Add(gryzItem);

            PS4CharacterItem rikaItem = new PS4CharacterItem("Rika",
                "11C01",
                "11C02",
                "11C06",
                "11C08",
                "11C0A",
                "11C0C",
                "11C10",
                "11C13",
                "11C16",
                "11C19",
                "11C44",
                "11C45",
                "11C46",
                "11C47",
                "11C1D",
                "11C21");
            ps4CharacterCmb.Items.Add(rikaItem);

            PS4CharacterItem demiItem = new PS4CharacterItem("Demi",
                "11C81",
                "11C82",
                "11C86",
                "11C88",
                "11C8A",
                "11C8C",
                "11C90",
                "11C93",
                "11C96",
                "11C99",
                "11CC4",
                "11CC5",
                "11CC6",
                "11CC7",
                "11C9D",
                "11CA1");
            ps4CharacterCmb.Items.Add(demiItem);

            PS4CharacterItem wrenItem = new PS4CharacterItem("Wren",
                "11D01",
                "11D02",
                "11D06",
                "11D08",
                "11D0A",
                "11D0C",
                "11D10",
                "11D13",
                "11D16",
                "11D19",
                "11D44",
                "11D45",
                "11D46",
                "11D47",
                "11D1D",
                "11D21");
            ps4CharacterCmb.Items.Add(wrenItem);

            PS4CharacterItem rajaItem = new PS4CharacterItem("Raja",
                "11D81",
                "11D82",
                "11D86",
                "11D88",
                "11D8A",
                "11D8C",
                "11D90",
                "11D93",
                "11D96",
                "11D99",
                "11DC4",
                "11DC5",
                "11DC6",
                "11DC7",
                "11D9D",
                "11DA1");
            ps4CharacterCmb.Items.Add(rajaItem);

            PS4CharacterItem kyraItem = new PS4CharacterItem("Kyra",
                "11E01",
                "11E02",
                "11E06",
                "11E08",
                "11E0A",
                "11E0C",
                "11E10",
                "11E13",
                "11E16",
                "11E19",
                "11E44",
                "11E45",
                "11E46",
                "11E47",
                "11E9D",
                "11E21");
            ps4CharacterCmb.Items.Add(kyraItem);

            PS4CharacterItem sethItem = new PS4CharacterItem("Seth",
                "11E81",
                "11E82",
                "11E86",
                "11E88",
                "11E8A",
                "11E8C",
                "11E90",
                "11E93",
                "11E96",
                "11E99",
                "11EC4",
                "11EC5",
                "11EC6",
                "11EC7",
                "11E9D",
                "11EA1");
            ps4CharacterCmb.Items.Add(sethItem);

            ps4CharacterCmb.DisplayMember = "Name";
        }

        private void PopulatePS4CharacterDetails(PS4CharacterItem charItem)
        {
            string value = GetValueByOffset(charItem.LevelLoc, 1);
            long val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps4CurrentLevelTb.Text = val.ToString();
            value = GetValueByOffset(charItem.ExpLoc, 4);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps4CurExpTb.Text = val.ToString();
            value = GetValueByOffset(charItem.CurrentHPLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps4CurHPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.MaxHPLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps4MaxHPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.CurrentTPLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps4CurTPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.MaxTPLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps4MaxTPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.StrengthLoc, 1);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps4StrTb.Text = val.ToString();
            value = GetValueByOffset(charItem.MentalLoc, 1);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps4MentalTb.Text = val.ToString();
            value = GetValueByOffset(charItem.AgilityLoc, 1);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps4AgilityTb.Text = val.ToString();
            value = GetValueByOffset(charItem.DexterityLoc, 1);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
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
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps4AtkPowTb.Text = val.ToString();
            value = GetValueByOffset(charItem.DefenseLoc, 1);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
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

        private void ResetPS1Controls()
        {
            ps1CurrentMesetaTb.Text = string.Empty;
            ps1NewMesetaTb.Text = string.Empty;
            ps1LevelTb.Text = string.Empty;
            ps1ExpTb.Text = string.Empty;
            ps1NewExpTb.Text = string.Empty;
            ps1CurrentHPTb.Text = string.Empty;
            ps1NewCurrentHPTb.Text = string.Empty;
            ps1MaxHPTb.Text = string.Empty;
            ps1NewMaxHPTb.Text = string.Empty;
            ps1CurrentMPTb.Text = string.Empty;
            ps1NewCurrentMPTb.Text = string.Empty;
            ps1MaxMPTb.Text = string.Empty;
            ps1NewMaxMPTb.Text = string.Empty;
            ps1AttackTb.Text = string.Empty;
            ps1NewAttackTb.Text = string.Empty;
            ps1DefenseTb.Text = string.Empty;
            ps1NewDefenseTb.Text = string.Empty;
        }

        private void PopulatePS1CharacterList()
        {
            ps1CharacterCmb.Items.Clear();

            PS1CharacterItem alisItem = new PS1CharacterItem("Alis Landale",
                "44BF",
                "44C1",
                "44BD",
                "44C2",
                "44BE",
                "44C3",
                "44C4",
                "44C5",
                string.Empty,
                string.Empty,
                string.Empty);
            ps1CharacterCmb.Items.Add(alisItem);

            PS1CharacterItem myauItem = new PS1CharacterItem("Myau",
                "44CF",
                "44D1",
                "44CD",
                "44D2",
                "44CE",
                "44D3",
                "44D4",
                "44D5",
                string.Empty,
                string.Empty,
                string.Empty);
            ps1CharacterCmb.Items.Add(myauItem);

            PS1CharacterItem odinItem = new PS1CharacterItem("Odin",
                "44DF",
                "44E1",
                "44DD",
                "44E2",
                "44DE",
                "44E3",
                "44E4",
                "44E5",
                string.Empty,
                string.Empty,
                string.Empty);
            ps1CharacterCmb.Items.Add(odinItem);

            PS1CharacterItem noahItem = new PS1CharacterItem("Noah",
                "44EF",
                "44F1",
                "44ED",
                "44F2",
                "44EE",
                "44F3",
                "44F4",
                "44F5",
                string.Empty,
                string.Empty,
                string.Empty);
            ps1CharacterCmb.Items.Add(noahItem);

            ps1CharacterCmb.DisplayMember = "Name";
        }

        private string GetPS1CurrentMeseta()
        {
            string hexVal = GetValueByOffset(PS1_MESETA_LOC, 2);
            hexVal = ReverseHexPairs(hexVal);
            long meseta = long.Parse(hexVal, System.Globalization.NumberStyles.HexNumber);
            return meseta.ToString();
        }

        private void PopulatePS1CurrentMeseta()
        {
            ps1CurrentMesetaTb.Text = GetPS1CurrentMeseta();
        }

        private void PopulatePS1CharacterDetails(PS1CharacterItem charItem)
        {
            string value = GetValueByOffset(charItem.LevelLoc, 1);
            long val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps1LevelTb.Text = val.ToString();

            value = GetValueByOffset(charItem.ExperienceLoc, 2);
            value = ReverseHexPairs(value);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps1ExpTb.Text = val.ToString();

            value = GetValueByOffset(charItem.CurrentHPLoc, 1);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps1CurrentHPTb.Text = val.ToString();

            value = GetValueByOffset(charItem.MaxHPLoc, 1);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps1MaxHPTb.Text = val.ToString();

            value = GetValueByOffset(charItem.CurrentMPLoc, 1);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps1CurrentMPTb.Text = val.ToString();

            value = GetValueByOffset(charItem.MaxMPLoc, 1);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
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
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps1AttackTb.Text = val.ToString();

            value = GetValueByOffset(charItem.DefenseLoc, 1);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps1DefenseTb.Text = val.ToString();
        }

        private void ResetPS2Controls()
        {
            ps2CurMesetaTb.Text = string.Empty;
            ps2NewMesetaTb.Text = string.Empty;
            ps2CurHPTb.Text = string.Empty;
            ps2MaxHPTb.Text = string.Empty;
            ps2CurTPTb.Text = string.Empty;
            ps2LevelTb.Text = string.Empty;
            ps2ExpTb.Text = string.Empty;
            ps2StrTb.Text = string.Empty;
            ps2MentalTb.Text = string.Empty;
            ps2AgilityTb.Text = string.Empty;
            ps2LuckTb.Text = string.Empty;
            ps2DexTb.Text = string.Empty;
            ps2AttackTb.Text = string.Empty;
            ps2DefTb.Text = string.Empty;
            ps2NewCurHPTb.Text = string.Empty;
            ps2NewMaxHPTb.Text = string.Empty;
            ps2NewCurTPTb.Text = string.Empty;
            ps2NewExpTb.Text = string.Empty;
            ps2NewStrTb.Text = string.Empty;
            ps2NewMentalTb.Text = string.Empty;
            ps2NewAgilityTb.Text = string.Empty;
            ps2NewLuckTb.Text = string.Empty;
            ps2NewDexTb.Text = string.Empty;
            ps2NewAttackTb.Text = string.Empty;
            ps2NewDefTb.Text = string.Empty;
        }

        private void PopulatePS2CharacterList()
        {
            ps3CharacterCmb.Items.Clear();

            PS2CharacterItem rolfItem = new PS2CharacterItem("Rolf Landale",
                "E47A",
                "E47C",
                "E47E",
                "E480",
                "E483",
                "E484",
                "E488",
                "E48A",
                "E48C",
                "E48E",
                "E490",
                "E494",
                "E496");
            ps2CharacterCmb.Items.Add(rolfItem);

            PS2CharacterItem neiItem = new PS2CharacterItem("Nei",
                "E4BA",
                "E4BC",
                "E4BE",
                "E4C0",
                "E4C3",
                "E4C4",
                "E4C8",
                "E4CA",
                "E4CC",
                "E4CE",
                "E4D0",
                "E4D4",
                "E4D6");
            ps2CharacterCmb.Items.Add(neiItem);

            PS2CharacterItem rudoItem = new PS2CharacterItem("Rudo Steiner",
                "E4FA",
                "E4FC",
                "E4FE",
                "E500",
                "E503",
                "E504",
                "E508",
                "E50A",
                "E50C",
                "E50E",
                "E510",
                "E514",
                "E516");
            ps2CharacterCmb.Items.Add(rudoItem);

            PS2CharacterItem amyItem = new PS2CharacterItem("Amy Sage",
                "E53A",
                "E53C",
                "E53E",
                "E540",
                "E543",
                "E544",
                "E548",
                "E54A",
                "E54C",
                "E54E",
                "E550",
                "E554",
                "E556");
            ps2CharacterCmb.Items.Add(amyItem);

            PS2CharacterItem hughItem = new PS2CharacterItem("Hugh Tompson",
                "E57A",
                "E57C",
                "E57E",
                "E580",
                "E583",
                "E584",
                "E588",
                "E58A",
                "E58C",
                "E58E",
                "E590",
                "E594",
                "E596");
            ps2CharacterCmb.Items.Add(hughItem);

            PS2CharacterItem annaItem = new PS2CharacterItem("Anna Zirski",
                "E5BA",
                "E5BC",
                "E5BE",
                "E5C0",
                "E5C3",
                "E5C4",
                "E5C8",
                "E5CA",
                "E5CC",
                "E5CE",
                "E5D0",
                "E5D4",
                "E5D6");
            ps2CharacterCmb.Items.Add(annaItem);

            PS2CharacterItem joshItem = new PS2CharacterItem("Josh Kain",
                "E5FA",
                "E5FC",
                "E5FE",
                "E600",
                "E603",
                "E604",
                "E608",
                "E60A",
                "E60C",
                "E60E",
                "E610",
                "E614",
                "E616");
            ps2CharacterCmb.Items.Add(joshItem);

            PS2CharacterItem shirItem = new PS2CharacterItem("Shir Gold",
                "E63A",
                "E63C",
                "E63E",
                "E640",
                "E643",
                "E644",
                "E648",
                "E64A",
                "E64C",
                "E64E",
                "E650",
                "E654",
                "E656");
            ps2CharacterCmb.Items.Add(shirItem);

            ps2CharacterCmb.DisplayMember = "Name";
        }

        private string GetPS2CurrentMeseta()
        {
            string hexVal = GetValueByOffset(PS2_MESETA_LOC, 4);
            long meseta = long.Parse(hexVal, System.Globalization.NumberStyles.HexNumber);
            return meseta.ToString();
        }

        private void PopulatePS2CurrentMeseta()
        {
            ps2CurMesetaTb.Text = GetPS2CurrentMeseta();
        }

        private void PopulatePS2CharacterDetails(PS2CharacterItem charItem)
        {
            string value = GetValueByOffset(charItem.CurrentHPLoc, 2);
            long val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps2CurHPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.MaxHPLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps2MaxHPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.CurrentTPLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps2CurTPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.MaxTPLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps2MaxTPTb.Text = val.ToString();
            value = GetValueByOffset(charItem.LevelLoc, 1);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps2LevelTb.Text = val.ToString();
            value = GetValueByOffset(charItem.ExperienceLoc, 4);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps2ExpTb.Text = val.ToString();
            value = GetValueByOffset(charItem.StrengthLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps2StrTb.Text = val.ToString();
            value = GetValueByOffset(charItem.MentalLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps2MentalTb.Text = val.ToString();
            value = GetValueByOffset(charItem.AgilityLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps2AgilityTb.Text = val.ToString();
            value = GetValueByOffset(charItem.LuckLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps2LuckTb.Text = val.ToString();
            value = GetValueByOffset(charItem.DexterityLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps2DexTb.Text = val.ToString();
            value = GetValueByOffset(charItem.AttackLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps2AttackTb.Text = val.ToString();
            value = GetValueByOffset(charItem.DefenseLoc, 2);
            val = long.Parse(value, System.Globalization.NumberStyles.HexNumber);
            ps2DefTb.Text = val.ToString();
        }

        private string GetPS3CurrentMeseta()
        {
            string hexVal = GetValueByOffset(PS3_MESETA_LOC, 4);
            long meseta = long.Parse(hexVal, System.Globalization.NumberStyles.HexNumber);
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
            ps3NewMesetaTb.Text = string.Empty;
            ps3NewSpeedTb.Text = string.Empty;
            ps3NewMaxHPTb.Text = string.Empty;
            ps3NewMaxTPTb.Text = string.Empty;
            ps3NewCurHPTb.Text = string.Empty;
            ps3NewCurTPTb.Text = string.Empty;
            ps3NewDmgTb.Text = string.Empty;
            ps3NewDefTb.Text = string.Empty;
            ps3NewExpTb.Text = string.Empty;
            ps3NewLuckTb.Text = string.Empty;
            ps3NewSkillTb.Text = string.Empty;
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

            MessageBox.Show("The save state update process has completed.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ResetPS3Controls();
            PopulatePS3CurrentMeseta();
            PopulatePS3CharacterDetails(charItem);
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
                    MessageBox.Show("You must enter a numeric value for the new meseta value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show("You must enter a numeric value for the new meseta value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show("You must enter a numeric value for the new meseta value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show("You must enter a numeric value for the new meseta value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps4NewStrTb.Text != string.Empty)
            {
                short str = 0;
                if (short.TryParse(ps4NewStrTb.Text, out str))
                {
                    SetValueByOffset(str, charItem.StrengthLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the new meseta value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps4NewMentalTb.Text != string.Empty)
            {
                short mental = 0;
                if (short.TryParse(ps4NewMentalTb.Text, out mental))
                {
                    SetValueByOffset(mental, charItem.MentalLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the new meseta value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps4NewAgilityTb.Text != string.Empty)
            {
                short agility = 0;
                if (short.TryParse(ps4NewAgilityTb.Text, out agility))
                {
                    SetValueByOffset(agility, charItem.AgilityLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the new meseta value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps4NewDexTb.Text != string.Empty)
            {
                short dex = 0;
                if (short.TryParse(ps4NewDexTb.Text, out dex))
                {
                    SetValueByOffset(dex, charItem.DexterityLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the new meseta value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps4NewAtkPowTb.Text != string.Empty)
            {
                short atk = 0;
                if (short.TryParse(ps4NewAtkPowTb.Text, out atk))
                {
                    SetValueByOffset(atk, charItem.AttackLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the new meseta value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (ps4NewDefPowTb.Text != string.Empty)
            {
                short def = 0;
                if (short.TryParse(ps4NewDefPowTb.Text, out def))
                {
                    SetValueByOffset(def, charItem.DefenseLoc);
                }
                else
                {
                    MessageBox.Show("You must enter a numeric value for the new meseta value.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            MessageBox.Show("The save state update process has completed.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ResetPS4Controls();
            PopulatePS4CurrentMeseta();
            PopulatePS4CharacterDetails(charItem);
        }
        #endregion        
    }
}
