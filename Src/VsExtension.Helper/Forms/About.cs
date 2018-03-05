using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace BridgeVs.Helper.Forms
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
            //LogoPictureBox.Image = Images.LINQBridgeLogo;
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
