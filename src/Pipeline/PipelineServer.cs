using System;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MSAgentAI.Logging;

namespace MSAgentAI.Pipeline
{
    /// <summary>
    /// Pipeline server for external application communication.
    /// Supports both Named Pipes (local) and TCP sockets (network).
    /// Games and scripts can connect to send commands.
    /// 
    /// Protocol:
    /// - SPEAK:text - Make agent speak the text
    /// - ANIMATION:name - Play an animation
    /// - CHAT:prompt - Send prompt to AI and speak response
    /// - HIDE - Hide the agent
    /// - SHOW - Show the agent
    /// - POKE - Trigger random AI dialog
    /// </summary>
    public class PipelineServer : IDisposable
    {
        public const string PipeName = "MSAgentAI";
        
        private CancellationTokenSource _cancellationTokenSource;
        private Task _serverTask;
        private bool _isRunning;
        private TcpListener _tcpListener;
        
        // Configuration
        private string _protocol;
        private string _ipAddress;
        private int _port;
        private string _pipeName;
        
        /// <summary>
        /// Event raised when a SPEAK command is received
        /// </summary>
        public event EventHandler<string> OnSpeakCommand;
        
        /// <summary>
        /// Event raised when an ANIMATION command is received
        /// </summary>
        public event EventHandler<string> OnAnimationCommand;
        
        /// <summary>
        /// Event raised when a CHAT command is received
        /// </summary>
        public event EventHandler<string> OnChatCommand;
        
        /// <summary>
        /// Event raised when a HIDE command is received
        /// </summary>
        public event EventHandler OnHideCommand;
        
        /// <summary>
        /// Event raised when a SHOW command is received
        /// </summary>
        public event EventHandler OnShowCommand;
        
        /// <summary>
        /// Event raised when a POKE command is received
        /// </summary>
        public event EventHandler OnPokeCommand;
        
        /// <summary>
        /// Event raised when a custom command is received (for extensibility)
        /// </summary>
        public event EventHandler<PipelineCommand> OnCustomCommand;
        
        public bool IsRunning => _isRunning;
        
        /// <summary>
        /// Constructor with default configuration (Named Pipe)
        /// </summary>
        public PipelineServer() : this("NamedPipe", "127.0.0.1", 8765, PipeName)
        {
        }
        
        /// <summary>
        /// Constructor with custom configuration
        /// </summary>
        /// <param name="protocol">Protocol type: "NamedPipe" or "TCP"</param>
        /// <param name="ipAddress">IP address for TCP mode</param>
        /// <param name="port">Port for TCP mode</param>
        /// <param name="pipeName">Pipe name for Named Pipe mode</param>
        public PipelineServer(string protocol, string ipAddress, int port, string pipeName)
        {
            _protocol = protocol ?? "NamedPipe";
            _ipAddress = ipAddress ?? "127.0.0.1";
            _port = port;
            _pipeName = pipeName ?? PipeName;
        }
        
        /// <summary>
        /// Starts the pipeline server
        /// </summary>
        public void Start()
        {
            if (_isRunning)
                return;
                
            _cancellationTokenSource = new CancellationTokenSource();
            _isRunning = true;
            
            if (_protocol.Equals("TCP", StringComparison.OrdinalIgnoreCase))
            {
                _serverTask = Task.Run(() => RunTcpServerAsync(_cancellationTokenSource.Token));
                Logger.Log($"Pipeline TCP server started on {_ipAddress}:{_port}");
            }
            else
            {
                _serverTask = Task.Run(() => RunNamedPipeServerAsync(_cancellationTokenSource.Token));
                Logger.Log($"Pipeline server started on pipe: \\\\.\\pipe\\{_pipeName}");
            }
        }
        
        /// <summary>
        /// Stops the pipeline server
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
                return;
                
            _isRunning = false;
            _cancellationTokenSource?.Cancel();
            _tcpListener?.Stop();
            
