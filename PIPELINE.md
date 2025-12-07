# MSAgent-AI Communication Pipeline

The MSAgent-AI application includes a **Named Pipe server** that allows external applications (games, scripts, mods) to send commands for AI interaction.

## Pipe Name
```
\\.\pipe\MSAgentAI
```

## Protocol
Commands are sent as plain text lines. Each command receives a response.

### Available Commands

| Command | Description | Example |
|---------|-------------|---------|
| `SPEAK:text` | Make the agent speak the given text | `SPEAK:Hello world!` |
| `ANIMATION:name` | Play a specific animation | `ANIMATION:Wave` |
| `CHAT:prompt` | Send prompt to Ollama AI and speak the response | `CHAT:Tell me a joke` |
| `HIDE` | Hide the agent | `HIDE` |
| `SHOW` | Show the agent | `SHOW` |
| `POKE` | Trigger a random AI-generated dialog | `POKE` |
| `PING` | Check if the server is running | `PING` |
| `VERSION` | Get the MSAgent-AI version | `VERSION` |

### Response Format
- `OK:COMMAND` - Command was executed successfully
- `ERROR:message` - Command failed with error message
- `PONG` - Response to PING
- `MSAgentAI:1.0.0` - Response to VERSION

## Examples

### Python
```python
import win32pipe
import win32file

def send_command(command):
    pipe = win32file.CreateFile(
        r'\\.\pipe\MSAgentAI',
        win32file.GENERIC_READ | win32file.GENERIC_WRITE,
        0, None,
        win32file.OPEN_EXISTING,
        0, None
    )
    
    # Send command
    win32file.WriteFile(pipe, (command + '\n').encode('utf-8'))
    
    # Read response
    result, data = win32file.ReadFile(pipe, 1024)
    response = data.decode('utf-8').strip()
    
    win32file.CloseHandle(pipe)
    return response

# Examples
send_command("SPEAK:Hello from Python!")
send_command("ANIMATION:Wave")
send_command("CHAT:What's the weather like?")
```

### C#
```csharp
using System.IO.Pipes;

void SendCommand(string command)
{
    using (var client = new NamedPipeClientStream(".", "MSAgentAI", PipeDirection.InOut))
    {
        client.Connect(5000); // 5 second timeout
        
        using (var reader = new StreamReader(client))
        using (var writer = new StreamWriter(client) { AutoFlush = true })
        {
            writer.WriteLine(command);
            string response = reader.ReadLine();
            Console.WriteLine($"Response: {response}");
        }
    }
}

// Examples
SendCommand("SPEAK:Hello from C#!");
SendCommand("POKE");
```

### AutoHotkey
```autohotkey
SendToAgent(command) {
    pipe := FileOpen("\\.\pipe\MSAgentAI", "rw")
    if (!pipe) {
        MsgBox, Failed to connect to MSAgentAI
        return
    }
    
    pipe.Write(command . "`n")
    pipe.Read(0)  ; Flush
    response := pipe.ReadLine()
    pipe.Close()
    
    return response
}

; Examples
SendToAgent("SPEAK:Hello from AutoHotkey!")
SendToAgent("ANIMATION:Surprised")
```

### Lua (for game mods)
```lua
-- Example for games with Lua scripting and pipe support
local pipe = io.open("\\\\.\\pipe\\MSAgentAI", "r+")
if pipe then
    pipe:write("SPEAK:Player scored a point!\n")
    pipe:flush()
    local response = pipe:read("*l")
    pipe:close()
end
```

### PowerShell
```powershell
$pipe = New-Object System.IO.Pipes.NamedPipeClientStream(".", "MSAgentAI", [System.IO.Pipes.PipeDirection]::InOut)
$pipe.Connect(5000)

$writer = New-Object System.IO.StreamWriter($pipe)
$reader = New-Object System.IO.StreamReader($pipe)
$writer.AutoFlush = $true

$writer.WriteLine("SPEAK:Hello from PowerShell!")
$response = $reader.ReadLine()
Write-Host "Response: $response"

$pipe.Close()
```

## Use Cases

### Game Integration
- Announce in-game events: `SPEAK:Player defeated the boss!`
- React to game state: `CHAT:The player just died, say something encouraging`
- Display emotions: `ANIMATION:Sad` followed by `SPEAK:Better luck next time`

### Automation
- Notify on build completion: `SPEAK:Your build has finished`
- Alert on email: `SPEAK:You have new mail`
- System status: `CHAT:Tell me about CPU usage at 90%`

### Streaming
- React to chat commands: `SPEAK:Thanks for the subscription!`
- Viewer interaction: `CHAT:Someone asked about your favorite game`

## Notes
- The pipe server starts automatically when MSAgent-AI launches
- Multiple commands can be sent in sequence
- Commands are processed asynchronously - CHAT commands may take time for AI response
- The pipe supports multiple simultaneous connections
- Logs are written to `MSAgentAI.log` for debugging
