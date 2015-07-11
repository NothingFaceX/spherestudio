﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using WeifenLuo.WinFormsUI.Docking;

using Sphere.Plugins;

namespace SphereStudio.IDE
{
    /// <summary>
    /// Represents an open document in the IDE.
    /// </summary>
    class DocumentTab
    {
        private static uint _unsavedID = 1;
        
        private DockContent _content;
        private IDEForm _ide;
        private string _tabText;
        
        /// <summary>
        /// Creates a new Sphere Studio document tab.
        /// </summary>
        /// <param name="ide">The IDE form that the tab will be created in.</param>
        /// <param name="view">The IDocumentView the tab is hosting.</param>
        /// <param name="filename">The fully-qualified filename of the document, or null if untitled.</param>
        public DocumentTab(IDEForm ide, IDocumentView view, string filename = null)
        {
            FileName = filename;
            View = view;

            View.Control.Dock = DockStyle.Fill;
            
            _tabText = filename != null ? Path.GetFileName(filename)
                : string.Format("Untitled{0}", _unsavedID++);
            _ide = ide;
            _content = new DockContent();
            _content.FormClosing += content_FormClosing;
            _content.FormClosed += on_FormClosed;
            _content.Tag = this;
            _content.Icon = View.Icon;
            _content.TabText = _tabText;
            _content.Controls.Add(View.Control);
            _content.Show(ide.MainDock, DockState.Document);
            View.DirtyChanged += on_DirtyChanged;

            UpdateTabText();
        }

        public void Dispose()
        {
            _ide.FormClosing -= content_FormClosing;
            _content.FormClosing -= content_FormClosing;
            _content.Dispose();
        }

        public event EventHandler Closed;
        
        /// <summary>
        /// Gets the fully-qualified file path for the document. If the document
        /// hasn't been saved yet, this will be null.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets the tab's title text, including the trailing asterisk if the
        /// document has been modified.
        /// </summary>
        public string Title
        {
            get { return _content.TabText; }
        }
        
        /// <summary>
        /// Gets the underlying IDocumentView for the tab.
        /// </summary>
        public IDocumentView View { get; private set; }

        /// <summary>
        /// Makes the tab active and notifies the underlying document that it has
        /// received focus.
        /// </summary>
        public void Activate()
        {
            _content.DockHandler.Activate();
            View.Activate();
        }

        public void Close()
        {
            _content.Close();
        }
        
        public void Copy()
        {
            View.Copy();
        }
        
        public void Cut()
        {
            View.Cut();
        }

        /// <summary>
        /// Notifies the underlying document that it has lost focus.
        /// </summary>
        public void Deactivate()
        {
            View.Deactivate();
        }

        public void Paste()
        {
            View.Paste();
        }
        
        public void Redo()
        {
            View.Redo();
        }

        public void Restyle()
        {
            View.Restyle();
        }
        
        /// <summary>
        /// Saves the document in this tab. If the document hasn't been saved yet,
        /// the user will be asked to provide a filename.
        /// </summary>
        /// <param name="path">The default directory for the Save As dialog.</param>
        public bool Save(string path = null)
        {
            if (FileName == null)
                return SaveAs(path);
            
            View.Save(FileName);
            return true;
        }

        /// <summary>
        /// Saves the document in this tab with a filename provided by the user.
        /// </summary>
        /// <param name="path">The default directory for the Save As dialog.</param>
        /// <returns>true if the file was saved, false on cancel.</returns>
        public bool SaveAs(string path = null)
        {
            using (var diag = new SaveFileDialog())
            {
                // set up the dialog parameters
                string filterString = "";
                foreach (string ext in View.FileExtensions)
                {
                    if (filterString != "") filterString += "|";
                    filterString += string.Format(".{0} File|*.{0}", ext);
                }
                diag.Title = "Save As";
                diag.InitialDirectory = path;
                diag.FileName = _tabText;
                diag.Filter = filterString;
                diag.DefaultExt = View.FileExtensions[0];
                
                // show the Save As dialog
                if (diag.ShowDialog() == DialogResult.OK)
                {
                    FileName = diag.FileName;
                    _tabText = Path.GetFileName(FileName);
                    UpdateTabText();
                    Save(path);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Undo()
        {
            View.Undo();
        }

        public void ZoomIn()
        {
            View.ZoomIn();
        }

        public void ZoomOut()
        {
            View.ZoomOut();
        }

        private void UpdateTabText()
        {
            _content.TabText = View.IsDirty ? _tabText + "*" : _tabText;
        }
        
        private void content_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (View.IsDirty)
            {
                Activate();
                DialogResult result = MessageBox.Show(string.Format("{0}\n\nThis document has been modified. Do you want to save it before closing?", _tabText),
                    "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                e.Cancel |= result == DialogResult.Cancel;
                if (result == DialogResult.Yes)
                {
                    Save();
                }
            }
        }

        private void on_DirtyChanged(object sender, EventArgs e)
        {
            UpdateTabText();
        }
        
        private void on_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Closed != null) Closed(this, EventArgs.Empty);
            Dispose();
        }
    }
}
