﻿using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using BoonieBear.DeckUnit.Core;
using BoonieBear.DeckUnit.Events;
using BoonieBear.DeckUnit.Models;
using BoonieBear.DeckUnit.Resource;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TinyMetroWpfLibrary.EventAggregation;
using TinyMetroWpfLibrary.Events;
using TinyMetroWpfLibrary.ViewModel;

namespace BoonieBear.DeckUnit.ViewModels
{

    public class ADViewModel : ViewModelBase, IHandleMessage<UpdateADByteCount>
    {
        #region Overrides of ViewModelBase

        private int lastCount = 0;
        private int currentCount = 0;
        private DispatcherTimer t;

        public override void Initialize()
        {
            GoBackCommand = RegisterCommand(ExecuteGoBackCommand, CanExecuteGoBackCommand, true);
            BeginAD = RegisterCommand(ExecuteBeginAD, CanExecuteBeginAD, true);
            StopAD = RegisterCommand(ExecuteStopAD, CanExecuteStopAD, true);
        }


        public override void InitializePage(object extraData)
        {
            IsWorking = false;
            SetGain = 39;
            TotalADByte = "-";
            RefreshInfos();
        }
        private void RefreshInfos()
        {
            var ir = GetSystemInfo.CreateResourcesProbe();
            if (MemInfos == null)
                MemInfos = new ObservableCollection<SystemInfo>();
            MemInfos.Clear();
            string MyExecPath = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);
            string diskname = MyExecPath.Substring(0, 2);
            double freesize = ir.GetDiskFree(diskname);//KB
            freesize /= 1024;//MB
            freesize /= 1024;//GB
            double totalsize = ir.GetDiskSize(diskname);//KB
            totalsize /= 1024;//MB
            totalsize /= 1024;//GB
            MemInfos.Add(new SystemInfo() { Name = "空闲", Size = Math.Round(freesize,2) });
            MemInfos.Add(new SystemInfo() { Name = "已使用", Size = Math.Round(totalsize - freesize,2) });
        }
        #endregion

        #region 绑定属性

        public string Status
        {
            get { return GetPropertyValue(() => Status); }
            set { SetPropertyValue(() => Status, value); }
        }
        public string SpeedStr
        {
            get { return GetPropertyValue(() => SpeedStr); }
            set { SetPropertyValue(() => SpeedStr, value); }
        }
        public bool IsWorking
        {
            get { return GetPropertyValue(() => IsWorking); }
            set { SetPropertyValue(() => IsWorking, value); }
        }
        public int SetGain
        {
            get { return GetPropertyValue(() => SetGain); }
            set { SetPropertyValue(() => SetGain, value); }
        }
        public string TotalADByte
        {
            get { return GetPropertyValue(() => TotalADByte); }
            set { SetPropertyValue(() => TotalADByte, value); }
        }
        public string ADPath
        {
            get { return GetPropertyValue(() => ADPath); }
            set { SetPropertyValue(() => ADPath, value); }
        }
        public ObservableCollection<SystemInfo> MemInfos
        {
            get { return GetPropertyValue(() => MemInfos); }
            set { SetPropertyValue(() => MemInfos, value); }
        }
        #endregion

        #region GoBack Command


        public ICommand GoBackCommand
        {
            get { return GetPropertyValue(() => GoBackCommand); }
            set { SetPropertyValue(() => GoBackCommand, value); }
        }


        public void CanExecuteGoBackCommand(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = true;
        }


        public async void ExecuteGoBackCommand(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            if (IsWorking)
            {
                var md = new MetroDialogSettings();
                md.AffirmativeButtonText = "确定";
                await MainFrameViewModel.pMainFrame.DialogCoordinator.ShowMessageAsync(MainFrameViewModel.pMainFrame, "提示",
                "尚在接收AD数据中，请先停止AD采集再离开页面", MessageDialogStyle.Affirmative, md);
                return;
            }
            EventAggregator.PublishMessage(new GoBackNavigationRequest());
        }

        public ICommand BeginAD
        {
            get { return GetPropertyValue(() => BeginAD); }
            set { SetPropertyValue(() => BeginAD, value); }
        }


        public void CanExecuteBeginAD(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = !IsWorking;
        }


        public async void ExecuteBeginAD(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "选择AD保存目录";
            fbd.RootFolder = Environment.SpecialFolder.MyComputer;
            fbd.ShowNewFolderButton = true;
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    UnitCore.Instance.UnitTraceService.SetADFilePath(fbd.SelectedPath);
                    string NetInput = "ad -r "+SetGain.ToString();
                    if(!UnitCore.Instance.NetEngine.IsWorking)
                        throw new Exception("没有有效网络连接");
                    await UnitCore.Instance.NetEngine.SendConsoleCMD(NetInput);
                    t = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, Tick, Dispatcher.CurrentDispatcher);
                    IsWorking = true;
                    lastCount = 0;
                    currentCount = 0;
                }
                catch (Exception ex)
                {
                    UnitCore.Instance.EventAggregator.PublishMessage(new ErrorEvent(ex, LogType.Both));
 
                }
            }
        }
        void Tick(object sender, EventArgs e)
        {
            int speed = currentCount - lastCount;
            lastCount = currentCount;
            if (speed > 1024)
                SpeedStr = ((float)speed/1024).ToString("F2") + " KB/s";
            else
            {
                SpeedStr = speed.ToString() + " B/s";
            }
        }
        public ICommand StopAD
        {
            get { return GetPropertyValue(() => StopAD); }
            set { SetPropertyValue(() => StopAD, value); }
        }


        public void CanExecuteStopAD(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = true;
        }


        public async void ExecuteStopAD(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            char cesc = (char)27;
            await UnitCore.Instance.NetEngine.SendConsoleCMD(cesc.ToString());
            t.Stop();
            t.IsEnabled = false;
            await TaskEx.Delay(3000);
            Status = "";
            IsWorking = false;
        }
        #endregion

        public void Handle(UpdateADByteCount message)
        {
            Status = "接收AD数据中……";
            //IsWorking = true;
            currentCount = message.AdCount;
            if (currentCount > 1024*1024)
                TotalADByte = ((float)currentCount/1024/1024).ToString("F2") + " MB";
            else if (currentCount > 1024)
                TotalADByte = ((float)currentCount/1024).ToString("F2") + " KB";
            else //must be Byte, GB is too large to storage
            {
                TotalADByte = currentCount.ToString() + " Bytes";
            }
        }
    }
}
