﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Sphere.Core.Utility;
using Sphere.Plugins;
using Sphere_Editor.Forms;
using Sphere_Editor.RadEditors;
using Sphere_Editor.Settings;
using Sphere_Editor.SubEditors;
using WeifenLuo.WinFormsUI.Docking;
using System.ComponentModel;

namespace Sphere_Editor
{
    public partial class EditorForm : Form, IPluginHost
    {
        // uninitialized data:
        private DockContent TreeContent;
        //private DockContent TaskContent;
        private DockContent StartContent;
        private StartPage start_page;
        private EditorObject CurrentControl;
        private TabControl AltTabInterface;
        private ProjectTree _tree;
        //private TaskList _tasks = new TaskList();
        private bool _firsttime;

        public event EventHandler OnOpenProject;
        public event EventHandler OnCloseProject;

        public EditorForm()
        {
            Global.LoadFunctions();
            _firsttime = !Global.CurrentEditor.LoadSettings();

            InitializeComponent();

            _tree = new ProjectTree();
            _tree.Dock = DockStyle.Fill; // _tasks.Dock;
            _tree.EditorForm = this;

            start_page = new StartPage(this);
            start_page.Dock = DockStyle.Fill;
            start_page.HelpLabel = this.HelpLabel;
            start_page.PopulateGameList();

            InitializeDocking();

            if (Global.CurrentEditor.AutoOpen)
                OpenLastProject(null, EventArgs.Empty);

            AutoCompleteItem.Checked = Global.CurrentEditor.ShowAutoComplete;

            Global.EvalPlugins((IPluginHost)this);
            Global.ActivatePlugins(Global.CurrentEditor.GetPluginList());

            // make sure this is active only when we use it.
            if (TreeContent != null) TreeContent.Activate();

            int spaces = Global.CurrentEditor.GetInt("script-spaces", 2);
            TwoUnitsItem.Checked = (spaces == 2);
            FourUnitsItem.Checked = (spaces == 4);

            UseTabsItem.Checked = Global.CurrentEditor.GetBool("script-tabs", true);
            FoldItem.Checked = Global.CurrentEditor.GetBool("script-fold", true);
            HighlightLineItem.Checked = Global.CurrentEditor.GetBool("script-hiline", true);
            HighlightBracesItem.Checked = Global.CurrentEditor.GetBool("script-hibraces", true);

            Text = string.Format("{0} ({1}) - v{2}", Application.ProductName,
                (IntPtr.Size == 4) ? "x86" : "x64", Application.ProductVersion);
        }

        bool IsProjectOpen { get { return Global.CurrentProject != null; } }

        private void UpdateButtons()
        {
            bool config = File.Exists(Global.CurrentEditor.ConfigPath);
            OptionsToolButton.Enabled = ConfigureSphereMenuItem.Enabled = config;

            bool sphere = File.Exists(Global.CurrentEditor.SpherePath);
            RunToolButton.Enabled = TestGameMenuItem.Enabled = sphere;

            bool last = !string.IsNullOrEmpty(Global.CurrentEditor.LastProjectPath);
            OpenLastProjectMenuItem.Enabled = last;

            GameSettingsMenuItem.Enabled = GameToolButton.Enabled = IsProjectOpen;
            OpenDirectoryMenuItem.Enabled = RefreshMenuItem.Enabled = IsProjectOpen;
        }

        #region interfaces
        private void InitializeDocking()
        {
            StartContent = new DockContent();
            StartContent.Icon = Icon.FromHandle(Properties.Resources.SphereEditor.GetHicon());
            StartContent.Text = "Start Page";
            StartContent.HideOnClose = true;
            StartContent.Show(DockTest, DockState.Document);
            StartContent.Controls.Add(start_page);

            TreeContent = new DockContent();
            TreeContent.Controls.Add(_tree);
            TreeContent.Text = "Project Explorer";
            TreeContent.DockAreas = DockAreas.DockLeft | DockAreas.DockRight;
            TreeContent.HideOnClose = true;
            TreeContent.Icon = Icon.FromHandle(Properties.Resources.SphereEditor.GetHicon());
            TreeContent.Show(DockTest, DockState.DockLeft);

            /*TaskContent = new DockContent();
            TaskContent.Controls.Add(_tasks);
            TaskContent.DockAreas = DockAreas.Document | DockAreas.DockLeft | DockAreas.DockRight | DockAreas.DockTop | DockAreas.DockBottom;
            TaskContent.Text = "Project Task List";
            TaskContent.HideOnClose = true;
            TaskContent.Icon = Icon.FromHandle(Properties.Resources.application_view_list.GetHicon());
            TaskContent.Show(TreeContent.Pane, DockAlignment.Bottom, 0.40);*/
        }

