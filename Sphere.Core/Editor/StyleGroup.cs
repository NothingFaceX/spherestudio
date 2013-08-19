﻿using System;
using System.Windows.Forms;

namespace Sphere.Core.Editor
{
    /// <summary>
    /// Styles different .NET controls with different styles.
    /// </summary>
    public class StyleGroup
    {
        /// <summary>
        /// Used for Labels.
        /// </summary>
        public Style Label { get; set; }

        /// <summary>
        /// Used for Panels, TabPages, and Panes.
        /// </summary>
        public Style Panel { get; set; }

        /// <summary>
        /// Used for PictureBoxes.
        /// </summary>
        public Style Image { get; set; }

        /// <summary>
        /// Used for Buttons.
        /// </summary>
        public Style Button { get; set; }

        /// <summary>
        /// Used for styling dialog window backgrounds.
        /// </summary>
        public Style Window { get; set; }

        /// <summary>
        /// Used for styling the status bar.
        /// </summary>
        public Style StatusBar { get; set; }

        /// <summary>
        /// Used for styling menubars.
        /// </summary>
        public Style MenuBar { get; set; }

        /// <summary>
        /// Used for styling toolbars.
        /// </summary>
        public Style ToolBar { get; set; }

        /// <summary>
        /// Applies a set style to the type of control.
        /// </summary>
        /// <param name="ctrl">The .NET control to style.</param>
        public void Apply(Control ctrl)
        {
            if (ctrl is Label && Label != null) Label.Apply(ctrl);
            else if (ctrl is Panel || ctrl is TabPage && Panel != null) Panel.Apply(ctrl);
            else if (ctrl is PictureBox && Image != null) Image.Apply(ctrl);
            else if (ctrl is Button && Button != null) Button.Apply(ctrl);
            else if (ctrl is MenuStrip && MenuBar != null) MenuBar.Apply(ctrl);
            else if (ctrl is StatusBar && StatusBar != null) StatusBar.Apply(ctrl);
            else if (ctrl is ToolStrip && ToolBar != null) ToolBar.Apply(ctrl);
            else if (ctrl is Form && Window != null) Window.Apply(ctrl);
        }
    }
}
