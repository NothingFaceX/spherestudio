﻿namespace minisphere.Gdk.SettingsPages
{
    partial class SettingsPage
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TabView = new System.Windows.Forms.TabControl();
            this.MainTabPage = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.Panel();
            this.MakeDebugPackage = new System.Windows.Forms.CheckBox();
            this.KeepConsoleOpen = new System.Windows.Forms.CheckBox();
            this.TestWithConsole = new System.Windows.Forms.CheckBox();
            this.editorLabel2 = new Sphere.Core.Editor.EditorLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.GDKPath = new System.Windows.Forms.TextBox();
            this.PathLabel = new System.Windows.Forms.Label();
            this.editorLabel1 = new Sphere.Core.Editor.EditorLabel();
            this.TabView.SuspendLayout();
            this.MainTabPage.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabView
            // 
            this.TabView.Controls.Add(this.MainTabPage);
            this.TabView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabView.Location = new System.Drawing.Point(0, 0);
            this.TabView.Name = "TabView";
            this.TabView.SelectedIndex = 0;
            this.TabView.Size = new System.Drawing.Size(606, 355);
            this.TabView.TabIndex = 3;
            // 
            // MainTabPage
            // 
            this.MainTabPage.Controls.Add(this.panel2);
            this.MainTabPage.Controls.Add(this.editorLabel2);
            this.MainTabPage.Controls.Add(this.panel1);
            this.MainTabPage.Controls.Add(this.editorLabel1);
            this.MainTabPage.Location = new System.Drawing.Point(4, 22);
            this.MainTabPage.Name = "MainTabPage";
            this.MainTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.MainTabPage.Size = new System.Drawing.Size(598, 329);
            this.MainTabPage.TabIndex = 0;
            this.MainTabPage.Text = "minisphere GDK";
            this.MainTabPage.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.MakeDebugPackage);
            this.panel2.Controls.Add(this.KeepConsoleOpen);
            this.panel2.Controls.Add(this.TestWithConsole);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 126);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(592, 200);
            this.panel2.TabIndex = 3;
            // 
            // MakeDebugPackage
            // 
            this.MakeDebugPackage.AutoSize = true;
            this.MakeDebugPackage.Location = new System.Drawing.Point(12, 58);
            this.MakeDebugPackage.Name = "MakeDebugPackage";
            this.MakeDebugPackage.Size = new System.Drawing.Size(247, 17);
            this.MakeDebugPackage.TabIndex = 2;
            this.MakeDebugPackage.Text = "Include debug map when compiling a package";
            this.MakeDebugPackage.UseVisualStyleBackColor = true;
            // 
            // KeepConsoleOpen
            // 
            this.KeepConsoleOpen.AutoSize = true;
            this.KeepConsoleOpen.Location = new System.Drawing.Point(12, 35);
            this.KeepConsoleOpen.Name = "KeepConsoleOpen";
            this.KeepConsoleOpen.Size = new System.Drawing.Size(330, 17);
            this.KeepConsoleOpen.TabIndex = 1;
            this.KeepConsoleOpen.Text = "Keep Debug Output pane visible after a debugging session ends";
            this.KeepConsoleOpen.UseVisualStyleBackColor = true;
            // 
            // TestWithConsole
            // 
            this.TestWithConsole.AutoSize = true;
            this.TestWithConsole.Location = new System.Drawing.Point(12, 12);
            this.TestWithConsole.Name = "TestWithConsole";
            this.TestWithConsole.Size = new System.Drawing.Size(240, 17);
            this.TestWithConsole.TabIndex = 0;
            this.TestWithConsole.Text = "Use minisphere &Console when testing a game";
            this.TestWithConsole.UseVisualStyleBackColor = true;
            // 
            // editorLabel2
            // 
            this.editorLabel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.editorLabel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.editorLabel2.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.editorLabel2.ForeColor = System.Drawing.Color.White;
            this.editorLabel2.Location = new System.Drawing.Point(3, 103);
            this.editorLabel2.Name = "editorLabel2";
            this.editorLabel2.Size = new System.Drawing.Size(592, 23);
            this.editorLabel2.TabIndex = 2;
            this.editorLabel2.Text = "Additional settings";
            this.editorLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.BrowseButton);
            this.panel1.Controls.Add(this.GDKPath);
            this.panel1.Controls.Add(this.PathLabel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 26);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(592, 77);
            this.panel1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(227, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "minisphere GDK version 2.0 or later is required.";
            // 
            // BrowseButton
            // 
            this.BrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseButton.Image = global::minisphere.Gdk.Properties.Resources.FolderIcon;
            this.BrowseButton.Location = new System.Drawing.Point(499, 39);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(80, 25);
            this.BrowseButton.TabIndex = 4;
            this.BrowseButton.Text = "Browse...";
            this.BrowseButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // GDKPath
            // 
            this.GDKPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GDKPath.Location = new System.Drawing.Point(77, 13);
            this.GDKPath.Name = "GDKPath";
            this.GDKPath.ReadOnly = true;
            this.GDKPath.Size = new System.Drawing.Size(502, 20);
            this.GDKPath.TabIndex = 2;
            // 
            // PathLabel
            // 
            this.PathLabel.AutoSize = true;
            this.PathLabel.Location = new System.Drawing.Point(16, 16);
            this.PathLabel.Name = "PathLabel";
            this.PathLabel.Size = new System.Drawing.Size(55, 13);
            this.PathLabel.TabIndex = 0;
            this.PathLabel.Text = "GDK Path";
            // 
            // editorLabel1
            // 
            this.editorLabel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.editorLabel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.editorLabel1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.editorLabel1.ForeColor = System.Drawing.Color.White;
            this.editorLabel1.Location = new System.Drawing.Point(3, 3);
            this.editorLabel1.Name = "editorLabel1";
            this.editorLabel1.Size = new System.Drawing.Size(592, 23);
            this.editorLabel1.TabIndex = 0;
            this.editorLabel1.Text = "Where is the minisphere GDK installed?";
            this.editorLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TabView);
            this.Name = "SettingsPage";
            this.Size = new System.Drawing.Size(606, 355);
            this.TabView.ResumeLayout(false);
            this.MainTabPage.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl TabView;
        private System.Windows.Forms.TabPage MainTabPage;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.TextBox GDKPath;
        private System.Windows.Forms.Label PathLabel;
        private Sphere.Core.Editor.EditorLabel editorLabel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.CheckBox TestWithConsole;
        private Sphere.Core.Editor.EditorLabel editorLabel2;
        private System.Windows.Forms.CheckBox KeepConsoleOpen;
        private System.Windows.Forms.CheckBox MakeDebugPackage;
        private System.Windows.Forms.Label label1;
    }
}
