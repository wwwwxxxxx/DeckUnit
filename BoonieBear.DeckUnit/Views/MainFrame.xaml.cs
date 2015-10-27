﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using BoonieBear.DeckUnit.ACNP;
using BoonieBear.DeckUnit.CommLib;
using BoonieBear.DeckUnit.Core;
using BoonieBear.DeckUnit.Events;
using BoonieBear.DeckUnit.JsonUtils;
using BoonieBear.DeckUnit.Models;
using Microsoft.Win32;
using TinyMetroWpfLibrary.Controller;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;
using System.IO;
using BoonieBear.DeckUnit.ViewModels;
using BoonieBear.DeckUnit.DAL;
using BoonieBear.DeckUnit.BaseType;
namespace BoonieBear.DeckUnit.Views
{

    public partial class MainFrame
    {
        public MainFrame()
        {
            InitializeComponent();
            MainFrameViewModel.pMainFrame.DialogCoordinator = DialogCoordinator.Instance;
            //DataContext = MainFrameViewModel.pMainFrame;
            
            Kernel.Instance.Controller.SetRootFrame(ContentFrame);
            //this.DebugLog.Text = MainFrameViewModel.pMainFrame.Shellstring;
            
        }

        private void ContentFrame_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

            ProgressDialogController remote = null;
            
            UnitCore.Instance.EventAggregator.PublishMessage(new GoHomePageNavigationEvent());
            UnitCore.Instance.Start();
            /*
            var remoteTask = this.ShowProgressAsync("请稍候...", "正在初始化系统");
            Task.Factory.StartNew(() => Thread.Sleep(2000)).ContinueWith(x => Dispatcher.Invoke(new Action(() =>
            {
                remote = remoteTask.Result;
                
            }))).ContinueWith(obj =>
            {
                //UnitCore.Instance.Start();
                remote.SetIndeterminate();
                //remote.SetCancelable(true);
                Dispatcher.Invoke(new Action(() =>
                {
                    if (UnitCore.Instance.ServiceOK)
                    {
                        remote.SetMessage("初始化成功!");
                    }
                    else
                    {
                        remote.SetMessage("初始化失败,详细错误信息请查看系统日志");
                    }
                }));
                Thread.Sleep(3000);
                Dispatcher.Invoke(new Action(() => remote.CloseAsync().ContinueWith(x =>
                {
                    if (!UnitCore.Instance.ServiceOK)
                    {
                        //导航到Home界面，下面的是示例
                        UnitCore.Instance.EventAggregator.PublishMessage(new GoHomePageNavigationEvent());
                        
                    }
                })));
            });*/

        }

        private void FlyOutView(int index)
        {
            var flyout = this.flyoutsControl.Items[index] as Flyout;
            if (flyout == null)
            {
                return;
            }

            flyout.IsOpen = !flyout.IsOpen;
        }
        
        private void LaunchDebugView(object sender, System.Windows.RoutedEventArgs e)
        {
            var flyout = this.flyoutsControl.Items[2] as Flyout;
            flyout.IsOpen = false;
            flyout = this.flyoutsControl.Items[1] as Flyout;
            flyout.IsOpen = false;
            FlyOutView(0);
        }

        private void LaunchDataView(object sender, System.Windows.RoutedEventArgs e)
        {
            var flyout = this.flyoutsControl.Items[2] as Flyout;
            flyout.IsOpen = false;
            flyout = this.flyoutsControl.Items[0] as Flyout;
            flyout.IsOpen = false;
            FlyOutView(1);
        }

        private async void ToggleButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
             
