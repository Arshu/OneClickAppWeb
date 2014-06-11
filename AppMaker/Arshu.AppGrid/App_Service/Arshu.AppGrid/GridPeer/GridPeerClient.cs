using System;
using System.Net;

namespace Arshu.AppGrid.Peer
{
    /// <summary>
    /// This class represents a GridClient that contains the client information.
    /// </summary>
    internal class GridPeerClient : IEquatable<GridPeerClient>
    {

        #region Properties

        /// <summary>
        /// Gets or Sets the client's address.
        /// </summary>
        /// <value>The client address.</value>
        public IPAddress Address { get; set; }

        /// <summary>
        /// Gets or Sets the name of the host.
        /// </summary>
        /// <value>The name of the host.</value>
        public string HostName { get; set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        public string ServiceType { get; private set; }

        /// <summary>
        /// Gets the client application port.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public string Message { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GridClient"/> class.
        /// </summary>
        /// <param name="address">The address of the client.</param>
        /// <param name="hostName">Name of the client host.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="name">The name of the application.</param>
        /// <param name="port">The application port.</param>
        /// <param name="message">The message.</param>
        public GridPeerClient(IPAddress address, string hostName, string serviceType, string name, int port, string message)
        {
            Address = address;

            HostName = hostName;

            ServiceType = serviceType;

            Name = name;

            Port = port;

            Message = message;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="gridClient1">The grid client1.</param>
        /// <param name="gridClient2">The grid client2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(GridPeerClient gridClient1, GridPeerClient gridClient2)
        {
            try
            {
                return (gridClient1.Address == gridClient2.Address && gridClient1.HostName.ToLower() == gridClient2.HostName.ToLower());
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="gridClient1">The grid client1.</param>
        /// <param name="gridClient2">The grid client2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(GridPeerClient gridClient1, GridPeerClient gridClient2)
        {
            return !(gridClient1 == gridClient2);
        }

        /// <summary>
        /// Sets the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SetMessage(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
        public bool Equals(GridPeerClient other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other.Address, Address) && Equals(other.HostName, HostName) && Equals(other.ServiceType, ServiceType) && other.Port == Port;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.</returns>
        /// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.
        ///   </exception>
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

            if (obj.GetType() != typeof (GridPeerClient))
            {
                return false;
            }

            return Equals((GridPeerClient) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current <see cref="T:System.Object"/>.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = Address.GetHashCode();
                result = (result*397) ^ HostName.GetHashCode();
                result = (result*397) ^ ServiceType.GetHashCode();
                result = (result*397) ^ Port;
                return result;
            }
        }

        #endregion
    }
}
