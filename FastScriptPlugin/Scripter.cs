﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using Sphere.Core;
using Sphere.Plugins;

namespace FastScriptPlugin
{
    public partial class Scripter : EditorObject
    {
        readonly Encoding ISO_8859_1 = Encoding.GetEncoding("iso-8859-1");
        private FastColoredTextBox _textbox;
        private IPluginHost _host;

        public Scripter(IPluginHost host)
        {
            _host = host;
            InitializeComponent();
            _textbox = new FastColoredTextBox();
            _textbox.Dock = DockStyle.Fill;
            _textbox.TextChangedDelayed += _textbox_TextChangedDelayed;
            Controls.Add(_textbox);
            UpdateStyle();
        }

        void _textbox_TextChangedDelayed(object sender, TextChangedEventArgs e)
        {
            MakeDirty();
        }

        public void UpdateStyle()
        {
            _textbox.TabLength = _host.EditorSettings.GetInt("script-spaces", 2);
            _textbox.AcceptsTab = _host.EditorSettings.GetBool("script-tabs", _textbox.AcceptsTab);
            _textbox.ShowFoldingLines = _host.EditorSettings.GetBool("script-fold", true);
            
            if (_host.EditorSettings.GetBool("script-hiline", true))
                _textbox.CurrentLineColor = Color.Yellow;
            else
                _textbox.CurrentLineColor = Color.White;

            string fontstring = _host.EditorSettings.GetString("script-font");
            if (!String.IsNullOrEmpty(fontstring))
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(Font));
                _textbox.Font = (Font)converter.ConvertFromString(fontstring);
            }
        }

        public override void LoadFile(string filename)
        {
            base.LoadFile(filename);

            if (filename.EndsWith(".js"))
                _textbox.Language = Language.JS;
            else
                _textbox.Language = Language.Custom;

            using (StreamReader reader = new StreamReader(filename))
            {
                _textbox.Text = reader.ReadToEnd();
                _textbox.ClearUndo();
                Parent.Text = Path.GetFileName(FileName);
            }
        }

        public override void Save()
        {
            if (!IsSaved()) SaveAs();
            else
            {
                using (StreamWriter writer = new StreamWriter(FileName, false, ISO_8859_1))
                {
                    writer.Write(_textbox.Text);
                    Parent.Text = Path.GetFileName(FileName);
                }
            }
        }

        public override void SaveAs()
        {
            using (SaveFileDialog diag = new SaveFileDialog())
            {
                diag.Filter = "Sphere Script Files (.js)|*.js";

                if (_host.CurrentGame != null)
                    diag.InitialDirectory = _host.CurrentGame.RootPath + "\\scripts";

                if (diag.ShowDialog() == DialogResult.OK)
                {
                    FileName = diag.FileName;
                    Save();
                }
            }
        }

        public override void SelectAll()
        {
            _textbox.SelectAll();
        }

        public override void Copy()
        {
            _textbox.Copy();
        }

        public override void Paste()
        {
            _textbox.Paste();
        }

        public override void Cut()
        {
            _textbox.Cut();
        }

        public override void Undo()
        {
            _textbox.Undo();
        }

        public override void Redo()
        {
            _textbox.Redo();
        }
    }
}