namespace PStarSaveEditor;

public partial class ErrorLogView : Form
{
    public ErrorLogView()
    {
        InitializeComponent();
        Theme.ApplyDialog(this);
        outputTb.Font = Theme.Body(9.5f);

        // The themed button uses a bolder font; let it size itself and keep it
        // right-aligned under the log.
        closeBtn.AutoSize = true;
        closeBtn.Padding = new Padding(10, 2, 10, 2);
        closeBtn.Left = outputTb.Right - closeBtn.Width;
    }

    private void ErrorLogView_Load(object? sender, EventArgs e)
    {
        outputTb.Text = ReadErrorLog();
        outputTb.Select(outputTb.TextLength, 0);
        outputTb.ScrollToCaret();
    }

    private void closeBtn_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private static string ReadErrorLog()
    {
        string filePath = Game.Log.FilePath;
        if (!File.Exists(filePath))
        {
            return "The error log is empty.";
        }

        try
        {
            string text = File.ReadAllText(filePath);
            return text.Length == 0 ? "The error log is empty." : text;
        }
        catch (Exception e)
        {
            return e.Message + " Occurred during call to ReadErrorLog().";
        }
    }
}
