using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MSAgentAI.Agent;
using MSAgentAI.AI;

namespace MSAgentAI.UI
{
    /// <summary>
    /// Chat form for interacting with Ollama AI through the agent
    /// </summary>
    public class ChatForm : Form
    {
        private OllamaClient _ollamaClient;
        private AgentManager _agentManager;
        private CancellationTokenSource _cancellationTokenSource;

        private TextBox _chatHistoryTextBox;
        private TextBox _inputTextBox;
        private Button _sendButton;
        private Button _clearButton;
        private Label _statusLabel;

        public ChatForm(OllamaClient ollamaClient, AgentManager agentManager)
        {
            _ollamaClient = ollamaClient;
            _agentManager = agentManager;
            _cancellationTokenSource = new CancellationTokenSource();

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Chat with Agent";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var historyLabel = new Label
            {
                Text = "Conversation:",
                Location = new Point(10, 10),
                Size = new Size(100, 20)
            };

            _chatHistoryTextBox = new TextBox
            {
                Location = new Point(10, 30),
                Size = new Size(465, 280),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White
            };

            var inputLabel = new Label
            {
                Text = "Your message:",
                Location = new Point(10, 320),
                Size = new Size(100, 20)
            };

            _inputTextBox = new TextBox
            {
                Location = new Point(10, 340),
                Size = new Size(365, 23)
            };
            _inputTextBox.KeyDown += OnInputKeyDown;

            _sendButton = new Button
            {
                Text = "Send",
                Location = new Point(385, 339),
                Size = new Size(90, 25)
            };
            _sendButton.Click += OnSendClick;

            _clearButton = new Button
            {
                Text = "Clear History",
                Location = new Point(10, 375),
                Size = new Size(100, 25)
            };
            _clearButton.Click += OnClearClick;

            _statusLabel = new Label
            {
                Text = "Ready",
                Location = new Point(120, 378),
                Size = new Size(355, 20),
                ForeColor = Color.Gray
            };

            this.Controls.AddRange(new Control[]
            {
                historyLabel, _chatHistoryTextBox,
                inputLabel, _inputTextBox, _sendButton,
                _clearButton, _statusLabel
            });

            this.AcceptButton = _sendButton;
        }

        private async void OnSendClick(object sender, EventArgs e)
        {
            await SendMessage();
        }

        private async void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                await SendMessage();
            }
        }

        private async Task SendMessage()
        {
            var message = _inputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(message))
                return;

            _inputTextBox.Clear();
            _sendButton.Enabled = false;
            _statusLabel.Text = "Thinking...";
            _statusLabel.ForeColor = Color.Blue;

            // Add user message to history
            AppendToHistory("You", message);

            try
            {
                var response = await _ollamaClient.ChatAsync(message, _cancellationTokenSource.Token);

                if (!string.IsNullOrEmpty(response))
                {
                    // Add response to history
                    AppendToHistory("Agent", response);

                    // Make the agent speak
                    if (_agentManager?.IsLoaded == true)
                    {
                        _agentManager.Speak(response);
                    }

                    _statusLabel.Text = "Ready";
                    _statusLabel.ForeColor = Color.Gray;
                }
                else
                {
                    _statusLabel.Text = "No response received";
                    _statusLabel.ForeColor = Color.Orange;
                }
            }
            catch (Exception ex)
            {
                _statusLabel.Text = "Error: " + ex.Message;
                _statusLabel.ForeColor = Color.Red;
            }
            finally
            {
                _sendButton.Enabled = true;
                _inputTextBox.Focus();
            }
        }

        private void AppendToHistory(string speaker, string message)
        {
            if (!string.IsNullOrEmpty(_chatHistoryTextBox.Text))
            {
                _chatHistoryTextBox.AppendText(Environment.NewLine);
            }
            _chatHistoryTextBox.AppendText($"{speaker}: {message}");
            _chatHistoryTextBox.ScrollToCaret();
        }

        private void OnClearClick(object sender, EventArgs e)
        {
            _chatHistoryTextBox.Clear();
            _ollamaClient.ClearHistory();
            _statusLabel.Text = "History cleared";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cancellationTokenSource.Cancel();
            base.OnFormClosing(e);
        }
    }
}
