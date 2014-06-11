using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Timers;

using Arshu.Web.Basic.Log;

namespace Arshu.AppGrid.Peer
{
    /// <summary>
    /// This class is used to register GridPeerService. 
    /// </summary>
    /// <remarks>GridPeerService registers a unique service that GridBrowser clients will find it immediately.</remarks>
    public class GridPeerService : IDisposable
    {
        #region Get IPAddress

        public delegate string GetIPAddress();
        public static event GetIPAddress OnGetIPAddress;
        private static string GetServiceIPAddress()
        {
            string retIPAddress = "";
            if (OnGetIPAddress != null)
            {
                try
                {
                    retIPAddress = OnGetIPAddress();
                }
                catch (Exception ex)
                {
                    LogManager.Log(LogType.Error, "GridPeerService-GetIPAddress", "Error:[" + ex.Message + "]");
                }
            }
            return retIPAddress;
        }

        #endregion

        #region Variables

        public const string GridMessageName = "ArshuGrid";

        /// <summary>
        /// The default GridService port.
        /// </summary>
        private const int port = 54183;

        //private const int defaultFrequency = 45;

        private string hostname;

        private readonly Timer timer;

        private IPEndPoint ipEndPoint;

        private UdpClient udpClient;

        private double frequency;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="GridPeerService"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled
        {
            get { return timer.Enabled; }
            set { timer.Enabled = value; }
        }

        /// <summary>
        /// Gets or sets the frequency in minute.
        /// </summary>
        /// <value>The frequency.</value>
        public double Frequency
        {
            get
            {
                return frequency;
            }
            set
            {
                frequency = value;

                timer.Interval = (1000 * 60) / frequency;
            }
        }

