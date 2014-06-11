using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;

namespace Arshu.AppGrid.Peer
{
    /// <summary>
    /// This class is used to find the GridPeerService servers.
    /// </summary>
    internal class GridPeerBrowser:IDisposable
    {
        #region Variables

        private List<GridClientDateTime> gridClientDateTimes;

        private const int port = 54183;

        private const int disappearanceDelaySeconds = 4;

        private UdpClient udpClient;

        private int delay;

        private TimeSpan disappearanceDelay;

        private Timer timer;

        private readonly IPEndPoint ipEndPoint;

        private string currentHostName;

        private readonly List<IPAddress> currentIPAddresses;

        private bool isBrowserRunning;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the salaam clients.
        /// </summary>
        public GridPeerClient[] GridClients
		{
			get
			{
                GridPeerClient[] gridClients = new GridPeerClient[gridClientDateTimes.Count];

			    for (int i = 0; i < gridClients.Length; i++)
			    {
			        gridClients[i] = gridClientDateTimes[i].Client;
			    }

			    return gridClients;
			}
		}

        /// <summary>
        /// Gets or sets a value indicating whether browser should receive clients from local machine.
        /// </summary>
        /// <value><c>true</c> if should receive from local machine otherwise, <c>false</c>.</value>
        public bool ReceiveFromLocalMachine { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="GridPeerBrowser"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled
        {
            get { return timer.Enabled; }
            set
            {
                timer.Enabled = value;

                if (value == true)
                {
                    Start(ServiceType);
                }
                else
                {
                    Stop();
                }
            }
        }

        /// <summary>
        /// Gets or sets the disappearance delay in seconds.
        /// </summary>
        /// <value>The disappearance delay.</value>
        public int DisappearanceDelay
        {
            get
            {
                return delay;
            }
            set
            {
                delay = value;

                timer.Interval = 1000 * ((double)delay / 5);

                disappearanceDelay = new TimeSpan(0, 0, DisappearanceDelay);
            }
        }

        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        public string ServiceType { get; private set; }

        #endregion

        #region Events

        ///<summary>
        /// Represents the method that will handle an event that contains the GridPeerClient object.
        ///</summary>
        ///<param name="sender">The source of the event.</param>
        ///<param name="e">A <c>GridPeerClientEventArgs</c> that contains GridPeerClient object.</param>
        public delegate void GridClientEventHandler(object sender, GridPeerClientEventArgs e);

        /// <summary>
        /// Occurs when a new client appears.
        /// </summary>
        public event GridClientEventHandler ClientAppeared;

        /// <summary>
        /// Occurs when a found client disappears.
        /// </summary>
        public event GridClientEventHandler ClientDisappeared;

        /// <summary>
        /// Occurs when the client message changes.
        /// </summary>
        public event GridClientEventHandler ClientMessageChanged;

        /// <summary>
        /// Occurs when the browser starts listening for incoming connections.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Occurs when browsers stops listening for incoming connections.
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Occurs when the browser fails to start.
        /// </summary>
        public event EventHandler StartFailed;

        /// <summary>
        /// Occurs when something happens and the browser cannot continue listening for incoming connections anymore.
        /// </summary>
        public event EventHandler BrowserFailed;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GridPeerBrowser"/> class.
        /// </summary>
        public GridPeerBrowser()
        {
            timer = new Timer();
            
            DisappearanceDelay = disappearanceDelaySeconds;

            timer.Elapsed += new ElapsedEventHandler(OnTimerElapsed);

            timer.Start();

            timer.Enabled = false;

            ipEndPoint = new IPEndPoint(IPAddress.Any, port);

            currentIPAddresses = new List<IPAddress>();

            gridClientDateTimes = new List<GridClientDateTime>();
        }

        ~GridPeerBrowser()
        {
            Dispose();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the specified service type.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        public void Start(string serviceType)
        {
            if (serviceType.Contains(";"))
            {
                throw new ArgumentException("Semicolon character is not allowed in ServiceType argument.");
            }

            try
            {
                gridClientDateTimes.Clear();

                currentHostName = Dns.GetHostName();

                currentIPAddresses.Clear();

                currentIPAddresses.AddRange(Dns.GetHostAddresses(currentHostName));

                ServiceType = serviceType;

                timer.Enabled = true;

                udpClient = new UdpClient { EnableBroadcast = true, ExclusiveAddressUse = true };

                udpClient.Client.Bind(ipEndPoint);

                udpClient.BeginReceive(OnUdpDataReceived, udpClient);

                if (Started != null)
                {
                    Started(this, new EventArgs());
                }

                isBrowserRunning = true;
            }
            catch
            {
                if (StartFailed != null)
                {
                    StartFailed(this, new EventArgs());
                }

                try
                {
                    timer.Enabled = false;
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        public void Stop()
        {
            isBrowserRunning = false;

            try
            {
                timer.Enabled = false;
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

            if (Stopped != null)
            {
                Stopped(this, new EventArgs());
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            try
            {
                timer.Stop();
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
                timer = null;
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

        #region Private Methods

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            List<GridClientDateTime> tempClientDateTimes = new List<GridClientDateTime>();

            for (int i = 0; i < gridClientDateTimes.Count; i++)
            {
                DateTime dateTime = gridClientDateTimes[i].Time;

                if (DateTime.Now.Subtract(dateTime) > disappearanceDelay)
                {
                    if (ClientDisappeared != null)
                    {
                        IPAddress ipAddress = gridClientDateTimes[i].Client.Address;

                        bool isFromLocal;

                        if (currentIPAddresses.Contains(ipAddress) || IPAddress.IsLoopback(ipAddress))
                        {
                            isFromLocal = true;
                        }
                        else
                        {
                            isFromLocal = false;
                        }

                        ClientDisappeared(this, new GridPeerClientEventArgs(gridClientDateTimes[i].Client, isFromLocal));
                    }
                }
                else
                {
                    tempClientDateTimes.Add(gridClientDateTimes[i]);
                }
            }

            gridClientDateTimes = tempClientDateTimes;
        }

        private void OnUdpDataReceived(IAsyncResult asyncResult)
        {
            IPEndPoint tempIPEndPoint = new IPEndPoint(ipEndPoint.Address, ipEndPoint.Port);

            try
            {
                UdpClient tempUdpClient = (UdpClient) asyncResult.AsyncState;

                byte[] bytes = tempUdpClient.EndReceive(asyncResult, ref tempIPEndPoint);

                ProcessClientData(bytes, tempIPEndPoint);
            }
            catch(ObjectDisposedException)
            {
            }

            try
            {
                udpClient.BeginReceive(OnUdpDataReceived, udpClient);
            }
            catch
            {
                if (BrowserFailed != null && isBrowserRunning)
                {
                    BrowserFailed(this, new EventArgs());
                }
            }
        }

        private void ProcessClientData(byte[] dataBytes, IPEndPoint clientIPEndPoint)
        {
            const string messageRegex = "^" + GridPeerService.GridMessageName + ":(?<Base64>(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?)$";

            const string messageDataRegex =
                @"^(?<Length>\d+);(?<HostName>.*?);(?<ServiceType>.*?);(?<Name>.*?);(?<Port>\d+);(?<Message>.*);(<(?<ProtocolMessage>[A-Z][A-Z0-9]{2,3})>)?$";

            string dataString = Encoding.UTF8.GetString(dataBytes);

            if (Regex.IsMatch(dataString, messageRegex))
            {
                try
                {
                    string messageData =
                        Encoding.UTF8.GetString(
                            Convert.FromBase64String(Regex.Match(dataString, messageRegex).Groups["Base64"].Value));

                    if (Regex.IsMatch(messageData, messageDataRegex))
                    {
                        try
                        {
                            Match dataMatch = Regex.Match(messageData,messageDataRegex);

                            int length = int.Parse(dataMatch.Groups["Length"].Value);

                            length += length.ToString().Length + 1;

                            if (messageData.Length == length)
                            {
                                string hostName = dataMatch.Groups["HostName"].Value;

                                string serviceType = dataMatch.Groups["ServiceType"].Value;

                                string name = dataMatch.Groups["Name"].Value;

                                int port = int.Parse(dataMatch.Groups["Port"].Value);

                                string message = dataMatch.Groups["Message"].Value;

                                IPAddress ipAddress = clientIPEndPoint.Address;

                                string protocolMessage = "";

                                if (dataMatch.Groups["ProtocolMessage"].Success)
                                {
                                    protocolMessage = dataMatch.Groups["ProtocolMessage"].Value;
                                }

                                bool isFromLocal = false;

                                if (hostName.ToLower() == currentHostName.ToLower() && (currentIPAddresses.Contains(ipAddress) || IPAddress.IsLoopback(ipAddress)))
                                {
                                    isFromLocal = true;

                                    if (!ReceiveFromLocalMachine)
                                    {
                                        return;
                                    }
                                }

                                if (ServiceType == "*" || serviceType.ToLower() == ServiceType.ToLower())
                                {
                                    GridPeerClient gridClient = new GridPeerClient(ipAddress, hostName, serviceType, name,
                                                                                 port, message);

                                    processGridClient(gridClient, protocolMessage, isFromLocal);
                                }
                                else
                                {
                                    return;
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private void processGridClient(GridPeerClient gridClient, string protocolMessage, bool isFromLocal)
        {
            bool contains = gridClientDateTimes.Contains(new GridClientDateTime(gridClient));

            if (contains)
            {
                int index = gridClientDateTimes.IndexOf(new GridClientDateTime(gridClient));

                gridClientDateTimes[index].Time = DateTime.Now;

                if (string.IsNullOrEmpty(protocolMessage))
                {
                    if (gridClient.Message != gridClientDateTimes[index].Client.Message)
                    {
                        gridClientDateTimes[index].Client.SetMessage(gridClient.Message);

                        if (ClientMessageChanged != null)
                        {
                            ClientMessageChanged(this, new GridPeerClientEventArgs(gridClientDateTimes[index].Client, isFromLocal));
                        }
                    }
                }
                else
                {
                    if (protocolMessage.ToUpper() == "EOS")
                    {
                        if (ClientDisappeared != null)
                        {
                            ClientDisappeared(this, new GridPeerClientEventArgs(gridClientDateTimes[index].Client, isFromLocal));

                            gridClientDateTimes.Remove(gridClientDateTimes[index]);
                        }
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(protocolMessage))
                {
                    gridClientDateTimes.Add(new GridClientDateTime(gridClient, DateTime.Now));

                    int index = gridClientDateTimes.Count - 1;

                    if (ClientAppeared != null)
                    {
                        ClientAppeared(this, new GridPeerClientEventArgs(gridClientDateTimes[index].Client, isFromLocal));
                    }
                }
                else
                {
                    if (protocolMessage.ToUpper() == "EOS")
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// A private class used to hold the DateTime objects for the Individual GridClients
        /// </summary>
        private class GridClientDateTime:IEquatable<GridClientDateTime>
        {
            public GridPeerClient Client { get; private set; }

            public DateTime Time { get; set; }

            public GridClientDateTime(GridPeerClient client, DateTime time = new DateTime())
            {
                Client = client;

                Time = time;
            }

            public static bool operator ==(GridClientDateTime gridClientDateTime1, GridClientDateTime gridClientDateTime2)
            {
                return gridClientDateTime1.Client == gridClientDateTime2.Client;
            }

            public static bool operator !=(GridClientDateTime gridClientDateTime1, GridClientDateTime gridClientDateTime2)
            {
                return !(gridClientDateTime1 == gridClientDateTime2);
            }

            public bool Equals(GridClientDateTime other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }

                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                return Equals(other.Client, Client);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != typeof (GridClientDateTime))
                {
                    return false;
                }

                return Equals((GridClientDateTime) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return Client.GetHashCode();
                }
            }
        }

        #endregion
    }
}
