# Advanced Features & Customization

This guide covers advanced features and customization options for power users.

## Customizing Prompts

All prompts sent to MSAgent-AI can be customized by editing `MSAgentGTAV.cs`.

### Vehicle Entry Prompt

**Location:** `CheckVehicleChange()` method

**Default:**
```csharp
SendChatPrompt($"The player just entered a {vehicleType} called {vehicleName}{valueInfo}. React to this vehicle!");
```

**Custom Example:**
```csharp
// More excited reactions
SendChatPrompt($"OH WOW! The player is now driving a {vehicleName}! That's a {vehicleType}{valueInfo}! Say something cool!");

// Roleplay as a car enthusiast
SendChatPrompt($"Vehicle spotted: {vehicleName}. Give a detailed review of this {vehicleType} like a car enthusiast!");

// Humorous reactions
SendChatPrompt($"The player just stole a {vehicleName}. Make a joke about their choice of vehicle!");
```

### Weather Change Prompt

**Location:** `CheckWeatherChange()` method

**Custom Examples:**
```csharp
// Weather forecaster style
SendChatPrompt($"Weather update: We're now experiencing {weatherName} conditions. Provide a weather report!");

// Poetic reactions
SendChatPrompt($"The sky turns to {weatherName}. Describe it poetically!");

// Concerned friend
SendChatPrompt($"Oh no, it's {weatherName}! Express concern about the player driving in this weather.");
```

### Death Prompt

**Location:** `OnTick()` method, player death check

**Custom Examples:**
```csharp
// Dark humor
SendChatPrompt("The player died AGAIN! Mock them gently about their driving skills.");

// Supportive
SendChatPrompt("The player died. Offer encouraging words and tell them they'll do better next time.");

// Statistics tracking
SendChatPrompt($"Player death #{deathCount}! Keep track and comment on how many times they've died.");
```

## Adding New Reaction Types

### Example: Speed Tracking

Add speed-based reactions:

```csharp
// Add to class variables
private float lastSpeed = 0;
private DateTime lastSpeedReactionTime = DateTime.MinValue;
private bool reactToSpeed = true;

// Add to menu setup
var speedToggle = new UIMenuCheckboxItem("React to Speed", reactToSpeed,
    "Enable/disable reactions to high speeds");
mainMenu.AddItem(speedToggle);

// Add to OnCheckboxChange handler
else if (item == speedToggle) reactToSpeed = checked_;

// Add new method
private void CheckSpeed(Ped player)
{
    if (!reactToSpeed) return;
    
    Vehicle vehicle = player.CurrentVehicle;
    if (vehicle == null) return;
    
    float currentSpeed = vehicle.Speed * 2.23694f; // Convert to MPH
    
    // React to high speed
    if (currentSpeed > 100 && lastSpeed <= 100)
    {
        if (CanReact(ref lastSpeedReactionTime, COOLDOWN_MS))
        {
            SendChatPrompt($"The player is going {currentSpeed:F0} MPH! React to their high speed!");
        }
    }
    
    lastSpeed = currentSpeed;
}

// Call in OnTick
CheckSpeed(player);
```

### Example: Combat Tracking

Track when player shoots weapons:

```csharp
// Add to class variables
private bool reactToCombat = true;
private DateTime lastCombatReactionTime = DateTime.MinValue;
private bool wasInCombat = false;

// Add method
private void CheckCombat(Ped player)
{
    if (!reactToCombat) return;
    
    bool isInCombat = player.IsInCombat;
    
    if (isInCombat && !wasInCombat)
    {
        if (CanReact(ref lastCombatReactionTime, COOLDOWN_MS))
        {
            SendChatPrompt("The player just started shooting! React to the combat!");
        }
    }
    else if (!isInCombat && wasInCombat)
    {
        if (CanReact(ref lastCombatReactionTime, COOLDOWN_MS))
        {
            SendChatPrompt("The combat ended. Comment on how it went.");
        }
    }
    
    wasInCombat = isInCombat;
}
```

### Example: Money Tracking

React to changes in player money:

```csharp
private bool reactToMoney = true;
private int lastMoney = 0;
private DateTime lastMoneyReactionTime = DateTime.MinValue;

private void CheckMoney(Ped player)
{
    if (!reactToMoney) return;
    
    int currentMoney = Game.Player.Money;
    
    if (lastMoney > 0 && currentMoney != lastMoney)
    {
        int difference = currentMoney - lastMoney;
        
        if (Math.Abs(difference) > 1000) // Only react to significant changes
        {
            if (CanReact(ref lastMoneyReactionTime, COOLDOWN_MS))
            {
                if (difference > 0)
                {
                    SendChatPrompt($"The player earned ${difference:N0}! Congratulate them!");
                }
                else
                {
                    SendChatPrompt($"The player lost ${Math.Abs(difference):N0}! React to their loss!");
                }
            }
        }
    }
    
    lastMoney = currentMoney;
}
```

## Advanced Menu Features

### Adding Submenus

Create organized submenus for different settings:

