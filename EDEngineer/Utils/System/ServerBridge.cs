using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EDEngineer.Utils.System
{
    public class ServerBridge : IDisposable
    {
        private readonly MainWindowViewModel viewModel;
        private CancellationTokenSource cts;

        public ServerBridge(MainWindowViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public void Start(int port)
        {
            cts = new CancellationTokenSource();

            Task.Factory.StartNew(() =>
            {
                Server.start(cts.Token, port, () => viewModel.Commanders.ToDictionary(kv => kv.Key, kv => kv.Value.State));
            }, cts.Token);
        }

        public void Stop()
        {
            if (cts?.IsCancellationRequested == false)
            {
                cts.Cancel();
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