        public void DockControl(DockContent content, DockState state)
        {
            content.Show(DockTest, state);
        }

        public void RemoveControl(string name)
        {
            DockContent c = FindDocument(name);
            if (c != null) c.DockHandler.Close();
        }

        public string ProjectPath
        {
            get
            {
                if (Global.CurrentProject == null) return "";
                else return Global.CurrentProject.RootPath;
            }
        }

        private ToolStripMenuItem GetMenuItem(ToolStripItemCollection collection, string name)
        {
            foreach (ToolStripItem item in collection)
            {
                if (item is ToolStripMenuItem)
                    if (item.Text.Replace("&", "") == name) return (ToolStripMenuItem)item;
            }
            return null;
        }

        public void AddMenuItem(string location, ToolStripItem new_item)
        {
            string[] items = location.Split('.');
            ToolStripMenuItem item = GetMenuItem(EditorMenu.Items, items[0]);
            if (item == null)
            {
                item = new ToolStripMenuItem(items[0]);
                EditorMenu.Items.Add(item);
            }

            for (int i = 1; i < items.Length - 1; ++i)
            {
                ToolStripMenuItem menuitem = GetMenuItem(item.DropDownItems, items[i]);
                if (menuitem == null)
                {
                    item.DropDownItems.Add(new ToolStripMenuItem(items[i]));
                }
                item = menuitem;
            }

            item.DropDownItems.Add(new_item);
        }

        public void RemoveMenuItem(ToolStripItem item)
        {
            if (item.OwnerItem is ToolStripMenuItem)
                ((ToolStripMenuItem)item.OwnerItem).DropDownItems.Remove(item);
        }
        #endregion

