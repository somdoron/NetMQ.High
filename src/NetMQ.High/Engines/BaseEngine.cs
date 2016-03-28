using NetMQ.Sockets;

namespace NetMQ.High.Engines
{
    internal abstract class BaseEngine : IShimHandler
    {
        protected NetMQPoller Poller { get; private set; }
        public PairSocket Shim { get; private set; }
        public Codec Codec { get; }

        protected abstract void Initialize();
        protected abstract void Cleanup();
        protected abstract void OnShimCommand(string command);

        public BaseEngine()
        {
            Codec = new Codec();
        }

        protected void OnShimReady(object sender, NetMQSocketEventArgs e)
        {
            string command = Shim.ReceiveFrameString();

            if (command == NetMQActor.EndShimMessage)
                Poller.Stop();
            else
                OnShimCommand(command);
        }

        void IShimHandler.Run(PairSocket shim)
        {
            Poller = new NetMQPoller();


            Shim = shim;
            Shim.ReceiveReady += OnShimReady;
            Poller.Add(Shim);

            Initialize();

            Shim.SignalOK();

            Poller.Run();

            Cleanup();
        }
    }
}