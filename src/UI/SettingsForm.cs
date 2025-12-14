using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using MSAgentAI.Agent;
using MSAgentAI.AI;
using MSAgentAI.Config;
using MSAgentAI.Voice;

namespace MSAgentAI.UI
{
    /// <summary>
    /// Settings form for configuring the application
    /// </summary>
    public class SettingsForm : Form
    {
        // Constants
        private const double VolumeScaleFactor = 655.35; // Converts 0-100 to 0-65535 for SAPI4

        private AppSettings _settings;
        private AgentManager _agentManager;
        private Sapi4Manager _voiceManager;
        private OllamaClient _ollamaClient;

        // Tabs
        private TabControl _tabControl;
        private TabPage _agentTab;
        private TabPage _voiceTab;
        private TabPage _pronunciationTab;
        private TabPage _ollamaTab;
        private TabPage _pipelineTab;
        private TabPage _linesTab;

        // Agent controls
        private TextBox _characterPathTextBox;
        private Button _browsePathButton;
        private ListBox _characterListBox;
        private Button _previewButton;
        private Button _selectButton;
        private Label _characterInfoLabel;
        private ListBox _animationsListBox;
        private Button _playAnimationButton;

        // Name system controls
        private TextBox _userNameTextBox;
        private TextBox _userNamePronunciationTextBox;
        private Button _testNameButton;

        // Voice controls
        private ComboBox _voiceComboBox;
        private TrackBar _speedTrackBar;
        private TrackBar _pitchTrackBar;
        private TrackBar _volumeTrackBar;
        private Label _speedValueLabel;
        private Label _pitchValueLabel;
        private Label _volumeValueLabel;
        private Button _testVoiceButton;
        
        // Call Mode / Speech Recognition controls
        private ComboBox _microphoneComboBox;
        private TrackBar _confidenceTrackBar;
        private Label _confidenceValueLabel;
        private TrackBar _silenceTrackBar;
        private Label _silenceValueLabel;

        // Ollama controls
        private TextBox _ollamaUrlTextBox;
        private ComboBox _ollamaModelComboBox;
        private Button _refreshModelsButton;
        private Button _testConnectionButton;
        private TextBox _personalityTextBox;
        private ComboBox _personalityPresetComboBox;
        private Button _applyPresetButton;
        private Button _savePresetButton;
        private CheckBox _enableChatCheckBox;
        private CheckBox _enableRandomDialogCheckBox;
        private NumericUpDown _randomChanceNumeric;
        private CheckBox _enablePrewrittenIdleCheckBox;
        private NumericUpDown _prewrittenIdleChanceNumeric;

        // Theme controls
        private ComboBox _themeComboBox;
        
        // Agent size control
        private TrackBar _agentSizeTrackBar;
        private Label _agentSizeValueLabel;

        // Lines controls
        private TabControl _linesTabControl;
        private Dictionary<string, TextBox> _linesTextBoxes;

        // Pronunciation dictionary controls
        private DataGridView _pronunciationGrid;
        private Button _exportDictionaryButton;
        private Button _importDictionaryButton;

        // Pipeline controls
        private ComboBox _pipelineProtocolComboBox;
        private TextBox _pipelineIpTextBox;
        private NumericUpDown _pipelinePortNumeric;
        private TextBox _pipelineNameTextBox;
        private Label _pipelineStatusLabel;

        // Dialog buttons
        private Button _okButton;
        private Button _cancelButton;
        private Button _applyButton;

        public SettingsForm(AppSettings settings, AgentManager agentManager, Sapi4Manager voiceManager, OllamaClient ollamaClient)
        {
            _settings = settings;
            _agentManager = agentManager;
            _voiceManager = voiceManager;
            _ollamaClient = ollamaClient;

            InitializeComponent();
            LoadSettings();
            ApplyTheme(); // Apply theme to settings form
        }
        
        /// <summary>
        /// Applies the current UI theme to this form and all child controls
        /// </summary>
        private void ApplyTheme()
        {
            var theme = AppSettings.GetThemeColors(_settings.UITheme);
            ApplyThemeToControl(this, theme);
        }
        
        /// <summary>
        /// Recursively applies theme colors to a control and its children
        /// </summary>
        public static void ApplyThemeToControl(Control control, ThemeColors theme)
        {
            control.BackColor = theme.Background;
            control.ForeColor = theme.Foreground;
            
            if (control is Button btn)
            {
                btn.BackColor = theme.ButtonBackground;
                btn.ForeColor = theme.ButtonForeground;
                btn.FlatStyle = theme.Background == System.Drawing.SystemColors.Control ? FlatStyle.Standard : FlatStyle.Flat;
            }
            else if (control is TextBox || control is RichTextBox)
            {
                control.BackColor = theme.InputBackground;
                control.ForeColor = theme.InputForeground;
            }
            else if (control is ListBox || control is ComboBox)
            {
                control.BackColor = theme.InputBackground;
                control.ForeColor = theme.InputForeground;
            }
            else if (control is TabControl tabControl)
            {
                // TabControl needs special handling
                foreach (TabPage page in tabControl.TabPages)
                {
                    page.BackColor = theme.Background;
                    page.ForeColor = theme.Foreground;
                    ApplyThemeToControl(page, theme);
                }
            }
            
            // Recurse into child controls
            foreach (Control child in control.Controls)
            {
                ApplyThemeToControl(child, theme);
            }
        }

        private void InitializeComponent()
        {
            this.Text = "MSAgent AI Settings";
            this.Size = new Size(650, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create main tab control
            _tabControl = new TabControl
            {
                Location = new Point(10, 10),
                Size = new Size(615, 450)
            };

            // Create tabs
            CreateAgentTab();
            CreateVoiceTab();
            CreatePronunciationTab();
            CreateOllamaTab();
            CreatePipelineTab();
            CreateLinesTab();

            _tabControl.TabPages.AddRange(new TabPage[] { _agentTab, _voiceTab, _pronunciationTab, _ollamaTab, _pipelineTab, _linesTab });

            // Dialog buttons
            _okButton = new Button
            {
                Text = "OK",
                Location = new Point(365, 470),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK
            };
            _okButton.Click += OnOkClick;

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(455, 470),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };

            _applyButton = new Button
            {
                Text = "Apply",
                Location = new Point(545, 470),
                Size = new Size(80, 30)
            };
            _applyButton.Click += OnApplyClick;

            this.Controls.AddRange(new Control[] { _tabControl, _okButton, _cancelButton, _applyButton });
            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;
        }

