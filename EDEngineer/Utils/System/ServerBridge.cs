using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EDEngineer.Utils.UI;
using EDEngineer.Views;
using EDEngineer.Views.Popups;

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

        public ServerBridge(MainWindowViewModel viewModel, bool autoRun)
        {
            this.viewModel = viewModel;

            if (autoRun)
            {
                Start(SettingsManager.ServerPort);
            }
        }

        private void Start(ushort? autoRunPort = null)
        {
            ushort port;
            if (autoRunPort.HasValue)
            {
                port = autoRunPort.Value;
            }
            else if (!TryGetPort(out port))
            {
                return;
            }

            SettingsManager.ServerPort = port;

            cts = new CancellationTokenSource();
            viewModel.ApiOn = true;
            Task.Factory.StartNew(() =>
            {
                Server.start(cts.Token,
                    port,
                    viewModel.Languages,
                    () => viewModel.Commanders.ToDictionary(kv => kv.Key, kv => kv.Value.State),
                    () => viewModel.Commanders.ToDictionary(kv => kv.Key, kv => kv.Value.ShoppingList.Composition),
                    (c, b, i) => viewModel.Commanders[c].ShoppingListChange(b, i),
                    c => viewModel.Commanders[c].JsonSettings,
                    viewModel.LogDirectory,
                    SettingsManager.AccessApiFromOtherComputers);
            }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default)
            .ContinueWith(t =>
            {
                if (t.Exception is AggregateException agg &&
                    agg.InnerExceptions.Count == 1 &&
                    agg.InnerExceptions[0] is OperationCanceledException)
                {
                    return;
                }

                try
                {
                    Stop();
                }
                catch
                {
                    // ignore
                }

                new ErrorWindow(t.Exception, "Local Server API Error").ShowDialog();
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.FromCurrentSynchronizationContext());

            Running = true;
        }

        public void Stop()
        {
            viewModel.ApiOn = false;
            if (cts?.IsCancellationRequested == false)
            {
                cts.Cancel();
            }

            SettingsManager.AutoRunServer = false;
            Running = false;
        }

        public void Dispose()
        {
            if (cts?.IsCancellationRequested == false)
            {
                cts.Cancel();
            }
        }

        private bool TryGetPort(out ushort port)
        {
            port = SettingsManager.ServerPort != 0 ? SettingsManager.ServerPort : (ushort)44405;

            return ServerPortPrompt.ShowDialog(port, out port);
        }
    }
}
