# MSAgent-AI Communication Pipeline

The MSAgent-AI application supports two communication protocols for external applications (games, scripts, mods) to send commands for AI interaction:

1. **Named Pipe** (Windows local communication)
2. **TCP Socket** (Network communication, supports remote connections)

## Configuration

Pipeline settings can be configured in the application's Settings dialog:
- **Protocol**: Choose between "NamedPipe" or "TCP"
- **IP Address**: For TCP mode (default: 127.0.0.1)
- **Port**: For TCP mode (default: 8765)
- **Pipe Name**: For Named Pipe mode (default: MSAgentAI)

## Named Pipe Mode

Default pipe name:
```
\\.\pipe\MSAgentAI
```

Named pipes only work for local processes on the same machine.

## TCP Socket Mode

Default connection:
```
IP: 127.0.0.1 (localhost only)
Port: 8765
```

### IP Address Settings

**IMPORTANT for LAN/Network access:**

- **`127.0.0.1`** - Accepts connections ONLY from localhost (same computer)
- **`0.0.0.0`** - Accepts connections from ANY network interface (LAN, Wi-Fi, Internet)
- **Specific IP** (e.g., `192.168.1.100`) - Binds to a specific network interface

**For LAN access, you MUST use `0.0.0.0`!**

### Setting Up LAN Access

1. **Open MSAgent-AI Settings**
   - Go to Settings → Pipeline tab
   - Set Protocol to "TCP"
   - Set IP Address to "0.0.0.0" ⚠️ CRITICAL for LAN!
   - Set Port to "8765"
   - Click Apply

2. **Configure Windows Firewall**
   - Open Windows Defender Firewall
   - Click "Advanced settings"
   - Click "Inbound Rules" → "New Rule"
   - Select "Port" → TCP → Specific local ports: 8765
   - Allow the connection
   - Apply to all profiles (Domain, Private, Public)
   - Name it "MSAgent-AI TCP Pipeline"

   Or run this command as Administrator:
   ```cmd
   netsh advfirewall firewall add rule name="MSAgent-AI TCP Pipeline" dir=in action=allow protocol=TCP localport=8765
   ```

3. **Find Your IP Address**
   - Open Command Prompt
   - Run: `ipconfig`
   - Look for "IPv4 Address" under your active network adapter
   - Example: `192.168.1.100`

4. **Test the Connection**
   From another machine on your network:
   ```python
   python test_lan_connection.py 192.168.1.100
   ```

TCP sockets can connect from:
- Local machine (127.0.0.1 or localhost)
- Remote machines on the network (when IP is set to 0.0.0.0)
- Internet (if firewall allows and IP is accessible)

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

### Python - Named Pipe
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

### Python - TCP Socket
```python
import socket

def send_command_tcp(command, host='127.0.0.1', port=8765):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.connect((host, port))
        
        # Send command
        s.sendall((command + '\n').encode('utf-8'))
        
        # Read response
        response = s.recv(1024).decode('utf-8').strip()
        return response

# Examples
send_command_tcp("SPEAK:Hello from Python over TCP!")
send_command_tcp("ANIMATION:Wave")
send_command_tcp("CHAT:What's the weather like?")

# Remote connection example
send_command_tcp("SPEAK:Hello from another computer!", host='192.168.1.100', port=8765)
```

### C# - Named Pipe
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

### C# - TCP Socket
```csharp
using System.Net.Sockets;
using System.Text;

void SendCommandTcp(string command, string host = "127.0.0.1", int port = 8765)
{
    using (var client = new TcpClient(host, port))
    using (var stream = client.GetStream())
    using (var reader = new StreamReader(stream, Encoding.UTF8))
    using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
    {
        writer.WriteLine(command);
        string response = reader.ReadLine();
        Console.WriteLine($"Response: {response}");
    }
}

// Examples
SendCommandTcp("SPEAK:Hello from C# over TCP!");
SendCommandTcp("POKE");

// Remote connection
SendCommandTcp("SPEAK:Hello from remote!", "192.168.1.100", 8765);
```

### AutoHotkey - Named Pipe
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

### Lua (for game mods) - Named Pipe
```lua
-- Example for games with Lua scripting and pipe support (Windows)
local pipe = io.open("\\\\.\\pipe\\MSAgentAI", "r+")
if pipe then
    pipe:write("SPEAK:Player scored a point!\n")
    pipe:flush()
    local response = pipe:read("*l")
    pipe:close()
end
```

### Lua (for game mods) - TCP Socket
```lua
-- TCP socket example (works cross-platform with LuaSocket)
local socket = require("socket")
local client = socket.tcp()

client:connect("127.0.0.1", 8765)
client:send("SPEAK:Player scored a point!\n")
local response = client:receive("*l")
client:close()

-- For remote connections
local remote = socket.tcp()
remote:connect("192.168.1.100", 8765)
remote:send("SPEAK:Hello from game!\n")
remote:close()
```

### PowerShell - Named Pipe
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