        private void CreateAgentTab()
        {
            _agentTab = new TabPage("Agent");

            var pathLabel = new Label
            {
                Text = "Character Folder:",
                Location = new Point(15, 20),
                Size = new Size(100, 20)
            };

            _characterPathTextBox = new TextBox
            {
                Location = new Point(120, 17),
                Size = new Size(280, 23)
            };

            _browsePathButton = new Button
            {
                Text = "...",
                Location = new Point(405, 16),
                Size = new Size(30, 25)
            };
            _browsePathButton.Click += OnBrowsePathClick;

            var refreshButton = new Button
            {
                Text = "Refresh",
                Location = new Point(440, 16),
                Size = new Size(60, 25)
            };
            refreshButton.Click += OnRefreshCharactersClick;

            // Name system
            var nameLabel = new Label
            {
                Text = "Your Name:",
                Location = new Point(15, 50),
                Size = new Size(100, 20)
            };

            _userNameTextBox = new TextBox
            {
                Location = new Point(120, 47),
                Size = new Size(150, 23)
            };

            var pronunciationLabel = new Label
            {
                Text = "Pronunciation:",
                Location = new Point(280, 50),
                Size = new Size(85, 20)
            };

            _userNamePronunciationTextBox = new TextBox
            {
                Location = new Point(365, 47),
                Size = new Size(90, 23)
            };

            _testNameButton = new Button
            {
                Text = "Test",
                Location = new Point(460, 46),
                Size = new Size(50, 25)
            };
            _testNameButton.Click += OnTestNameClick;

            var nameHintLabel = new Label
            {
                Text = "Use ## in lines to insert your name. Use &&Animation for animations.",
                Location = new Point(120, 72),
                Size = new Size(400, 15),
                ForeColor = Color.Gray,
                Font = new Font(this.Font.FontFamily, 7.5f)
            };

            var listLabel = new Label
            {
                Text = "Available Characters:",
                Location = new Point(15, 92),
                Size = new Size(200, 20)
            };

            _characterListBox = new ListBox
            {
                Location = new Point(15, 112),
                Size = new Size(200, 200)
            };
            _characterListBox.SelectedIndexChanged += OnCharacterSelectionChanged;

            _previewButton = new Button
            {
                Text = "Preview",
                Location = new Point(15, 318),
                Size = new Size(80, 30),
                Enabled = false
            };
            _previewButton.Click += OnPreviewClick;

            _selectButton = new Button
            {
                Text = "Use This Character",
                Location = new Point(100, 318),
                Size = new Size(115, 30),
                Enabled = false
            };
            _selectButton.Click += OnSelectCharacterClick;

            _characterInfoLabel = new Label
            {
                Text = "Select a character to see information",
                Location = new Point(225, 112),
                Size = new Size(170, 80),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Animations list
            var animLabel = new Label
            {
                Text = "Animations:",
                Location = new Point(225, 197),
                Size = new Size(100, 20)
            };

            _animationsListBox = new ListBox
            {
                Location = new Point(225, 217),
                Size = new Size(170, 100)
            };

            _playAnimationButton = new Button
            {
                Text = "Play Animation",
                Location = new Point(225, 318),
                Size = new Size(100, 30),
                Enabled = false
            };
            _playAnimationButton.Click += OnPlayAnimationClick;

            // Emphasis hint
            var empHintLabel = new Label
            {
                Text = "TIP: Use /emp/ in text for emphasis",
                Location = new Point(405, 112),
                Size = new Size(190, 40),
                ForeColor = Color.Gray,
                Font = new Font(this.Font.FontFamily, 7.5f)
            };
            
            // Agent size control
            var agentSizeLabel = new Label
            {
                Text = "Agent Size:",
                Location = new Point(405, 160),
                Size = new Size(70, 20)
            };
            
            _agentSizeTrackBar = new TrackBar
            {
                Location = new Point(405, 180),
                Size = new Size(150, 45),
                Minimum = 25,
                Maximum = 200,
                Value = 100,
                TickFrequency = 25
            };
            _agentSizeTrackBar.ValueChanged += (s, e) => _agentSizeValueLabel.Text = _agentSizeTrackBar.Value.ToString() + "%";
            
            _agentSizeValueLabel = new Label
            {
                Text = "100%",
                Location = new Point(560, 160),
                Size = new Size(40, 20)
            };

            _agentTab.Controls.AddRange(new Control[]
            {
                pathLabel, _characterPathTextBox, _browsePathButton, refreshButton,
                nameLabel, _userNameTextBox, pronunciationLabel, _userNamePronunciationTextBox, _testNameButton, nameHintLabel,
                listLabel, _characterListBox, _previewButton, _selectButton, _characterInfoLabel,
                animLabel, _animationsListBox, _playAnimationButton, empHintLabel,
                agentSizeLabel, _agentSizeTrackBar, _agentSizeValueLabel
            });
        }

        private void CreateVoiceTab()
        {
            _voiceTab = new TabPage("Voice (SAPI4)");

            var voiceLabel = new Label
            {
                Text = "Voice:",
                Location = new Point(15, 20),
                Size = new Size(80, 20)
            };

            _voiceComboBox = new ComboBox
            {
                Location = new Point(100, 17),
                Size = new Size(300, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Speed
            var speedLabel = new Label
            {
                Text = "Speed:",
                Location = new Point(15, 55),
                Size = new Size(80, 20)
            };

            _speedTrackBar = new TrackBar
            {
                Location = new Point(100, 45),
                Size = new Size(400, 35),
                Minimum = 50,
                Maximum = 350,
                Value = 150,
                TickFrequency = 25
            };
            _speedTrackBar.ValueChanged += (s, e) => _speedValueLabel.Text = _speedTrackBar.Value.ToString();

            _speedValueLabel = new Label
            {
                Text = "150",
                Location = new Point(510, 55),
                Size = new Size(50, 20)
            };

            // Pitch
            var pitchLabel = new Label
            {
                Text = "Pitch:",
                Location = new Point(15, 90),
                Size = new Size(80, 20)
            };

            _pitchTrackBar = new TrackBar
            {
                Location = new Point(100, 80),
                Size = new Size(400, 35),
                Minimum = 50,
                Maximum = 400,
                Value = 100,
                TickFrequency = 25
            };
            _pitchTrackBar.ValueChanged += (s, e) => _pitchValueLabel.Text = _pitchTrackBar.Value.ToString();

            _pitchValueLabel = new Label
            {
                Text = "100",
                Location = new Point(510, 90),
                Size = new Size(50, 20)
            };

            // Volume
            var volumeLabel = new Label
            {
                Text = "Volume:",
                Location = new Point(15, 125),
                Size = new Size(80, 20)
            };

            _volumeTrackBar = new TrackBar
            {
                Location = new Point(100, 115),
                Size = new Size(400, 35),
                Minimum = 0,
                Maximum = 100,
                Value = 100,
                TickFrequency = 10
            };
            _volumeTrackBar.ValueChanged += (s, e) => _volumeValueLabel.Text = _volumeTrackBar.Value.ToString() + "%";

            _volumeValueLabel = new Label
            {
                Text = "100%",
                Location = new Point(510, 125),
                Size = new Size(50, 20)
            };

            _testVoiceButton = new Button
            {
                Text = "Test Voice",
                Location = new Point(15, 155),
                Size = new Size(100, 30)
            };
            _testVoiceButton.Click += OnTestVoiceClick;
            
            // Call Mode / Speech Recognition Section
            var callModeLabel = new Label
            {
                Text = "═══ Call Mode (Voice Chat) Settings ═══",
                Location = new Point(15, 200),
                Size = new Size(400, 20),
                Font = new Font(this.Font, FontStyle.Bold)
            };
            
            // Microphone selection
            var micLabel = new Label
            {
                Text = "Microphone:",
                Location = new Point(15, 230),
                Size = new Size(80, 20)
            };

            _microphoneComboBox = new ComboBox
            {
                Location = new Point(100, 227),
                Size = new Size(300, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _microphoneComboBox.Items.Add("(Default Device)");
            _microphoneComboBox.SelectedIndex = 0;
            
            // Confidence threshold
            var confidenceLabel = new Label
            {
                Text = "Confidence:",
                Location = new Point(15, 265),
                Size = new Size(80, 20)
            };

            _confidenceTrackBar = new TrackBar
            {
                Location = new Point(100, 255),
                Size = new Size(400, 35),
                Minimum = 5,
                Maximum = 100,
                Value = 20,
                TickFrequency = 10
            };
            _confidenceTrackBar.ValueChanged += (s, e) => _confidenceValueLabel.Text = _confidenceTrackBar.Value.ToString() + "%";

            _confidenceValueLabel = new Label
            {
                Text = "20%",
                Location = new Point(510, 265),
                Size = new Size(50, 20)
            };
            
            var confidenceHint = new Label
            {
                Text = "Lower = more sensitive but may pick up noise. Higher = more accurate but may miss speech.",
                Location = new Point(100, 290),
                Size = new Size(500, 15),
                ForeColor = Color.Gray,
                Font = new Font(this.Font.FontFamily, 7.5f)
            };
            
            // Silence detection
            var silenceLabel = new Label
            {
                Text = "Silence (ms):",
                Location = new Point(15, 315),
                Size = new Size(80, 20)
            };

            _silenceTrackBar = new TrackBar
            {
                Location = new Point(100, 305),
                Size = new Size(400, 35),
                Minimum = 500,
                Maximum = 5000,
                Value = 1500,
                TickFrequency = 500
            };
            _silenceTrackBar.ValueChanged += (s, e) => _silenceValueLabel.Text = _silenceTrackBar.Value.ToString() + "ms";

            _silenceValueLabel = new Label
            {
                Text = "1500ms",
                Location = new Point(510, 315),
                Size = new Size(60, 20)
            };
            
            var silenceHint = new Label
            {
                Text = "How long to wait after you stop speaking before the AI responds.",
                Location = new Point(100, 340),
                Size = new Size(500, 15),
                ForeColor = Color.Gray,
                Font = new Font(this.Font.FontFamily, 7.5f)
            };

            _voiceTab.Controls.AddRange(new Control[]
            {
                voiceLabel, _voiceComboBox,
                speedLabel, _speedTrackBar, _speedValueLabel,
                pitchLabel, _pitchTrackBar, _pitchValueLabel,
                volumeLabel, _volumeTrackBar, _volumeValueLabel,
                _testVoiceButton,
                callModeLabel,
                micLabel, _microphoneComboBox,
                confidenceLabel, _confidenceTrackBar, _confidenceValueLabel, confidenceHint,
                silenceLabel, _silenceTrackBar, _silenceValueLabel, silenceHint
            });
        }

        private void CreatePronunciationTab()
        {
            _pronunciationTab = new TabPage("Pronunciation");

            var infoLabel = new Label
            {
                Text = "Pronunciation Dictionary - Words will be pronounced using the \\map\\ SAPI4 command.\nWhen the AI or any text contains a matching word, it will be pronounced as specified.",
                Location = new Point(15, 15),
                Size = new Size(580, 35)
            };

            var gridLabel = new Label
            {
                Text = "Word → Pronunciation mappings:",
                Location = new Point(15, 55),
                Size = new Size(250, 20)
            };

            _pronunciationGrid = new DataGridView
            {
                Location = new Point(15, 80),
                Size = new Size(580, 280),
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            // Add columns
            var wordColumn = new DataGridViewTextBoxColumn
            {
                Name = "Word",
                HeaderText = "Word",
                FillWeight = 50
            };
            var pronunciationColumn = new DataGridViewTextBoxColumn
            {
                Name = "Pronunciation",
                HeaderText = "Pronunciation",
                FillWeight = 50
            };
            _pronunciationGrid.Columns.Add(wordColumn);
            _pronunciationGrid.Columns.Add(pronunciationColumn);

            _exportDictionaryButton = new Button
            {
                Text = "Export XML...",
                Location = new Point(15, 370),
                Size = new Size(100, 30)
            };
            _exportDictionaryButton.Click += OnExportDictionaryClick;

            _importDictionaryButton = new Button
            {
                Text = "Import XML...",
                Location = new Point(125, 370),
                Size = new Size(100, 30)
            };
            _importDictionaryButton.Click += OnImportDictionaryClick;

            var hintLabel = new Label
            {
                Text = "Export/Import allows sharing pronunciation dictionaries as XML files.",
                Location = new Point(235, 377),
                Size = new Size(360, 20),
                ForeColor = Color.Gray,
                Font = new Font(this.Font.FontFamily, 7.5f)
            };

            _pronunciationTab.Controls.AddRange(new Control[]
            {
                infoLabel, gridLabel, _pronunciationGrid, 
                _exportDictionaryButton, _importDictionaryButton, hintLabel
            });
        }

        private void OnExportDictionaryClick(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
                dialog.DefaultExt = "xml";
                dialog.FileName = "pronunciation_dictionary.xml";
                dialog.Title = "Export Pronunciation Dictionary";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var entries = new List<KeyValuePair<string, string>>();
                        foreach (DataGridViewRow row in _pronunciationGrid.Rows)
                        {
                            if (row.IsNewRow) continue;
                            var word = row.Cells["Word"].Value?.ToString();
                            var pronunciation = row.Cells["Pronunciation"].Value?.ToString();
                            if (!string.IsNullOrEmpty(word) && !string.IsNullOrEmpty(pronunciation))
                            {
                                entries.Add(new KeyValuePair<string, string>(word, pronunciation));
                            }
                        }

                        var xml = new XDocument(
                            new XDeclaration("1.0", "utf-8", "yes"),
                            new XElement("PronunciationDictionary",
                                entries.Select(e => new XElement("Entry",
                                    new XElement("Word", e.Key),
                                    new XElement("Pronunciation", e.Value)
                                ))
                            )
                        );

                        xml.Save(dialog.FileName);
                        MessageBox.Show($"Dictionary exported successfully!\n{entries.Count} entries saved.", 
                            "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to export dictionary: {ex.Message}", 
                            "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void OnImportDictionaryClick(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
                dialog.Title = "Import Pronunciation Dictionary";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var xml = XDocument.Load(dialog.FileName);
                        var entries = xml.Root?.Elements("Entry")
                            .Select(e => new
                            {
                                Word = e.Element("Word")?.Value,
                                Pronunciation = e.Element("Pronunciation")?.Value
                            })
                            .Where(e => !string.IsNullOrEmpty(e.Word) && !string.IsNullOrEmpty(e.Pronunciation))
                            .ToList();

                        if (entries == null || entries.Count == 0)
                        {
                            MessageBox.Show("No valid entries found in the XML file.", 
                                "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Ask user if they want to replace or merge
                        var result = MessageBox.Show(
                            $"Found {entries.Count} entries.\n\nReplace existing dictionary?\n(Yes = Replace, No = Merge)",
                            "Import Dictionary", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                        if (result == DialogResult.Cancel) return;

                        if (result == DialogResult.Yes)
                        {
                            _pronunciationGrid.Rows.Clear();
                        }

                        foreach (var entry in entries)
                        {
                            // Check if word already exists (for merge)
                            bool exists = false;
                            foreach (DataGridViewRow row in _pronunciationGrid.Rows)
                            {
                                if (row.IsNewRow) continue;
                                if (row.Cells["Word"].Value?.ToString()?.Equals(entry.Word, StringComparison.OrdinalIgnoreCase) == true)
                                {
                                    row.Cells["Pronunciation"].Value = entry.Pronunciation;
                                    exists = true;
                                    break;
                                }
                            }
                            if (!exists)
                            {
                                _pronunciationGrid.Rows.Add(entry.Word, entry.Pronunciation);
                            }
                        }

                        MessageBox.Show($"Dictionary imported successfully!\n{entries.Count} entries loaded.", 
                            "Import Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to import dictionary: {ex.Message}", 
                            "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void CreateOllamaTab()
        {
            _ollamaTab = new TabPage("Ollama AI");

            var urlLabel = new Label
            {
                Text = "Ollama URL:",
                Location = new Point(15, 20),
                Size = new Size(100, 20)
            };

            _ollamaUrlTextBox = new TextBox
            {
                Location = new Point(120, 17),
                Size = new Size(300, 23)
            };

            _testConnectionButton = new Button
            {
                Text = "Test",
                Location = new Point(430, 16),
                Size = new Size(60, 25)
            };
            _testConnectionButton.Click += OnTestConnectionClick;

            var modelLabel = new Label
            {
                Text = "Model:",
                Location = new Point(15, 55),
                Size = new Size(100, 20)
            };

            _ollamaModelComboBox = new ComboBox
            {
                Location = new Point(120, 52),
                Size = new Size(300, 23),
                DropDownStyle = ComboBoxStyle.DropDown
            };

            _refreshModelsButton = new Button
            {
                Text = "Refresh",
                Location = new Point(430, 51),
                Size = new Size(60, 25)
            };
            _refreshModelsButton.Click += OnRefreshModelsClick;

            var presetLabel = new Label
            {
                Text = "Preset:",
                Location = new Point(15, 90),
                Size = new Size(100, 20)
            };

            _personalityPresetComboBox = new ComboBox
            {
                Location = new Point(120, 87),
                Size = new Size(200, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            // Add personality presets (built-in + custom)
            _personalityPresetComboBox.Items.Add("(Custom)");
            foreach (var preset in AppSettings.PersonalityPresets.Keys)
            {
                _personalityPresetComboBox.Items.Add(preset);
            }
            // Add custom presets from settings
            foreach (var preset in _settings.CustomPersonalityPresets.Keys)
            {
                _personalityPresetComboBox.Items.Add("[Custom] " + preset);
            }
            _personalityPresetComboBox.SelectedIndex = 0;

            _applyPresetButton = new Button
            {
                Text = "Apply",
                Location = new Point(330, 86),
                Size = new Size(60, 25)
            };
            _applyPresetButton.Click += OnApplyPresetClick;

            _savePresetButton = new Button
            {
                Text = "Save As...",
                Location = new Point(395, 86),
                Size = new Size(70, 25)
            };
            _savePresetButton.Click += OnSavePresetClick;

            var themeLabel = new Label
            {
                Text = "UI Theme:",
                Location = new Point(475, 90),
                Size = new Size(60, 20)
            };

            _themeComboBox = new ComboBox
            {
                Location = new Point(535, 87),
                Size = new Size(55, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _themeComboBox.Items.AddRange(new object[] { "Default", "Dark", "Deep Blue", "Deep Purple", "Wine Red", "Deep Green", "Pure Black" });
            _themeComboBox.SelectedIndex = 0;

            var personalityLabel = new Label
            {
                Text = "Personality Prompt:",
                Location = new Point(15, 120),
                Size = new Size(200, 20)
            };

            _personalityTextBox = new TextBox
            {
                Location = new Point(15, 140),
                Size = new Size(580, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            _enableChatCheckBox = new CheckBox
            {
                Text = "Enable Ollama Chat",
                Location = new Point(15, 250),
                Size = new Size(200, 25)
            };

            _enableRandomDialogCheckBox = new CheckBox
            {
                Text = "Enable Random Dialog (uses Ollama)",
                Location = new Point(15, 280),
                Size = new Size(250, 25)
            };

            var chanceLabel = new Label
            {
                Text = "Random Chance (1 in N per second):",
                Location = new Point(15, 315),
                Size = new Size(220, 20)
            };

            _randomChanceNumeric = new NumericUpDown
            {
                Location = new Point(240, 312),
                Size = new Size(100, 23),
                Minimum = 100,
                Maximum = 100000,
                Value = 9000
            };

            _enablePrewrittenIdleCheckBox = new CheckBox
            {
                Text = "Enable Pre-written Idle Lines",
                Location = new Point(15, 345),
                Size = new Size(220, 25)
            };

            var prewrittenChanceLabel = new Label
            {
                Text = "Idle Chance (1 in N):",
                Location = new Point(240, 348),
                Size = new Size(130, 20)
            };

            _prewrittenIdleChanceNumeric = new NumericUpDown
            {
                Location = new Point(370, 345),
                Size = new Size(80, 23),
                Minimum = 1,
                Maximum = 1000,
                Value = 30
            };

            var promptsLabel = new Label
            {
                Text = "Edit random dialog prompts in the 'Lines' tab. AI uses /emp/ for emphasis and &&Animation for animations.",
                Location = new Point(15, 380),
                Size = new Size(580, 20),
                ForeColor = Color.Gray
            };

            _ollamaTab.Controls.AddRange(new Control[]
            {
                urlLabel, _ollamaUrlTextBox, _testConnectionButton,
                modelLabel, _ollamaModelComboBox, _refreshModelsButton,
                presetLabel, _personalityPresetComboBox, _applyPresetButton, _savePresetButton,
                themeLabel, _themeComboBox,
                personalityLabel, _personalityTextBox,
                _enableChatCheckBox, _enableRandomDialogCheckBox,
                chanceLabel, _randomChanceNumeric,
                _enablePrewrittenIdleCheckBox, prewrittenChanceLabel, _prewrittenIdleChanceNumeric,
                promptsLabel
            });
        }

        private void CreatePipelineTab()
        {
            _pipelineTab = new TabPage("Pipeline");

            var descLabel = new Label
            {
                Text = "Configure the communication pipeline for external applications.\nChoose between Named Pipe (local only) or TCP Socket (network).",
                Location = new Point(15, 15),
                Size = new Size(570, 40),
                AutoSize = false
            };

            var protocolLabel = new Label
            {
                Text = "Protocol:",
                Location = new Point(15, 65),
                Size = new Size(100, 20)
            };

            _pipelineProtocolComboBox = new ComboBox
            {
                Location = new Point(120, 62),
                Size = new Size(150, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _pipelineProtocolComboBox.Items.AddRange(new object[] { "NamedPipe", "TCP" });
            _pipelineProtocolComboBox.SelectedIndexChanged += OnPipelineProtocolChanged;

            // Named Pipe settings
            var pipeNameLabel = new Label
            {
                Text = "Pipe Name:",
                Location = new Point(15, 100),
                Size = new Size(100, 20)
            };

            _pipelineNameTextBox = new TextBox
            {
                Location = new Point(120, 97),
                Size = new Size(300, 23)
            };

            var pipeHelpLabel = new Label
            {
                Text = "Full path: \\\\.\\pipe\\{PipeName}",
                Location = new Point(120, 125),
                Size = new Size(400, 20),
                ForeColor = System.Drawing.Color.Gray
            };

            // TCP settings
            var ipLabel = new Label
            {
                Text = "IP Address:",
                Location = new Point(15, 155),
                Size = new Size(100, 20)
            };

            _pipelineIpTextBox = new TextBox
            {
                Location = new Point(120, 152),
                Size = new Size(150, 23)
            };

            var portLabel = new Label
            {
                Text = "Port:",
                Location = new Point(285, 155),
                Size = new Size(40, 20)
            };

            _pipelinePortNumeric = new NumericUpDown
            {
                Location = new Point(330, 152),
                Size = new Size(90, 23),
                Minimum = 1,
                Maximum = 65535,
                Value = 8765
            };

            var tcpHelpLabel = new Label
            {
                Text = "• 127.0.0.1 = Local connections only\n" +
                       "• 0.0.0.0 = Accept from any network interface (LAN/Internet)\n" +
                       "• Specific IP = Bind to one network interface\n" +
                       "⚠ For LAN access, use 0.0.0.0 and configure your firewall!",
                Location = new Point(120, 180),
                Size = new Size(470, 60),
                ForeColor = System.Drawing.Color.Gray,
                AutoSize = false
            };

            _pipelineStatusLabel = new Label
            {
                Text = "Pipeline server will restart when settings are applied.",
                Location = new Point(15, 250),
                Size = new Size(570, 40),
                ForeColor = System.Drawing.Color.DarkOrange,
                AutoSize = false
            };

            var examplesLabel = new Label
            {
                Text = "Connection Examples:",
                Location = new Point(15, 295),
                Size = new Size(200, 20),
                Font = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Bold)
            };

            var examplesText = new Label
            {
                Text = "Named Pipe (Python): win32file.CreateFile(r'\\\\.\\pipe\\MSAgentAI', ...)\n" +
                       "TCP Socket (Python): socket.connect(('127.0.0.1', 8765))\n" +
                       "See PIPELINE.md for complete examples in multiple languages.",
                Location = new Point(15, 320),
                Size = new Size(570, 60),
                ForeColor = System.Drawing.Color.Gray,
                AutoSize = false
            };

            _pipelineTab.Controls.AddRange(new Control[]
            {
                descLabel, protocolLabel, _pipelineProtocolComboBox,
                pipeNameLabel, _pipelineNameTextBox, pipeHelpLabel,
                ipLabel, _pipelineIpTextBox, portLabel, _pipelinePortNumeric, tcpHelpLabel,
                _pipelineStatusLabel, examplesLabel, examplesText
            });
        }

        private void OnPipelineProtocolChanged(object sender, EventArgs e)
        {
            // Enable/disable controls based on protocol selection
            bool isTcp = _pipelineProtocolComboBox.SelectedItem?.ToString() == "TCP";
            
            _pipelineIpTextBox.Enabled = isTcp;
            _pipelinePortNumeric.Enabled = isTcp;
            _pipelineNameTextBox.Enabled = !isTcp;
        }

        private void CreateLinesTab()
        {
            _linesTab = new TabPage("Lines");
            _linesTextBoxes = new Dictionary<string, TextBox>();

            _linesTabControl = new TabControl
            {
                Location = new Point(5, 5),
                Size = new Size(600, 410)
            };

            // Create sub-tabs for each line type
            CreateLinesSubTab("Welcome", "welcomeLines", "Lines spoken when the agent first appears");
            CreateLinesSubTab("Idle", "idleLines", "Lines spoken randomly while idle");
            CreateLinesSubTab("Moved", "movedLines", "Lines spoken when the agent is dragged");
            CreateLinesSubTab("Clicked", "clickedLines", "Lines spoken when the agent is clicked");
            CreateLinesSubTab("Exit", "exitLines", "Lines spoken when exiting");
            CreateLinesSubTab("Jokes", "jokes", "Jokes the agent can tell");
            CreateLinesSubTab("Thoughts", "thoughts", "Thoughts shown in thought bubbles");
            CreateLinesSubTab("Random Prompts", "randomPrompts", "Prompts sent to Ollama for random dialog");

            _linesTab.Controls.Add(_linesTabControl);
        }

        private void CreateLinesSubTab(string title, string key, string description)
        {
            var tab = new TabPage(title);

            var descLabel = new Label
            {
                Text = description,
                Location = new Point(10, 10),
                Size = new Size(570, 20)
            };

            var infoLabel = new Label
            {
                Text = "(One line per entry)",
                Location = new Point(10, 30),
                Size = new Size(200, 20),
                ForeColor = Color.Gray
            };

            var textBox = new TextBox
            {
                Location = new Point(10, 55),
                Size = new Size(570, 310),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                AcceptsReturn = true
            };

            _linesTextBoxes[key] = textBox;

            tab.Controls.AddRange(new Control[] { descLabel, infoLabel, textBox });
            _linesTabControl.TabPages.Add(tab);
        }

        private void LoadSettings()
        {
            // Agent settings
            _characterPathTextBox.Text = _settings.CharacterPath;
            _userNameTextBox.Text = _settings.UserName;
            _userNamePronunciationTextBox.Text = _settings.UserNamePronunciation;
            RefreshCharacterList();

            // Voice settings
            LoadVoices();
            _speedTrackBar.Value = Math.Max(_speedTrackBar.Minimum, Math.Min(_speedTrackBar.Maximum, _settings.VoiceSpeed));
            _pitchTrackBar.Value = Math.Max(_pitchTrackBar.Minimum, Math.Min(_pitchTrackBar.Maximum, _settings.VoicePitch));
            _volumeTrackBar.Value = (int)(_settings.VoiceVolume / VolumeScaleFactor); // Convert from 0-65535 to 0-100
            
            // Speech recognition settings
            _confidenceTrackBar.Value = Math.Max(_confidenceTrackBar.Minimum, Math.Min(_confidenceTrackBar.Maximum, _settings.SpeechConfidenceThreshold));
            _confidenceValueLabel.Text = _confidenceTrackBar.Value.ToString() + "%";
            _silenceTrackBar.Value = Math.Max(_silenceTrackBar.Minimum, Math.Min(_silenceTrackBar.Maximum, _settings.SilenceDetectionMs));
            _silenceValueLabel.Text = _silenceTrackBar.Value.ToString() + "ms";
            
            // Agent size
            _agentSizeTrackBar.Value = Math.Max(_agentSizeTrackBar.Minimum, Math.Min(_agentSizeTrackBar.Maximum, _settings.AgentSize));
            _agentSizeValueLabel.Text = _agentSizeTrackBar.Value.ToString() + "%";

            // Ollama settings
            _ollamaUrlTextBox.Text = _settings.OllamaUrl;
            _ollamaModelComboBox.Text = _settings.OllamaModel;
            _personalityTextBox.Text = _settings.PersonalityPrompt;
            _enableChatCheckBox.Checked = _settings.EnableOllamaChat;
            _enableRandomDialogCheckBox.Checked = _settings.EnableRandomDialog;
            _randomChanceNumeric.Value = Math.Max(_randomChanceNumeric.Minimum, 
                Math.Min(_randomChanceNumeric.Maximum, _settings.RandomDialogChance));
            _enablePrewrittenIdleCheckBox.Checked = _settings.EnablePrewrittenIdle;
            _prewrittenIdleChanceNumeric.Value = Math.Max(_prewrittenIdleChanceNumeric.Minimum,
                Math.Min(_prewrittenIdleChanceNumeric.Maximum, _settings.PrewrittenIdleChance));

            // Theme
            int themeIndex = _themeComboBox.Items.IndexOf(_settings.UITheme);
            if (themeIndex >= 0)
                _themeComboBox.SelectedIndex = themeIndex;
            else
                _themeComboBox.SelectedIndex = 0;

            // Pronunciation Dictionary
            _pronunciationGrid.Rows.Clear();
            if (_settings.PronunciationDictionary != null)
            {
                foreach (var entry in _settings.PronunciationDictionary)
                {
                    _pronunciationGrid.Rows.Add(entry.Key, entry.Value);
                }
            }

            // Lines
            _linesTextBoxes["welcomeLines"].Text = string.Join(Environment.NewLine, _settings.WelcomeLines);
            _linesTextBoxes["idleLines"].Text = string.Join(Environment.NewLine, _settings.IdleLines);
            _linesTextBoxes["movedLines"].Text = string.Join(Environment.NewLine, _settings.MovedLines);
            _linesTextBoxes["clickedLines"].Text = string.Join(Environment.NewLine, _settings.ClickedLines);
            _linesTextBoxes["exitLines"].Text = string.Join(Environment.NewLine, _settings.ExitLines);
            _linesTextBoxes["jokes"].Text = string.Join(Environment.NewLine, _settings.Jokes);
            _linesTextBoxes["thoughts"].Text = string.Join(Environment.NewLine, _settings.Thoughts);
            _linesTextBoxes["randomPrompts"].Text = string.Join(Environment.NewLine, _settings.RandomDialogPrompts);

            // Pipeline settings
            string protocol = _settings.PipelineProtocol ?? "NamedPipe";
            int protocolIndex = _pipelineProtocolComboBox.Items.IndexOf(protocol);
            _pipelineProtocolComboBox.SelectedIndex = protocolIndex >= 0 ? protocolIndex : 0;
            
            _pipelineIpTextBox.Text = _settings.PipelineIPAddress ?? "127.0.0.1";
            _pipelinePortNumeric.Value = Math.Max(_pipelinePortNumeric.Minimum, 
                Math.Min(_pipelinePortNumeric.Maximum, _settings.PipelinePort));
            _pipelineNameTextBox.Text = _settings.PipelineName ?? "MSAgentAI";
            OnPipelineProtocolChanged(null, EventArgs.Empty); // Update UI based on protocol
        }

        private void SaveSettings()
        {
            // Agent settings
            _settings.CharacterPath = _characterPathTextBox.Text;
            _settings.UserName = _userNameTextBox.Text;
            _settings.UserNamePronunciation = _userNamePronunciationTextBox.Text;
            if (_characterListBox.SelectedItem is CharacterItem selected)
            {
                _settings.SelectedCharacterFile = selected.FilePath;
            }

            // Voice settings
            if (_voiceComboBox.SelectedItem is VoiceInfo voice)
            {
                _settings.SelectedVoiceId = voice.Id;
            }
            _settings.VoiceSpeed = _speedTrackBar.Value;
            _settings.VoicePitch = _pitchTrackBar.Value;
            _settings.VoiceVolume = (int)(_volumeTrackBar.Value * VolumeScaleFactor);
            
            // Speech recognition settings
            _settings.SpeechConfidenceThreshold = _confidenceTrackBar.Value;
            _settings.SilenceDetectionMs = _silenceTrackBar.Value;
            
            // Agent size
            _settings.AgentSize = _agentSizeTrackBar.Value;

            // Ollama settings
            _settings.OllamaUrl = _ollamaUrlTextBox.Text;
            _settings.OllamaModel = _ollamaModelComboBox.Text;
            _settings.PersonalityPrompt = _personalityTextBox.Text;
            _settings.EnableOllamaChat = _enableChatCheckBox.Checked;
            _settings.EnableRandomDialog = _enableRandomDialogCheckBox.Checked;
            _settings.RandomDialogChance = (int)_randomChanceNumeric.Value;
            _settings.EnablePrewrittenIdle = _enablePrewrittenIdleCheckBox.Checked;
            _settings.PrewrittenIdleChance = (int)_prewrittenIdleChanceNumeric.Value;

            // Theme
            _settings.UITheme = _themeComboBox.SelectedItem?.ToString() ?? "Default";

            // Pronunciation Dictionary
            _settings.PronunciationDictionary = new Dictionary<string, string>();
            foreach (DataGridViewRow row in _pronunciationGrid.Rows)
            {
                if (row.IsNewRow) continue;
                var word = row.Cells["Word"].Value?.ToString();
                var pronunciation = row.Cells["Pronunciation"].Value?.ToString();
                if (!string.IsNullOrEmpty(word) && !string.IsNullOrEmpty(pronunciation))
                {
                    _settings.PronunciationDictionary[word] = pronunciation;
                }
            }

            // Lines
            _settings.WelcomeLines = ParseLines(_linesTextBoxes["welcomeLines"].Text);
            _settings.IdleLines = ParseLines(_linesTextBoxes["idleLines"].Text);
            _settings.MovedLines = ParseLines(_linesTextBoxes["movedLines"].Text);
            _settings.ClickedLines = ParseLines(_linesTextBoxes["clickedLines"].Text);
            _settings.ExitLines = ParseLines(_linesTextBoxes["exitLines"].Text);
            _settings.Jokes = ParseLines(_linesTextBoxes["jokes"].Text);
            _settings.Thoughts = ParseLines(_linesTextBoxes["thoughts"].Text);
            _settings.RandomDialogPrompts = ParseLines(_linesTextBoxes["randomPrompts"].Text);

            // Pipeline settings - with validation
            _settings.PipelineProtocol = _pipelineProtocolComboBox.SelectedItem?.ToString() ?? "NamedPipe";
            
            // Validate IP address
            if (!System.Net.IPAddress.TryParse(_pipelineIpTextBox.Text, out _))
            {
                MessageBox.Show("Invalid IP address. Using default 127.0.0.1", "Invalid IP Address", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _settings.PipelineIPAddress = "127.0.0.1";
                _pipelineIpTextBox.Text = "127.0.0.1"; // Update UI to show the fallback value
            }
            else
            {
                _settings.PipelineIPAddress = _pipelineIpTextBox.Text;
            }
            
            _settings.PipelinePort = (int)_pipelinePortNumeric.Value;
            
            // Validate pipe name - remove invalid characters
            string pipeName = _pipelineNameTextBox.Text;
            var invalidChars = new HashSet<char>(System.IO.Path.GetInvalidFileNameChars());
            invalidChars.Add('\\');
            invalidChars.Add('/');
            
            // Filter out invalid characters efficiently
            var validChars = new System.Text.StringBuilder();
            foreach (char c in pipeName)
            {
                if (!invalidChars.Contains(c))
                    validChars.Append(c);
            }
            pipeName = validChars.ToString().Trim();
            
            if (string.IsNullOrWhiteSpace(pipeName))
            {
                pipeName = "MSAgentAI";
                _pipelineNameTextBox.Text = pipeName; // Update UI to show the fallback value
            }
            _settings.PipelineName = pipeName;

            _settings.Save();
        }

        private List<string> ParseLines(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            return text.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => s.Trim())
                       .Where(s => !string.IsNullOrEmpty(s))
                       .ToList();
        }

        private void LoadVoices()
        {
            _voiceComboBox.Items.Clear();

            if (_voiceManager != null)
            {
                var voices = _voiceManager.GetAvailableVoices();
                foreach (var voice in voices)
                {
                    _voiceComboBox.Items.Add(voice);
                }

                // Select current voice
                if (!string.IsNullOrEmpty(_settings.SelectedVoiceId))
                {
                    for (int i = 0; i < _voiceComboBox.Items.Count; i++)
                    {
                        if (_voiceComboBox.Items[i] is VoiceInfo v && v.Id == _settings.SelectedVoiceId)
                        {
                            _voiceComboBox.SelectedIndex = i;
                            break;
                        }
                    }
                }

                if (_voiceComboBox.SelectedIndex < 0 && _voiceComboBox.Items.Count > 0)
                {
                    _voiceComboBox.SelectedIndex = 0;
                }
            }
        }

        private void RefreshCharacterList()
        {
            _characterListBox.Items.Clear();

            if (_agentManager != null)
            {
                var characters = _agentManager.GetAvailableCharacters(_characterPathTextBox.Text);
                foreach (var charPath in characters)
                {
                    var item = new CharacterItem
                    {
                        FilePath = charPath,
                        Name = Path.GetFileNameWithoutExtension(charPath)
                    };
                    _characterListBox.Items.Add(item);

                    // Select if this is the current character
                    if (charPath == _settings.SelectedCharacterFile)
                    {
                        _characterListBox.SelectedItem = item;
                    }
                }
            }
        }

        #region Event Handlers

        private void OnBrowsePathClick(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = _characterPathTextBox.Text;
                dialog.Description = "Select the folder containing MS Agent character files (.acs)";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _characterPathTextBox.Text = dialog.SelectedPath;
                    RefreshCharacterList();
                }
            }
        }

        private void OnRefreshCharactersClick(object sender, EventArgs e)
        {
            RefreshCharacterList();
        }

        private void OnCharacterSelectionChanged(object sender, EventArgs e)
        {
            var enabled = _characterListBox.SelectedItem != null;
            _previewButton.Enabled = enabled;
            _selectButton.Enabled = enabled;

            if (_characterListBox.SelectedItem is CharacterItem item)
            {
                _characterInfoLabel.Text = $"Name: {item.Name}\n\nPath: {item.FilePath}";
                
                // Load animations for the selected character
                RefreshAnimationsList();
            }
        }

        private void RefreshAnimationsList()
        {
            _animationsListBox.Items.Clear();
            _playAnimationButton.Enabled = false;

            if (_agentManager != null && _agentManager.IsLoaded)
            {
                var animations = _agentManager.GetAnimations();
                foreach (var anim in animations)
                {
                    _animationsListBox.Items.Add(anim);
                }
                _playAnimationButton.Enabled = _animationsListBox.Items.Count > 0;
            }
        }

        private void OnPlayAnimationClick(object sender, EventArgs e)
        {
            if (_animationsListBox.SelectedItem != null && _agentManager?.IsLoaded == true)
            {
                string animName = _animationsListBox.SelectedItem.ToString();
                _agentManager.PlayAnimation(animName);
            }
        }

        private void OnTestNameClick(object sender, EventArgs e)
        {
            if (_agentManager?.IsLoaded == true)
            {
                string displayName = _userNameTextBox.Text;
                string pronunciation = _userNamePronunciationTextBox.Text;
                
                if (string.IsNullOrEmpty(displayName))
                {
                    MessageBox.Show("Please enter a name first.", "Name Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                // If pronunciation is empty, use display name
                if (string.IsNullOrEmpty(pronunciation))
                {
                    pronunciation = displayName;
                }
                
                // Show what the user will see and what agent will say
                MessageBox.Show($"Display Name: {displayName}\nPronounced as: {pronunciation}", 
                    "Name Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Agent speaks using the pronunciation
                _agentManager.Speak($"Hello, {pronunciation}! Nice to meet you!");
            }
            else
            {
                MessageBox.Show("Please load an agent first to test the name.", "No Agent",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnApplyPresetClick(object sender, EventArgs e)
        {
            if (_personalityPresetComboBox.SelectedIndex > 0)
            {
                string presetName = _personalityPresetComboBox.SelectedItem.ToString();
                
                // Check if it's a custom preset
                if (presetName.StartsWith("[Custom] "))
                {
                    string customName = presetName.Substring(9);
                    if (_settings.CustomPersonalityPresets.TryGetValue(customName, out string customPreset))
                    {
                        _personalityTextBox.Text = customPreset;
                    }
                }
                else if (AppSettings.PersonalityPresets.TryGetValue(presetName, out string preset))
                {
                    _personalityTextBox.Text = preset;
                }
            }
        }

        private void OnSavePresetClick(object sender, EventArgs e)
        {
            using (var dialog = new InputDialog("Save Personality Preset", "Enter a name for this personality preset:"))
            {
                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.InputText))
                {
                    string presetName = dialog.InputText.Trim();
                    _settings.CustomPersonalityPresets[presetName] = _personalityTextBox.Text;
                    
                    // Add to combo box if not already there
                    string displayName = "[Custom] " + presetName;
                    if (!_personalityPresetComboBox.Items.Contains(displayName))
                    {
                        _personalityPresetComboBox.Items.Add(displayName);
                    }
                    
                    MessageBox.Show($"Personality preset '{presetName}' saved!", "Preset Saved",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void OnPreviewClick(object sender, EventArgs e)
        {
            if (_characterListBox.SelectedItem is CharacterItem item && _agentManager != null)
            {
                try
                {
                    _agentManager.LoadCharacter(item.FilePath);
                    _agentManager.Show(false);
                    _agentManager.PlayAnimation("Greet");
                    _agentManager.Speak("Hello! This is a preview of " + item.Name);
                    
                    // Refresh animations list after loading
                    RefreshAnimationsList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to preview character: {ex.Message}", "Preview Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnSelectCharacterClick(object sender, EventArgs e)
        {
            if (_characterListBox.SelectedItem is CharacterItem item)
            {
                _settings.SelectedCharacterFile = item.FilePath;
            }
        }

        private void OnTestVoiceClick(object sender, EventArgs e)
        {
            if (_agentManager?.IsLoaded == true)
            {
                // Apply current slider values before testing
                _agentManager.SetSpeechSpeed(_speedTrackBar.Value);
                _agentManager.SetSpeechPitch(_pitchTrackBar.Value);
                
                // Convert volume from 0-100 to 0-65535
                int volumeValue = (int)(_volumeTrackBar.Value * VolumeScaleFactor);
                _agentManager.SetSpeechVolume(volumeValue);
                
                // Apply selected voice if one is selected
                if (_voiceComboBox.SelectedItem is VoiceInfo voice)
                {
                    _agentManager.SetTTSModeID(voice.ModeId ?? voice.Id);
                }
                
                _agentManager.Speak("This is a test of the text to speech voice.");
            }
            else
            {
                MessageBox.Show("Please load an agent first to test the voice.", "No Agent",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void OnTestConnectionClick(object sender, EventArgs e)
        {
            _testConnectionButton.Enabled = false;
            _testConnectionButton.Text = "...";

            try
            {
                _ollamaClient.BaseUrl = _ollamaUrlTextBox.Text;
                var success = await _ollamaClient.TestConnectionAsync();

                if (success)
                {
                    MessageBox.Show("Connection successful!", "Ollama", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await RefreshOllamaModels();
                }
                else
                {
                    MessageBox.Show("Connection failed. Please check the URL and ensure Ollama is running.",
                        "Ollama", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                _testConnectionButton.Enabled = true;
                _testConnectionButton.Text = "Test";
            }
        }

        private async void OnRefreshModelsClick(object sender, EventArgs e)
        {
            await RefreshOllamaModels();
        }

        private async Task RefreshOllamaModels()
        {
            _refreshModelsButton.Enabled = false;
            _refreshModelsButton.Text = "...";

            try
            {
                _ollamaClient.BaseUrl = _ollamaUrlTextBox.Text;
                var models = await _ollamaClient.GetAvailableModelsAsync();

                _ollamaModelComboBox.Items.Clear();
                foreach (var model in models)
                {
                    _ollamaModelComboBox.Items.Add(model);
                }

                if (_ollamaModelComboBox.Items.Count > 0)
                {
                    _ollamaModelComboBox.SelectedIndex = 0;
                }
            }
            finally
            {
                _refreshModelsButton.Enabled = true;
                _refreshModelsButton.Text = "Refresh";
            }
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void OnApplyClick(object sender, EventArgs e)
        {
            SaveSettings();
            MessageBox.Show("Settings applied.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        private class CharacterItem
        {
            public string Name { get; set; }
            public string FilePath { get; set; }

            public override string ToString() => Name;
        }
    }
}
