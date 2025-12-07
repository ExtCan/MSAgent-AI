using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MSAgentAI.Logging;

namespace MSAgentAI.Pipeline
{
    /// <summary>
    /// Named pipe server for external application communication.
    /// Games and scripts can connect to "MSAgentAI" pipe to send commands.
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
        /// Starts the named pipe server
        /// </summary>
        public void Start()
        {
            if (_isRunning)
                return;
                
            _cancellationTokenSource = new CancellationTokenSource();
            _isRunning = true;
            
            _serverTask = Task.Run(() => RunServerAsync(_cancellationTokenSource.Token));
            Logger.Log($"Pipeline server started on pipe: \\\\.\\pipe\\{PipeName}");
        }
        
        /// <summary>
        /// Stops the named pipe server
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
                return;
                
            _isRunning = false;
            _cancellationTokenSource?.Cancel();
            
            Logger.Log("Pipeline server stopped");
        }
        
        private async Task RunServerAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Create a new pipe server for each connection
                    using (var pipeServer = new NamedPipeServerStream(
                        PipeName,
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
                        await HandleConnectionAsync(pipeServer, cancellationToken);
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
        
        private async Task HandleConnectionAsync(NamedPipeServerStream pipeServer, CancellationToken cancellationToken)
        {
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
