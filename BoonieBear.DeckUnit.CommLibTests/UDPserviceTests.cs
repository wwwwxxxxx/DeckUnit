﻿using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using BoonieBear.DeckUnit.CommLib;
using BoonieBear.DeckUnit.CommLib.Protocol;
using BoonieBear.DeckUnit.CommLib.UDP;
using BoonieBear.DeckUnit.Utilities.JSON;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace BoonieBear.DeckUnit.CommLibTests
{
    [TestClass]
    public class UDPserviceTests
    {
        UdpClient udpClient = null;
        private static AutoResetEvent autoReset = null;
        private static string str = null;
        [TestInitialize]
        public void Init()
        {
             udpClient=new UdpClient(8080);
            autoReset = new AutoResetEvent(false);
            ACNProtocol.Init();
        }

        class MyClass:CommLib.IObserver<CustomEventArgs>
        {
            public void Handle(object sender, CustomEventArgs e)
            {
                if (e.ParseOK)
                {
                    if (e.Mode == CallMode.NoneMode)
                    {
                        str = e.Outstring;
                        autoReset.Set();
                    }
                    if (e.Mode == CallMode.DataMode)
                    {
                        var bytes = new byte[e.DataBufferLength];
                        Buffer.BlockCopy(e.DataBuffer,0,bytes,0,e.DataBufferLength);
                        ACNProtocol.GetData(bytes);
                        if (ACNProtocol.Parse())
                        {
                            var nodetree = StringListToTree.TransListToNodeLogic(ACNProtocol.parselist);
                            var newtree = StringListToTree.RemoveFatherPointer(nodetree);
                            var jsonstr = JsonConvert.SerializeObject(newtree);
                            str = jsonstr;
                            Debug.Write(str);
                            autoReset.Set();
                        }
                        else
                        {
                            Assert.Fail();
                        }

                    }
                }
                else
                {
                    Debug.WriteLine(e.ErrorMsg);
                }
            }
        }

        
        [TestMethod]
        public void SetUpDebugUDPServiceTest()
        {
            var servicefactory = new UDPDebugServiceFactory();
            var debugservice = servicefactory.CreateService();
            debugservice.Register(new MyClass() );
            if (debugservice.Init(udpClient))
            {
                if (debugservice.Start())
                {
                    if (autoReset.WaitOne())
                    {
                        Assert.AreEqual("aaa",str);
                    }
                    else
                    {
                        
                        Assert.Fail();
                    }
                }
                else
                {

                    Assert.Fail();
                }
            }
            else
                Assert.Fail();


        }
        [TestMethod]
        public void SetUpDataServiceTest()
        {
            var servicefactory = new UDPDataServiceFactory();
            var debugservice = servicefactory.CreateService();
            debugservice.Register(new MyClass());
            if (debugservice.Init(udpClient))
            {
                if (debugservice.Start())
                {
                    if (autoReset.WaitOne())
                    {
                        Assert.IsNotNull(str,"str=null");
                    }
                    else
                    {

                        Assert.Fail();
                    }
                }
                else
                {

                    Assert.Fail();
                }
            }
            else
                Assert.Fail();
        }
        [TestMethod]
        public void BroadCastUdpTest()
        {
            
        }
    }
}
