using Aurora.Profiles.Discord.GSI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

using System.IO;

namespace Aurora.Profiles.Discord
{
    /// <summary>
    /// Interaction logic for Control_Minecraft.xaml
    /// </summary>
    public partial class Control_Discord : UserControl {
        private Application profile;

        public Control_Discord(Application profile) {
            this.profile = profile;

            InitializeComponent();
            SetSettings();         

            profile.ProfileChanged += (sender, e) => SetSettings();
        }

        private void SetSettings() {
            GameEnabled.IsChecked = profile.Settings.IsEnabled;
        }

        private void GameEnabled_Checked(object sender, RoutedEventArgs e) {
            if (IsLoaded) {
                profile.Settings.IsEnabled = GameEnabled.IsChecked ?? false;
                profile.SaveProfiles();
            }
        }

        private void PatchButton_Click(object sender, RoutedEventArgs e)
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string pluginDirectory = Path.Combine(appdata, "BetterDiscord", "plugins");

            if (!Directory.Exists(pluginDirectory))
                Directory.CreateDirectory(pluginDirectory);

            string pluginFile = Path.Combine(pluginDirectory, "AuroraGSI.plugin.js");
            WriteFile(pluginFile);
        }

        private void UnpatchButton_Click(object sender, RoutedEventArgs e)
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path = Path.Combine(appdata, "BetterDiscord", "plugins", "AuroraGSI.plugin.js");

            if (File.Exists(path))
            {
                File.Delete(path);
                MessageBox.Show("Plugin uninstalled successfully");
                return;
            }
            else
            {
                MessageBox.Show("Plugin not found.");
                return;
            }
        }

        private void ManualPatchButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                string pluginFile = Path.Combine(dialog.SelectedPath, "AuroraGSI.plugin.js");
                WriteFile(pluginFile);
            }
        }

        private void PowercordPatchButton_Click(object sender, RoutedEventArgs e)
        {
            string profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pluginDirectory = Path.Combine(profile, "powercord", "src", "Powercord", "plugins");

            if (!Directory.Exists(pluginDirectory))
            {
                MessageBox.Show("Error installing plugin: Unable to find a Powercord installation, you will need to manually install the plugin.");
                return;
			}
            if (Directory.Exists(Path.Combine(pluginDirectory, "Aurora-GSI-Powercord")))
            {
                MessageBox.Show("Error installing plugin: The pluigin is already installed.");
                return;
            }
            try
            {
                ClonePlugin(pluginDirectory);
            }
            catch (Exception er)
            {
                MessageBox.Show("Error installng plugin: " + er.Message);
            }
        }

        private void PowercordUnpatchButton_Click(object sender, RoutedEventArgs e)
        {
            string profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pluginDirectory = Path.Combine(profile, "powercord", "src", "Powercord", "plugins");
            string path = Path.Combine(pluginDirectory, "Aurora-GSI-Powercord");

            if (Directory.Exists(path))
            {
                FixGitFolderPermissions(Path.Combine(path, ".git"));
                Directory.Delete(path, true);
                MessageBox.Show("Plugin uninstalled successfully");
                return;
            }
            else
            {
                MessageBox.Show("Plugin not found.");
                return;
            }
        }

        private void PowercordManualPatchButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Select the Powercord plugins folder.");
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                string pluginFile = Path.Combine(dialog.SelectedPath);
                ClonePlugin(pluginFile);
            }
        }

        private void ClonePlugin(string directory)
		{
            var gitCommandInfo = new ProcessStartInfo("git", "clone https://github.com/ADoesGit/Aurora-GSI-Powercord.git");
            gitCommandInfo.WorkingDirectory = directory;

            Process gitProcess = Process.Start(gitCommandInfo);
            gitProcess.WaitForExit();

            if (gitProcess.ExitCode != 0)
            {
                throw new Exception ("Git clone failed, you likely don't have Git installed.");
            }
            else
            {
                MessageBox.Show("Plugin installed successfully");
                return;
            }
        }

        private void FixGitFolderPermissions(string directory)
        {
            foreach (var subdirectory in Directory.EnumerateDirectories(directory))
            {
                FixGitFolderPermissions(subdirectory);
            }
            foreach (var fileName in Directory.EnumerateFiles(directory))
            {
                var fileInfo = new FileInfo(fileName);
                fileInfo.Attributes = FileAttributes.Normal;
            }
        }

        private void WriteFile(string pluginFile)
        {
            if (File.Exists(pluginFile))
            {
                MessageBox.Show("Plugin already installed");
                return;
            }

            try
            {
                using (FileStream pluginStream = File.Create(pluginFile))
                {
                    pluginStream.Write(Properties.Resources.DiscordGSIPlugin, 0, Properties.Resources.DiscordGSIPlugin.Length);
                }
                MessageBox.Show("Plugin installed successfully");
            }
            catch (Exception e)
            {
                MessageBox.Show("Error installng plugin: " + e.Message);
            }
        }
    }
}