            if (LoadcheckButton.IsChecked==true)
            {
                
                var LoaderCheck = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "确定",
                    NegativeButtonText = "取消",
                    //FirstAuxiliaryButtonText = "Cancel",
                    ColorScheme = MetroDialogOptions.ColorScheme
                };
                MessageDialogResult result = await this.ShowMessageAsync("串口模式", "进入Loader模式？",
                MessageDialogStyle.AffirmativeAndNegative, LoaderCheck);
                if (result == MessageDialogResult.Affirmative)
                {
                    var cmd = MSPHexBuilder.Pack242();
                    var ret = UnitCore.Instance.CommEngine.SendCMD(cmd);
                    await ret;
                    if(ret.Result)
                        UnitCore.Instance.CommEngine.SerialService.ChangeMode(SerialServiceMode.LoaderMode);
                }
                else
                {
                    LoadcheckButton.IsChecked = false;
                }
                return;
            }
            if (LoadcheckButton.IsChecked==false)
            {
                
                var LoaderCheck = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "确定",
                    NegativeButtonText = "取消",
                    //FirstAuxiliaryButtonText = "Cancel",
                    ColorScheme = MetroDialogOptions.ColorScheme
                };
                MessageDialogResult result = await this.ShowMessageAsync("串口模式", "退出Loader模式？",
                MessageDialogStyle.AffirmativeAndNegative, LoaderCheck);
                if (result == MessageDialogResult.Affirmative)
                {
                    UnitCore.Instance.CommEngine.SerialService.ChangeMode(SerialServiceMode.HexMode);
                }
                else
                {
                    LoadcheckButton.IsChecked = true;
                }
                return;
            }
            
        }

        private async void SelectFileButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenFileDialog OpenMspFile =new OpenFileDialog();
            if (OpenMspFile.ShowDialog() == true)
            {
                try
                {
                    if (UnitCore.Instance.CommEngine.IsWorking)
                    {
                        await UnitCore.Instance.CommEngine.SendFile(OpenMspFile.OpenFile());
                        UnitCore.Instance.EventAggregator.PublishMessage(new LogEvent("下载数据完成", LogType.OnlyInfo));
                    }
                    else
                    {
                        UnitCore.Instance.EventAggregator.PublishMessage(new LogEvent("串口进程未正常工作", LogType.OnlyInfo));
                    }
                    
                }
                catch (Exception MyEx)
                {

                    UnitCore.Instance.EventAggregator.PublishMessage(new ErrorEvent(MyEx, LogType.Both));
                }
            }
        }

        private async void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
             var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "确定",
                NegativeButtonText = "取消",
                AnimateShow = true,
                AnimateHide = false
            };

            var result = await this.ShowMessageAsync("退出",
                "确信要退出程序吗？",
                MessageDialogStyle.AffirmativeAndNegative, mySettings);

            

            if (result == MessageDialogResult.Affirmative)
            {
                UnitCore.Instance.Stop();
                System.Windows.Application.Current.Shutdown();
            }
                
        }

        private void FilterableListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            try
            {
                CommandLog cl = (CommandLog)DataListView.SelectedItem;
                if (cl == null)
                    return;
                var fr = File.Open(cl.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var br = new BinaryReader(fr);
                UnitCore.Instance.AcnMutex.WaitOne();
                ACNProtocol.GetDataForParse(br.ReadBytes((int)fr.Length));
                if (ACNProtocol.Parse())
                {
                    var tree = StringListToTree.TransListToNodeWriteLineic(ACNProtocol.parselist);
                    UnitCore.Instance.AcnMutex.ReleaseMutex();
                    var datatree = new DataTreeModel(tree);
                    this._tree.Model = datatree;
                    MainFrameViewModel.pMainFrame.DataRecvTime = cl.LogTime.ToString();
                    var flyout = this.flyoutsControl.Items[2] as Flyout;
                    flyout.IsOpen = true;
                }
                else
                {
                    UnitCore.Instance.AcnMutex.ReleaseMutex();
                    UnitCore.Instance.EventAggregator.PublishMessage(new ErrorEvent(new Exception(ACNProtocol.Errormessage), LogType.Both));
                }
            }
            catch (Exception ex)
            {
                UnitCore.Instance.AcnMutex.ReleaseMutex();
                UnitCore.Instance.EventAggregator.PublishMessage(new ErrorEvent(ex, LogType.Both));
            }
            

        }

        private async void SetADBtn(object sender, System.Windows.RoutedEventArgs e)
        {
            var newdialog = (BaseMetroDialog) App.Current.MainWindow.Resources["SetADDialog"];
            var NumericUpDown = newdialog.FindChild<NumericUpDown>("ADNumUp");
            var cmd = MSPHexBuilder.Pack255((int)NumericUpDown.Value);
            var result = UnitCore.Instance.CommEngine.SendCMD(cmd);
            await result;
            var ret = result.Result;
            var md = new MetroDialogSettings();
            md.AffirmativeButtonText = "确定";
            if(ret==false)
                await MainFrameViewModel.pMainFrame.DialogCoordinator.ShowMessageAsync(MainFrameViewModel.pMainFrame, "发送失败",
                UnitCore.Instance.NetEngine.Error,MessageDialogStyle.Affirmative,md);
            else
            {
                var dialog = (BaseMetroDialog)App.Current.MainWindow.Resources["CustomInfoDialog"];
                dialog.Title = "设备命令";
                await MainFrameViewModel.pMainFrame.DialogCoordinator.ShowMetroDialogAsync(MainFrameViewModel.pMainFrame,
                    dialog);

                var textBlock = dialog.FindChild<TextBlock>("MessageTextBlock");
                textBlock.Text = "发送成功！";

                await TaskEx.Delay(2000);

                await MainFrameViewModel.pMainFrame.DialogCoordinator.HideMetroDialogAsync(MainFrameViewModel.pMainFrame,dialog);
                await MainFrameViewModel.pMainFrame.DialogCoordinator.HideMetroDialogAsync(MainFrameViewModel.pMainFrame, newdialog);
            }
        }

        private async void CloseSetAD(object sender, System.Windows.RoutedEventArgs e)
        {
            await MainFrameViewModel.pMainFrame.DialogCoordinator.HideMetroDialogAsync(MainFrameViewModel.pMainFrame, (BaseMetroDialog)App.Current.MainWindow.Resources["SetADDialog"]);
        }

        private async void SetWakeBtn(object sender, System.Windows.RoutedEventArgs e)
        {
            var newdialog = (BaseMetroDialog)App.Current.MainWindow.Resources["SetWakeUpDialog"];
            var NumericUpDown1 = newdialog.FindChild<NumericUpDown>("Com2WakeNumUp");
            var NumericUpDown2 = newdialog.FindChild<NumericUpDown>("Com3WakeNumUp");
            var cmd = MSPHexBuilder.Pack248((int)NumericUpDown1.Value, (int)NumericUpDown2.Value);
            var result = UnitCore.Instance.CommEngine.SendCMD(cmd);
            await result;
            var ret = result.Result;
            var md = new MetroDialogSettings();
            md.AffirmativeButtonText = "确定";
            if (ret == false)
                await MainFrameViewModel.pMainFrame.DialogCoordinator.ShowMessageAsync(MainFrameViewModel.pMainFrame, "发送失败",
                UnitCore.Instance.NetEngine.Error,MessageDialogStyle.Affirmative,md);
            else
            {
                var dialog = (BaseMetroDialog)App.Current.MainWindow.Resources["CustomInfoDialog"];
                dialog.Title = "设备命令";
                await MainFrameViewModel.pMainFrame.DialogCoordinator.ShowMetroDialogAsync(MainFrameViewModel.pMainFrame,
                    dialog);

                var textBlock = dialog.FindChild<TextBlock>("MessageTextBlock");
                textBlock.Text = "发送成功！";

                await TaskEx.Delay(2000);

                await MainFrameViewModel.pMainFrame.DialogCoordinator.HideMetroDialogAsync(MainFrameViewModel.pMainFrame, dialog);
                await MainFrameViewModel.pMainFrame.DialogCoordinator.HideMetroDialogAsync(MainFrameViewModel.pMainFrame, newdialog);
            }
        }

        private async void CloseWakeTimeBtn(object sender, System.Windows.RoutedEventArgs e)
        {
            await MainFrameViewModel.pMainFrame.DialogCoordinator.HideMetroDialogAsync(MainFrameViewModel.pMainFrame, (BaseMetroDialog)App.Current.MainWindow.Resources["SetWakeUpDialog"]);
        }

        private void DebugLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            DebugLog.ScrollToEnd();
        }

       

        private void DataListView_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                CommandLog cl = (CommandLog)DataListView.SelectedItem;
                if (cl == null)
                    return;
                var fr = File.Open(cl.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var br = new BinaryReader(fr);
                UnitCore.Instance.AcnMutex.WaitOne();
                ACNProtocol.GetDataForParse(br.ReadBytes((int)fr.Length));
                if (ACNProtocol.Parse())
                {
                    var tree = StringListToTree.TransListToNodeWriteLineic(ACNProtocol.parselist);
                    UnitCore.Instance.AcnMutex.ReleaseMutex();
                    var datatree = new DataTreeModel(tree);
                    this._tree.Model = datatree;
                    MainFrameViewModel.pMainFrame.DataRecvTime = cl.LogTime.ToString();
                    var flyout = this.flyoutsControl.Items[2] as Flyout;
                    flyout.IsOpen = true;
                }
                else
                {
                    UnitCore.Instance.AcnMutex.ReleaseMutex();
                    UnitCore.Instance.EventAggregator.PublishMessage(new ErrorEvent(new Exception(ACNProtocol.Errormessage), LogType.Both));
                }
            }
            catch (Exception ex)
            {
                UnitCore.Instance.AcnMutex.ReleaseMutex();
                UnitCore.Instance.EventAggregator.PublishMessage(new ErrorEvent(ex, LogType.Both));
            }
        }

        private void DataListView_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
  
        }

    }
}
