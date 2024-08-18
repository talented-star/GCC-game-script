namespace GrabCoin.GameWorld.Network
{
    public class ValidateTransferResponseModel
    {
        public ValidateTransferResponseModel(string networkAddress, ushort port, string scene)
        {
            NetworkAddress = networkAddress;
            Port = port;
            Scene = scene;
        }

        public string NetworkAddress { get; set; }
        public ushort Port { get; set; }
        public string Scene { get; set; }
    }
}