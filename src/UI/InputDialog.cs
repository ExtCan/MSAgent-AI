using System;
using System.Drawing;
using System.Windows.Forms;

namespace MSAgentAI.UI
{
    /// <summary>
    /// Simple input dialog for getting text from the user
    /// </summary>
    public class InputDialog : Form
    {
        private TextBox _inputTextBox;
        private Button _okButton;
        private Button _cancelButton;
        private Label _promptLabel;

        public string InputText => _inputTextBox.Text;

        public InputDialog(string title, string prompt)
        {
            InitializeComponent(title, prompt);
        }

        private void InitializeComponent(string title, string prompt)
        {
            this.Text = title;
            this.Size = new Size(400, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            _promptLabel = new Label
            {
                Text = prompt,
                Location = new Point(15, 15),
                Size = new Size(355, 20)
            };

            _inputTextBox = new TextBox
            {
                Location = new Point(15, 40),
                Size = new Size(355, 23)
            };

            _okButton = new Button
            {
                Text = "OK",
                Location = new Point(210, 75),
                Size = new Size(75, 25),
                DialogResult = DialogResult.OK
            };

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(295, 75),
                Size = new Size(75, 25),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.AddRange(new Control[] { _promptLabel, _inputTextBox, _okButton, _cancelButton });
            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;
        }
    }
}
