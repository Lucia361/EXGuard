using System;
using System.Net;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;

using EXGuard.Properties;

using DevExpress.XtraEditors;

using KeyAuth;

namespace EXGuard.Forms
{
    [System.Reflection.Obfuscation(Feature = "Apply to member * when method or constructor: virtualization", Exclude = false)]
    public partial class Login : XtraForm
    {
        public api KeyAuthApp
        {
            get;
            private set;
        }

        public WebClient GetWebClient
        {
            get;
            private set;
        }

        public Login()
        {
            GetWebClient = new WebClient();
            GetWebClient.Headers.Add("Content-Type", "binary/octet-stream");

            InitializeComponent();

            Program.GetMain = new Main();

            RememberMECheckBox.Checked = Settings.Default.RememberMe;

            UsernameBox.Text = Settings.Default.Username;
            PasswordBox.Text = Settings.Default.Password;
        }

        private void Login_Load(object sender, EventArgs e)
        {
            if (!Utils.InternetGetConnectedState(out _, 0))
            {
                MessageBox.Show("Please check your network connection and try again later.", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);

                Process.GetCurrentProcess().Kill();
            }

            KeyAuthApp = new api(name: "EXGuard", ownerid: "LQ7bYN8ZZF", secret: "bf64a128486912957bec38f420681666ecc10b0bbfb43630535a792e515af10e", version: "1.0");
            KeyAuthApp.init();
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            string serverBuffer = GetWebClient.DownloadString("http://textbin.net/public/raw/a9x1mekhim");

            if (serverBuffer == Resources.Version)
            {
                KeyAuthApp.login(UsernameBox.Text, PasswordBox.Text);

                if (KeyAuthApp.response.success)
                {
                    if (RememberMECheckBox.Checked)
                    {
                        Settings.Default.RememberMe = RememberMECheckBox.Checked;

                        Settings.Default.Username = UsernameBox.Text;
                        Settings.Default.Password = PasswordBox.Text;

                        Settings.Default.Save();
                    }
                    else
                    {
                        Settings.Default.RememberMe = RememberMECheckBox.Checked;

                        Settings.Default.Username = string.Empty;
                        Settings.Default.Password = string.Empty;

                        Settings.Default.Save();
                    }

                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    XMLUtils.UserName = UsernameBox.Text;
                    XMLUtils.Password = PasswordBox.Text;

                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    MessageBox.Show($"Welcome \"{ UsernameBox.Text }\"!", "Info?", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    this.Visible = false;
                    this.Hide();

                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    Program.GetMain.ShowDialog();

                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    this.Close();
                }
                else
                    MessageBox.Show(KeyAuthApp.response.message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                var dialog = MessageBox.Show("EXGuard has been updated. Shall we direct you to the download address of the new version?", "Info?", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (dialog == DialogResult.OK)
                    Process.Start(GetWebClient.DownloadString("https://textbin.net/raw/fawjcw9zsx"));

                Environment.Exit(0);
            }           
        }

        private void NULinkLabel0_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.gg/WNWf3QArZh");
        }
    }
}