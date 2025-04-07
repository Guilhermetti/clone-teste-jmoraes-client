public static class Prompt
{
    public static string? ShowDialog(string text, string caption, string defaultValue = "")
    {
        Form prompt = new()
        {
            Width = 400,
            Height = 160,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = caption,
            StartPosition = FormStartPosition.CenterScreen,
            MinimizeBox = false,
            MaximizeBox = false
        };

        Label textLabel = new() { Left = 20, Top = 20, Text = text, AutoSize = true };
        TextBox textBox = new() { Left = 20, Top = 50, Width = 340, Text = defaultValue };

        Button confirmation = new() { Text = "OK", Left = 200, Width = 75, Top = 80, DialogResult = DialogResult.OK };
        Button cancel = new() { Text = "Cancelar", Left = 285, Width = 75, Top = 80, DialogResult = DialogResult.Cancel };

        prompt.Controls.Add(textLabel);
        prompt.Controls.Add(textBox);
        prompt.Controls.Add(confirmation);
        prompt.Controls.Add(cancel);

        prompt.AcceptButton = confirmation;
        prompt.CancelButton = cancel;

        var result = prompt.ShowDialog();

        return result == DialogResult.OK ? textBox.Text : null;
    }
}