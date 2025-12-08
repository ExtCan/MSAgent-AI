# Troubleshooting Checklist

## Installation Verification

Use this checklist to verify your installation is correct:

### MSAgent-AI Application

- [ ] MSAgentAI.exe is built and runs without errors
- [ ] Agent character appears on desktop
- [ ] System tray icon is visible
- [ ] Can open Settings dialog
- [ ] Voice/SAPI4 is configured and working
- [ ] (Optional) Ollama is running if AI features are desired
- [ ] Check `MSAgentAI.log` shows "Pipeline server started on pipe: \\.\pipe\MSAgentAI"

### GTA V Base Game

- [ ] GTA V is installed and launches correctly
- [ ] Game is up to date (latest version)
- [ ] Can load into Story Mode
- [ ] No existing issues with the game

### ScriptHookV Installation

- [ ] Downloaded latest ScriptHookV from http://www.dev-c.com/gtav/scripthookv/
- [ ] `ScriptHookV.dll` exists in GTA V root folder (same folder as GTA5.exe)
- [ ] `dinput8.dll` exists in GTA V root folder
- [ ] ScriptHookV version matches your game version
- [ ] Check `ScriptHookV.log` in GTA V folder for errors

### ScriptHookVDotNet Installation

- [ ] Downloaded latest ScriptHookVDotNet from https://github.com/scripthookvdotnet/scripthookvdotnet/releases
- [ ] `ScriptHookVDotNet.asi` exists in GTA V root folder
- [ ] `ScriptHookVDotNet2.dll` or `ScriptHookVDotNet3.dll` exists in GTA V root folder
- [ ] `scripts` folder exists in GTA V root folder
- [ ] No errors in ScriptHookVDotNet logs

### MSAgentGTAV Script Installation

- [ ] `MSAgentGTAV.dll` exists in `[GTA V]\scripts\` folder
- [ ] File size is reasonable (>10KB, typically 20-50KB)
- [ ] File is not blocked (Right-click → Properties → Unblock if needed)

## Testing Steps

### Step 1: Test MSAgent-AI Alone

1. Launch MSAgentAI.exe
2. Agent should appear on screen
3. Right-click tray icon → Speak → Say Something
4. Type "Test" and click OK
5. Agent should say "Test"

**If this fails:** Fix MSAgent-AI installation first before proceeding.

### Step 2: Test Pipe Communication

Open PowerShell and run:

```powershell
$pipe = New-Object System.IO.Pipes.NamedPipeClientStream(".", "MSAgentAI", [System.IO.Pipes.PipeDirection]::InOut)
$pipe.Connect(5000)
$writer = New-Object System.IO.StreamWriter($pipe)
$reader = New-Object System.IO.StreamReader($pipe)
$writer.AutoFlush = $true
$writer.WriteLine("PING")
$response = $reader.ReadLine()
Write-Host "Response: $response"
$pipe.Close()
```

Expected output: `Response: PONG`

**If this fails:** MSAgent-AI pipe server is not running correctly.

### Step 3: Test GTA V with ScriptHookV

1. Ensure GTA V is NOT running
2. Launch GTA V
3. Load into Story Mode
4. Check `ScriptHookV.log` in GTA V folder
5. Should see "INIT: SUCCESS" or similar

**If this fails:** ScriptHookV is not installed correctly or version mismatch.

### Step 4: Test ScriptHookVDotNet

1. Create a test file: `[GTA V]\scripts\test.txt`
2. Launch GTA V and load Story Mode
3. Check if ScriptHookVDotNet is loading scripts
4. Check ScriptHookVDotNet logs for errors

**If this fails:** ScriptHookVDotNet is not installed correctly.

### Step 5: Test MSAgentGTAV Script

1. Ensure MSAgent-AI is running
2. Launch GTA V
3. Load into Story Mode
4. Look for notification: "GTA V integration loaded!"
5. MSAgent should say: "GTA V integration loaded! I'm ready to commentate!"

**If this fails:** See specific error scenarios below.

### Step 6: Test Menu

1. In game, press F9
2. Menu should appear with title "MSAgent-AI"
3. Should see 8 checkbox items
4. Press F9 again to close

**If this fails:** Script is not running or F9 key conflict.

### Step 7: Test Reactions

1. Ensure MSAgent-AI is running
2. Open menu (F9) and verify all options are enabled
3. Get into any vehicle
4. Within 3 seconds, MSAgent should react

**If this fails:** Check each component individually.

## Common Error Scenarios

### Error: "Script not loading"

**Symptoms:**
- No "GTA V integration loaded!" notification
- MSAgent doesn't say anything
- F9 menu doesn't appear

**Possible Causes:**

1. **ScriptHookV version mismatch**
   - Solution: Download latest ScriptHookV that matches your game version
   - After game updates, you must update ScriptHookV

2. **ScriptHookVDotNet not installed**
   - Solution: Install ScriptHookVDotNet from official repo
   - Ensure all files are extracted to GTA V root

3. **DLL not in scripts folder**
   - Solution: Verify `MSAgentGTAV.dll` is in `[GTA V]\scripts\`
   - Create the folder if it doesn't exist

4. **DLL is blocked by Windows**
   - Solution: Right-click `MSAgentGTAV.dll` → Properties → Unblock → OK

5. **Build error in DLL**
   - Solution: Try downloading a pre-built version
   - Or rebuild from source and check for errors

### Error: "MSAgent not responding"

**Symptoms:**
- Script loads (notification appears)
- F9 menu works
- But MSAgent doesn't react to events

**Possible Causes:**

1. **MSAgent-AI not running**
   - Solution: Launch MSAgentAI.exe BEFORE starting GTA V
   - Check system tray for icon

2. **Pipe server not started**
   - Solution: Check `MSAgentAI.log` for "Pipeline server started"
   - Restart MSAgent-AI if needed

3. **Pipe name conflict**
   - Solution: Ensure only one instance of MSAgent-AI is running
   - Kill any orphaned processes

4. **All reactions disabled**
   - Solution: Open menu (F9) and enable reaction types

5. **Cooldown preventing reactions**
   - Solution: Wait 10 seconds between events
   - Or reduce cooldown times in source code

### Error: "Game crashes on startup"

**Symptoms:**
- GTA V crashes during loading
- Game becomes unstable
- CTD (Crash to Desktop)

**Possible Causes:**

1. **Too many scripts/mods**
   - Solution: Disable other scripts temporarily
   - Test MSAgentGTAV alone

2. **Corrupted game files**
   - Solution: Verify game integrity in launcher
   - Reinstall if necessary

3. **Outdated mods**
   - Solution: Update all mods to latest versions
   - Remove incompatible mods

4. **Insufficient memory**
   - Solution: Close other applications
   - Increase virtual memory

### Error: "Menu doesn't open (F9 not working)"

**Symptoms:**
- F9 key does nothing
- Menu never appears
- Script seems to load otherwise

**Possible Causes:**

1. **Key conflict with another mod**
   - Solution: Change keybind in source code
   - Rebuild the script

2. **Script not fully loaded**
   - Solution: Wait 30 seconds after game loads
   - Try again

3. **Menu pool issue**
   - Solution: Check for other NativeUI mods
   - May need to update ScriptHookVDotNet

### Error: "Too many reactions / Spam"

**Symptoms:**
- MSAgent talks constantly
- Reactions every second
- Overwhelming amount of commentary

**Possible Causes:**

1. **Cooldowns too short**
   - Solution: Open menu and disable some reaction types
   - Or increase cooldown times in source

2. **Multiple reaction types triggering**
   - Solution: Keep only the reactions you want enabled
   - Use menu to toggle off unwanted types

### Error: "No reactions for specific events"

**Symptoms:**
- Some events work, others don't
- Vehicles work but weather doesn't
- Inconsistent behavior

**Possible Causes:**

1. **Specific reaction disabled**
   - Solution: Check menu (F9) settings
   - Ensure the reaction type is checked

2. **Event not triggering in game**
   - Solution: Some events are rare
   - Try forcing the event (weather cheat, etc.)

3. **Cooldown still active**
   - Solution: Wait longer between similar events
   - Each event type has its own cooldown

## Advanced Debugging

### Enable Detailed Logging

1. Check `MSAgentAI.log` in MSAgent-AI folder
2. Check `ScriptHookV.log` in GTA V folder
3. Look for error messages

### Test Pipe Communication Manually

Use this C# test program:

```csharp
using System;
using System.IO.Pipes;
using System.IO;