```csharp
private void SetupAdvancedMenu()
{
    menuPool = new MenuPool();
    mainMenu = new UIMenu("MSAgent-AI", "~b~GTA V Integration");
    menuPool.Add(mainMenu);
    
    // Create submenus
    var vehicleMenu = menuPool.AddSubMenu(mainMenu, "Vehicle Settings");
    var environmentMenu = menuPool.AddSubMenu(mainMenu, "Environment Settings");
    var playerMenu = menuPool.AddSubMenu(mainMenu, "Player Settings");
    
    // Add items to vehicle submenu
    vehicleMenu.AddItem(new UIMenuCheckboxItem("React to Entry", reactToVehicles));
    vehicleMenu.AddItem(new UIMenuCheckboxItem("React to Value", reactToVehicleValue));
    vehicleMenu.AddItem(new UIMenuCheckboxItem("React to Speed", reactToSpeed));
    
    // Add items to environment submenu
    environmentMenu.AddItem(new UIMenuCheckboxItem("React to Weather", reactToWeather));
    environmentMenu.AddItem(new UIMenuCheckboxItem("React to Time", reactToTime));
    environmentMenu.AddItem(new UIMenuCheckboxItem("React to Location", reactToLocation));
    
    // Add items to player submenu
    playerMenu.AddItem(new UIMenuCheckboxItem("React to Health", reactToPlayerState));
    playerMenu.AddItem(new UIMenuCheckboxItem("React to Death", reactToPlayerState));
    playerMenu.AddItem(new UIMenuCheckboxItem("React to Wanted Level", reactToPlayerState));
}
```

### Adding Sliders for Cooldowns

Allow in-game adjustment of cooldown times:

```csharp
// Add to menu setup
var cooldownSlider = new UIMenuSliderItem("Reaction Cooldown (seconds)", 
    new List<object> { 1, 3, 5, 10, 15, 30 }, 3, "Adjust time between reactions");
mainMenu.AddItem(cooldownSlider);

// Handle slider changes
mainMenu.OnSliderChange += (sender, item, index) =>
{
    if (item == cooldownSlider)
    {
        COOLDOWN_MS = ((int)cooldownSlider.Items[index]) * 1000;
        Notification.Show($"~g~Cooldown set to {cooldownSlider.Items[index]} seconds");
    }
};
```

## Performance Optimization

### Reduce Tick Rate

If experiencing performance issues, reduce update frequency:

```csharp
private int tickCounter = 0;
private const int TICK_INTERVAL = 10; // Only run every 10 frames

private void OnTick(object sender, EventArgs e)
{
    tickCounter++;
    if (tickCounter < TICK_INTERVAL)
        return;
    
    tickCounter = 0;
    
    // Rest of OnTick code here...
}
```

### Async Reactions

Make reactions truly non-blocking:

```csharp
private async Task SendChatPromptAsync(string prompt)
{
    await Task.Run(() => SendChatPrompt(prompt));
}

// Usage
await SendChatPromptAsync("Your prompt here");
```

### Batch Reactions

Queue multiple reactions and send them together:

```csharp
private Queue<string> reactionQueue = new Queue<string>();
private DateTime lastQueueProcess = DateTime.Now;

private void QueueReaction(string prompt)
{
    reactionQueue.Enqueue(prompt);
}

private void ProcessQueue()
{
    if (reactionQueue.Count == 0)
        return;
    
    if ((DateTime.Now - lastQueueProcess).TotalSeconds < 5)
        return;
    
    string combined = string.Join(" Also, ", reactionQueue);
    reactionQueue.Clear();
    SendChatPrompt(combined);
    lastQueueProcess = DateTime.Now;
}
```

## Integration with Other Mods

### Sharing State with Other Scripts

Create a shared state file:

```csharp
using System.IO;
using Newtonsoft.Json;

private void SaveState()
{
    var state = new
    {
        LastVehicle = lastVehicle?.FriendlyName,
        LastLocation = lastZone,
        WantedLevel = lastWantedLevel,
        Timestamp = DateTime.Now
    };
    
    string json = JsonConvert.SerializeObject(state, Formatting.Indented);
    File.WriteAllText("scripts\\MSAgentState.json", json);
}
```

### Responding to Other Mod Events

Hook into other mods' events if they expose them:

```csharp
// Example: React to a mission mod's events
public MSAgentGTAV()
{
    // ... existing code ...
    
    // If another mod exposes events
    AnotherMod.OnMissionComplete += (sender, missionName) =>
    {
        SendChatPrompt($"Mission '{missionName}' completed! Congratulate the player!");
    };
}
```

## Using Native Functions

Access more game data using native functions:

### Get Current Radio Station

```csharp
private void CheckRadioStation()
{
    string station = Function.Call<string>(Hash.GET_PLAYER_RADIO_STATION_NAME);
    
    if (!string.IsNullOrEmpty(station) && station != lastStation)
    {
        lastStation = station;
        SendChatPrompt($"The player changed to radio station {station}. Comment on their music taste!");
    }
}
```

### Get Nearest Street Name