        private void EditorForm_Shown(object sender, EventArgs e)
        {
            if (_firsttime)
            {
                MessageBox.Show("Hello! It's your first time here! I would love to help you " +
                                "set a few things up!", "Welcome First Timer", MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                OpenEditorSettings(this, null);
                _firsttime = false;
            }
        }

        /// <summary>
        /// Opens a Sphere project for editor use.
        /// </summary>
        /// <param name="filename">The filename of the project.</param>
        public void OpenProject(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return;
            CloseProject(null, EventArgs.Empty);
            Global.CurrentProject = new ProjectSettings();
            Global.CurrentProject.LoadSettings(filename);
            RefreshProject();
            if (OnOpenProject != null) OnOpenProject(null, EventArgs.Empty);
            if (TreeContent != null) TreeContent.Activate();
            //_tasks.LoadList();
            HelpLabel.Text = "Game project loaded successfully!";
            UpdateButtons();
        }

        private void UpdateMenu(string name)
        {
            ImageMenu.Visible = false;
            MapMenu.Visible = false;
            if (Global.IsImage(ref name)) ImageMenu.Visible = true;
            else if (Global.IsMap(ref name)) MapMenu.Visible = true;
        }

        // Loads and adds a new document to the editor.
        private void LoadDocument(Control control, string path)
        {
            AddDocument(control, Path.GetFileName(path));
            if (CurrentControl != null) CurrentControl.LoadFile(path);
        }

        // adds a new document to the form.
        #region document addition handling
        private void AddNewDocument(Control control, string name)
        {
            AddDocument(control, name);
            if (CurrentControl != null) CurrentControl.CreateNew();
        }

        private void AddDocument(Control control, string text)
        {
            DockContent content;
            if (control is AudioPlayer)
            {
                control.Dock = DockStyle.Top;
                content = FindDocument("Audio");
                if (content == null) content = new DockContent();
            }
            else
            {
                if (control is Drawer2) ((Drawer2)control).CanDirty = true;
                control.Dock = DockStyle.Fill;
                content = new DockContent();
            }
            content.Controls.Add(control);
            if (DockTest.DocumentsCount == 0) content.Show(DockTest, DockState.Document);
            else content.Show(DockTest.Panes[0], null);
            content.DockState = DockState.Document;
            content.DockAreas = DockAreas.Document;
            content.Text = text;
            content.FormClosing += new FormClosingEventHandler(Content_FormClosing);

            ImageMenu.Visible = MapMenu.Visible = false;
            SpritesetMenu.Visible = false;

            SetCurrentControl(control);

            if (text == "Sphere") { content.AllowEndUserDocking = false; }
            else if (control is Drawer2)
                content.Icon = Icon.FromHandle(Properties.Resources.palette.GetHicon());
            else if (control is ScriptEditor)
                content.Icon = Icon.FromHandle(Properties.Resources.script_edit.GetHicon());
            else if (control is MapEditor)
                content.Icon = Icon.FromHandle(Properties.Resources.map.GetHicon());
            else if (control is SpritesetEditor)
                content.Icon = Icon.FromHandle(Properties.Resources.palette.GetHicon());
            else if (Global.IsSound(ref text))
                content.Icon = Icon.FromHandle(Properties.Resources.sound.GetHicon());
            else if (control is FontEditor)
                content.Icon = Icon.FromHandle(Properties.Resources.font.GetHicon());
            else if (control is WindowStyleEditor)
                content.Icon = Icon.FromHandle(Properties.Resources.palette.GetHicon());
            else
                content.Icon = Icon.FromHandle(Properties.Resources.page_white_edit.GetHicon());

            if (CurrentControl != null) CurrentControl.HelpLabel = HelpLabel;
        }
        #endregion

        // used to process modified forms when closing:
        void Content_FormClosing(object sender, FormClosingEventArgs e)
        {
            DockContent obj = (DockContent)sender;
            if (obj.Text.EndsWith("*"))
            {
                switch (MessageBox.Show("File has been modified, save?", obj.Text,
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button3))
                {
                    case System.Windows.Forms.DialogResult.Yes:
                        SaveMenuItem_Click(null, EventArgs.Empty);
                        break;
                    case System.Windows.Forms.DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }

            if (!e.Cancel) obj.Dispose();
        }

        /// <summary>
        /// Searches any EditorObject controls in the dock contents for a file.
        /// </summary>
        /// <param name="filepath">The name to search against.</param>
        /// <returns>null if none found, or the IDockContent.</returns>
        public IDockContent GetDocument(string filepath)
        {
            Form form;
            foreach (IDockContent content in DockTest.Contents)
            {
                form = content.DockHandler.Form;
                if (form.Controls.Count > 0 && form.Controls[0] is EditorObject)
                {
                    if (((EditorObject)form.Controls[0]).FileName == filepath)
                        return content;
                }
            }
            return null;
        }

        private DockContent FindDocument(string name)
        {
            foreach (DockContent content in DockTest.Contents)
            {
                if (content.DockHandler.TabText == name) return content;
            }
            return null;
        }

        /// <summary>
        /// Selects a document by tab name, this is not ideal for editors but useful for
        /// persistent objects like the project tree and plugins.
        /// </summary>
        /// <param name="name">The content's tab text to look for.</param>
        public void SelectDocument(string name)
        {
            foreach (IDockContent content in DockTest.Contents)
                if (content.DockHandler.TabText == name)
                    content.DockHandler.Activate();
        }

        private void CloseAllDocuments()
        {
            foreach (IDockContent content in DockTest.Contents)
                content.DockHandler.Close();
        }

        private void SaveAllDocuments()
        {
            foreach (IDockContent content in DockTest.Contents)
            {
                Form f = content.DockHandler.Form;
                if (f.Controls.Count > 0 && f.Controls[0] is EditorObject)
                    ((EditorObject)content.DockHandler.Form.Controls[0]).Save();
            }
        }

        #region open functions
        public void OpenFont(string filename)
        {
            CurrentControl = new FontEditor(filename);
            LoadDocument(CurrentControl, filename);
        }

        public void OpenImage(string filename)
        {
            CurrentControl = new Drawer2();
            LoadDocument(CurrentControl, filename);
        }

        public void OpenMap(string filename)
        {
            CurrentControl = new MapEditor();
            LoadDocument(CurrentControl, filename);
        }

        public void OpenScript(string filename)
        {
            CurrentControl = new ScriptEditor();
            LoadDocument(CurrentControl, filename);
        }

        public void OpenSound(string filename)
        {
            CurrentControl = null;
            AddDocument(new AudioPlayer(filename), "Audio");
        }

        public void OpenSpriteset(string filename)
        {
            CurrentControl = new SpritesetEditor();
            LoadDocument(CurrentControl, filename);
        }

        public void OpenWindowStyle(string filename)
        {
            CurrentControl = new WindowStyleEditor();
            LoadDocument(CurrentControl, filename);
        }
        #endregion

        #region file menu options
        private void FileMenu_DropDownOpened(object sender, EventArgs e)
        {
            SaveAsMenuItem.Enabled = SaveMenuItem.Enabled = (CurrentControl != null);
            CloseProjectMenuItem.Enabled = IsProjectOpen;
            OpenLastProjectMenuItem.Enabled = (!IsProjectOpen ||
                Global.CurrentEditor.LastProjectPath != Global.CurrentProject.RootPath);
        }

        public void CallNewProject(object sender, EventArgs e)
        {
            Project MyProject = new Project();

            Control.ControlCollection NewProjectControls = MyProject.Controls["ProjectBox"].Controls;
            MyProject.Text = "New Project";
            string rootpath = Global.CurrentEditor.GetGamePaths()[0];
            NewProjectControls["FolderBox"].Text = rootpath;
            NewProjectControls["DirectoryBox"].Text = rootpath + "\\";
            if (MyProject.ShowDialog() == DialogResult.OK)
            {
                CloseProject(null, EventArgs.Empty);

                if (Global.CurrentProject == null)
                    Global.CurrentProject = new ProjectSettings();

                Global.CurrentProject.SetDataNew(MyProject);
                Global.CurrentProject.Create();
                Global.CurrentProject.Script = "main.js";
                Global.CurrentProject.SaveSettings();

                // automatically create the starting script //
                using (StreamWriter startscript = new StreamWriter(File.Open(Global.CurrentProject.RootPath + "\\scripts\\main.js", FileMode.CreateNew)))
                {
                    string header = "/**\n* Script: main.js\n* Written by: {0}\n* Updated: {1}\n**/\n\nfunction game()\n{{\n\t\n}}";
                    startscript.Write(string.Format(header, Global.CurrentProject.Author, DateTime.Today.ToShortDateString()));
                    startscript.Flush();
                }

                RefreshProject(null, EventArgs.Empty);
                start_page.PopulateGameList();
                OpenScript(Global.CurrentProject.RootPath + "\\scripts\\main.js");
            }
        }

        private void OpenProjectMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ProjDiag = new OpenFileDialog())
            {
                ProjDiag.Title = "Open Project";
                ProjDiag.Filter = "Game Files|*.sgm|All Files|*.*";

                string[] paths = Global.CurrentEditor.GetGamePaths();
                if (paths.Length > 0)
                    ProjDiag.InitialDirectory = paths[0];

                if (ProjDiag.ShowDialog() == DialogResult.OK)
                    OpenProject(ProjDiag.FileName);
            }
        }