### PowerShell - TCP Socket
```powershell
$client = New-Object System.Net.Sockets.TcpClient("127.0.0.1", 8765)
$stream = $client.GetStream()
$writer = New-Object System.IO.StreamWriter($stream)
$reader = New-Object System.IO.StreamReader($stream)
$writer.AutoFlush = $true

$writer.WriteLine("SPEAK:Hello from PowerShell over TCP!")
$response = $reader.ReadLine()
Write-Host "Response: $response"

$client.Close()

# Remote connection
$remoteClient = New-Object System.Net.Sockets.TcpClient("192.168.1.100", 8765)
# ... (same pattern as above)
```

### Node.js - TCP Socket
```javascript
const net = require('net');

function sendCommand(command, host = '127.0.0.1', port = 8765) {
    return new Promise((resolve, reject) => {
        const client = new net.Socket();
        
        client.connect(port, host, () => {
            client.write(command + '\n');
        });
        
        client.on('data', (data) => {
            const response = data.toString().trim();
            client.destroy();
            resolve(response);
        });
        
        client.on('error', (err) => {
            reject(err);
        });
    });
}

// Examples
sendCommand('SPEAK:Hello from Node.js!').then(response => console.log(response));
sendCommand('ANIMATION:Wave').then(response => console.log(response));

// Remote connection
sendCommand('SPEAK:Hello from remote!', '192.168.1.100', 8765);
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

### Remote Monitoring
- Monitor servers from your desktop: Connect from remote machines to trigger alerts
- Multi-machine coordination: Have agents on different computers communicate status

## Troubleshooting

### TCP Connection Issues

**Problem: "Connection refused" or "Cannot connect"**

1. **Check MSAgent-AI is running**
   - Look for MSAgent-AI in system tray
   - If not running, start the application

2. **Verify TCP mode is enabled**
   - Open Settings → Pipeline tab
   - Confirm Protocol is set to "TCP" (not "NamedPipe")
   - Click Apply if you made changes

3. **Check IP address configuration**
   - **For localhost only**: Use `127.0.0.1`
   - **For LAN access**: Use `0.0.0.0` ⚠️ CRITICAL!
   - Common mistake: Using `127.0.0.1` for LAN (this won't work!)

4. **Verify port number**
   - Default is 8765
   - Make sure client uses same port as server

5. **Check firewall settings**
   - Windows Firewall must allow TCP port 8765
   - Run as Administrator: `netsh advfirewall firewall add rule name="MSAgent-AI" dir=in action=allow protocol=TCP localport=8765`

6. **Test with diagnostic tool**
   ```bash
   python test_lan_connection.py YOUR_IP_ADDRESS
   ```

**Problem: "Timeout" after connecting**

- Server accepted connection but didn't respond
- Check MSAgent-AI logs (`MSAgentAI.log`)
- Verify command format (must end with `\n`)
- Try: `echo "PING" | nc YOUR_IP 8765`

**Problem: Works on localhost but not from LAN**

- **IP address MUST be `0.0.0.0`**, not `127.0.0.1`!
- Check firewall on server machine
- Check network connectivity: `ping YOUR_IP`
- Verify both machines are on same network/VLAN

### Named Pipe Issues

**Problem: "Pipe not found"**

- Ensure MSAgent-AI is running
- Verify Protocol is set to "NamedPipe"
- Check pipe name matches (default: `MSAgentAI`)
- Named pipes only work on same Windows machine

### General Debug Steps

1. **Check logs**: `MSAgentAI.log` in application folder
2. **Test with PING**: Simplest command to verify connectivity
3. **Try localhost first**: Test with `127.0.0.1` before LAN
4. **Use diagnostic tools**: Python test scripts provided
5. **Restart application**: After changing settings, restart MSAgent-AI

### Quick LAN Setup Checklist

- [ ] MSAgent-AI is running
- [ ] Settings → Pipeline → Protocol = "TCP"
- [ ] Settings → Pipeline → IP Address = "0.0.0.0" ⚠️
- [ ] Settings → Pipeline → Port = 8765
- [ ] Clicked "Apply" in Settings
- [ ] Windows Firewall allows port 8765
- [ ] Found server IP with `ipconfig`
- [ ] Can ping server from client: `ping SERVER_IP`
- [ ] Test script runs successfully: `python test_lan_connection.py SERVER_IP`

## Notes
- The pipeline server starts automatically when MSAgent-AI launches
- Protocol can be configured in Settings (Named Pipe or TCP)
- **Named Pipe** is recommended for local applications (better performance, Windows-only)
- **TCP Socket** is recommended for:
  - Network communication between computers
  - Cross-platform clients (Linux, macOS can connect via TCP)
  - Remote/internet-based integrations
  - When you need IP/Port customization
- Multiple commands can be sent in sequence
- Commands are processed asynchronously - CHAT commands may take time for AI response
- TCP mode supports multiple simultaneous connections
- Logs are written to `MSAgentAI.log` for debugging
- For TCP mode, ensure your firewall allows connections on the configured port
- Default TCP port 8765 can be changed in Settings
