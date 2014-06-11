using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Arshu.Web.Basic.Utility;
using Arshu.Web.Common;
using Arshu.Web.IO;
using Arshu.Web.Json;

using Arshu.AppGrid.Peer;

namespace Arshu.AppGrid
{
    public class PeerClientInfo
    {
        public string HostName { get; set; }
        public string Address { get; set; }
        public string Message { get; set; }
        public string ServiceType { get; set; }
        public string ServiceName { get; set; }
        public int ServicePort { get; set; }
    }


    [JsonRpcHelp("Json Peer Handler", IsSecure = true)]
    public class JsonPeerHandler : JsonRpcHandler
    {
        public static string JsonPeerHandlerName = "JsonPeer.ashx";
        public static string JsonPeerHandlerTypeName = typeof(JsonPeerHandler).FullName;

        private static JsonPeerService service = new JsonPeerService();
        public JsonPeerHandler()
        {
            if (service == null) service = new JsonPeerService();
        }
    }

    [JsonRpcHelp("Json Peer Service")]
    public class JsonPeerService : JsonRpcService
    {
        [JsonRpcMethod("RegisterPeerService", Idempotent = true)]
        [JsonRpcHelp("Register Service [gridApp._tcp, Grid App, 2000]. Returns(status, message)")]
        public JsonObject RegisterPeerService(string serviceType, string serviceName, int servicePort, int serviceFrequency, string broadcastAddress, bool enableBroadcast, bool exclusiveAddressUse)
        {
            JsonObject retMessage = new JsonObject();

            try
            {
                bool status = IsGridServiceRegistered();
                if (status == false)
                {
                    bool gridRegistered = RegisterGridService(serviceType, serviceName, servicePort, serviceFrequency, broadcastAddress, enableBroadcast, exclusiveAddressUse);
                    if (gridRegistered == true)
                    {
                        retMessage.Add("status", "true");
                        retMessage.Add("message", "Successfully Registered Peer Service");
                    }
                    else
                    {
                        retMessage.Add("error", "Unable to Create the Peer Service");
                    }
                }
                else
                {
                    retMessage.Add("error", "Please Unregister the Existing Service before Registering Again");
                }
            }
            catch (Exception ex)
            {
                retMessage.Add("error", "Error:" + ex.Message);
            }

            return retMessage;
        }

        [JsonRpcMethod("IsPeerRegistered", Idempotent = false)]
        [JsonRpcHelp("Get where the Peer Service has been Registered. Returns JsonObject(status)")]
        public JsonObject IsPeerRegistered()
        {
            var retMessage = new JsonObject();
            bool status = IsGridServiceRegistered();
            if (status == true)
            {
                retMessage.Add("status", "true");
                retMessage.Add("message", "Peer Service is Registered");
            }
            else
            {
                retMessage.Add("error", "Peer Service is Not Registered");
            }
            return retMessage;
        }

        [JsonRpcMethod("UnRegisterPeerService", Idempotent = true)]
        [JsonRpcHelp("UnRegister Peer Service Returns(status, message)")]
        public JsonObject UnRegisterPeerService()
        {
            JsonObject retMessage = new JsonObject();

            try
            {
                bool status = IsGridServiceRegistered();
                if (status == true)
                {
                    bool gridUnRegistered = UnRegisterGridService();
                    if (gridUnRegistered == true)
                    {
                        retMessage.Add("status", "true");
                        retMessage.Add("message", "Successfully UnRegistered Peer Service");
                    }
                    else
                    {
                        retMessage.Add("error", "Error in UnRegistering Grid Service");
                    }
                }
                else
                {
                    retMessage.Add("error", "Grid Service has not been Registered");
                }
            }
            catch (Exception ex)
            {
                retMessage.Add("error", "Error:" + ex.Message);
            }

            return retMessage;
        }

