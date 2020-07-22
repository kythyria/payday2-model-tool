using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Linq;

namespace PD2ModelParser.Misc
{
    static class Updates
    {
        public enum UpdateCheckModes
        {
            Unset, Enabled, Disabled
        }

        public static bool CheckingForUpdates { get; private set; }

        public delegate void OnCheckCompleteHandler(bool new_version);
        public static event OnCheckCompleteHandler OnCheckComplete;

        public static void Startup()
        {
            if (Enabled)
            {
                CheckForUpdates();
            }
        }

        public static async void CheckForUpdates()
        {
            CheckingForUpdates = true;

            string build_type = "release";

            try
            {
                WebRequest request = WebRequest.Create("https://superblt.znix.xyz/modeltool/latest.xml");

                WebResponse response_object = await Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, request);
                var response_stream = response_object.GetResponseStream();
                var sr = new StreamReader(response_stream);
                string received = await sr.ReadToEndAsync();

                System.Threading.Thread.Sleep(1000);

                if (received == null)
                    throw new Exception("received null response");

                XElement root = XElement.Parse(received);

                IEnumerable<XElement> server_version_tags = root?.Element("Updates")?.Elements("CurrentVersion");

                if (server_version_tags == null || server_version_tags.Count() == 0)
                    throw new Exception("missing tags for server version: " + root.ToString());

                string server_version = server_version_tags
                    .First(elem => elem.Attribute("type")?.Value == build_type).Value;

                if (server_version == null)
                    throw new Exception("missing tag for type " + build_type + ": " + root.ToString());

                CheckingForUpdates = false;

                if (server_version == CurrentVersion)
                {
                    OnCheckComplete?.Invoke(false);
                    return;
                }
                OnCheckComplete?.Invoke(true);

                UpdateFound(server_version);

                return;
            }
            catch (Exception e)
            {
                Log.Default.Warn("Update check error: {0}", e.Message);
            }
        }

        public static void UpdateFound(string new_version)
        {
            string message = "An update is available: " + new_version + " - do you want to go to the download page?"
                + "\nYou can disable automatic updates in settings if you don't want this.";
            string caption = "Update Checker";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;

            bool response = MessageBox.Show(message, caption, buttons) == System.Windows.Forms.DialogResult.Yes;

            if (response)
            {
                Process.Start("https://superblt.znix.xyz/modeltool/");
                Environment.Exit(0);
            }
        }

        public static string CurrentVersion
        {
            get
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                return FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
            }
        }

        public static bool Enabled
        {
            get
            {
                if (Properties.Settings.Default.AutoUpdates == UpdateCheckModes.Unset)
                {
                    AskForUpdates();
                }

                return Properties.Settings.Default.AutoUpdates == UpdateCheckModes.Enabled;
            }

            set
            {
                Properties.Settings.Default.AutoUpdates = value ? UpdateCheckModes.Enabled : UpdateCheckModes.Disabled;
                Properties.Settings.Default.Save();
            }
        }

        private static void AskForUpdates()
        {
            string message = "Do you want to check for updates on startup?";
            string caption = "Update Checker";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result;

            // Displays the MessageBox.

            result = MessageBox.Show(message, caption, buttons);

            Enabled = result == DialogResult.Yes;
        }
    }
}
