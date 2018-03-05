using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using BridgeVs.Helper.Installer.VsRestart;
using EnvDTE;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace BridgeVs.Helper.Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Welcome : MetroWindow, System.IDisposable
    {
        private readonly LinkedList<MetroTabItem> Tabs;
        private LinkedListNode<MetroTabItem> _currentTab;
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

        //private async void BtnNext_Click(object sender, RoutedEventArgs e)
        //{
        //    SetNextTab();
        //    if (_currentTab.Value.Name == "hdrSteady")
        //    {
        //        if (!IsElevated)
        //        {
        //            grdPrerequisites.Visibility = Visibility.Visible;
        //            grdInstallation.Visibility = Visibility.Hidden;
        //            btnNext.IsEnabled = false;
        //        }
        //        else
        //        {
        //            grdPrerequisites.Visibility = Visibility.Hidden;
        //            grdInstallation.Visibility = Visibility.Visible;
        //            await Task.Run(() => Install());
        //            prgInstallProgress.IsActive = false;
        //            SetNextTab();
        //            btnNext.Visibility = Visibility.Hidden;
        //            btnSkip.Content = "Finish";
        //        }
        //    }

        //    if (_currentTab.Value.Name == "hdrReady")
        //    {
        //        this.Close();
        //    }
        //}

        //private void SetNextTab()
        //{
        //    _currentTab.Value.Focusable = false;
        //    _currentTab = _currentTab.Next ?? _currentTab.List.First;
        //    _currentTab.Value.Focusable = true;
        //    _currentTab.Value.Focus();
        //}
       
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
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Quit",
                NegativeButtonText = "Cancel",
                AnimateShow = true,
                AnimateHide = false
            };

            var result = await this.ShowMessageAsync("Quit Wizard?",
                                                  BridgeVs.Helper.Resources.SkipMessage,
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
