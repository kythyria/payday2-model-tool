using System;
using System.Drawing;
using System.Windows.Forms;

using PD2ModelParser.Misc;

namespace PD2ModelParser.UI
{
    public partial class SettingsPanel : UserControl
    {
        public SettingsPanel()
        {
            InitializeComponent();
        }

        Color default_updatecheck_bg;
        private void SettingsPanel_Load(object sender, EventArgs e)
        {
            enableAutomaticUpdates.Checked = Updates.Enabled;

            default_updatecheck_bg = checkForUpdates.BackColor;
            Updates.OnCheckComplete += OnCheckCompleteHandler;
            UpdateUpCheckBackground();
        }

        private void enableAutomaticUpdates_CheckedChanged(object sender, EventArgs e)
        {
            Updates.Enabled = enableAutomaticUpdates.Checked;
        }

        bool updatecheck_called_in_settings;
        private void checkForUpdates_Click(object sender, EventArgs e)
        {
            updatecheck_called_in_settings = true;
            Updates.CheckForUpdates();
            UpdateUpCheckBackground();
        }

        private void UpdateUpCheckBackground()
        {
            checkForUpdates.BackColor = Updates.CheckingForUpdates ? Color.Green : default_updatecheck_bg;
        }

        private void OnCheckCompleteHandler(bool new_version)
        {
            UpdateUpCheckBackground();

            if (updatecheck_called_in_settings && !new_version)
            {
                MessageBox.Show("Already up to date");
            }
        }
    }
}
