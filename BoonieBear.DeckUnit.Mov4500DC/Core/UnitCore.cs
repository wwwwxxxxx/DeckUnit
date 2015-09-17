﻿using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using BoonieBear.DeckUnit.ACMP;
using BoonieBear.DeckUnit.CommLib.Properties;
using BoonieBear.DeckUnit.CommLib.UDP;
using BoonieBear.DeckUnit.LiveService;
using TinyMetroWpfLibrary.EventAggregation;
using BoonieBear.DeckUnit.CommLib;
using BoonieBear.DeckUnit.Mov4500Conf;
using BoonieBear.DeckUnit.Mov4500TraceService;
using BoonieBear.DeckUnit.ICore;
using BoonieBear.DeckUnit.BaseType;
using BoonieBear.DeckUnit.Mov4500UI.Events;
namespace BoonieBear.DeckUnit.Mov4500UI.Core
{
    /// <summary>
    /// 核心业务类，包括通信服务，数据解析，服务状态及一些其他的系统变量
    /// </summary>
    public class UnitCore
    {
        private readonly static object SyncObject = new object();
        //静态接口，用于在程序域中任意位置操作UnitCore中的成员
        private static UnitCore _instance;
        //事件绑定接口，用于事件广播
        private IEventAggregator _eventAggregator;
        //网络服务接口
        private INetCore _iNetCore;
        //文件服务接口
        private IFileCore _iFileCore;
        private MovTraceService _unitTraceService;
        //基础配置信息
        private MovConf _mov4500Conf;
        private MovConfInfo _movConfInfo;
        private CommConfInfo _commConf;
        private Observer<CustomEventArgs> _observer; 
        private bool _serviceStarted = false;
        public string Error { get; private set; }
        public MonitorMode WorkMode{get; private set;}
        public Mutex ACMMutex { get; set; }//全局解析锁

        
        public MovTraceService UnitTraceService
        {
            get { return _unitTraceService ?? (_unitTraceService = new MovTraceService(WorkMode)); }
        }


        public static UnitCore GetInstance()
        {
            lock (SyncObject)
            {

                return _instance ?? (_instance = new UnitCore());
            }
        }

        protected UnitCore()
        {
            
            ACMMutex = new Mutex();

        }

        public bool LoadConfiguration()
        {
            bool ret = true;
            try
            {
                _mov4500Conf = MovConf.GetInstance();
                _commConf = _mov4500Conf.GetCommConfInfo();
                _movConfInfo = _mov4500Conf.GetMovConfInfo();
                WorkMode = (MonitorMode)Enum.Parse(typeof(MonitorMode),_movConfInfo.Mode.ToString());
            }
            catch (Exception ex)
            {
                ret = false;
                EventAggregator.PublishMessage(new LogEvent(ex.Message, ex, LogType.Error));
            }
            return ret;
        }


        public IEventAggregator EventAggregator
        {
            get { return _eventAggregator ?? (_eventAggregator = UnitKernal.Instance.EventAggregator); }
        }

        

        public INetCore INetCore
        {
            get { return _iNetCore ?? (_iNetCore = NetLiveService_ACM.GetInstance(_commConf,_movConfInfo, _observer)); }
        }

        public bool Start()
        {
            try
            {
                if(!LoadConfiguration()) throw new Exception("无法读取基本配置");
                INetCore.Initialize();
                INetCore.Start();
                _serviceStarted = INetCore.IsWorking;
                Error = INetCore.Error;
                return _serviceStarted;
            }
            catch (Exception e)
            {
                Error = e.Message;
                return false;
            }
            
            
        }

        public void Stop()
        {
            if(_serviceStarted)
                INetCore.Stop();
            _serviceStarted = false;
        }
        public bool IsWorking
        {
            get { return _serviceStarted; }
        }

        public MovConf MovConfigueService
        {
            get { return _mov4500Conf; }
        }

        public Observer<CustomEventArgs> Observer
        {
            get { return _observer ?? (_observer = new Mov4500DataObserver()); }

        }

        public IFileCore IFileCore
        {
            get { return _iFileCore; }
            set { _iFileCore = value; }
        }

        public static UnitCore Instance
        {
            get { return GetInstance(); }
        }
    }
}
