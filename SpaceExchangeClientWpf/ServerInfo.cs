namespace SpaceExchangeClientWpf
{
    public class ServerInfo
    {
        public string Ip { get; set; } = "";
        public int TcpPort { get; set; }
        public int UdpPort { get; set; }
        public int WsPort { get; set; }

        public string DisplayName => $"{Ip} | TCP={TcpPort}, UDP={UdpPort}, WS={WsPort}";
    }
}