        [JsonRpcMethod("SendPeerMessage", Idempotent = false)]
        [JsonRpcHelp("Send Peer Message. Returns JsonObject(status)")]
        public JsonObject SendPeerMessage(string peerMessage)
        {
            var retMessage = new JsonObject();
            bool status = IsGridServiceRegistered();
            if (status == true)
            {
                status = SetGridMessage(peerMessage);
                if (status == true)
                {
                    retMessage.Add("status", "true");
                    retMessage.Add("message", "Peer Message is Set");
                }
                else
                {
                    retMessage.Add("error", "Peer Messaage is Not Set");
                }
            }
            else
            {
                retMessage.Add("error", "Peer Service is not registered");
            }

            return retMessage;
        }

        [JsonRpcMethod("IsPeerBrowserStarted", Idempotent = false)]
        [JsonRpcHelp("Get where the Peer Browser has been Started. Returns JsonObject(status)")]
        public JsonObject IsPeerBrowserStarted()
        {
            var retMessage = new JsonObject();
            bool status = IsGridBrowserStarted();
            if (status == true)
            {
                retMessage.Add("status", "true");
                retMessage.Add("message", "Peer Browser is Started");
            }
            else
            {
                retMessage.Add("error", "Peer Browser is Not Started");
            }
            return retMessage;
        }

        [JsonRpcMethod("StartPeerBrowser", Idempotent = true)]
        [JsonRpcHelp("Start the Peer Browser[gridApp._tcp]. Returns(status, message)")]
        public JsonObject StartPeerBrowser(string serviceType, bool receiveFromLocal)
        {
            JsonObject retMessage = new JsonObject();

            try
            {
                bool status = IsGridBrowserStarted();
                if (status == false)
                {
                    bool ret = StartGridBrowser(serviceType, receiveFromLocal);
                    if (ret == true)
                    {
                        retMessage.Add("status", "true");
                        retMessage.Add("message", "Successfully Started Grid Browser");
                    }
                    else
                    {
                        retMessage.Add("error", "Unable to Start the Grid Browser");
                    }
                }
                else
                {
                    retMessage.Add("error", "Grid Browser is Allready Started");
                }
            }
            catch (Exception ex)
            {
                retMessage.Add("error", "Error:" + ex.Message);
            }

            return retMessage;
        }

        [JsonRpcMethod("StopPeerBrowser", Idempotent = true)]
        [JsonRpcHelp("Stop the Peer Browser[gridApp._tcp]. Returns(status, message)")]
        public JsonObject StopPeerBrowser()
        {
            JsonObject retMessage = new JsonObject();

            try
            {
                bool status = IsGridBrowserStarted();
                if (status == true)
                {
                    bool ret = StopGridBrowser();
                    if (ret == true)
                    {
                        retMessage.Add("status", "true");
                        retMessage.Add("message", "Successfully Stopped Grid Browser");
                    }
                    else
                    {
                        retMessage.Add("error", "Unable to Stopped the Grid Browser");
                    }
                }
                else
                {
                    retMessage.Add("error", "Grid Browser is not Started");
                }
            }
            catch (Exception ex)
            {
                retMessage.Add("error", "Error:" + ex.Message);
            }

            return retMessage;
        }

        [JsonRpcMethod("GetPeerClientInfo", Idempotent = true)]
        [JsonRpcHelp("Get the Peer Client Info. Returns(peerlist, message)")]
        public JsonObject GetPeerClientInfo(string serviceType)
        {
            JsonObject retMessage = new JsonObject();

            try
            {
                if (IsGridBrowserStarted() == true)
                {
                    Dictionary<string, GridPeerClient> peerList = GetGridClient();
                    if (peerList.Count > 0)
                    {
                        List<PeerClientInfo> peerClientInfoList = new List<PeerClientInfo>();
                        foreach (var item in peerList)
                        {
                            PeerClientInfo peerClientInfo = new PeerClientInfo();
                            peerClientInfo.HostName = item.Value.HostName;
                            peerClientInfo.Address = item.Value.Address.ToString();

                            peerClientInfo.Message = item.Value.Message;
                            peerClientInfo.ServiceType = item.Value.ServiceType;
                            peerClientInfo.ServiceName = item.Value.Name;
                            peerClientInfo.ServicePort = item.Value.Port;

                            peerClientInfoList.Add(peerClientInfo);
                        }
                        retMessage.Add("peerlist", peerClientInfoList);
                        retMessage.Add("message", "Successfully Retrieved Peer Client");
                    }
                    else
                    {
                        retMessage.Add("error", "Unable to find any Peer Client");
                    }
                }
                else
                {
                    retMessage.Add("error", "Grid Browser has not been Started");
                }
            }
            catch (Exception ex)
            {
                retMessage.Add("error", "Error:" + ex.Message);
            }

            return retMessage;
        }