            Logger.Log("Pipeline server stopped");
        }
        
        private async Task RunNamedPipeServerAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Create a new pipe server for each connection
                    using (var pipeServer = new NamedPipeServerStream(
                        _pipeName,
                        PipeDirection.InOut,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Message,
                        PipeOptions.Asynchronous))
                    {
                        Logger.Log("Pipeline: Waiting for connection...");
                        
                        // Wait for a client connection
                        await pipeServer.WaitForConnectionAsync(cancellationToken);
                        
                        Logger.Log("Pipeline: Client connected");
                        
                        // Handle the connection
                        await HandleNamedPipeConnectionAsync(pipeServer, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Normal cancellation, exit the loop
                    break;
                }
                catch (Exception ex)
                {
                    Logger.LogError("Pipeline server error", ex);
                    
                    // Wait a bit before retrying
                    try
                    {
                        await Task.Delay(1000, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }
        
        private async Task RunTcpServerAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Validate and parse IP address
                if (!IPAddress.TryParse(_ipAddress, out IPAddress ipAddr))
                {
                    Logger.LogError($"TCP Pipeline: Invalid IP address '{_ipAddress}'. Using loopback address.", null);
                    ipAddr = IPAddress.Loopback;
                }
                
                _tcpListener = new TcpListener(ipAddr, _port);
                _tcpListener.Start();
                
                Logger.Log($"TCP Pipeline: Listening on {ipAddr}:{_port}");
                
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // Accept client connection
                        var client = await _tcpListener.AcceptTcpClientAsync();
                        Logger.Log($"TCP Pipeline: Client connected from {client.Client.RemoteEndPoint}");
                        
                        // Handle each client in a separate task with proper exception handling
                        // Using fire-and-forget pattern is acceptable for server scenarios where:
                        // 1. Each task fully handles its own exceptions (logged)
                        // 2. Client disconnection properly disposes resources (using statements)
                        // 3. Tasks are short-lived and self-contained
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await HandleTcpConnectionAsync(client, cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError("TCP Pipeline: Unhandled exception in client handler", ex);
                            }
                        }, cancellationToken);
                    }
                    catch (ObjectDisposedException)
                    {
                        // Listener was stopped
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            Logger.LogError("TCP Pipeline: Error accepting connection", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("TCP Pipeline: Server error", ex);
            }
        }
        
        private async Task HandleNamedPipeConnectionAsync(NamedPipeServerStream pipeServer, CancellationToken cancellationToken)
        {
            const int MaxMessageLength = 8192; // 8KB max message size
            
            try
            {
                using (var reader = new StreamReader(pipeServer, Encoding.UTF8))
                using (var writer = new StreamWriter(pipeServer, Encoding.UTF8) { AutoFlush = true })
                {
                    // Read commands until the client disconnects
                    while (pipeServer.IsConnected && !cancellationToken.IsCancellationRequested)
                    {
                        string line = await reader.ReadLineAsync();
                        
                        if (string.IsNullOrEmpty(line))
                            break;
                        
                        // Enforce message size limit to prevent DoS
                        if (line.Length > MaxMessageLength)
                        {
                            Logger.Log($"Pipeline: Command too long ({line.Length} bytes), rejecting");
                            await writer.WriteLineAsync("ERROR:Command too long");
                            break;
                        }
                            
                        Logger.Log($"Pipeline: Received command: {line}");
                        
                        // Parse and process the command
                        var response = ProcessCommand(line);
                        
                        // Send response
                        await writer.WriteLineAsync(response);
                    }
                }
            }
            catch (IOException)
            {
                // Client disconnected
                Logger.Log("Pipeline: Client disconnected");
            }
            catch (Exception ex)
            {
                Logger.LogError("Pipeline: Error handling connection", ex);
            }
        }
        