class Test
{
    static void Main()
    {
        try
        {
            using (var client = new NamedPipeClientStream(".", "MSAgentAI", PipeDirection.InOut))
            {
                Console.WriteLine("Connecting to MSAgentAI pipe...");
                client.Connect(5000);
                Console.WriteLine("Connected!");
                
                using (var reader = new StreamReader(client))
                using (var writer = new StreamWriter(client) { AutoFlush = true })
                {
                    writer.WriteLine("SPEAK:This is a test from GTA V script!");
                    string response = reader.ReadLine();
                    Console.WriteLine($"Response: {response}");
                }
            }
            Console.WriteLine("Success!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
```

### Inspect Running Scripts

Use a tool like Process Explorer to verify:
- GTA5.exe is running
- MSAgentAI.exe is running
- Both processes are active

### Clean Reinstall

If all else fails:

1. Remove `MSAgentGTAV.dll` from scripts folder
2. Remove all ScriptHookV files
3. Remove all ScriptHookVDotNet files
4. Restart computer
5. Reinstall in order: ScriptHookV → ScriptHookVDotNet → MSAgentGTAV
6. Test each step

## Getting Help

If you've tried everything:

1. Check the full [README.md](README.md)
2. Review [PIPELINE.md](../PIPELINE.md)
3. Check both log files
4. Create an issue on GitHub with:
   - Your GTA V version
   - ScriptHookV version
   - ScriptHookVDotNet version
   - Contents of both log files
   - Exact error message or behavior

## Performance Tips

### Reduce CPU Usage

- Disable reaction types you don't use
- Increase cooldown times
- Use SPEAK instead of CHAT commands (faster)

### Improve Response Time

- Use a faster Ollama model (e.g., llama3.2:1b)
- Enable Ollama GPU acceleration
- Keep MSAgent-AI personality prompt short

### Prevent Interruptions

- Increase cooldown times in source code
- Disable frequent reaction types (vehicles, location)
- Keep only essential reactions enabled
