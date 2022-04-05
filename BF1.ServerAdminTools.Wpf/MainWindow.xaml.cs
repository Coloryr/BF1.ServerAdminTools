﻿using BF1.ServerAdminTools.Common.Models;
using BF1.ServerAdminTools.Common.Utils;
using BF1.ServerAdminTools.Common.Views;

namespace BF1.ServerAdminTools.Common
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 主窗口全局提示信息委托
        /// </summary>
        public static Action<int, string> _SetOperatingState;
        /// <summary>
        /// 主窗口选项卡控件选择委托
        /// </summary>
        public static Action<int> _TabControlSelect;

        public delegate void ClosingDispose();
        public static event ClosingDispose ClosingDisposeEvent;


        public static MainWindow ThisMainWindow;

        public MainModel MainModel { get; set; }

        // 声明一个变量，用于存储软件开始运行的时间
        private DateTime Origin_DateTime;

        ///////////////////////////////////////////////////////

        public MainWindow()
        {
            InitializeComponent();

            // 提示信息委托
            _SetOperatingState = SetOperatingState;
            // TabControl 选择切换委托
            _TabControlSelect = TabControlSelect;
        }

        private void Window_Main_Loaded(object sender, RoutedEventArgs e)
        {
            MainModel = new MainModel();

            MainModel.AppRunTime = "运行时间 : Loading...";

            ThisMainWindow = this;

            ////////////////////////////////

            Title = CoreUtil.MainAppWindowName + CoreUtil.ClientVersionInfo + "预览版 - 最后编译时间 : " + File.GetLastWriteTime(Process.GetCurrentProcess().MainModule.FileName);

            // 获取当前时间，存储到对于变量中
            Origin_DateTime = DateTime.Now;

            ////////////////////////////////

            new Thread(UpdateState)
            {
                Name = "UpdateStateThead",
                IsBackground = true
            }.Start();

            new Thread(InitThread)
            {
                Name = "InitThread",
                IsBackground = true
            }.Start();

            this.DataContext = this;

            if (Core.MsgAllocateMemory())
                Core.LogInfo($"中文聊天指针分配成功 0x{Core.MsgGetAllocateMemoryAddress():x}");
            else
                Core.LogError($"中文聊天指针分配失败");
        }

        private void Window_Main_Closing(object sender, CancelEventArgs e)
        {
            // 关闭事件
            ClosingDisposeEvent();
            Core.LogInfo($"调用关闭事件成功");
            Core.SaveConfig();
            ConfigUtil.SaveAll();
            Core.SQLClose();
            Core.MsgFreeMemory();
            Core.HookClose();

            Core.LogInfo($"程序关闭\n\n");
            Application.Current.Shutdown();
        }

        private void InitThread()
        {
            // 调用刷新SessionID功能
            Core.LogInfo($"开始调用刷新SessionID功能");
            AuthView._AutoRefreshSID();
        }

        private void UpdateState()
        {
            while (true)
            {
                // 获取软件运行时间
                MainModel.AppRunTime = "运行时间 : " + CoreUtil.ExecDateDiff(Origin_DateTime, DateTime.Now);

                if (Globals.IsGameRun)
                {
                    if (!Core.IsGameRun())
                    {
                        Globals.IsToolInit = false;
                        Globals.IsGameRun = false;
                        MsgBoxUtil.WarningMsgBox("游戏已退出，功能已关闭");
                    }
                }

                Thread.Sleep(1000);
            }
        }

        #region 常用方法
        /// <summary>
        /// 提示信息，绿色信息1，灰色警告2，红色错误3
        /// </summary>
        /// <param name="index">绿色信息1，灰色警告2，红色错误3</param>
        /// <param name="str">消息内容</param>
        private void SetOperatingState(int index, string str)
        {
            if (index == 1)
            {
                Border_OperateState.Background = Brushes.Green;
                TextBlock_OperateState.Text = $"信息 : {str}";
            }
            else if (index == 2)
            {
                Border_OperateState.Background = Brushes.Gray;
                TextBlock_OperateState.Text = $"警告 : {str}";
            }
            else if (index == 3)
            {
                Border_OperateState.Background = Brushes.Red;
                TextBlock_OperateState.Text = $"错误 : {str}";
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ProcessUtil.OpenLink(e.Uri.OriginalString);
            e.Handled = true;
        }
        #endregion

        ///////////////////////////////////////////////////////

        private void TabControlSelect(int index)
        {
            TabControl_Main.SelectedIndex = index;
        }
    }
}
