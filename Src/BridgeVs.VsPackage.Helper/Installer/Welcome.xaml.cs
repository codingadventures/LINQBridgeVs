using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using BridgeVs.VsPackage.Helper.Installer.VsRestart;
using EnvDTE;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace BridgeVs.VsPackage.Helper.Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Welcome :  System.IDisposable
    {
        private readonly LinkedList<MetroTabItem> Tabs;
        private readonly LinkedListNode<MetroTabItem> _currentTab;
        private volatile DTE _dte;

        public Welcome()
        {
            InitializeComponent();
            Tabs = new LinkedList<MetroTabItem>();
            // Tabs.AddLast(hdrReady);
            Tabs.AddLast(hdrSteady);
            // Tabs.AddLast(hdrGo);
            _currentTab = Tabs.First;
        }

        public Welcome(DTE dte) : this()
        {
            _dte = dte;
        }
        
        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            //restart logic
            _dte.Restart();
        }

        private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            if (_currentTab.Value.Name == "hdrGo")
                this.Close();
            else
                MetroWindow_Closing(sender, new System.ComponentModel.CancelEventArgs(true));
        }

        private async void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_currentTab.Value.Name == "hdrGo")
            {
                return;
            }
            MetroDialogSettings mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Quit",
                NegativeButtonText = "Cancel",
                AnimateShow = true,
                AnimateHide = false
            };

            MessageDialogResult result = await this.ShowMessageAsync("Quit Wizard?",
                                                  Configuration.Resources.SkipMessage,
                                                  MessageDialogStyle.AffirmativeAndNegative, mySettings);

            bool shutdown = result == MessageDialogResult.Affirmative;

            if (shutdown)
            {
                this.Close();
            }
        }

        public void Dispose()
        {
            _dte = null;
        }
    }
}