        private async Task HandleTcpConnectionAsync(TcpClient client, CancellationToken cancellationToken)
        {
            const int MaxMessageLength = 8192; // 8KB max message size
            const int ReadTimeoutMs = 30000; // 30 second timeout
            
            try
            {
                using (client)
                using (var stream = client.GetStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    // Set read timeout for the stream
                    stream.ReadTimeout = ReadTimeoutMs;
                    
                    // Read commands until the client disconnects
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        string line = await reader.ReadLineAsync();
                        
                        if (string.IsNullOrEmpty(line))
                            break;
                        
                        // Enforce message size limit to prevent DoS
                        if (line.Length > MaxMessageLength)
                        {
                            Logger.Log($"TCP Pipeline: Command too long ({line.Length} bytes), rejecting");
                            await writer.WriteLineAsync("ERROR:Command too long");
                            break;
                        }
                            
                        Logger.Log($"TCP Pipeline: Received command: {line}");
                        
                        // Parse and process the command
                        var response = ProcessCommand(line);
                        
                        // Send response
                        await writer.WriteLineAsync(response);
                    }
                }
                
                Logger.Log($"TCP Pipeline: Client disconnected");
            }
            catch (IOException ex)
            {
                // Client disconnected or timeout
                Logger.Log($"TCP Pipeline: Client disconnected (IO error: {ex.Message})");
            }
            catch (Exception ex)
            {
                Logger.LogError("TCP Pipeline: Error handling connection", ex);
            }
        }
        
        private string ProcessCommand(string commandLine)
        {
            try
            {
                // Parse command: COMMAND:data or just COMMAND
                string command;
                string data = null;
                
                int colonIndex = commandLine.IndexOf(':');
                if (colonIndex > 0)
                {
                    command = commandLine.Substring(0, colonIndex).Trim().ToUpperInvariant();
                    data = commandLine.Substring(colonIndex + 1);
                }
                else
                {
                    command = commandLine.Trim().ToUpperInvariant();
                }
                
                switch (command)
                {
                    case "SPEAK":
                        if (!string.IsNullOrEmpty(data))
                        {
                            OnSpeakCommand?.Invoke(this, data);
                            return "OK:SPEAK";
                        }
                        return "ERROR:SPEAK requires text";
                        
                    case "ANIMATION":
                    case "ANIM":
                        if (!string.IsNullOrEmpty(data))
                        {
                            OnAnimationCommand?.Invoke(this, data);
                            return "OK:ANIMATION";
                        }
                        return "ERROR:ANIMATION requires animation name";
                        
                    case "CHAT":
                        if (!string.IsNullOrEmpty(data))
                        {
                            OnChatCommand?.Invoke(this, data);
                            return "OK:CHAT";
                        }
                        return "ERROR:CHAT requires prompt";
                        
                    case "HIDE":
                        OnHideCommand?.Invoke(this, EventArgs.Empty);
                        return "OK:HIDE";
                        
                    case "SHOW":
                        OnShowCommand?.Invoke(this, EventArgs.Empty);
                        return "OK:SHOW";
                        
                    case "POKE":
                        OnPokeCommand?.Invoke(this, EventArgs.Empty);
                        return "OK:POKE";
                        
                    case "PING":
                        return "PONG";
                        
                    case "VERSION":
                        return "MSAgentAI:1.0.0";
                        
                    default:
                        // Custom command - pass to handlers
                        var customCmd = new PipelineCommand { Command = command, Data = data };
                        OnCustomCommand?.Invoke(this, customCmd);
                        return $"OK:CUSTOM:{command}";
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Pipeline: Error processing command: {commandLine}", ex);
                return $"ERROR:{ex.Message}";
            }
        }
        
        public void Dispose()
        {
            Stop();
            _cancellationTokenSource?.Dispose();
        }
    }
    
    /// <summary>
    /// Represents a pipeline command
    /// </summary>
    public class PipelineCommand
    {
        public string Command { get; set; }
        public string Data { get; set; }
    }
}
