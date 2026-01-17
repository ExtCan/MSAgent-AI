using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MSAgentAI.AI;
using MSAgentAI.Config;

namespace MSAgentAI.UI
{
    /// <summary>
    /// Form for managing AI memories
    /// </summary>
    public class MemoryManagerForm : Form
    {
        private MemoryManager _memoryManager;
        private AppSettings _settings;
        
        private DataGridView _memoriesGrid;
        private Button _addButton;
        private Button _editButton;
        private Button _deleteButton;
        private Button _clearAllButton;
        private Button _importButton;
        private Button _exportButton;
        private Button _refreshButton;
        private TextBox _searchBox;
        private Label _statsLabel;
        private ComboBox _categoryFilterComboBox;

        public MemoryManagerForm(MemoryManager memoryManager, AppSettings settings = null)
        {
            _memoryManager = memoryManager;
            _settings = settings ?? AppSettings.Load();
            
            InitializeComponent();
            LoadMemories();
            UpdateStats();
            ApplyTheme();
        }

        private void InitializeComponent()
        {
            this.Text = "Memory Manager";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(700, 400);

            // Search and filter controls
            var searchLabel = new Label
            {
                Text = "Search:",
                Location = new Point(10, 15),
                Size = new Size(50, 20)
            };

            _searchBox = new TextBox
            {
                Location = new Point(65, 12),
                Size = new Size(200, 23)
            };
            _searchBox.TextChanged += OnSearchTextChanged;

            var categoryLabel = new Label
            {
                Text = "Category:",
                Location = new Point(280, 15),
                Size = new Size(60, 20)
            };

            _categoryFilterComboBox = new ComboBox
            {
                Location = new Point(345, 12),
                Size = new Size(120, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _categoryFilterComboBox.Items.AddRange(new object[] { "All", "user_info", "preference", "important", "recurring", "conversation", "general" });
            _categoryFilterComboBox.SelectedIndex = 0;
            _categoryFilterComboBox.SelectedIndexChanged += OnCategoryFilterChanged;

            _refreshButton = new Button
            {
                Text = "Refresh",
                Location = new Point(475, 11),
                Size = new Size(80, 25)
            };
            _refreshButton.Click += (s, e) => { LoadMemories(); UpdateStats(); };

            // Stats label
            _statsLabel = new Label
            {
                Text = "Stats: 0 memories",
                Location = new Point(570, 15),
                Size = new Size(300, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            // Memories grid
            _memoriesGrid = new DataGridView
            {
                Location = new Point(10, 45),
                Size = new Size(860, 440),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            _memoriesGrid.DoubleClick += OnEditClick;

            // Action buttons
            _addButton = new Button
            {
                Text = "Add Memory",
                Location = new Point(10, 495),
                Size = new Size(100, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            _addButton.Click += OnAddClick;

            _editButton = new Button
            {
                Text = "Edit",
                Location = new Point(120, 495),
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            _editButton.Click += OnEditClick;

            _deleteButton = new Button
            {
                Text = "Delete",
                Location = new Point(210, 495),
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            _deleteButton.Click += OnDeleteClick;

            _clearAllButton = new Button
            {
                Text = "Clear All",
                Location = new Point(300, 495),
                Size = new Size(90, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                ForeColor = Color.Red
            };
            _clearAllButton.Click += OnClearAllClick;

            _importButton = new Button
            {
                Text = "Import",
                Location = new Point(680, 495),
                Size = new Size(90, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            _importButton.Click += OnImportClick;

            _exportButton = new Button
            {
                Text = "Export",
                Location = new Point(780, 495),
                Size = new Size(90, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            _exportButton.Click += OnExportClick;

            this.Controls.AddRange(new Control[]
            {
                searchLabel, _searchBox, categoryLabel, _categoryFilterComboBox, _refreshButton,
                _statsLabel, _memoriesGrid,
                _addButton, _editButton, _deleteButton, _clearAllButton,
                _importButton, _exportButton
            });
        }

        private void ApplyTheme()
        {
            if (_settings == null) return;
            
            var colors = AppSettings.GetThemeColors(_settings.UITheme);
            this.BackColor = colors.Background;
            this.ForeColor = colors.Foreground;
            
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Button btn && btn != _clearAllButton)
                {
                    btn.BackColor = colors.ButtonBackground;
                    btn.ForeColor = colors.ButtonForeground;
                    btn.FlatStyle = FlatStyle.Flat;
                }
                else if (ctrl is TextBox || ctrl is ComboBox)
                {
                    ctrl.BackColor = colors.InputBackground;
                    ctrl.ForeColor = colors.InputForeground;
                }
                else if (ctrl is Label)
                {
                    ctrl.ForeColor = colors.Foreground;
                }
                else if (ctrl is DataGridView grid)
                {
                    grid.BackgroundColor = colors.InputBackground;
                    grid.DefaultCellStyle.BackColor = colors.InputBackground;
                    grid.DefaultCellStyle.ForeColor = colors.InputForeground;
                    grid.ColumnHeadersDefaultCellStyle.BackColor = colors.ButtonBackground;
                    grid.ColumnHeadersDefaultCellStyle.ForeColor = colors.ButtonForeground;
                }
            }
        }

        private void LoadMemories(string searchTerm = null, string categoryFilter = null)
        {
            var memories = string.IsNullOrEmpty(searchTerm) 
                ? _memoryManager.GetAllMemories() 
                : _memoryManager.SearchMemories(searchTerm);

            if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "All")
            {
                memories = memories.Where(m => m.Category == categoryFilter).ToList();
            }

            // Sort by importance and recency
            memories = memories.OrderByDescending(m => m.Importance).ThenByDescending(m => m.Timestamp).ToList();

            _memoriesGrid.DataSource = null;
            _memoriesGrid.DataSource = memories.Select(m => new
            {
                m.Id,
                Content = m.Content.Length > 100 ? m.Content.Substring(0, 97) + "..." : m.Content,
                m.Importance,
                m.Category,
                Created = m.Timestamp.ToString("yyyy-MM-dd HH:mm"),
                Accessed = m.AccessCount
            }).ToList();

            // Hide the ID column
            if (_memoriesGrid.Columns.Count > 0)
            {
                _memoriesGrid.Columns["Id"].Visible = false;
            }
        }

        private void UpdateStats()
        {
            var stats = _memoryManager.GetStats();
            _statsLabel.Text = $"Total: {stats.TotalMemories} | Avg Importance: {stats.AverageImportance:F1} | Categories: {stats.CategoriesCount}";
        }

        private void OnSearchTextChanged(object sender, EventArgs e)
        {
            var categoryFilter = _categoryFilterComboBox.SelectedItem?.ToString();
            LoadMemories(_searchBox.Text, categoryFilter);
        }

        private void OnCategoryFilterChanged(object sender, EventArgs e)
        {
            var categoryFilter = _categoryFilterComboBox.SelectedItem?.ToString();
            LoadMemories(_searchBox.Text, categoryFilter);
        }

        private void OnAddClick(object sender, EventArgs e)
        {
            using (var dialog = new MemoryEditDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _memoryManager.AddMemory(
                        dialog.MemoryContent,
                        dialog.MemoryImportance,
                        dialog.MemoryCategory,
                        dialog.MemoryTags
                    );
                    LoadMemories();
                    UpdateStats();
                }
            }
        }

        private void OnEditClick(object sender, EventArgs e)
        {
            if (_memoriesGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a memory to edit.", "Edit Memory", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRow = _memoriesGrid.SelectedRows[0];
            var memoryId = selectedRow.Cells["Id"].Value.ToString();
            var memory = _memoryManager.GetAllMemories().FirstOrDefault(m => m.Id == memoryId);

            if (memory != null)
            {
                using (var dialog = new MemoryEditDialog(memory))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _memoryManager.UpdateMemory(
                            memory.Id,
                            dialog.MemoryContent,
                            dialog.MemoryImportance,
                            dialog.MemoryCategory,
                            dialog.MemoryTags
                        );
                        LoadMemories();
                        UpdateStats();
                    }
                }
            }
        }

        private void OnDeleteClick(object sender, EventArgs e)
        {
            if (_memoriesGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a memory to delete.", "Delete Memory", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this memory?", "Confirm Delete", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                var selectedRow = _memoriesGrid.SelectedRows[0];
                var memoryId = selectedRow.Cells["Id"].Value.ToString();
                _memoryManager.RemoveMemory(memoryId);
                LoadMemories();
                UpdateStats();
            }
        }

        private void OnClearAllClick(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete ALL memories? This cannot be undone!",
                "Clear All Memories",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                _memoryManager.ClearAllMemories();
                LoadMemories();
                UpdateStats();
            }
        }

        private void OnExportClick(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog
            {
                Title = "Export Memories",
                Filter = "JSON Files|*.json",
                FileName = $"memories_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _memoryManager.ExportMemories(sfd.FileName);
                        MessageBox.Show("Memories exported successfully!", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void OnImportClick(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog
            {
                Title = "Import Memories",
                Filter = "JSON Files|*.json"
            })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _memoryManager.ImportMemories(ofd.FileName);
                        LoadMemories();
                        UpdateStats();
                        MessageBox.Show("Memories imported successfully!", "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Import failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Dialog for adding/editing a memory
    /// </summary>
    public class MemoryEditDialog : Form
    {
        private TextBox _contentTextBox;
        private NumericUpDown _importanceNumeric;
        private ComboBox _categoryComboBox;
        private TextBox _tagsTextBox;
        private Button _okButton;
        private Button _cancelButton;

        public string MemoryContent { get; private set; }
        public double MemoryImportance { get; private set; }
        public string MemoryCategory { get; private set; }
        public string[] MemoryTags { get; private set; }

        public MemoryEditDialog(Memory existingMemory = null)
        {
            InitializeComponent();

            if (existingMemory != null)
            {
                this.Text = "Edit Memory";
                _contentTextBox.Text = existingMemory.Content;
                _importanceNumeric.Value = (decimal)existingMemory.Importance;
                _categoryComboBox.Text = existingMemory.Category;
                _tagsTextBox.Text = existingMemory.Tags != null ? string.Join(", ", existingMemory.Tags) : "";
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Add Memory";
            this.Size = new Size(500, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var contentLabel = new Label
            {
                Text = "Content:",
                Location = new Point(15, 20),
                Size = new Size(100, 20)
            };

            _contentTextBox = new TextBox
            {
                Location = new Point(15, 45),
                Size = new Size(450, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            var importanceLabel = new Label
            {
                Text = "Importance (0.1 - 10):",
                Location = new Point(15, 155),
                Size = new Size(150, 20)
            };

            _importanceNumeric = new NumericUpDown
            {
                Location = new Point(170, 152),
                Size = new Size(80, 23),
                Minimum = 0.1m,
                Maximum = 10m,
                DecimalPlaces = 1,
                Increment = 0.1m,
                Value = 5m
            };

            var categoryLabel = new Label
            {
                Text = "Category:",
                Location = new Point(15, 185),
                Size = new Size(100, 20)
            };

            _categoryComboBox = new ComboBox
            {
                Location = new Point(170, 182),
                Size = new Size(150, 23)
            };
            _categoryComboBox.Items.AddRange(new object[] { "user_info", "preference", "important", "recurring", "conversation", "general" });
            _categoryComboBox.SelectedIndex = 5; // Default to "general"

            var tagsLabel = new Label
            {
                Text = "Tags (comma-separated):",
                Location = new Point(15, 215),
                Size = new Size(150, 20)
            };

            _tagsTextBox = new TextBox
            {
                Location = new Point(170, 212),
                Size = new Size(295, 23)
            };

            _okButton = new Button
            {
                Text = "OK",
                Location = new Point(275, 250),
                Size = new Size(90, 30),
                DialogResult = DialogResult.OK
            };
            _okButton.Click += OnOkClick;

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(375, 250),
                Size = new Size(90, 30),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.AddRange(new Control[]
            {
                contentLabel, _contentTextBox,
                importanceLabel, _importanceNumeric,
                categoryLabel, _categoryComboBox,
                tagsLabel, _tagsTextBox,
                _okButton, _cancelButton
            });

            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_contentTextBox.Text))
            {
                MessageBox.Show("Please enter memory content.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            MemoryContent = _contentTextBox.Text.Trim();
            MemoryImportance = (double)_importanceNumeric.Value;
            MemoryCategory = _categoryComboBox.Text;
            MemoryTags = _tagsTextBox.Text.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                         .Select(t => t.Trim())
                                         .Where(t => !string.IsNullOrEmpty(t))
                                         .ToArray();
        }
    }
}