        /// <summary>
        /// Gets or sets the broadcast address.
        /// </summary>
        /// <value>The broadcast address.</value>
        /// <remarks>The default IPAddress is 255.255.255.255 which broadcasts to all the networks.</remarks>
        public IPAddress BroadcastAddress { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        /// <remarks>This is a custom message for your application.</remarks>
        public string Message { get; set; }

        /// <summary>
        /// Gets the application name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        /// <remarks>The service type is a custom string meant to be used to identify the GridPeerService. The universal form of this string is preferred to be <c>_applicationProtocolName</c>.<c>_networkProtocolName</c>. e.g. _teleporter._tcp </remarks>
        public string ServiceType { get; private set; }

        /// <summary>
        /// Gets the application port.
        /// </summary>
        public int Port { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the GridPeerService fails to create the service.
        /// </summary>
        public event EventHandler CreationFailed;

        /// <summary>
        /// Occurs when the GridPeerService fails to broadcast on the network.
        /// </summary>
        public event EventHandler BroadcastFailed;

        /// <summary>
        /// Occurs when the service is successfully registered.
        /// </summary>
        public event EventHandler Registered;

        /// <summary>
        /// Occurs when the service is successfully unregistered.
        /// </summary>
        public event EventHandler Unregistered;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GridPeerService"/> class.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="name">The name of the application.</param>
        /// <param name="port">The port at which the mail application is listening.</param>
        public GridPeerService(string serviceType, string name, int port, int serviceFrequency, string broadcastAddress, bool enableBroadcast, bool exclusiveAddressUse)
        {
            if (serviceType.Contains(";"))
            {
                throw new ArgumentException("Semicolon character is not allowed in ServiceType argument.");
            }

            if (name.Contains(";"))
            {
                throw new ArgumentException("Semicolon character is not allowed in Name argument.");
            }

            ServiceType = serviceType;

            Name = name;

            Port = port;

            try
            {
                udpClient = new UdpClient { EnableBroadcast = enableBroadcast, ExclusiveAddressUse = exclusiveAddressUse };

                if (enableBroadcast == true)
                {
                    BroadcastAddress = IPAddress.Broadcast;
                }
                else
                {
                    IPAddress ipAddrress = IPAddress.Broadcast ;
                    if (IPAddress.TryParse(broadcastAddress, out ipAddrress) == true)
                    {
                        BroadcastAddress = ipAddrress;
                    }
                    else
                    {
                        BroadcastAddress = IPAddress.Broadcast;
                    }
                }

                timer = new Timer
                {
                    AutoReset = true
                };

                Frequency = serviceFrequency;

                timer.Elapsed += OnTimerElapsed;

                timer.Start();

                timer.Enabled = false;

                hostname = Dns.GetHostName();

                Message = "";
            }
            catch
            {
                if (CreationFailed != null)
                {
                    CreationFailed(this, new EventArgs());
                }
            }
        }

        ~GridPeerService()
        {
            Dispose();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Registers the GridPeerService.
        /// </summary>
        public void Register()
        {
            IPAddress endAddress = IPAddress.Broadcast;

#if PC
            string serviceIpAddress = GetServiceIPAddress();
            if (string.IsNullOrEmpty(serviceIpAddress) == true)
            {
                try
                {
                    IPHostEntry host = Dns.GetHostEntry(hostname);
                    if (host != null && host.AddressList != null && host.AddressList.Length > 0)
                    {
                        foreach (IPAddress item in host.AddressList)
                        {
                            if (item.AddressFamily == AddressFamily.InterNetwork)
                            {
                                endAddress = item;
                                break;
                            }
                        }
                    }

                    NetworkInterface[] networkInterfaceList = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (NetworkInterface ni in networkInterfaceList)
                    {
                        if ((ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) || (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
                        {
                            if ((ni.OperationalStatus == OperationalStatus.Up) && (ni.Name.ToUpper().Contains("Internal".ToUpper()) == false))
                            {
                                //Console.WriteLine(ni.Name);
                                IPInterfaceProperties ipInterfaceProperties = ni.GetIPProperties();
                                UnicastIPAddressInformationCollection unicastIpAddressInfoList = ipInterfaceProperties.UnicastAddresses;
                                foreach (UnicastIPAddressInformation ip in unicastIpAddressInfoList)
                                {
                                    if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                    {
                                        endAddress = ip.Address;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    endAddress = IPAddress.Broadcast;
                    LogManager.Log(LogType.Error, "GridPeerService-Register", "Error:" + ex.Message);
                }
            }
            else
            {                
                IPHostEntry host = Dns.GetHostEntry(serviceIpAddress);
                if (host != null && host.AddressList != null && host.AddressList.Length > 0)
                {
                    foreach (IPAddress item in host.AddressList)
                    {
                        if (item.AddressFamily == AddressFamily.InterNetwork)
                        {
                            endAddress = item;
                            break;
                        }
                    }
                }
            }
#endif

            ipEndPoint = new IPEndPoint(endAddress, port);

            timer.Enabled = true;

            OnTimerElapsed(null, null);

            if (Registered != null)
            {
                Registered(this, new EventArgs());
            }
        }

        /// <summary>
        /// Unregisters the GridPeerService.
        /// </summary>
        public void Unregister()
        {
            timer.Enabled = false;

            if (Unregistered != null)
            {
                Unregistered(this, new EventArgs());
            }

            SendDisappearanceMessage();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            SendDisappearanceMessage();

            try
            {
                timer.Enabled = false;
            }
            catch
            {
            }

            try
            {
                timer.Dispose();
            }
            catch
            {
            }

            try
            {
                udpClient.Client.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            }

            try
            {
                udpClient.Close();
            }
            catch
            {
            }

            try
            {
                udpClient = null;
            }
            catch
            {
            }
        }

        #endregion

        #region Private Method

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            string broadcastMessage = string.Format("{0};{1};{2};{3};{4};", hostname, ServiceType, Name, Port, Message);

            broadcastMessage = broadcastMessage.Length + ";" + broadcastMessage;

            broadcastMessage = GridMessageName + ":" + Convert.ToBase64String(Encoding.UTF8.GetBytes(broadcastMessage));

            byte[] messageBytes = Encoding.UTF8.GetBytes(broadcastMessage);

            try
            {
                udpClient.Send(messageBytes, messageBytes.Length, ipEndPoint);
            }
            catch
            {
                if (BroadcastFailed != null)
                {
                    BroadcastFailed(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Sends the disappearance message.
        /// </summary>
        private void SendDisappearanceMessage()
        {
            try
            {
                string broadcastMessage = string.Format("{0};{1};{2};{3};{4};<EOS>", hostname, ServiceType, Name, Port, Message);

                broadcastMessage = broadcastMessage.Length + ";" + broadcastMessage;

                broadcastMessage = GridMessageName + ":" + Convert.ToBase64String(Encoding.UTF8.GetBytes(broadcastMessage));

                byte[] messageBytes = Encoding.UTF8.GetBytes(broadcastMessage);

                try
                {
                    udpClient.Send(messageBytes, messageBytes.Length, ipEndPoint);
                }
                catch
                {
                }
            }
            catch
            {
            }
        }

        #endregion
    }
}