﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sphere.Plugins;
using Sphere.Plugins.Interfaces;
using minisphere.Gdk.Debugger;

namespace minisphere.Gdk.Plugins
{
    class minisphereStarter : IDebugStarter
    {
        private ISettings config;

        public minisphereStarter(ISettings conf)
        {
            config = conf;
        }

        public bool CanConfigure { get { return false; } }

        public void Start(string gamePath, bool isPackage)
        {
            string gdkPath = config.GetString("gdkPath", "");
            bool wantConsole = config.GetBoolean("testWithConsole", false);

            string enginePath = Path.Combine(gdkPath, wantConsole ? "msphere.exe" : "msphere-nc.exe");
            string options = string.Format(@"""{0}""", gamePath);
            Process.Start(enginePath, options);
        }

        public void Configure()
        {
            throw new NotSupportedException("minisphere doesn't support configuration.");
        }

        public IDebugger Debug(string gamePath, bool isPackage, IProject project)
        {
            string gdkPath = config.GetString("gdkPath", "");

            string enginePath = Path.Combine(gdkPath, "msphere.exe");
            string options = string.Format(@"--debug --game ""{0}""", gamePath);
            Process engine = Process.Start(enginePath, options);
            return new DebugSession(gamePath, enginePath, engine, project, config);
        }
    }
}
