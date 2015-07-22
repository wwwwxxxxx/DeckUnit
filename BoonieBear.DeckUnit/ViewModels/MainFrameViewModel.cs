﻿using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using BoonieBear.DeckUnit.Core;
using BoonieBear.DeckUnit.DAL;
using BoonieBear.DeckUnit.Events;
using TinyMetroWpfLibrary.Events;
using TinyMetroWpfLibrary.Frames;
using TinyMetroWpfLibrary.ViewModel;
using TinyMetroWpfLibrary.EventAggregation;
using BoonieBear.DeckUnit.Models;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using CollectionMtLib;
namespace BoonieBear.DeckUnit.ViewModels
{
    /// <summary>
    ///程序主框架viewmodel，用于处理主框架消息
    /// </summary>
    public class MainFrameViewModel : MainWindowViewModelBase, IHandleMessage<StatusNotify>
    {
        public static MainFrameViewModel pMainFrame;
        private IDialogCoordinator _dialogCoordinator;

        //数据来源节点号集合
        public CollectionMtWithAsyncObservableCollectionReadOnlyCopy<string> NodeCollMt { set; get; }
        public CollectionMtWithAsyncObservableCollectionReadOnlyCopy<string> TraceCollMt { set; get; }
        public CollectionMtWithAsyncObservableCollectionReadOnlyCopy<CommandLog> DataCollMt { set; get; }
        public override void Initialize()
        {
            base.Initialize();
            
            SwapMode = RegisterCommand(ExecuteSwapMode, CanExecuteSwapMode, true);
            pMainFrame = this;
            //绑定属性初始化
            AddPropertyChangedNotification(() => StatusHeader);
            AddPropertyChangedNotification(()=>StatusDescription);
            AddPropertyChangedNotification(() => ModeType);
            StatusHeader = "水声通信机";
            StatusDescription = "正在运行";
            Level = NotifyLevel.Info;
            ModeType = true;
        }
        public IDialogCoordinator DialogCoordinator
        {
            get { return _dialogCoordinator; }
            set { _dialogCoordinator = value; }
        }
        #region 绑定属性
        public string StatusHeader
        {
            get { return GetPropertyValue(() => StatusHeader); }
            set { SetPropertyValue(() => StatusHeader, value); }
        }
        public string StatusDescription
        {
            get { return GetPropertyValue(() => StatusDescription); }
            set { SetPropertyValue(() => StatusDescription, value); }
        }
        public NotifyLevel Level
        {
            get { return GetPropertyValue(() => Level); }
            set { SetPropertyValue(() => Level, value); }
        }
        //串口or网络选择 true：网络，false：串口
        public  bool ModeType
        {
            get { return GetPropertyValue(() => ModeType); }
            set
            {
                SetPropertyValue(() => ModeType, value);
            }
        }
        public string NetInput
        {
            get { return GetPropertyValue(() => NetInput); }
            set { SetPropertyValue(() => NetInput, value); }
        }
        public string CommInput
        {
            get { return GetPropertyValue(() => CommInput); }
            set { SetPropertyValue(() => CommInput, value); }
        }

        public int FilterIndex//-1表示空
        {
            get { return GetPropertyValue(() => FilterIndex); }
            set { SetPropertyValue(() => FilterIndex, value); }
        }
        public int Filterlayer//-1表示空
        {
            get { return GetPropertyValue(() => Filterlayer); }
            set { SetPropertyValue(() => Filterlayer, value); }
        }
        public int RecvMessage
        {
            get { return GetPropertyValue(() => RecvMessage); }
            set { SetPropertyValue(() => RecvMessage, value); }
        }
        public string Shellstring
        {
            get { return GetPropertyValue(() => Shellstring); }
            set { SetPropertyValue(() => Shellstring, value); }
        }
        public string Serialstring
        {
            get {return GetPropertyValue(() => Serialstring); }
            set { SetPropertyValue(() => Serialstring, value); }
        }
        public bool LoaderMode
        {
            get { return GetPropertyValue(() => LoaderMode); }
            set { SetPropertyValue(() => LoaderMode, value); }
        }
        #endregion


        #region SwapMode CMD
        public ICommand SwapMode
        {
            get { return GetPropertyValue(() => SwapMode); }
            set { SetPropertyValue(() => SwapMode, value); }
        }


        private void CanExecuteSwapMode(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = true;
        }


        private void ExecuteSwapMode(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            ModeType = !ModeType;
        }
        #endregion

        #region SendCMD
        public ICommand SendCMD
        {
            get { return GetPropertyValue(() => SendCMD); }
            set { SetPropertyValue(() => SendCMD, value); }
        }

        


        private void CanExecuteSendCMD(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = true;
        }


        private void ExecuteSendCMD(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            if (ModeType)
            {

            }
            else
            {
                

            }
        }
        #endregion

        #region 消息响应
        public void Handle(StatusNotify message)
        {
            if (message != null)
            {
                StatusHeader = message.Source;
                StatusDescription = message.Msg;
                Level = message.Level;
            }
        }
        #endregion

        
    }
   
}
