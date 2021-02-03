using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VncHelperLib;

namespace VNCRemoteWPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            VNCHelpInitial();
        }

        UVncHelper _UVncHelper1;

        #region UVncHelperInitial()
        private void VNCHelpInitial()
        {
            try
            {
                txtBlockHostName.Text = UVncOption.HostName;
                txtBlockHostIP.Text = UVncOption.HostIP;

                _UVncHelper1 = new UVncHelper();

                // 訂閱事件
                SubscribeUVncHelperEvent();

                // binding 依賴屬性
                BindingVNCNotity();

                // 啟動 winvnc.exe    
                LaunchRemoteSupport();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region binding 依賴屬性
        private void BindingVNCNotity()
        {
            var bindingVNCPasswd = new Binding("UVncPasswordNotity") { Source = _UVncHelper1 };
            txtBlockVNCPasswd.SetBinding(TextBlock.TextProperty, bindingVNCPasswd);

            var bindingVNCStatus = new Binding("UVncStatusNotity") { Source = _UVncHelper1 };
            txtBlockStatus.SetBinding(TextBlock.TextProperty, bindingVNCStatus);
        }
        #endregion

        #region 訂閱 Event
        private void SubscribeUVncHelperEvent()
        {
            _UVncHelper1.SubscribeUVncHelperEvent(() =>
            {
                // 訂閱彈出 賬戶密碼輸入框 事件
                _UVncHelper1.ValidateAccountOrOpenInputAccountBoxEvent += _UVncHelper1_ValidateAccountOrOpenInputAccountBoxEvent;

                // 訂閱 ShowProgressBarEvent
                _UVncHelper1.ShowProgressBarEvent += _UVncHelper1_ShowProgressBarEvent;
            });
        }
        #endregion

        #region LaunchRemoteSupport
        private void LaunchRemoteSupport()
        {
            Task.Factory.StartNew(() =>
            {
                _UVncHelper1.ToLaunchRemoteSupport();
            });
        }
        #endregion

        #region ShowProgressBarEvent
        private bool _UVncHelper1_ShowProgressBarEvent(object sender, UVncOption option)
        {
            var vnc = (UVncHelper)sender;
            if (vnc.IsShowProgressBar)
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.IsEnabled = false;
                    progressBar1.Visibility = Visibility.Visible;
                });
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.IsEnabled = true;
                    progressBar1.Visibility = Visibility.Hidden;
                });
            }
            return true;
        }
        #endregion

        #region ValidateAccountOrOpenInputAccountBoxEvent
        private bool _UVncHelper1_ValidateAccountOrOpenInputAccountBoxEvent(object sender, UVncOption option)
        {
            return this.Dispatcher.Invoke(() =>
            {
                InputAccountWindow inputAccountWindow = new InputAccountWindow();
                inputAccountWindow.Owner = this;
                inputAccountWindow.ShowDialog();
                var input = UVncOption.ValidateInputAccount();
                return input;
            });
        }
        #endregion

        #region IsConfirmUninstallServiceEvent
        //private bool VNCHelper1_IsConfirmUninstallServiceEvent(VNCHelper sender)
        //{
        //    return this.Dispatcher.Invoke(() =>
        //    {
        //        var show = MessageBox.Show("是否刪除 uvnc服務？", "詢問", MessageBoxButton.YesNo, MessageBoxImage.Information);
        //        if (show == MessageBoxResult.Yes)
        //        {
        //            return true;
        //        }
        //        return false;
        //    });
        //}
        #endregion

        #region Menu Button Click
        private void menuReboot_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            Task.Factory.StartNew(() =>
            {
                _UVncHelper1.InvokeShowProgressBar(true);

                _UVncHelper1.ToRebootSystem();

                _UVncHelper1.InvokeShowProgressBar(false);
            });
        }

        private void menuLogOff_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Task.Factory.StartNew(() =>
            {
                _UVncHelper1.InvokeShowProgressBar(true);

                _UVncHelper1.ToLogOff();

                _UVncHelper1.InvokeShowProgressBar(false);

            });
        }

        private void menuLockComputer_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Task.Factory.StartNew(() =>
            {
                _UVncHelper1.InvokeShowProgressBar(true);

                _UVncHelper1.ToLockComputer();

                _UVncHelper1.InvokeShowProgressBar(false);
            });
        }

        #region menuInstallUvncService_Click
        //private void menuInstallUvncService_Click(object sender, RoutedEventArgs e)
        //{
        //    e.Handled = true;
        //    Task.Factory.StartNew(() => {
        //        _VNCHelper1.InvokeShowProgressBar(true);

        //        _VNCHelper1.ToInstall_uvnc_service();

        //        _VNCHelper1.InvokeShowProgressBar(false);
        //    });
        //}

        //private void menuUninstallUvncService_Click(object sender, RoutedEventArgs e)
        //{
        //    e.Handled = true;
        //    Task.Factory.StartNew(() => {

        //        _VNCHelper1.InvokeShowProgressBar(true);

        //        _VNCHelper1.ToUninstall_uvnc_service();
        //        Thread.Sleep(3000);
        //        _VNCHelper1.ExecuteWinVncExe();

        //        _VNCHelper1.InvokeShowProgressBar(false);
        //    });          
        //}
        #endregion
        #endregion

        #region 關閉 程式
        private void btnCloseApp_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            _UVncHelper1.ToCloseRemoteSupport();
        }
        #endregion

    }
}
