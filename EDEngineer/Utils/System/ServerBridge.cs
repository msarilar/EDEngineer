using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EDEngineer.Utils.UI;

namespace EDEngineer.Utils.System
{
    public class ServerBridge : IDisposable
    {
        private readonly MainWindowViewModel viewModel;
        private CancellationTokenSource cts;

        public bool Running { get; private set; }

        public bool Toggle()
        {
            if (Running)
            {
                Stop();
            }
            else
            {
                Start();
            }

            return Running;
        }

        public ServerBridge(MainWindowViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        private void Start()
        {
            ushort port;
            if (!TryGetPort(out port))
            {
                return;
            }

            cts = new CancellationTokenSource();

            Task.Factory.StartNew(() =>
            {
                Server.start(cts.Token, port, () => viewModel.Commanders.ToDictionary(kv => kv.Key, kv => kv.Value.State));
            }, cts.Token);

            Running = true;
        }

        public void Stop()
        {
            if (cts?.IsCancellationRequested == false)
            {
                cts.Cancel();
            }

            Running = false;
        }

        public void Dispose()
        {
            Stop();
        }

        private bool TryGetPort(out ushort port)
        {
            port = SettingsManager.ServerPort != 0 ? SettingsManager.ServerPort : (ushort)44405;

            return ServerPortPrompt.ShowDialog(port, out port);
        }
    }
}
