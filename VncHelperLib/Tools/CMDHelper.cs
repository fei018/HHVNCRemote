using System;
using System.Diagnostics;

namespace VncHelperLib
{
    internal class CMDHelper
    {
        private readonly int _waitMilliSeconds = 0;

        public bool CreateProcessExe(string app)
        {
            try
            {
                var start = new ProcessStartInfo();
                start.FileName = app;
                start.UseShellExecute = false;
                start.WorkingDirectory = Environment.CurrentDirectory;
                start.CreateNoWindow = true;
                start.RedirectStandardError = true;
                start.RedirectStandardInput = true;
                start.RedirectStandardOutput = true;

                using Process p = new Process();
                p.StartInfo = start;
                var run = p.Start();

                p.WaitForExit(_waitMilliSeconds);
                return run;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool CreateProcessExe(string app, string argument)
        {
            try
            {
                var start = new ProcessStartInfo();
                start.FileName = app;
                start.Arguments = argument;
                start.UseShellExecute = false;
                start.WorkingDirectory = Environment.CurrentDirectory;
                start.CreateNoWindow = true;
                start.RedirectStandardError = true;
                start.RedirectStandardInput = true;
                start.RedirectStandardOutput = true;

                using Process ps = new Process();
                ps.StartInfo = start;
                var run = ps.Start();

                ps.WaitForExit(_waitMilliSeconds);
                return run;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool CreateProcessExeAsUser(string app, string argument, string admin, string password)
        {
            try
            {
                var start = new ProcessStartInfo();
                start.FileName = app;
                start.Arguments = argument;
                start.UseShellExecute = false;
                start.WorkingDirectory = Environment.CurrentDirectory;
                start.CreateNoWindow = true;
                start.RedirectStandardError = true;
                start.RedirectStandardInput = true;
                start.RedirectStandardOutput = true;

                if (!string.IsNullOrWhiteSpace(admin))
                {
                    start.UserName = admin;
                }

                using var sPass = new System.Security.SecureString();
                if (!string.IsNullOrWhiteSpace(password))
                {
                    foreach (var s in password)
                    {
                        sPass.AppendChar(s);
                    }
                    start.Password = sPass;
                }

                using Process ps = new Process();
                ps.StartInfo = start;

                var run = ps.Start();

                ps.WaitForExit(_waitMilliSeconds);
                return run;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool CreateProcessExeRunAS(string app, string argument)
        {
            try
            {
                var start = new ProcessStartInfo();
                start.FileName = app;
                start.Arguments = argument;
                start.UseShellExecute = true;
                start.WorkingDirectory = Environment.CurrentDirectory;
                start.CreateNoWindow = true;
                //start.RedirectStandardError = true;
                //start.RedirectStandardInput = true;
                //start.RedirectStandardOutput = true;
                start.Verb = "runas";

                using Process p = new Process();
                p.StartInfo = start;

                var run = p.Start();

                p.WaitForExit(_waitMilliSeconds);
                return run;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region cmd
        public bool ExecuteCmd(string cmdString, out string error)
        {
            var start = new ProcessStartInfo();
            start.FileName = "cmd.exe";
            start.UseShellExecute = false;
            start.WorkingDirectory = Environment.CurrentDirectory;
            start.CreateNoWindow = true;
            start.RedirectStandardError = true;
            start.RedirectStandardInput = true;
            start.RedirectStandardOutput = true;

            using Process p = new Process();
            p.StartInfo = start;

            var run = p.Start();

            p.StandardInput.WriteLine(cmdString);
            p.StandardInput.WriteLine("exit");

            error = p.StandardError.ReadToEnd();
            //var output = p.StandardOutput.ReadToEnd(); // dont export because will show password
            return run;
        }

        public bool ExecuteCmdRunAs(string cmdString)
        {
            var start = new ProcessStartInfo();
            start.FileName = "cmd.exe";
            start.Arguments = cmdString;
            start.UseShellExecute = true;
            start.WorkingDirectory = Environment.CurrentDirectory;
            start.CreateNoWindow = true;
            start.Verb = "runas";
            //start.RedirectStandardError = true;
            //start.RedirectStandardInput = true;
            //start.RedirectStandardOutput = true;

            using Process p = new Process();
            p.StartInfo = start;

            var run = p.Start();

            p.WaitForExit(10000);

            return run;
        }

        public bool ExecuteCmdAsUser(string admin, string password, string cmdString, out string error)
        {
            var start = new ProcessStartInfo();
            start.FileName = "cmd.exe";
            start.UseShellExecute = false;
            start.WorkingDirectory = Environment.CurrentDirectory;
            start.CreateNoWindow = true;
            start.RedirectStandardError = true;
            start.RedirectStandardInput = true;
            start.RedirectStandardOutput = true;

            if (!string.IsNullOrWhiteSpace(admin))
            {
                start.UserName = admin;
            }

            using var sPass = new System.Security.SecureString();
            if (!string.IsNullOrWhiteSpace(password))
            {
                foreach (var s in password)
                {
                    sPass.AppendChar(s);
                }
                start.Password = sPass;
            }

            using Process p = new Process();
            p.StartInfo = start;
            var run = p.Start();

            p.StandardInput.WriteLine(cmdString);
            p.StandardInput.WriteLine("exit");

            error = p.StandardError.ReadToEnd();
            //var output = p.StandardOutput.ReadToEnd(); // dont export because will show password
            //p.WaitForExit(10000);

            return run;
        }
        #endregion
    }
}