        // remember to clear opened tabs!
        private void CloseProject(object sender, EventArgs e)
        {
            if (OnCloseProject != null) OnCloseProject(null, EventArgs.Empty);
            _tree.Close();
            //_tasks.SaveList();
            Global.CurrentProject = null;
            _tree.ProjectName = "Project Name";
            //_tasks.Clear();
            OpenLastProjectMenuItem.Enabled = (Global.CurrentEditor.LastProjectPath.Length > 0);
            UpdateButtons();
        }

        private void OpenLastProject(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Global.CurrentEditor.LastProjectPath) &&
                Directory.Exists(Global.CurrentEditor.LastProjectPath))
            {
                OpenProject(Global.CurrentEditor.LastProjectPath + "\\game.sgm");
            }
            else UpdateButtons();
        }

        private void SaveMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentControl != null) CurrentControl.Save();
        }

        private void SaveAsMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentControl != null) CurrentControl.SaveAs();
        }

        private void SaveOpenedItem_Click(object sender, EventArgs e)
        {
            SaveAllDocuments();
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion
        #region new sub-menu items
        public void NewFont(object sender, EventArgs e)
        {
            AddNewDocument(new FontEditor(), "Untitled.rfn");
        }

        public void NewImage(object sender, EventArgs e)
        {
            AddNewDocument(new Drawer2(), "Untitled.png");
        }

        public void NewMap(object sender, EventArgs e)
        {
            using (NewMapDialogue diag = new NewMapDialogue())
            {
                if (diag.ShowDialog() == DialogResult.OK)
                {
                    MapEditor x = new MapEditor();
                    x.CreateNew(diag.MapWidth, diag.MapHeight, diag.TileWidth, diag.TileHeight, diag.Tileset);
                    AddDocument(x, "Untitled.rmp");
                }
            }
        }

        public void NewSpriteset(object sender, EventArgs e)
        {
            AddNewDocument(new SpritesetEditor(), "Untitled.rss");
        }

        public void NewScript(object sender, EventArgs e)
        {
            AddNewDocument(new ScriptEditor(), "Untitled.js");
        }

        public void NewWindowStyle(object sender, EventArgs e)
        {
            AddNewDocument(new WindowStyleEditor(), "Untitled.rws");
        }
        #endregion
        #region open sub-menu items
        private void OpenFontMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fontfiles = new OpenFileDialog())
            {
                fontfiles.Filter = "Font Files (.rfn)|*.rfn";
                if (fontfiles.ShowDialog() == DialogResult.OK) OpenFont(fontfiles.FileName);
            }
        }

        private void OpenImageEvent(object sender, EventArgs e)
        {
            using (OpenFileDialog OpenItem = new OpenFileDialog())
            {
                OpenItem.Filter = "Image Files (.png, .jpg, .bmp, .gif)|*.png;*.jpg;*.bmp;*.gif";
                if (OpenItem.ShowDialog() == DialogResult.OK) OpenImage(OpenItem.FileName);
            }
        }

        private void OpenMapMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog mapfiles = new OpenFileDialog())
            {
                mapfiles.Filter = "Map files (.rmp)|*.rmp";
                if (mapfiles.ShowDialog() == DialogResult.OK) OpenMap(mapfiles.FileName);
            }
        }

        private void OpenScriptEvent(object sender, EventArgs e)
        {
            using (OpenFileDialog OpenScriptDiag = new OpenFileDialog())
            {
                OpenScriptDiag.Filter = "Script Files (.js)|*.js|Any File|*.*";
                if (OpenScriptDiag.ShowDialog() == DialogResult.OK) OpenScript(OpenScriptDiag.FileName);
            }
        }

        private void OpenSoundMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog soundfiles = new OpenFileDialog())
            {
                soundfiles.Filter = "Sound Files (.it, .mp3, .ogg, .xm, .mod, .wav, .aiff)|*.it;*.mp3;*.ogg;*.xm;*.mod;*.aiff*.wav;";
                if (soundfiles.ShowDialog() == DialogResult.OK) OpenSound(soundfiles.FileName);
            }
        }

        private void OpenSpritesetMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog OpenSpriteDiag = new OpenFileDialog())
            {
                OpenSpriteDiag.Filter = "Sprite Files (.rss)|*.rss";

                if (OpenSpriteDiag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    OpenSpriteset(OpenSpriteDiag.FileName);
            }
        }

        private void OpenWindowStyleMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog OpenWindowDiag = new OpenFileDialog())
            {
                OpenWindowDiag.Filter = "WindowStyle files (.rws)|*.rws";

                if (OpenWindowDiag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    OpenWindowStyle(OpenWindowDiag.FileName);
            }
        }
        #endregion

        #region edit items
        private void PasteMenuItem_Click(object sender, EventArgs e)
        {
            IDataObject obj = System.Windows.Forms.Clipboard.GetDataObject();
            if (obj.GetDataPresent(DataFormats.Dib))
            {
                if (CurrentControl == null || CurrentControl is ScriptEditor)
                {
                    Bitmap img = BitmapLoader.BitmapFromDIB((MemoryStream)obj.GetData(DataFormats.Dib));
                    Drawer DrawControl = new Drawer();
                    DrawControl.SetImage(img);
                    img.Dispose();
                    AddDocument(DrawControl, "Unitiled.png");
                }
                else CurrentControl.Paste();
            }
            if (obj.GetDataPresent(DataFormats.Text))
            {
                if (CurrentControl is ScriptEditor || CurrentControl is FontEditor)
                {
                    CurrentControl.Paste();
                }
                else
                {
                    ScriptEditor ScriptControl = new ScriptEditor();
                    ScriptControl.Text = (string)obj.GetData(DataFormats.Text);
                    AddNewDocument(ScriptControl, "Untitled.js");
                }
            }
        }

        private void CopyMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentControl != null) CurrentControl.Copy();
        }

        private void CutMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentControl != null) CurrentControl.Cut();
        }

        private void EditMenu_DropDownOpening(object sender, EventArgs e)
        {
            CutMenuItem.Enabled = SelectAllMenuItem.Enabled = CurrentControl is ScriptEditor;
            CopyToolButton.Enabled = CopyMenuItem.Enabled = RedoMenuItem.Enabled = UndoMenuItem.Enabled = CurrentControl != null;
            SaveLayoutMenuItem.Enabled = CutMenuItem.Enabled = CutToolButton.Enabled = CurrentControl != null;
            PasteMenuItem.Enabled = PasteToolButton.Enabled = true;
            ZoomInMenuItem.Enabled = ZoomOutMenuItem.Enabled = !(CurrentControl is ScriptEditor);
        }

        private void SelectAllMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentControl != null) CurrentControl.SelectAll();
        }

        private void UndoMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentControl != null) CurrentControl.Undo();
        }

        private void RedoMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentControl != null) CurrentControl.Redo();
        }

        private void ZoomInMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentControl != null) CurrentControl.ZoomIn();
        }

        private void ZoomOutMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentControl != null) CurrentControl.ZoomOut();
        }
        #endregion

        #region project menu items
        private void OptionsToolButton_Click(object sender, EventArgs e)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Global.CurrentEditor.ConfigPath));
            if (File.Exists(Global.CurrentEditor.ConfigPath))
                System.Diagnostics.Process.Start(Global.CurrentEditor.ConfigPath);
            Directory.SetCurrentDirectory(Application.StartupPath);
        }

        private void ViewGameSettings(object sender, EventArgs e)
        {
            using (GameSettings ViewSettings = new GameSettings(Global.CurrentProject))
            {
                if (ViewSettings.ShowDialog() == DialogResult.OK)
                {
                    Global.CurrentProject.SetData(ViewSettings);
                    Global.CurrentProject.SaveSettings();
                }
            }
        }

        public void OpenEditorSettings(object sender, EventArgs e)
        {
            if (Global.EditSettings())
            {
                UpdateButtons();
                start_page.PopulateGameList();
            }
        }

        private void OpenDirectoryMenuItem_Click(object sender, EventArgs e)
        {
            string path = Global.CurrentProject.RootPath;
            Process p = Process.Start("explorer.exe", string.Format("/select, \"{0}\\game.sgm\"", path));
            p.Dispose();
        }

        private void RunToolButton_Click(object sender, EventArgs e)
        {
            if (File.Exists(Global.CurrentEditor.SpherePath))
            {
                _tree.Pause();
                Process p;

                if (IsProjectOpen)
                {
                    Global.CurrentProject.SaveSettings();
                    p = Process.Start(Global.CurrentEditor.SpherePath, "-game \"" + Global.CurrentProject.RootPath + "\"");
                }
                else
                    p = Process.Start(Global.CurrentEditor.SpherePath);
                p.EnableRaisingEvents = true;
                p.Exited += delegate { _tree.Resume(); };
            }
        }

        public void RefreshProject()
        {
            _tree.Open();
            _tree.UpdateTree();
            Global.CurrentEditor.LastProjectPath = Global.CurrentProject.RootPath;
            Global.CurrentEditor.SaveSettings();
            UpdateButtons();
            _tree.ProjectName = "Project: " + Global.CurrentProject.Name;
        }

        private void RefreshProject(object sender, EventArgs e)
        {
            RefreshProject();
        }
        #endregion

        #region map menu items
        private void MapPropertiesMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentControl == null) return;
            if (!(CurrentControl is MapEditor)) return;

            MapEditor editor = (MapEditor)CurrentControl;
            using (MapPropertiesForm form = new MapPropertiesForm(editor.Map))
            {
                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    editor.SetTileSize(editor.Map.Tileset.TileWidth, editor.Map.Tileset.TileHeight);
            }
        }

        private void ExportTilesetItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog diag = new SaveFileDialog())
            {
                diag.InitialDirectory = Global.CurrentProject.RootPath;
                diag.Filter = "Image Files (.png)|*.png;";
                diag.DefaultExt = "png";

                if (diag.ShowDialog() == DialogResult.OK)
                    ((MapEditor)CurrentControl).SaveTileset(diag.FileName);
            }
        }

        private void UpdateFromFileItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog diag = new OpenFileDialog())
            {
                diag.InitialDirectory = Global.CurrentProject.RootPath;
                diag.Filter = "Image Files (.png)|*.png";

                if (diag.ShowDialog() == DialogResult.OK)
                    ((MapEditor)CurrentControl).UpdateTileset(diag.FileName);
            }
        }

        private void recenterMapItem_Click(object sender, EventArgs e)
        {
            if (CurrentControl != null && CurrentControl is MapEditor)
                ((MapEditor)CurrentControl).MapControl.CenterMap();
        }
        #endregion

        #region image menu items
        private void ResizeMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentControl == null) return;
            if (!(CurrentControl is Drawer2)) return;
            using (SizeForm form = new SizeForm())
            {
                form.WidthSize = ((Drawer2)CurrentControl).ImageWidth;
                form.HeightSize = ((Drawer2)CurrentControl).ImageHeight;
                form.UseScale = false;
                if (form.ShowDialog() == DialogResult.OK)
                    ((Drawer2)CurrentControl).SetSize(form.WidthSize, form.HeightSize);
            }
        }

        private void RescaleMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentControl == null) return;
            if (!(CurrentControl is Drawer2)) return;
            using (SizeForm form = new SizeForm())
            {
                form.WidthSize = ((Drawer2)CurrentControl).ImageWidth;
                form.HeightSize = ((Drawer2)CurrentControl).ImageHeight;
                if (form.ShowDialog() == DialogResult.OK)
                    ((Drawer2)CurrentControl).SetScale(form.WidthSize, form.HeightSize, form.mode);
            }
        }
        #endregion

        #region view menu items
        private void StartPageMenuItem_Click(object sender, EventArgs e)
        {
            if (StartContent.IsHidden) StartContent.Show(DockTest);
            else StartContent.Hide();
        }

        private void ProjectExplorerMenuItem_Click(object sender, EventArgs e)
        {
            if (TreeContent.IsHidden) TreeContent.Show(DockTest, DockState.DockLeft);
            else TreeContent.Hide();
        }

        private void TaskListMenuItem_Click(object sender, EventArgs e)
        {
            /*if (Global.CurrentEditor.UseDockForm)
            {
                if (TaskContent.IsHidden) TaskContent.Show(DockTest, DockState.DockLeft);
                else TaskContent.Hide();
            }
            else _tasks.Visible = !_tasks.Visible;*/
        }
        #endregion

        #region help items
        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutSphereEditor About = new AboutSphereEditor())
            {
                About.ShowDialog();
            }
        }
        #endregion

        private void SetCurrentControl(Control value)
        {
            CurrentControl = (value is EditorObject) ? (EditorObject)value : null;
            SpritesetMenu.Visible = false;
            MapMenu.Visible = ImageMenu.Visible = ScriptMenu.Visible = false;

            if (value is MapEditor) MapMenu.Visible = true;
            else if (value is Drawer2) ImageMenu.Visible = true;
            else if (value is ScriptEditor) ScriptMenu.Visible = true;
            else if (value is SpritesetEditor) SpritesetMenu.Visible = true;

            bool save = CurrentControl != null;
            SaveAsMenuItem.Enabled = SaveToolButton.Enabled = SaveMenuItem.Enabled = save;
        }

        private void DockTest_ActiveDocumentChanged(object sender, EventArgs e)
        {
            if (DockTest.ActiveDocument == null) return;
            Form form = DockTest.ActiveDocument.DockHandler.Form;
            if (form.Controls.Count == 0) return;
            SetCurrentControl(form.Controls[0]);
        }

        void AltTabInterface_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetCurrentControl(AltTabInterface.SelectedTab.Controls[0]);
        }

        private void EditorForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Global.CurrentEditor.SaveSettings();
            CloseProject(null, EventArgs.Empty);
            //_tasks.SaveList();
        }

        private void MenuDesignerItem_Click(object sender, EventArgs e)
        {
            AddDocument(new StateEditor(), "RadLib Menu Editor");
        }

        private void TestItem_Click(object sender, EventArgs e)
        {
            //Sphere_Editor.Forms.ColorPicker.ColorPicker picker = new Forms.ColorPicker.ColorPicker();
            //picker.Show();
            GC.Collect();
        }

        private void SsResizeMenuItem_Click(object sender, EventArgs e)
        {
            ((SpritesetEditor)CurrentControl).ResizeAll();
        }

        private void SsRescaleMenuItem_Click(object sender, EventArgs e)
        {
            ((SpritesetEditor)CurrentControl).RescaleAll();
        }

        private void ExportSsMenuItem_Click(object sender, EventArgs e)
        {
            // not implemented.
        }

        private void ImportSsMenuItem_Click(object sender, EventArgs e)
        {
            // not implemented.
        }

        private void SaveLayoutMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentControl != null) CurrentControl.SaveLayout();
        }

        // put here things to store into the settings file via menu-item check
        private void UpdateCheck(object sender, EventArgs e)
        {
            Global.CurrentEditor.ShowAutoComplete = AutoCompleteItem.Checked;
        }

        private void ViewMenu_DropDownOpening(object sender, EventArgs e)
        {
            if (DockTest.Contents.Count == 0) return;
            ToolStripSeparator ts = new ToolStripSeparator();
            ts.Name = "zz_v";
            ViewMenu.DropDownItems.Add(ts);
            foreach (IDockContent content in DockTest.Contents)
            {
                Form f = content.DockHandler.Form;
                ToolStripMenuItem item = new ToolStripMenuItem(content.DockHandler.TabText);
                item.Name = "zz_v";
                item.Click += new EventHandler(item_Click);

                if (f.Controls.Count > 0 && f.Controls[0] is EditorObject)
                    item.Tag = ((EditorObject)f.Controls[0]).FileName;
                else
                    item.Tag = content.DockHandler.TabText;

                if (content.DockHandler.IsActivated) item.CheckState = CheckState.Checked;
                ViewMenu.DropDownItems.Add(item);
            }
        }

        void item_Click(object sender, EventArgs e)
        {
            IDockContent content = GetDocument((string)((ToolStripItem)sender).Tag);
            if (content != null) content.DockHandler.Activate();
            else SelectDocument((string)((ToolStripMenuItem)sender).Tag);
        }

        private void ViewMenu_DropDownClosed(object sender, EventArgs e)
        {
            for (int i = 0; i < ViewMenu.DropDownItems.Count; ++i)
            {
                if (ViewMenu.DropDownItems[i].Name == "zz_v")
                {
                    ViewMenu.DropDownItems.RemoveAt(i);
                    i--;
                }
            }
        }

        private void UpdateScriptControls()
        {
            foreach (DockContent content in DockTest.Contents)
                if (content.Controls[0] is ScriptEditor)
                    ((ScriptEditor)content.Controls[0]).UpdateStyle();
        }

        private void ChangeFontItem_Click(object sender, EventArgs e)
        {
            using (FontDialog diag = new FontDialog())
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(Font));
                string fontstring = Global.CurrentEditor.GetString("script-font");

                if (!String.IsNullOrEmpty(fontstring))
                    diag.Font = (Font)converter.ConvertFromString(fontstring);

                try
                {
                    if (diag.ShowDialog() == DialogResult.OK)
                    {
                        fontstring = converter.ConvertToString(diag.Font);
                        Global.CurrentEditor.SaveObject("script-font", fontstring);
                        UpdateScriptControls();
                    }
                }
                catch (ArgumentException)
                {
                    MessageBox.Show("GDI+ only uses TrueType fonts.", "Type Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UseTabsItem_Click(object sender, EventArgs e)
        {
            Global.CurrentEditor.SaveObject("script-tabs", UseTabsItem.Checked);
            UpdateScriptControls();
        }

        private void TwoUnitsItem_Click(object sender, EventArgs e)
        {
            Global.CurrentEditor.SaveObject("script-spaces", 2);
            FourUnitsItem.CheckState = CheckState.Unchecked;
            TwoUnitsItem.CheckState = CheckState.Checked;
            UpdateScriptControls();
        }

        private void FourUnitsItem_Click(object sender, EventArgs e)
        {
            Global.CurrentEditor.SaveObject("script-spaces", 4);
            TwoUnitsItem.CheckState = CheckState.Unchecked;
            FourUnitsItem.CheckState = CheckState.Checked;
            UpdateScriptControls();
        }

        private void FoldItem_Click(object sender, EventArgs e)
        {
            Global.CurrentEditor.SaveObject("script-fold", FoldItem.Checked);
        }

        private void HighlightLineItem_Click(object sender, EventArgs e)
        {
            Global.CurrentEditor.SaveObject("script-hiline", HighlightLineItem.Checked);
            UpdateScriptControls();
        }

        private void HighlightBracesItem_Click(object sender, EventArgs e)
        {
            Global.CurrentEditor.SaveObject("script-hibraces", HighlightBracesItem.Checked);
            UpdateScriptControls();
        }

        private void ClosePaneItem_Click(object sender, EventArgs e)
        {
            if (DockTest.ActiveDocument == null) return;

            if (DockTest.ActiveDocument is DockContent &&
                ((DockContent)DockTest.ActiveDocument).Controls[0] is StartPage)
            {
                StartPageMenuItem_Click(null, EventArgs.Empty);
                return;
            }
            else DockTest.ActiveDocument.DockHandler.Close();
        }
    }
}