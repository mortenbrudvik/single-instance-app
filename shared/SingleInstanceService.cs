using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static System.Console;

namespace shared
{
    /// <summary>
    /// Handles the scenario when you only want one instance of the application running on the local user logon.
    /// There is also the option to send commands to the other instance using name pipes communication.
    /// </summary>
    public class SingleInstanceService
    {
        private Mutex _mutex;
        private CancellationTokenSource _serverCancellationTokenSource;
        private readonly string _appId = "some-unique_id-" + Environment.UserName;
        private NamedPipeServerStream _pipeServer;

        public event EventHandler<List<string>> CommandsReceived;

        public async Task<bool>  Start()
        {
            if (_mutex != null)
                throw new InvalidOperationException("Mutext has all ready been set. Please call Stop to release the mutext.");

            _mutex = new Mutex(true, _appId, out var firstInstance);

            if (firstInstance)
                await Task.Factory.StartNew(StartServer, _serverCancellationTokenSource, TaskCreationOptions.LongRunning);

            return firstInstance;
        }

        public void Stop()
        {
            _serverCancellationTokenSource?.Cancel();
            _pipeServer?.Close();
            _pipeServer = null;

            _mutex?.Close();
            _mutex = null;
        }

        public async Task SignalFirstInstance(List<string> commands)
        {
            if (_mutex == null)
                throw new InvalidOperationException("Please call Start before your call this method.");

            if (_pipeServer != null)
                throw new InvalidOperationException("This method should only be called from the second instance to signal the first.");

            try
            {
                WriteLine("Client");
                var pipe = new NamedPipeClientStream(".", _appId, PipeDirection.Out, PipeOptions.Asynchronous);
                WriteLine("Connecting");
                await pipe.ConnectAsync();

                var serialized = JsonConvert.SerializeObject(new SingleInstanceMessage { Commands = commands });
                var messageBytes = Encoding.UTF8.GetBytes(serialized);

                await pipe.WriteAsync(messageBytes, 0, messageBytes.Length);
                await pipe.FlushAsync();

                WriteLine("Done");
            }
            catch (Exception e)
            {
                WriteLine("An error occurred: " + e.Message);
            }
        }

        private async Task StartServer(object o)
        {
            _serverCancellationTokenSource = new CancellationTokenSource();
            _serverCancellationTokenSource.CancelAfter(1000);
            WriteLine("Starting Server");

            _pipeServer = new NamedPipeServerStream(_appId, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            while (!_serverCancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    WriteLine("Waiting for connection....");
                    await _pipeServer.WaitForConnectionAsync(_serverCancellationTokenSource.Token);

                    if (_serverCancellationTokenSource.IsCancellationRequested)
                        break;

                    WriteLine("Connected");

                    var messageBuilder = new StringBuilder();
                    var messageBuffer = new byte[5];
                    do
                    {
                        await _pipeServer.ReadAsync(messageBuffer, 0, messageBuffer.Length);
                        var messageChunk = Encoding.UTF8.GetString(messageBuffer);
                        messageBuilder.Append(messageChunk);
                        messageBuffer = new byte[messageBuffer.Length];
                    }
                    while (!_pipeServer.IsMessageComplete);

                    var message = JsonConvert.DeserializeObject<SingleInstanceMessage>(messageBuilder.ToString());

                    CommandsReceived?.Invoke(this, message.Commands);
                    WriteLine(message);
                    _pipeServer.Disconnect();

                }
                catch (Exception e)
                {
                    WriteLine("An error occurred: " +  e.Message);
                }

            }
            WriteLine("Pipe server has been disconnected");
        }
    }

    public class SingleInstanceMessage
    {
        public List<string> Commands { get; set; }

        public override string ToString() => string.Join(" ", Commands);
    }
}
