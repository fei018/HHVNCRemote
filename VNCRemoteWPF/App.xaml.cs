using System;
using System.Windows;
using VncHelperLib;

namespace VNCRemoteWPF
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.STAThreadAttribute()]
        public static void Main()
        {
            AppMutex();
        }

        #region AppMutex
        private static void AppMutex()
        {
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, "RemoteSupport123456", out bool flag);
            //第一个参数:true--给调用线程赋予互斥体的初始所属权  
            //第一个参数:互斥体的名称  
            //第三个参数:返回值,如果调用线程已被授予互斥体的初始所属权,则返回true  
            if (!flag)
            {
                MessageBox.Show("程式已運行！", "信息", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Environment.Exit(1);//退出程序  
            }
            else
            {
                VNCRemoteWPF.App app = new VNCRemoteWPF.App();
                app.InitializeComponent();
                app.Run();
            }
        }
        #endregion


        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if(e != null && e.Args != null)
            {
                UVncOption.ToLauchRemoteSupportGetArguments(e.Args);
            }
            else
            {
                UVncOption.ToLauchRemoteSupportGetArguments(null);
            }
           
        }
    }
}
