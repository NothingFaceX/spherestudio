﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Sphere.Plugins;
using Sphere.Plugins.Interfaces;
using minisphere.GDK.Utility;

namespace minisphere.GDK.Debugger
{
    class DebugSession : IDebugger
    {
        private string sgmPath;
        private DuktapeClient duktape;
        private Process engineProcess;
        private string engineDir;
        private ConcurrentQueue<dynamic[]> replies = new ConcurrentQueue<dynamic[]>();
        private System.Threading.Timer focusTimer;
        private string sourcePath;
        private System.Threading.Timer updateTimer;
        private ISettings config;

        public DebugSession(string gamePath, string enginePath, Process engine, IProject project, ISettings conf)
        {
            config = conf;
            sgmPath = gamePath;
            sourcePath = project.RootPath;
            engineProcess = engine;
            engineDir = Path.GetDirectoryName(enginePath);
            focusTimer = new System.Threading.Timer(
                FocusEngine, this,
                Timeout.Infinite,
                Timeout.Infinite);
            updateTimer = new System.Threading.Timer(
                UpdateDebugViews, this,
                Timeout.Infinite,
                Timeout.Infinite);
        }

        public string FileName { get; private set; }

        public int LineNumber { get; private set; }

        public bool Running { get; private set; }

        public event EventHandler Attached;

        public event EventHandler Detached;

        public event EventHandler Paused;

        public event EventHandler Resumed;

        public async Task<bool> Attach()
        {
            try
            {
                await Connect("localhost", 1208);
                return true;
            }
            catch (TimeoutException)
            {
                return false;
            }
        }

        public async Task Detach()
        {
            focusTimer.Change(Timeout.Infinite, Timeout.Infinite);
            await duktape.Detach();
            duktape.Dispose();
        }

        private async Task Connect(string hostname, int port, uint timeout = 5000)
        {
            long end = DateTime.Now.Ticks + timeout * 10000;
            while (DateTime.Now.Ticks < end)
            {
                try
                {
                    duktape = new DuktapeClient();
                    duktape.Attached += duktape_Attached;
                    duktape.Detached += duktape_Detached;
                    duktape.ErrorThrown += duktape_ErrorThrown;
                    duktape.Alert += duktape_Print;
                    duktape.Print += duktape_Print;
                    duktape.Status += duktape_Status;
                    await duktape.Connect(hostname, port);
                    return;
                }
                catch (SocketException) { } // *munch*
            }
            throw new TimeoutException();
        }

        private void duktape_Attached(object sender, EventArgs e)
        {
            PluginManager.Core.Invoke(new Action(() =>
            {
                if (Attached != null)
                    Attached(this, EventArgs.Empty);

                Panes.Inspector.CurrentSession = this;
                Panes.Stack.CurrentSession = this;
                Panes.Errors.CurrentSession = this;
                Panes.Inspector.Enabled = false;
                Panes.Stack.Enabled = false;
                Panes.Console.Clear();
                Panes.Errors.Clear();
                Panes.Inspector.Clear();
                Panes.Stack.Clear();

                Panes.Console.Print(string.Format("minisphere GDK for Sphere Studio"));
                Panes.Console.Print(string.Format("(c) 2015 Fat Cerberus"));
                Panes.Console.Print("");
                Panes.Console.Print(string.Format("Debuggee IDs as {0}.", duktape.TargetID));
                Panes.Console.Print(string.Format("Duktape {0}", duktape.Version));
                Panes.Console.Print("");

                PluginManager.Core.Docking.Show(Panes.Inspector);
                PluginManager.Core.Docking.Show(Panes.Stack);
            }), null);
        }

        private void duktape_Detached(object sender, EventArgs e)
        {
            PluginManager.Core.Invoke(new Action(() =>
            {
                if (Detached != null)
                    Detached(this, EventArgs.Empty);
                PluginManager.Core.Docking.Hide(Panes.Inspector);
                PluginManager.Core.Docking.Hide(Panes.Stack);
                Panes.Errors.HideIfClean();

                PluginManager.Core.Docking.Show(Panes.Console);
                Panes.Console.Print("");
                Panes.Console.Print(duktape.TargetID + " detached.");
                if (!config.GetBoolean("keepConsoleOutput", true))
                    PluginManager.Core.Docking.Hide(Panes.Console);
            }), null);
        }

        private void duktape_ErrorThrown(object sender, ErrorThrownEventArgs e)
        {
            PluginManager.Core.Invoke(new Action(() =>
            {
                Panes.Errors.Add(e.Message, e.IsFatal, e.FileName, e.LineNumber);
                PluginManager.Core.Docking.Show(Panes.Errors);
            }), null);
        }

        private void duktape_Print(object sender, TraceEventArgs e)
        {
            PluginManager.Core.Invoke(new Action(() =>
            {
                Panes.Console.Print(e.Text);
            }), null);
        }