        #region Grid Service Methods

        private static GridPeerService _gridService;
        private static int _gridUnregisterLoopCounter = 0;
        private const int _maxGridUnregisterLoopCounter = 50;

        private bool RegisterGridService(string serviceType, string serviceName, int servicePort, int serviceFrequency, string broadcastAddress, bool enableBroadcast, bool exclusiveAddressUse)
        {
            bool ret = false;
            if (_gridService != null)
            {
                _gridService.Unregister();
                _gridUnregisterLoopCounter = 0;
                while ((_gridServiceRegistered == true) && (_gridUnregisterLoopCounter < _maxGridUnregisterLoopCounter))
                {
                    System.Threading.Thread.Sleep(100);
                    _gridUnregisterLoopCounter++;
                }
                _gridService = null;
            }
            if (_gridService == null)
            {
                _gridService = new GridPeerService(serviceType, serviceName, servicePort, serviceFrequency, broadcastAddress, enableBroadcast, exclusiveAddressUse);
            }
            if (_gridService != null)
            {
                _gridService.Registered -= gridService_Registered;
                _gridService.Registered += gridService_Registered;

                _gridService.Unregistered -= gridService_Unregistered;
                _gridService.Unregistered += gridService_Unregistered;

                _gridService.BroadcastFailed -= _gridService_BroadcastFailed;
                _gridService.BroadcastFailed += _gridService_BroadcastFailed;

                _gridService.Message = serviceType + " Registered";
                _gridService.Register();
                ret = true;
            }

            return ret;
        }

        private bool IsGridServiceRegistered()
        {
            bool ret = false;
            if ((_gridService != null) && (_gridServiceRegistered == true))
            {
                ret = true;
            }
            return ret;
        }

        private bool UnRegisterGridService()
        {
            bool ret = false;

            if ((_gridService != null) && (_gridServiceRegistered == true))
            {
                _gridService.Unregister();
                ret = true;
            }

            return ret;
        }

        private bool SetGridMessage(string peerMessage)
        {
            bool ret = false;
            if ((_gridService != null) && (_gridServiceRegistered == true))
            {
                _gridService.Message = peerMessage;
                ret = true;
            }
            return ret;
        }
        #endregion

        #region Grid Service Event Handlers

        void _gridService_BroadcastFailed(object sender, EventArgs e)
        {
            _gridService.Registered -= gridService_Registered;
            _gridService.Unregistered -= gridService_Unregistered;
            _gridService.BroadcastFailed -= _gridService_BroadcastFailed;
            _gridServiceRegistered = false;
            _gridService = null;
        }

        private bool _gridServiceRegistered = false;
        void gridService_Registered(object sender, EventArgs e)
        {
            _gridServiceRegistered = true;
        }

        void gridService_Unregistered(object sender, EventArgs e)
        {
            _gridServiceRegistered = false;
        }

        #endregion

        #region Grid Browser Methods

        private bool IsGridBrowserStarted()
        {
            bool ret = false;
            if ((_gridBrowser != null) && (_gridBrowserStarted==true))
            {
                ret = true;
            }
            return ret;
        }

