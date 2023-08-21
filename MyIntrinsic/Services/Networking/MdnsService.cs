using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public class MdnsService : IDisposable
{
    readonly UdpClient _udpClient;
    bool _disposed;
    static readonly IPAddress MulticastIpAddress = IPAddress.Parse("224.0.0.251");

    public MdnsService()
    {
        _udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, 5353))
        {
            EnableBroadcast = true
        };

        var adapter = NetworkInterface.GetAllNetworkInterfaces()
            .FirstOrDefault(a => a.OperationalStatus == OperationalStatus.Up &&
                                  a.Supports(NetworkInterfaceComponent.IPv4));

        if (adapter != null)
        {
            //Find adapter that supports unicasting
            var addr = adapter.GetIPProperties().UnicastAddresses
                .FirstOrDefault(ua => ua.Address.AddressFamily == AddressFamily.InterNetwork);

            if (addr != null)
            {
                _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpClient.ExclusiveAddressUse = false;


                var bindAddress = new IPEndPoint(addr.Address, ((IPEndPoint)_udpClient.Client.LocalEndPoint).Port);
                _udpClient.Client.Bind(bindAddress);

                _udpClient.JoinMulticastGroup(MulticastIpAddress, addr.Address);
                _udpClient.MulticastLoopback = true;
            }
        }

        NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;
        
    }

    private void OnNetworkAddressChanged(object sender, EventArgs e)
    {
        if (_disposed)
        {
            return;
        }

        if (_udpClient.Client?.IsBound ?? false)
        {
            _udpClient.Close();
            _udpClient.Client.Dispose();
        }

        var adapter = NetworkInterface.GetAllNetworkInterfaces()
            .FirstOrDefault(a => a.OperationalStatus == OperationalStatus.Up &&
                                  a.Supports(NetworkInterfaceComponent.IPv4));

        if (adapter != null)
        {
            var addr = adapter.GetIPProperties()
                .UnicastAddresses.FirstOrDefault(ua => ua.Address.AddressFamily == AddressFamily.InterNetwork);

            if (addr != null)
            {
                _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpClient.ExclusiveAddressUse = false;

                var bindAddress = new IPEndPoint(addr.Address, ((IPEndPoint)_udpClient.Client.LocalEndPoint).Port);
                _udpClient.Client.Bind(bindAddress);

                _udpClient.JoinMulticastGroup(MulticastIpAddress, addr.Address);
                _udpClient.MulticastLoopback = true;
            }
        }
    }

    public void Dispose()
    {
        _disposed = true;
        if (_udpClient.Client?.IsBound ?? false)
        {
            _udpClient?.Close();
            _udpClient?.Client?.Dispose();
        }
    }

    public void Send(string message)
    {
        var data = Encoding.UTF8.GetBytes(message);

        _udpClient.Send(data, data.Length, new IPEndPoint(MulticastIpAddress, 5353));
    }

    public async Task<byte[]> ReceiveMdnsBroadcastAsync(int timeout = 10000)
    {
        byte[] dataBytes = null; 
        var client = new UdpClient(5353);
        var multicastAddress = IPAddress.Parse("224.0.0.251");

        client.JoinMulticastGroup(multicastAddress);
        client.MulticastLoopback = true;

        //var endPoint = new IPEndPoint(IPAddress.Any, 5353);

        var timeoutTask = Task.Delay(timeout);
        var receiveTask = client.ReceiveAsync();

        var completedTask = await Task.WhenAny(timeoutTask, receiveTask);
        {
            if (completedTask == receiveTask)
            {
                dataBytes = receiveTask.Result.Buffer;
            }
        }

        return dataBytes;
    }
}