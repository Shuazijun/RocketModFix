using System.Net;
using System.Net.Sockets;
using System.Linq;
using System;

namespace Rocket.Core.RCON
{
    public class RCONConnection
    {
        public TcpClient Client;
        public bool Authenticated;
        public bool Interactive;
        public int InstanceID { get; private set; }
        public DateTime ConnectedTime { get; private set; }

        public RCONConnection(TcpClient client, int instance)
        {
            this.Client = client;
            Authenticated = false;
            Interactive = true;
            InstanceID = instance;
            ConnectedTime = DateTime.Now;
        }

        public void Send(string command, bool nonewline = false)
        {
            if (Interactive)
            {
                if (nonewline == true)
                    RCONServer.Send(Client, command);
                else
                    RCONServer.Send(Client, command + (!command.Contains('\n') ? "\r\n" : ""));
                return;
            }
        }

        public string Read()
        {
            return RCONServer.Read(Client, Authenticated);
        }

        public void Close()
        {
            try
            {
                Client?.Close();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException)
            {
            }
        }

        public string Address => TryGetRemoteAddress(Client);

        internal static string TryGetRemoteAddress(TcpClient? client)
        {
            if (client?.Client == null)
            {
                return "?";
            }

            try
            {
                EndPoint? endpoint = client.Client.RemoteEndPoint;
                return endpoint?.ToString() ?? "?";
            }
            catch (SocketException)
            {
            }
            catch (ObjectDisposedException)
            {
            }

            return "?";
        }

        internal static bool TryGetRemoteIpAddress(TcpClient? client, out IPAddress? address)
        {
            address = null;
            if (client?.Client == null)
            {
                return false;
            }

            try
            {
                if (client.Client.RemoteEndPoint is IPEndPoint endpoint)
                {
                    address = endpoint.Address;
                    return true;
                }
            }
            catch (SocketException)
            {
            }
            catch (ObjectDisposedException)
            {
            }

            return false;
        }
    }

}