```csharp
private string GetNearestStreet(Vector3 position)
{
    string streetName = "";
    string crossingRoad = "";
    
    unsafe
    {
        int streetHash = 0;
        int crossingHash = 0;
        Function.Call(Hash.GET_STREET_NAME_AT_COORD, 
            position.X, position.Y, position.Z,
            &streetHash, &crossingHash);
        
        streetName = Function.Call<string>(Hash.GET_STREET_NAME_FROM_HASH_KEY, streetHash);
    }
    
    return streetName;
}
```

### Get Vehicle Damage

```csharp
private void CheckVehicleDamage(Vehicle vehicle)
{
    if (vehicle == null) return;
    
    float health = vehicle.Health;
    float maxHealth = vehicle.MaxHealth;
    float healthPercent = (health / maxHealth) * 100;
    
    if (healthPercent < 50 && healthPercent > 0)
    {
        if (CanReact(ref lastDamageReactionTime, COOLDOWN_MS))
        {
            SendChatPrompt($"The vehicle is badly damaged ({healthPercent:F0}% health)! React to the poor condition!");
        }
    }
}
```

## Custom Personalities

Create different MSAgent personalities:

```csharp
private enum Personality
{
    Friendly,
    Sarcastic,
    Professional,
    Excited
}

private Personality currentPersonality = Personality.Friendly;

private string FormatPrompt(string basePrompt)
{
    string prefix = currentPersonality switch
    {
        Personality.Friendly => "In a friendly and supportive tone: ",
        Personality.Sarcastic => "With sarcasm and wit: ",
        Personality.Professional => "In a professional news reporter style: ",
        Personality.Excited => "With extreme excitement and enthusiasm: ",
        _ => ""
    };
    
    return prefix + basePrompt;
}

// Usage
SendChatPrompt(FormatPrompt("The player just crashed their car."));
```

## Debugging Features

### Visual Debug Info

Display debug information on screen:

```csharp
private bool showDebugInfo = false;

private void DisplayDebugInfo()
{
    if (!showDebugInfo) return;
    
    var lines = new List<string>
    {
        $"Last Reaction: {(DateTime.Now - lastReactionTime).TotalSeconds:F1}s ago",
        $"Current Zone: {lastZone}",
        $"Wanted Level: {lastWantedLevel}",
        $"Vehicle: {lastVehicle?.FriendlyName ?? "None"}",
        $"Time: {World.CurrentDayTime.Hours:D2}:{World.CurrentDayTime.Minutes:D2}"
    };
    
    float y = 0.5f;
    foreach (var line in lines)
    {
        new UIResText(line, new Point(10, (int)(y * Screen.Height)), 0.3f).Draw();
        y += 0.03f;
    }
}

// Call in OnTick
DisplayDebugInfo();
```

### Logging to File

Create detailed logs:

```csharp
private void Log(string message)
{
    string logFile = "scripts\\MSAgentGTAV.log";
    string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
    File.AppendAllText(logFile, entry);
}

// Usage
Log($"Vehicle changed to: {vehicle.FriendlyName}");
Log($"Sent prompt: {prompt}");
```

## Configuration File

Load settings from a file:

```csharp
using Newtonsoft.Json;

public class Config
{
    public int StandardCooldownMs { get; set; } = 10000;
    public int FastCooldownMs { get; set; } = 3000;
    public string MenuKey { get; set; } = "F9";
    public Dictionary<string, bool> Toggles { get; set; } = new();
}

private Config LoadConfig()
{
    string configPath = "scripts\\MSAgentGTAV.json";
    
    if (File.Exists(configPath))
    {
        string json = File.ReadAllText(configPath);
        return JsonConvert.DeserializeObject<Config>(json);
    }
    
    return new Config();
}

private void SaveConfig(Config config)
{
    string json = JsonConvert.SerializeObject(config, Formatting.Indented);
    File.WriteAllText("scripts\\MSAgentGTAV.json", json);
}
```

## Easter Eggs

Add fun surprises:

```csharp
private void CheckForEasterEggs(Ped player)
{
    // React if player is at a specific location
    if (player.Position.DistanceTo(new Vector3(-1337, 4567, 21)) < 10)
    {
        SendChatPrompt("You found the secret location! Make an excited announcement!");
    }
    
    // React to specific vehicle combinations
    if (lastVehicle?.Model.Hash == VehicleHash.Faggio.GetHashCode() && 
        Game.Player.WantedLevel >= 4)
    {
        SendChatPrompt("Running from 4-star cops on a scooter? You're either brave or crazy!");
    }
    
    // React to time-based events
    if (World.CurrentDayTime.Hours == 4 && World.CurrentDayTime.Minutes == 20)
    {
        SendChatPrompt("It's 4:20 in the game! Make a joke about the time!");
    }
}
```

## Contributing Your Customizations

If you create cool features:

1. Fork the repository
2. Add your features to a new file (e.g., `MSAgentGTAV_Extended.cs`)
3. Document your additions
4. Submit a pull request

Share your creativity with the community!
