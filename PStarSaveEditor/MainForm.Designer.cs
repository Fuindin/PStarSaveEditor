namespace PStarSaveEditor;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;

    private MenuStrip _menu = null!;
    private ToolStripMenuItem _mnuFile = null!, _mnuOpen = null!, _mnuSave = null!, _mnuReload = null!, _mnuExit = null!;
    private ToolStripMenuItem _mnuGame = null!;
    private ToolStripMenuItem _mnuHelp = null!, _mnuInstructions = null!, _mnuViewLog = null!, _mnuClearLog = null!, _mnuAbout = null!;

    private Panel _header = null!;
    private Label _lblFile = null!, _lblInfo = null!, _lblTitle = null!;
    private Panel _toolbar = null!;
    private Label _lblGame = null!, _lblMeseta = null!;
    private ComboBox _cboGame = null!;
    private NumericUpDown _numMeseta = null!;
    private Button _btnOpen = null!, _btnSave = null!;

    private Label _lblBanner = null!;

    private Panel _leftPanel = null!;
    private Label _lblParty = null!, _partyNote = null!;
    private ListBox _lstChars = null!;

    private Panel _divider = null!;

    private Panel _coverPanel = null!;
    private PictureBox _picCover = null!;

    private Panel _rightPanel = null!;
    private Panel _namePlate = null!;
    private PhantasyTabControl _tabs = null!;
    private Panel _tabStatus = null!, _tabEquip = null!, _tabItems = null!, _tabTech = null!;
    private TableLayoutPanel _fieldTable = null!;
    private Label _lblEquipHdr = null!, _equipNote = null!;
    private ListBox _equipList = null!;
    private Label _lblItemsHdr = null!, _itemsNote = null!, _lblTechHdr = null!, _techNote = null!;
    private ListBox _itemList = null!, _techList = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _menu = new MenuStrip();
        _mnuFile = new ToolStripMenuItem();
        _mnuOpen = new ToolStripMenuItem();
        _mnuSave = new ToolStripMenuItem();
        _mnuReload = new ToolStripMenuItem();
        _mnuExit = new ToolStripMenuItem();
        _mnuGame = new ToolStripMenuItem();
        _mnuHelp = new ToolStripMenuItem();
        _mnuInstructions = new ToolStripMenuItem();
        _mnuViewLog = new ToolStripMenuItem();
        _mnuClearLog = new ToolStripMenuItem();
        _mnuAbout = new ToolStripMenuItem();
        _header = new Panel();
        _lblTitle = new Label();
        _toolbar = new Panel();
        _lblGame = new Label();
        _cboGame = new ComboBox();
        _lblMeseta = new Label();
        _numMeseta = new NumericUpDown();
        _btnOpen = new Button();
        _btnSave = new Button();
        _lblInfo = new Label();
        _lblFile = new Label();
        _lblBanner = new Label();
        _leftPanel = new Panel();
        _lstChars = new ListBox();
        _lblParty = new Label();
        _partyNote = new Label();
        _divider = new Panel();
        _coverPanel = new Panel();
        _picCover = new PictureBox();
        _rightPanel = new Panel();
        _tabs = new PhantasyTabControl();
        _tabStatus = new Panel();
        _fieldTable = new TableLayoutPanel();
        _tabEquip = new Panel();
        _equipList = new ListBox();
        _lblEquipHdr = new Label();
        _equipNote = new Label();
        _tabItems = new Panel();
        _itemList = new ListBox();
        _lblItemsHdr = new Label();
        _itemsNote = new Label();
        _tabTech = new Panel();
        _techList = new ListBox();
        _lblTechHdr = new Label();
        _techNote = new Label();
        _namePlate = new Panel();
        _menu.SuspendLayout();
        _header.SuspendLayout();
        _toolbar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_numMeseta).BeginInit();
        _leftPanel.SuspendLayout();
        _coverPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_picCover).BeginInit();
        _rightPanel.SuspendLayout();
        _tabs.SuspendLayout();
        _tabStatus.SuspendLayout();
        _tabEquip.SuspendLayout();
        _tabItems.SuspendLayout();
        _tabTech.SuspendLayout();
        SuspendLayout();
        //
        // _menu
        //
        _menu.ImageScalingSize = new Size(24, 24);
        _menu.Items.AddRange(new ToolStripItem[] { _mnuFile, _mnuGame, _mnuHelp });
        _menu.Location = new Point(0, 0);
        _menu.Name = "_menu";
        _menu.Padding = new Padding(9, 3, 0, 3);
        _menu.Size = new Size(1452, 35);
        _menu.TabIndex = 6;
        //
        // _mnuFile
        //
        _mnuFile.DropDownItems.AddRange(new ToolStripItem[] { _mnuOpen, _mnuSave, _mnuReload, new ToolStripSeparator(), _mnuExit });
        _mnuFile.Name = "_mnuFile";
        _mnuFile.Text = "&File";
        _mnuOpen.Name = "_mnuOpen";
        _mnuOpen.ShortcutKeys = Keys.Control | Keys.O;
        _mnuOpen.Text = "&Open Save State…";
        _mnuSave.Name = "_mnuSave";
        _mnuSave.ShortcutKeys = Keys.Control | Keys.S;
        _mnuSave.Text = "&Save All Changes";
        _mnuReload.Name = "_mnuReload";
        _mnuReload.Text = "&Reload from Disk (discard changes)";
        _mnuExit.Name = "_mnuExit";
        _mnuExit.Text = "E&xit";
        //
        // _mnuGame (items are added at runtime from Games.All)
        //
        _mnuGame.Name = "_mnuGame";
        _mnuGame.Text = "&Game";
        //
        // _mnuHelp
        //
        _mnuHelp.DropDownItems.AddRange(new ToolStripItem[] { _mnuInstructions, new ToolStripSeparator(), _mnuViewLog, _mnuClearLog, new ToolStripSeparator(), _mnuAbout });
        _mnuHelp.Name = "_mnuHelp";
        _mnuHelp.Text = "&Help";
        _mnuInstructions.Name = "_mnuInstructions";
        _mnuInstructions.ShortcutKeys = Keys.F1;
        _mnuInstructions.Text = "How to Use…";
        _mnuViewLog.Name = "_mnuViewLog";
        _mnuViewLog.Text = "View Error Log…";
        _mnuClearLog.Name = "_mnuClearLog";
        _mnuClearLog.Text = "Clear Error Log";
        _mnuAbout.Name = "_mnuAbout";
        _mnuAbout.Text = "About Phantasy Star Save Editor…";
        //
        // _header
        //
        _header.Controls.Add(_lblTitle);
        _header.Controls.Add(_toolbar);
        _header.Controls.Add(_lblInfo);
        _header.Controls.Add(_lblFile);
        _header.Dock = DockStyle.Top;
        _header.Location = new Point(0, 35);
        _header.Name = "_header";
        _header.Padding = new Padding(17, 13, 17, 13);
        _header.Size = new Size(1452, 153);
        _header.TabIndex = 5;
        //
        // _lblTitle
        //
        _lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _lblTitle.Location = new Point(935, 13);
        _lblTitle.Name = "_lblTitle";
        _lblTitle.Size = new Size(500, 62);
        _lblTitle.TabIndex = 3;
        _lblTitle.Text = "PHANTASY STAR EDITOR";
        _lblTitle.TextAlign = ContentAlignment.MiddleCenter;
        //
        // _toolbar
        //
        _toolbar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _toolbar.Controls.Add(_lblGame);
        _toolbar.Controls.Add(_cboGame);
        _toolbar.Controls.Add(_lblMeseta);
        _toolbar.Controls.Add(_numMeseta);
        _toolbar.Controls.Add(_btnOpen);
        _toolbar.Controls.Add(_btnSave);
        _toolbar.Location = new Point(20, 87);
        _toolbar.Name = "_toolbar";
        _toolbar.Size = new Size(1415, 57);
        _toolbar.TabIndex = 0;
        //
        // _lblGame
        //
        _lblGame.AutoSize = true;
        _lblGame.Location = new Point(0, 12);
        _lblGame.Name = "_lblGame";
        _lblGame.TabIndex = 0;
        _lblGame.Text = "GAME";
        //
        // _cboGame
        //
        _cboGame.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboGame.FlatStyle = FlatStyle.Flat;
        _cboGame.Location = new Point(80, 8);
        _cboGame.Name = "_cboGame";
        _cboGame.Size = new Size(290, 33);
        _cboGame.TabIndex = 1;
        //
        // _lblMeseta
        //
        _lblMeseta.AutoSize = true;
        _lblMeseta.Location = new Point(400, 10);
        _lblMeseta.Name = "_lblMeseta";
        _lblMeseta.TabIndex = 2;
        _lblMeseta.Text = "MESETA";
        //
        // _numMeseta
        //
        _numMeseta.Location = new Point(515, 6);
        _numMeseta.Name = "_numMeseta";
        _numMeseta.Size = new Size(240, 31);
        _numMeseta.TabIndex = 3;
        _numMeseta.ThousandsSeparator = true;
        _numMeseta.TextAlign = HorizontalAlignment.Right;
        //
        // _btnOpen
        //
        _btnOpen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnOpen.Location = new Point(1060, 0);
        _btnOpen.Name = "_btnOpen";
        _btnOpen.Size = new Size(169, 53);
        _btnOpen.TabIndex = 4;
        _btnOpen.Text = "Open…";
        //
        // _btnSave
        //
        _btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnSave.Location = new Point(1243, 0);
        _btnSave.Name = "_btnSave";
        _btnSave.Size = new Size(169, 53);
        _btnSave.TabIndex = 5;
        _btnSave.Text = "Save All";
        //
        // _lblInfo
        //
        _lblInfo.AutoSize = true;
        _lblInfo.Location = new Point(20, 50);
        _lblInfo.Name = "_lblInfo";
        _lblInfo.TabIndex = 1;
        //
        // _lblFile
        //
        _lblFile.AutoSize = true;
        _lblFile.Location = new Point(20, 17);
        _lblFile.Name = "_lblFile";
        _lblFile.TabIndex = 2;
        _lblFile.Text = "No save state loaded.";
        //
        // _lblBanner
        //
        _lblBanner.Dock = DockStyle.Top;
        _lblBanner.Location = new Point(0, 188);
        _lblBanner.Name = "_lblBanner";
        _lblBanner.Padding = new Padding(17, 0, 17, 0);
        _lblBanner.Size = new Size(1452, 0);
        _lblBanner.TabIndex = 4;
        _lblBanner.TextAlign = ContentAlignment.MiddleLeft;
        _lblBanner.Visible = false;
        //
        // _leftPanel
        //
        _leftPanel.Controls.Add(_lstChars);
        _leftPanel.Controls.Add(_lblParty);
        _leftPanel.Controls.Add(_partyNote);
        _leftPanel.Dock = DockStyle.Left;
        _leftPanel.Location = new Point(0, 188);
        _leftPanel.Name = "_leftPanel";
        _leftPanel.Padding = new Padding(17, 17, 9, 20);
        _leftPanel.Size = new Size(430, 983);
        _leftPanel.TabIndex = 3;
        //
        // _lstChars
        //
        _lstChars.BorderStyle = BorderStyle.None;
        _lstChars.Dock = DockStyle.Fill;
        _lstChars.DrawMode = DrawMode.OwnerDrawFixed;
        _lstChars.IntegralHeight = false;
        _lstChars.ItemHeight = 120;
        _lstChars.Name = "_lstChars";
        _lstChars.TabIndex = 0;
        //
        // _lblParty
        //
        _lblParty.Dock = DockStyle.Top;
        _lblParty.Name = "_lblParty";
        _lblParty.Size = new Size(404, 47);
        _lblParty.TabIndex = 1;
        _lblParty.Text = "PARTY";
        //
        // _partyNote
        //
        _partyNote.Dock = DockStyle.Bottom;
        _partyNote.Name = "_partyNote";
        _partyNote.Size = new Size(404, 0);
        _partyNote.TabIndex = 2;
        _partyNote.Visible = false;
        //
        // _divider
        //
        _divider.Dock = DockStyle.Left;
        _divider.Name = "_divider";
        _divider.Size = new Size(3, 983);
        _divider.TabIndex = 2;
        //
        // _coverPanel
        //
        _coverPanel.Controls.Add(_picCover);
        _coverPanel.Dock = DockStyle.Left;
        _coverPanel.Name = "_coverPanel";
        _coverPanel.Padding = new Padding(24, 26, 24, 26);
        _coverPanel.Size = new Size(320, 983);
        _coverPanel.TabIndex = 1;
        //
        // _picCover
        //
        _picCover.Dock = DockStyle.Top;
        _picCover.Name = "_picCover";
        _picCover.Size = new Size(272, 380);
        _picCover.SizeMode = PictureBoxSizeMode.Zoom;
        _picCover.TabIndex = 0;
        _picCover.TabStop = false;
        //
        // _rightPanel
        //
        _rightPanel.Controls.Add(_tabs);
        _rightPanel.Controls.Add(_namePlate);
        _rightPanel.Dock = DockStyle.Fill;
        _rightPanel.Name = "_rightPanel";
        _rightPanel.Padding = new Padding(23, 20, 23, 20);
        _rightPanel.TabIndex = 0;
        //
        // _tabs
        //
        // Pages are attached at runtime with _tabs.SetPages (depends on the game).
        _tabs.Dock = DockStyle.Fill;
        _tabs.Name = "_tabs";
        _tabs.TabIndex = 0;
        //
        // _tabStatus
        //
        _tabStatus.AutoScroll = true;
        _tabStatus.Controls.Add(_fieldTable);
        _tabStatus.Name = "_tabStatus";
        _tabStatus.Padding = new Padding(23, 20, 23, 20);
        _tabStatus.TabIndex = 0;
        _tabStatus.Text = "STATUS";
        //
        // _fieldTable
        //
        _fieldTable.ColumnCount = 4;
        _fieldTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _fieldTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _fieldTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _fieldTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        // Sized explicitly to the page width in MainForm.FitFieldTable.
        _fieldTable.Name = "_fieldTable";
        _fieldTable.TabIndex = 0;
        //
        // _tabEquip
        //
        _tabEquip.Controls.Add(_equipList);
        _tabEquip.Controls.Add(_lblEquipHdr);
        _tabEquip.Controls.Add(_equipNote);
        _tabEquip.Name = "_tabEquip";
        _tabEquip.Padding = new Padding(23, 20, 23, 20);
        _tabEquip.TabIndex = 1;
        _tabEquip.Text = "EQUIP";
        //
        // _equipList
        //
        _equipList.BorderStyle = BorderStyle.None;
        _equipList.Dock = DockStyle.Fill;
        _equipList.DrawMode = DrawMode.OwnerDrawFixed;
        _equipList.IntegralHeight = false;
        _equipList.ItemHeight = 40;
        _equipList.Name = "_equipList";
        _equipList.SelectionMode = SelectionMode.None;
        _equipList.TabIndex = 0;
        //
        // _lblEquipHdr
        //
        _lblEquipHdr.Dock = DockStyle.Top;
        _lblEquipHdr.Name = "_lblEquipHdr";
        _lblEquipHdr.Size = new Size(669, 43);
        _lblEquipHdr.TabIndex = 1;
        _lblEquipHdr.Text = "EQUIPMENT";
        //
        // _equipNote
        //
        _equipNote.Dock = DockStyle.Bottom;
        _equipNote.Name = "_equipNote";
        _equipNote.Size = new Size(669, 37);
        _equipNote.TabIndex = 2;
        //
        // _tabItems
        //
        _tabItems.Controls.Add(_itemList);
        _tabItems.Controls.Add(_lblItemsHdr);
        _tabItems.Controls.Add(_itemsNote);
        _tabItems.Name = "_tabItems";
        _tabItems.Padding = new Padding(23, 20, 23, 20);
        _tabItems.TabIndex = 2;
        //
        // _itemList
        //
        _itemList.BorderStyle = BorderStyle.None;
        _itemList.Dock = DockStyle.Fill;
        _itemList.DrawMode = DrawMode.OwnerDrawFixed;
        _itemList.IntegralHeight = false;
        _itemList.ItemHeight = 36;
        _itemList.Name = "_itemList";
        _itemList.SelectionMode = SelectionMode.None;
        _itemList.TabIndex = 0;
        //
        // _lblItemsHdr
        //
        _lblItemsHdr.Dock = DockStyle.Top;
        _lblItemsHdr.Name = "_lblItemsHdr";
        _lblItemsHdr.Size = new Size(669, 43);
        _lblItemsHdr.TabIndex = 1;
        //
        // _itemsNote
        //
        _itemsNote.Dock = DockStyle.Bottom;
        _itemsNote.Name = "_itemsNote";
        _itemsNote.Size = new Size(669, 37);
        _itemsNote.TabIndex = 2;
        //
        // _tabTech
        //
        _tabTech.Controls.Add(_techList);
        _tabTech.Controls.Add(_lblTechHdr);
        _tabTech.Controls.Add(_techNote);
        _tabTech.Name = "_tabTech";
        _tabTech.Padding = new Padding(23, 20, 23, 20);
        _tabTech.TabIndex = 3;
        //
        // _techList
        //
        _techList.BorderStyle = BorderStyle.None;
        _techList.Dock = DockStyle.Fill;
        _techList.DrawMode = DrawMode.OwnerDrawFixed;
        _techList.IntegralHeight = false;
        _techList.ItemHeight = 36;
        _techList.Name = "_techList";
        _techList.SelectionMode = SelectionMode.None;
        _techList.TabIndex = 0;
        //
        // _lblTechHdr
        //
        _lblTechHdr.Dock = DockStyle.Top;
        _lblTechHdr.Name = "_lblTechHdr";
        _lblTechHdr.Size = new Size(669, 43);
        _lblTechHdr.TabIndex = 1;
        //
        // _techNote
        //
        _techNote.Dock = DockStyle.Bottom;
        _techNote.Name = "_techNote";
        _techNote.Size = new Size(669, 37);
        _techNote.TabIndex = 2;
        //
        // _namePlate
        //
        _namePlate.Dock = DockStyle.Top;
        _namePlate.Name = "_namePlate";
        _namePlate.Size = new Size(723, 107);
        _namePlate.TabIndex = 1;
        //
        // MainForm
        //
        AllowDrop = true;
        AutoScaleDimensions = new SizeF(10F, 25F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1452, 1100);
        Controls.Add(_rightPanel);
        Controls.Add(_coverPanel);
        Controls.Add(_divider);
        Controls.Add(_leftPanel);
        Controls.Add(_lblBanner);
        Controls.Add(_header);
        Controls.Add(_menu);
        KeyPreview = true;
        MainMenuStrip = _menu;
        MinimumSize = new Size(1300, 900);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Phantasy Star Save Editor";
        _menu.ResumeLayout(false);
        _menu.PerformLayout();
        _header.ResumeLayout(false);
        _header.PerformLayout();
        _toolbar.ResumeLayout(false);
        _toolbar.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_numMeseta).EndInit();
        _leftPanel.ResumeLayout(false);
        _coverPanel.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_picCover).EndInit();
        _rightPanel.ResumeLayout(false);
        _tabs.ResumeLayout(false);
        _tabStatus.ResumeLayout(false);
        _tabStatus.PerformLayout();
        _tabEquip.ResumeLayout(false);
        _tabItems.ResumeLayout(false);
        _tabTech.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
}
