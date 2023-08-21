using System.Net.Sockets; 
using System.Net.NetworkInformation;
using System.Text;
using System.Net;

namespace MyIntrinsic.Services
{
    public class UdpService

    {
        public UdpService(ILogger<UdpService> logger)
        {
            _logger = logger;
        }

        ILogger<UdpService> _logger;
        System.Net.Sockets.UdpClient _udpClient;

        public static void Ping(string ipAddress, string data)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            options.DontFragment = true;

            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            PingReply reply = pingSender.Send(ipAddress, timeout, buffer, options);
            if (reply.Status == IPStatus.Success)
            {
                Console.WriteLine("Address: {0}", reply.Address.ToString());
                Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
                Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
                Console.WriteLine("Don't fragment: {0}", reply.Options.DontFragment);
                Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);
            }
        }

        public bool CreateUdpSocket(string ipAddressString, int port)
        {
            bool result;

            if (_udpClient != null)
            {
                _udpClient.Close();
            }

            _udpClient = new UdpClient(port);

            result = IPEndPoint.TryParse(ipAddressString, out IPEndPoint ipAddress);
            if (result)
            {
                try
                {
                    _udpClient.Connect(ipAddress);
                    result = true;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error creating socket");
                    result = false;
                }
            }

            return result;
        }

        public void CloseSocket()
        {
            if ((_udpClient != null) && (_udpClient.Client != null))
            {
                _udpClient.Close();
            }
        }

        public async Task<int> TransmitDataAsync(string message)
        {
            int numBytesSent = 0;
            var bytes = Encoding.ASCII.GetBytes(message);

            if (_udpClient == null)
            {
                return numBytesSent;
            }

            numBytesSent = await _udpClient.SendAsync(bytes, bytes.Length);

            return numBytesSent;
        }

        public async Task<string> ReceiveDataAsStringAsync()
        {
            var resultMessage = string.Empty;

            if (_udpClient == null)
            {
                return resultMessage;
            }

            var recvResult = await _udpClient.ReceiveAsync();

            _logger.LogInformation($"Received {recvResult.Buffer.Length} bytes from {recvResult.RemoteEndPoint}");

            if (recvResult.Buffer.Length > 0)
            {
                resultMessage = Encoding.UTF8.GetString(recvResult.Buffer);
            }

            return resultMessage;
        }
    }
}