        private void duktape_Status(object sender, EventArgs e)
        {
            PluginManager.Core.Invoke(new Action(async () =>
            {
                bool wantPause = !duktape.Running;
                bool wantResume = !Running && duktape.Running;
                Running = duktape.Running;
                if (wantPause)
                {
                    focusTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    FileName = ResolvePath(duktape.FileName);
                    LineNumber = duktape.LineNumber;
                    if (!File.Exists(FileName))
                    {
                        // filename reported by Duktape doesn't exist; walk callstack for a
                        // JavaScript call as a fallback
                        var callStack = await duktape.GetCallStack();
                        var topCall = callStack.First(entry => entry.Item2 != duktape.TargetID || entry.Item3 != 0);
                        FileName = ResolvePath(topCall.Item2);
                        LineNumber = topCall.Item3;
                        Panes.Stack.UpdateStack(callStack);
                        Panes.Stack.Enabled = true;
                    }
                    updateTimer.Change(500, Timeout.Infinite);
                }
                if (wantResume && duktape.Running)
                {
                    focusTimer.Change(250, Timeout.Infinite);
                    updateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    Panes.Errors.ClearHighlight();
                }
                if (wantPause && Paused != null)
                    Paused(this, EventArgs.Empty);
                if (wantResume && Resumed != null)
                    Resumed(this, EventArgs.Empty);
            }), null);
        }

        public async Task SetBreakpoint(string filename, int lineNumber, bool isSet)
        {
            // convert filename to a SphereFS path
            string relativePath = UnresolvePath(filename);

            // clear all matching breakpoints
            var breaks = await duktape.ListBreak();
            for (int i = breaks.Length - 1; i >= 0; --i)
            {
                string fn = breaks[i].Item1;
                int line = breaks[i].Item2;
                if (relativePath == fn && lineNumber == line)
                    await duktape.DelBreak(i);
            }
            
            // set the breakpoint if needed
            if (isSet)
            {
                await duktape.AddBreak(relativePath, lineNumber);
            }
        }

        public async Task Resume()
        {
            await duktape.Resume();
        }

        public async Task Pause()
        {
            await duktape.Pause();
        }

        public async Task<string> Evaluate(string expression)
        {
            return await duktape.Eval(expression);
        }

        public async Task StepInto()
        {
            await duktape.StepInto();
        }

        public async Task StepOut()
        {
            await duktape.StepOut();
        }

        public async Task StepOver()
        {
            await duktape.StepOver();
        }

        private static void FocusEngine(object state)
        {
            PluginManager.Core.Invoke(new Action(() =>
            {
                DebugSession me = (DebugSession)state;
                NativeMethods.SetForegroundWindow(me.engineProcess.MainWindowHandle);
                PluginManager.Core.Docking.Show(Panes.Console);
                Panes.Inspector.Enabled = false;
                Panes.Stack.Enabled = false;
                Panes.Inspector.Clear();
                Panes.Stack.Clear();
            }), null);
        }

        private static void UpdateDebugViews(object state)
        {
            PluginManager.Core.Invoke(new Action(async () =>
            {
                DebugSession me = (DebugSession)state;
                var callStack = await me.duktape.GetCallStack();
                var vars = await me.duktape.GetLocals();
                if (!me.Running)
                {
                    Panes.Stack.UpdateStack(callStack);
                    Panes.Stack.Enabled = true;
                    Panes.Inspector.SetVariables(vars);
                    Panes.Inspector.Enabled = true;
                    PluginManager.Core.Docking.Show(Panes.Inspector);
                }
            }), null);
        }

        /// <summary>
        /// Resolves a SphereFS path into an absolute one.
        /// </summary>
        /// <param name="path">The SphereFS path to resolve.</param>
        internal string ResolvePath(string path)
        {
            if (Path.IsPathRooted(path))
                return path.Replace('/', Path.DirectorySeparatorChar);
            if (path.StartsWith("~/") || path.StartsWith("~sgm/"))
                path = Path.Combine(sourcePath, path.Substring(path.IndexOf('/') + 1));
            else if (path.StartsWith("~sys/"))
                path = Path.Combine(engineDir, "system", path.Substring(5));
            else if (path.StartsWith("~usr/"))
                path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "minisphere", path.Substring(5));
            else
                path = Path.Combine(sourcePath, path);
            return path.Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Converts an absolute path into a SphereFS path. If this is
        /// not possible, leaves the path as-is.
        /// </summary>
        /// <param name="path">The absolute path to unresolve.</param>
        internal string UnresolvePath(string path)
        {
            var pathSep = Path.DirectorySeparatorChar.ToString();
            string sourceRoot = sourcePath.EndsWith(pathSep)
                ? sourcePath : sourcePath + pathSep;
            string sysRoot = Path.Combine(engineDir, @"system") + pathSep;

            if (path.StartsWith(sysRoot))
                path = string.Format("~sys/{0}", path.Substring(sysRoot.Length).Replace(pathSep, "/"));
            else if (path.StartsWith(sourceRoot))
                path = path.Substring(sourceRoot.Length).Replace(pathSep, "/");
            return path;
        }

        private void UpdateStatus()
        {
            FileName = ResolvePath(duktape.FileName);
            LineNumber = duktape.LineNumber;
            Running = duktape.Running;
        }
    }
}