        private static GridPeerBrowser _gridBrowser;
        private static int _gridStopLoopCounter = 0;
        private const int _maxGridStopLoopCounter = 50;
        private bool StartGridBrowser(string serviceType, bool receiveFromLocal)
        {
            bool ret = false;
            if (_gridBrowser != null)
            {
                _gridBrowser.Stop();
                _gridStopLoopCounter = 0;
                while ((_gridBrowserStarted == true) && (_gridStopLoopCounter < _maxGridStopLoopCounter))
                {
                    System.Threading.Thread.Sleep(100);
                    _gridStopLoopCounter++;
                }
                _gridBrowser = null;
            }
            if (_gridBrowser == null) _gridBrowser = new GridPeerBrowser();
            if (_gridBrowser != null)
            {
                _gridBrowser.ReceiveFromLocalMachine = receiveFromLocal;

                _gridBrowser.Started -= _gridBrowser_Started;
                _gridBrowser.Started += _gridBrowser_Started;

                _gridBrowser.Stopped -= _gridBrowser_Stopped;
                _gridBrowser.Stopped += _gridBrowser_Stopped;

                _gridBrowser.BrowserFailed -= _gridBrowser_BrowserFailed;
                _gridBrowser.BrowserFailed += _gridBrowser_BrowserFailed;

                _gridBrowser.ClientAppeared -= _gridBrowser_ClientAppeared;
                _gridBrowser.ClientAppeared += _gridBrowser_ClientAppeared;

                _gridBrowser.ClientDisappeared -= _gridBrowser_ClientDisappeared;
                _gridBrowser.ClientDisappeared += _gridBrowser_ClientDisappeared;

                _gridBrowser.ClientMessageChanged -= _gridBrowser_ClientMessageChanged;
                _gridBrowser.ClientMessageChanged += _gridBrowser_ClientMessageChanged;

                _gridBrowser.Start(serviceType);
                ret = true;
            }
            return ret;
        }

        private bool StopGridBrowser()
        {
            bool ret = false;
            if (_gridBrowser != null)
            {
                _gridBrowser.Stop();
                _gridStopLoopCounter = 0;
                while ((_gridBrowserStarted == true) && (_gridStopLoopCounter < _maxGridStopLoopCounter))
                {
                    System.Threading.Thread.Sleep(100);
                    _gridStopLoopCounter++;
                }
                _gridBrowser = null;
                ret = true;
            }
            return ret;
        }

        private Dictionary<string, GridPeerClient> GetGridClient()
        {
            Dictionary<string, GridPeerClient> peerClientList = new Dictionary<string, GridPeerClient>();
            foreach (var item in _gridPeerClientList)
            {
                peerClientList.Add(item.Key, item.Value);
            }
            return peerClientList;
        }

        #endregion

        #region Grid Browser Event Handlers

        private bool _gridBrowserStarted = false;
        void _gridBrowser_Stopped(object sender, EventArgs e)
        {
            _gridBrowserStarted = false;
        }

        void _gridBrowser_Started(object sender, EventArgs e)
        {
            _gridBrowserStarted = true;
        }

        void _gridBrowser_BrowserFailed(object sender, EventArgs e)
        {
            _gridBrowser.BrowserFailed -= _gridBrowser_BrowserFailed;
            _gridBrowser.ClientAppeared -= _gridBrowser_ClientAppeared;
            _gridBrowser.ClientDisappeared -= _gridBrowser_ClientDisappeared;
            _gridBrowser.ClientMessageChanged -= _gridBrowser_ClientMessageChanged;
            _gridBrowserStarted = false;

            _gridBrowser = null;
        }

        private static SafeDictionary<string, GridPeerClient> _gridPeerClientList = new SafeDictionary<string, GridPeerClient>();
        void _gridBrowser_ClientAppeared(object sender, GridPeerClientEventArgs e)
        {
            string key = e.Client.Address.ToString();
            _gridPeerClientList.Add(key, e.Client);
        }

        void _gridBrowser_ClientDisappeared(object sender, GridPeerClientEventArgs e)
        {
            string key = e.Client.Address.ToString();
            _gridPeerClientList.Remove(key);
        }

        void _gridBrowser_ClientMessageChanged(object sender, GridPeerClientEventArgs e)
        {
            string key = e.Client.Address.ToString();
            _gridPeerClientList.Remove(key);
            _gridPeerClientList.Add(key, e.Client);
        }

        #endregion
    }
}
