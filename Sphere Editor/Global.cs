﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Sphere.Core.Settings;
using Sphere.Plugins;
using Sphere_Editor.Forms;
using Sphere_Editor.Utility;

namespace Sphere_Editor
{
    public class Global
    {
        public static Dictionary<string, PluginWrapper> plugins = new Dictionary<string, PluginWrapper>();
        public static List<string> functions = new List<string>();

        public static void EvalPlugins(IPluginHost host)
        {
            string path = Application.StartupPath + "/Plugins";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (FileInfo file in dir.GetFiles("*.dll"))
            {
                Assembly assembly = Assembly.LoadFrom(file.FullName);
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.GetInterface("IPlugin") != null)
                    {
                        IPlugin b = type.InvokeMember(null,
                                                   BindingFlags.CreateInstance,
                                                   null, null, null) as IPlugin;
                        if (b == null) continue;
                        b.Host = host;
                        string name = Path.GetFileNameWithoutExtension(file.Name);
                        plugins[name] = new PluginWrapper(b, name);
                    }
                }
            }
        }

        public static void ActivatePlugins(string[] plugin_names)
        {
            foreach (string s in plugin_names)
            {
                if (plugins.ContainsKey(s)) plugins[s].Activate();
            }
        }

        public static void LoadFunctions()
        {
            FileInfo file = new FileInfo(Application.StartupPath + "/docs/functions.txt");
            if (!file.Exists) return;

            using (StreamReader reader = file.OpenText())
            {
                while (!reader.EndOfStream)
                    functions.Add(reader.ReadLine());
            }
        }

        public static ProjectSettings CurrentProject = null;
        public static SphereSettings CurrentEditor = new SphereSettings();
        public static Sphere.Core.Entity CopiedEnt { get; set; }

        // Extention checking functions. Globally useable. :)
        public static bool IsScript(ref string name)
        {
            return name.ToLower().EndsWith(".js");
        }

        public static bool IsSound(ref string name)
        {
            name = name.ToLower();
            return (name.EndsWith(".mod") || name.EndsWith(".xm") || name.EndsWith(".it") ||
                  name.EndsWith(".mp3") || name.EndsWith(".ogg") || name.EndsWith(".wav") ||
                  name.EndsWith(".s3m"));
        }

        public static bool IsImage(ref string name)
        {
            name = name.ToLower();
            return (name.Contains(".png") || name.Contains(".gif") ||
                    name.Contains(".jpg") || name.Contains(".bmp"));
        }

        public static bool IsSpriteset(ref string name)
        {
            return name.ToLower().Contains(".rss");
        }

        public static bool IsWindowStyle(ref string name)
        {
            return name.ToLower().Contains(".rws");
        }

        public static bool IsFont(ref string name)
        {
            return name.Contains(".rfn");
        }

        public static bool IsMap(ref string name)
        {
            return name.Contains(".rmp");
        }

        public static bool IsTileset(ref string name)
        {
            return name.Contains(".rts");
        }

        public static bool IsText(ref string name)
        {
            return (name.Contains(".txt") || name.Contains(".sav") || name.Contains(".sgm") ||
                name.Contains(".rtf") || name.Contains(".dat") || name.Contains(".ini"));
        }

        // returns true if settings were altered
        public static bool EditSettings()
        {
            EditorSettings Settings = new EditorSettings(Global.CurrentEditor);
            if (Settings.ShowDialog() == DialogResult.OK)
            {
                Global.CurrentEditor.SetSettings(Settings.GetSettings());
                return true;
            }
            return false;
        }
    }

    class EditorPanel : Panel
    {
        private int _y_snap = 0;
        private int _x_snap = 0;

        public EditorPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override Point ScrollToControl(Control activeControl)
        {
            return DisplayRectangle.Location;
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            if (_x_snap == 0 && _y_snap == 0) base.OnScroll(se);
            else
            {
                if (se.ScrollOrientation == ScrollOrientation.HorizontalScroll)
                    HorizontalScroll.Value = se.NewValue / _x_snap * _x_snap;
                else
                    VerticalScroll.Value = se.NewValue / _y_snap * _y_snap;
            }
        }

        public int XSnap
        {
            get { return _x_snap; }
            set { _x_snap = value; }
        }

        public int YSnap
        {
            get { return _y_snap; }
            set { _y_snap = value; }
        }
    }

    class EditorLabel : Label
    {
        private static Brush bgBrush = new TextureBrush(Sphere_Editor.Properties.Resources.BarImage);
        public EditorLabel() { }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            Font = new System.Drawing.Font(Global.CurrentEditor.LabelFont, 10.5f, GraphicsUnit.Point);
            TextAlign = ContentAlignment.MiddleLeft;
            Height = 23;
            AutoSize = false;
            ForeColor = Color.MidnightBlue;
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            base.OnPaintBackground(pevent);
            pevent.Graphics.FillRectangle(bgBrush, this.ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Font = new System.Drawing.Font(Global.CurrentEditor.LabelFont, 10.5f, GraphicsUnit.Point);
            base.OnPaint(e);
        }
    }

    class TipLabel : Label
    {
        private bool show = false;
        private bool clear = true;

        public TipLabel() { }

        protected override void OnCreateControl()
        {
            Image = Sphere_Editor.Properties.Resources.resultset_next;
            Text = "Rollover an item to view help.";
            Font = new System.Drawing.Font(Global.CurrentEditor.LabelFont, 7.75F);
            TextAlign = ContentAlignment.MiddleCenter;
            ImageAlign = ContentAlignment.MiddleLeft;
            AutoSize = false;
            Height = 21;
            BackColor = Color.Lavender;

            base.OnCreateControl();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            this.Font = new System.Drawing.Font(Global.CurrentEditor.LabelFont, 7.75F);
            base.OnPaint(e);
        }

        public override string Text
        {
            get { return base.Text; }
            set
            {
                if (this.clear)
                {
                    this.Image = Sphere_Editor.Properties.Resources.information;
                    this.TextAlign = ContentAlignment.MiddleCenter;
                    this.clear = false;
                }
                base.Text = value;
            }
        }

        public bool AlwaysShow
        {
            get { return show; }
            set { show = value; }
        }

        public void Clear()
        {
            this.Image = Sphere_Editor.Properties.Resources.resultset_next;
            this.TextAlign = ContentAlignment.MiddleCenter;
            this.Text = "Rollover an item to view help.";
            this.clear = true;
        }
    }
}
