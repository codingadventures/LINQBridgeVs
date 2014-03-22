using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace LINQBridgeVs.Extension.Forms
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();

            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            VersionLabel.Text = fvi.ProductVersion;
            GitHubLinkLabel.LinkClicked += LinkedLabelClicked;
            LogoPictureBox.Image = Resources.LINQBridgeLogo;
        }

        private static void LinkedLabelClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/nbasakuragi/LINQBridge");
        }

        private void CloseButtonClick(object sender, EventArgs e)
        {
            Close();
        }
    }
}
