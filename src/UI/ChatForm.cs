using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MSAgentAI.Agent;
using MSAgentAI.AI;
using MSAgentAI.Config;

namespace MSAgentAI.UI
{
    /// <summary>
    /// Chat form for interacting with Ollama AI through the agent
    /// </summary>
    public class ChatForm : Form
    {
        private OllamaClient _ollamaClient;
        private AgentManager _agentManager;
        private AppSettings _settings;
        private CancellationTokenSource _cancellationTokenSource;

        private RichTextBox _chatHistoryTextBox;
        private TextBox _inputTextBox;
        private Button _sendButton;
        private Button _clearButton;
        private Button _attachButton;
        private Button _historyButton;
        private Label _statusLabel;
        private string _attachedFilePath;

        public ChatForm(OllamaClient ollamaClient, AgentManager agentManager, AppSettings settings = null)
        {
            _ollamaClient = ollamaClient;
            _agentManager = agentManager;
            _settings = settings ?? AppSettings.Load();
            _cancellationTokenSource = new CancellationTokenSource();

            InitializeComponent();
            ApplyTheme();
        }

        private void InitializeComponent()
        {
            this.Text = "Chat with Agent";
            this.Size = new Size(600, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(500, 400);

            var historyLabel = new Label
            {
                Text = "Conversation:",
                Location = new Point(10, 10),
                Size = new Size(100, 20)
            };

            _chatHistoryTextBox = new RichTextBox
            {
                Location = new Point(10, 30),
                Size = new Size(565, 370),
                ReadOnly = true,
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            var inputLabel = new Label
            {
                Text = "Your message:",
                Location = new Point(10, 410),
                Size = new Size(100, 20),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            _inputTextBox = new TextBox
            {
                Location = new Point(10, 430),
                Size = new Size(365, 23),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            _inputTextBox.KeyDown += OnInputKeyDown;

            _sendButton = new Button
            {
                Text = "Send",
                Location = new Point(385, 429),
                Size = new Size(90, 25),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            _sendButton.Click += OnSendClick;

            _attachButton = new Button
            {
                Text = "📎",
                Location = new Point(480, 429),
                Size = new Size(30, 25),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            _attachButton.Click += OnAttachClick;

            _clearButton = new Button
            {
                Text = "Clear",
                Location = new Point(10, 465),
                Size = new Size(80, 25),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            _clearButton.Click += OnClearClick;

            var promptButton = new Button
            {
                Text = "Prompt",
                Location = new Point(100, 465),
                Size = new Size(80, 25),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            promptButton.Click += OnPromptClick;

            _historyButton = new Button
            {
                Text = "History",
                Location = new Point(190, 465),
                Size = new Size(80, 25),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            _historyButton.Click += OnHistoryClick;

            var exportButton = new Button
            {
                Text = "Export",
                Location = new Point(280, 465),
                Size = new Size(80, 25),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            exportButton.Click += OnExportClick;

            _statusLabel = new Label
            {
                Text = "Ready",
                Location = new Point(370, 468),
                Size = new Size(205, 20),
                ForeColor = Color.Gray,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            this.Controls.AddRange(new Control[]
            {
                historyLabel, _chatHistoryTextBox,
                inputLabel, _inputTextBox, _sendButton, _attachButton,
                _clearButton, promptButton, _historyButton, exportButton, _statusLabel
            });

            this.AcceptButton = _sendButton;
        }

        private void ApplyTheme()
        {
            if (_settings == null) return;
            
            var colors = AppSettings.GetThemeColors(_settings.UITheme);
            this.BackColor = colors.Background;
            this.ForeColor = colors.Foreground;
            
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.BackColor = colors.ButtonBackground;
                    btn.ForeColor = colors.ButtonForeground;
                    btn.FlatStyle = FlatStyle.Flat;
                }
                else if (ctrl is TextBox txt)
                {
                    txt.BackColor = colors.InputBackground;
                    txt.ForeColor = colors.InputForeground;
                }
                else if (ctrl is RichTextBox rtb)
                {
                    rtb.BackColor = colors.InputBackground;
                    rtb.ForeColor = colors.InputForeground;
                }
                else if (ctrl is Label lbl)
                {
                    lbl.ForeColor = colors.Foreground;
                }
            }
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
            AppendToHistory("You", message, Color.Blue);
            
            // Add attachment info if present
            string fullMessage = message;
            if (!string.IsNullOrEmpty(_attachedFilePath))
            {
                fullMessage = $"[Attached: {Path.GetFileName(_attachedFilePath)}]\n{message}";
                _attachedFilePath = null;
            }

            try
            {
                var response = await _ollamaClient.ChatAsync(fullMessage, _cancellationTokenSource.Token);

                if (!string.IsNullOrEmpty(response))
                {
                    // Add response to history
                    AppendToHistory("Agent", response, Color.DarkGreen);

                    // Make the agent speak with animations and emphasis support
                    if (_agentManager?.IsLoaded == true)
                    {
                        SpeakWithAnimations(response);
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

        /// <summary>
        /// Speaks text with animation support. Extracts &&Animation triggers from text,
        /// plays them, then speaks the processed text.
        /// </summary>
        private void SpeakWithAnimations(string text, string defaultAnimation = null)
        {
            if (_agentManager?.IsLoaded != true || string.IsNullOrEmpty(text))
                return;
                
            // Extract animation triggers (&&AnimationName)
            var (cleanText, animations) = AppSettings.ExtractAnimationTriggers(text);
            
            // Process text for ## name replacement and /emp/ emphasis
            cleanText = _settings.ProcessText(cleanText);
            
            // Play animations
            if (animations.Count > 0)
            {
                foreach (var anim in animations)
                {
                    _agentManager.PlayAnimation(anim);
                }
            }
            else if (!string.IsNullOrEmpty(defaultAnimation))
            {
                _agentManager.PlayAnimation(defaultAnimation);
            }
            
            // Speak the processed text - check if truncation is enabled
            if (_settings.TruncateSpeech)
            {
                var sentences = AppSettings.SplitIntoSentences(cleanText);
                _agentManager.SpeakSentences(sentences);
            }
            else
            {
                _agentManager.Speak(cleanText);
            }
        }

        private void AppendToHistory(string speaker, string message, Color color)
        {
            if (_chatHistoryTextBox.TextLength > 0)
            {
                _chatHistoryTextBox.AppendText(Environment.NewLine);
            }
            
            int start = _chatHistoryTextBox.TextLength;
            _chatHistoryTextBox.AppendText($"{speaker}: ");
            _chatHistoryTextBox.Select(start, _chatHistoryTextBox.TextLength - start);
            _chatHistoryTextBox.SelectionColor = color;
            _chatHistoryTextBox.SelectionFont = new Font(_chatHistoryTextBox.Font, FontStyle.Bold);
            
            start = _chatHistoryTextBox.TextLength;
            _chatHistoryTextBox.AppendText(message);
            _chatHistoryTextBox.Select(start, _chatHistoryTextBox.TextLength - start);
            _chatHistoryTextBox.SelectionColor = _chatHistoryTextBox.ForeColor;
            _chatHistoryTextBox.SelectionFont = new Font(_chatHistoryTextBox.Font, FontStyle.Regular);
            
            _chatHistoryTextBox.SelectionStart = _chatHistoryTextBox.TextLength;
            _chatHistoryTextBox.ScrollToCaret();
        }

        private void OnAttachClick(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog
            {
                Title = "Attach File",
                Filter = "All Files|*.*|Text Files|*.txt|Images|*.png;*.jpg;*.jpeg;*.gif|Documents|*.pdf;*.doc;*.docx"
            })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _attachedFilePath = ofd.FileName;
                    _statusLabel.Text = $"Attached: {Path.GetFileName(_attachedFilePath)}";
                    _statusLabel.ForeColor = Color.Purple;
                }
            }
        }

        private void OnHistoryClick(object sender, EventArgs e)
        {
            var historyPath = GetChatHistoryPath();
            if (Directory.Exists(historyPath))
            {
                using (var historyForm = new ChatHistoryViewerForm(historyPath))
                {
                    historyForm.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("No chat history found.", "History", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnExportClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_chatHistoryTextBox.Text))
            {
                MessageBox.Show("No conversation to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var sfd = new SaveFileDialog
            {
                Title = "Export Chat",
                Filter = "Text Files|*.txt|Rich Text|*.rtf",
                FileName = $"chat_{DateTime.Now:yyyyMMdd_HHmmss}"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (sfd.FilterIndex == 2) // RTF
                            _chatHistoryTextBox.SaveFile(sfd.FileName, RichTextBoxStreamType.RichText);
                        else
                            File.WriteAllText(sfd.FileName, _chatHistoryTextBox.Text);
                        
                        _statusLabel.Text = "Exported successfully";
                        _statusLabel.ForeColor = Color.Green;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private string GetChatHistoryPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MSAgentAI",
                "ChatHistory"
            );
        }

        private void OnClearClick(object sender, EventArgs e)
        {
            _chatHistoryTextBox.Clear();
            _ollamaClient.ClearHistory();
            _statusLabel.Text = "History cleared";
        }

        private async void OnPromptClick(object sender, EventArgs e)
        {
            using (var dialog = new InputDialog("Send Prompt", "Enter a custom prompt for Ollama AI (no history, direct response):"))
            {
                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dialog.InputText))
                {
                    _sendButton.Enabled = false;
                    _statusLabel.Text = "Processing prompt...";
                    _statusLabel.ForeColor = Color.Blue;

                    try
                    {
                        var response = await _ollamaClient.GenerateRandomDialogAsync(dialog.InputText, _cancellationTokenSource.Token);

                        if (!string.IsNullOrEmpty(response))
                        {
                            AppendToHistory("Prompt", dialog.InputText, Color.Purple);
                            AppendToHistory("Agent", response, Color.DarkGreen);

                            if (_agentManager?.IsLoaded == true)
                            {
                                SpeakWithAnimations(response);
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
                    }
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Save chat history before closing
            SaveChatHistory();
            _cancellationTokenSource.Cancel();
            base.OnFormClosing(e);
        }

        private void SaveChatHistory()
        {
            if (string.IsNullOrEmpty(_chatHistoryTextBox.Text))
                return;

            try
            {
                var historyPath = GetChatHistoryPath();
                if (!Directory.Exists(historyPath))
                    Directory.CreateDirectory(historyPath);

                var filePath = Path.Combine(historyPath, $"chat_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                File.WriteAllText(filePath, _chatHistoryTextBox.Text);
            }
            catch { }
        }
    }

    /// <summary>
    /// Chat history viewer form
    /// </summary>
    public class ChatHistoryViewerForm : Form
    {
        public ChatHistoryViewerForm(string historyPath)
        {
            this.Text = "Chat History";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            var listBox = new ListBox
            {
                Location = new Point(10, 10),
                Size = new Size(200, 440),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };

            var contentBox = new RichTextBox
            {
                Location = new Point(220, 10),
                Size = new Size(355, 440),
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Load history files
            var files = Directory.GetFiles(historyPath, "*.txt").OrderByDescending(f => f).ToArray();
            foreach (var file in files)
            {
                listBox.Items.Add(Path.GetFileName(file));
            }

            listBox.SelectedIndexChanged += (s, e) =>
            {
                if (listBox.SelectedIndex >= 0)
                {
                    var selectedFile = Path.Combine(historyPath, listBox.SelectedItem.ToString());
                    contentBox.Text = File.ReadAllText(selectedFile);
                }
            };

            this.Controls.AddRange(new Control[] { listBox, contentBox });
        }
    }
}
