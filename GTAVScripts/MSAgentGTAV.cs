using System;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.UI;
using GTA.Math;
using GTA.Native;
using NativeUI;

namespace MSAgentGTAV
{
    /// <summary>
    /// MSAgent-AI integration for GTA V
    /// Live commentary and reactions to in-game events via TCP or Named Pipe communication
    /// </summary>
    public class MSAgentGTAV : Script
    {
        // Configuration (loaded from INI file)
        private string protocol = "TCP"; // TCP or NamedPipe
        private string ipAddress = "127.0.0.1";
        private int port = 8765;
        private string pipeName = "MSAgentAI";
        private int COOLDOWN_MS = 10000; // 10 second cooldown between reactions (configurable)
        private int FAST_COOLDOWN_MS = 3000; // 3 second cooldown for frequent events (configurable)
        
        // Menu system
        private UIMenu mainMenu;
        private MenuPool menuPool;
        private bool menuEnabled = true;
        
        // Toggle switches for different reaction types
        private bool reactToVehicles = true;
        private bool reactToMissions = true;
        private bool reactToWeather = true;
        private bool reactToTime = true;
        private bool reactToLocation = true;
        private bool reactToPlayerState = true;
        private bool reactToCharacterSwitch = true;
        private bool reactToVehicleValue = true;
        
        // State tracking
        private Vehicle lastVehicle;
        private Weather lastWeather;
        private int lastHour = -1;
        private string lastZone = "";
        private int lastWantedLevel = 0;
        private DateTime lastReactionTime = DateTime.MinValue;
        private DateTime lastVehicleReactionTime = DateTime.MinValue;
        private DateTime lastLocationReactionTime = DateTime.MinValue;
        private DateTime lastWeatherReactionTime = DateTime.MinValue;
        private Model lastCharacterModel;
        private bool playerWasDead = false;
        private Hash lastMissionHash = (Hash)0;
        
        // Cached character hashes for performance
        private readonly int michaelHash = PedHash.Michael.GetHashCode();
        private readonly int franklinHash = PedHash.Franklin.GetHashCode();
        private readonly int trevorHash = PedHash.Trevor.GetHashCode();
        
        // Vehicle value tracking
        private Dictionary<VehicleHash, int> vehicleValues = new Dictionary<VehicleHash, int>();
        
        public MSAgentGTAV()
        {
            LoadConfiguration();
            InitializeVehicleValues();
            SetupMenu();
            
            // Event handlers
            Tick += OnTick;
            Aborted += OnAborted;
            
            SendToAgent("SPEAK:GTA V integration loaded! I'm ready to commentate!");
        }
        
        private void LoadConfiguration()
        {
            try
            {
                string iniPath = "scripts\\MSAgentGTAV.ini";
                if (!File.Exists(iniPath))
                {
                    // Use defaults if INI doesn't exist
                    return;
                }
                
                foreach (string line in File.ReadAllLines(iniPath))
                {
                    if (line.StartsWith(";") || line.StartsWith("[") || string.IsNullOrWhiteSpace(line))
                        continue;
                    
                    string[] parts = line.Split(new[] { '=' }, 2);
                    if (parts.Length != 2)
                        continue;
                    
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    
                    switch (key)
                    {
                        case "Protocol":
                            protocol = value;
                            break;
                        case "IPAddress":
                            ipAddress = value;
                            break;
                        case "Port":
                            if (int.TryParse(value, out int p))
                                port = p;
                            break;
                        case "PipeName":
                            pipeName = value;
                            break;
                        case "SlowCooldown":
                            if (int.TryParse(value, out int sc))
                                COOLDOWN_MS = sc;
                            break;
                        case "FastCooldown":
                            if (int.TryParse(value, out int fc))
                                FAST_COOLDOWN_MS = fc;
                            break;
                        case "ReactToVehicles":
                            reactToVehicles = bool.Parse(value);
                            break;
                        case "ReactToMissions":
                            reactToMissions = bool.Parse(value);
                            break;
                        case "ReactToWeather":
                            reactToWeather = bool.Parse(value);
                            break;
                        case "ReactToTime":
                            reactToTime = bool.Parse(value);
                            break;
                        case "ReactToLocation":
                            reactToLocation = bool.Parse(value);
                            break;
                        case "ReactToPlayerState":
                            reactToPlayerState = bool.Parse(value);
                            break;
                        case "ReactToCharacterSwitch":
                            reactToCharacterSwitch = bool.Parse(value);
                            break;
                        case "ReactToVehicleValue":
                            reactToVehicleValue = bool.Parse(value);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                GTA.UI.Notification.Show($"~r~MSAgent Config Error: {ex.Message}");
            }
        }
        
        private void InitializeVehicleValues()
        {
            // Approximate vehicle values in GTA$ (these are rough estimates)
            // Super cars
            vehicleValues[VehicleHash.Adder] = 1000000;
            vehicleValues[VehicleHash.Zentorno] = 725000;
            vehicleValues[VehicleHash.Osiris] = 1950000;
            vehicleValues[VehicleHash.T20] = 2200000;
            vehicleValues[VehicleHash.Turismor] = 500000;
            vehicleValues[VehicleHash.EntityXF] = 795000;
            vehicleValues[VehicleHash.Infernus] = 440000;
            vehicleValues[VehicleHash.Vacca] = 240000;
            vehicleValues[VehicleHash.Bullet] = 155000;
            vehicleValues[VehicleHash.Cheetah] = 650000;
            vehicleValues[VehicleHash.Voltic] = 150000;
            vehicleValues[VehicleHash.Banshee] = 105000;
            
            // Sports cars
            vehicleValues[VehicleHash.Carbonizzare] = 195000;
            vehicleValues[VehicleHash.Coquette] = 138000;
            vehicleValues[VehicleHash.Elegy2] = 0; // Free
            vehicleValues[VehicleHash.Feltzer2] = 145000;
            vehicleValues[VehicleHash.Ninef] = 130000;
            
            // Motorcycles
            vehicleValues[VehicleHash.Akuma] = 9000;
            vehicleValues[VehicleHash.Bati] = 15000;
            vehicleValues[VehicleHash.Hakuchou] = 82000;
            vehicleValues[VehicleHash.PCJ] = 9000;
            
            // Helicopters
            vehicleValues[VehicleHash.Buzzard2] = 1750000;
            vehicleValues[VehicleHash.Frogger] = 1300000;
            vehicleValues[VehicleHash.Maverick] = 780000;
            
            // Planes
            vehicleValues[VehicleHash.Luxor] = 1500000;
            vehicleValues[VehicleHash.Shamal] = 1150000;
            vehicleValues[VehicleHash.Velum] = 450000;
            
            // Boats
            vehicleValues[VehicleHash.Jetmax] = 299000;
            vehicleValues[VehicleHash.Marquis] = 413000;
            vehicleValues[VehicleHash.Seashark] = 16000;
        }
        
        private void SetupMenu()
        {
            menuPool = new MenuPool();
            mainMenu = new UIMenu("MSAgent-AI", "~b~GTA V Integration Settings");
            menuPool.Add(mainMenu);
            
            // Add toggle items
            var vehicleToggle = new UIMenuCheckboxItem("React to Vehicles", reactToVehicles, 
                "Enable/disable reactions when entering vehicles");
            mainMenu.AddItem(vehicleToggle);
            
            var missionToggle = new UIMenuCheckboxItem("React to Missions", reactToMissions,
                "Enable/disable reactions to mission events");
            mainMenu.AddItem(missionToggle);
            
            var weatherToggle = new UIMenuCheckboxItem("React to Weather", reactToWeather,
                "Enable/disable reactions to weather changes");
            mainMenu.AddItem(weatherToggle);
            
            var timeToggle = new UIMenuCheckboxItem("React to Time", reactToTime,
                "Enable/disable reactions to time of day");
            mainMenu.AddItem(timeToggle);
            
            var locationToggle = new UIMenuCheckboxItem("React to Location", reactToLocation,
                "Enable/disable reactions when entering new areas");
            mainMenu.AddItem(locationToggle);
            
            var playerStateToggle = new UIMenuCheckboxItem("React to Player State", reactToPlayerState,
                "Enable/disable reactions to health, wanted level, etc.");
            mainMenu.AddItem(playerStateToggle);
            
            var characterToggle = new UIMenuCheckboxItem("React to Character Switch", reactToCharacterSwitch,
                "Enable/disable reactions when switching characters");
            mainMenu.AddItem(characterToggle);
            
            var vehicleValueToggle = new UIMenuCheckboxItem("React to Vehicle Value", reactToVehicleValue,
                "Enable/disable reactions based on vehicle worth");
            mainMenu.AddItem(vehicleValueToggle);
            
            // Handle checkbox changes
            mainMenu.OnCheckboxChange += (sender, item, checked_) =>
            {
                if (item == vehicleToggle) reactToVehicles = checked_;
                else if (item == missionToggle) reactToMissions = checked_;
                else if (item == weatherToggle) reactToWeather = checked_;
                else if (item == timeToggle) reactToTime = checked_;
                else if (item == locationToggle) reactToLocation = checked_;
                else if (item == playerStateToggle) reactToPlayerState = checked_;
                else if (item == characterToggle) reactToCharacterSwitch = checked_;
                else if (item == vehicleValueToggle) reactToVehicleValue = checked_;
                
                Notification.Show($"~g~MSAgent: {item.Text} " + (checked_ ? "enabled" : "disabled"));
            };
            
            mainMenu.RefreshIndex();
        }
        
        private void OnTick(object sender, EventArgs e)
        {
            // Process menu
            menuPool.ProcessMenus();
            
            // Toggle menu with F9 key (can be changed)
            if (Game.IsKeyPressed(System.Windows.Forms.Keys.F9))
            {
                if (mainMenu.Visible)
                    mainMenu.Visible = false;
                else
                    mainMenu.Visible = true;
                
                Wait(200); // Debounce
            }
            
            Ped player = Game.Player.Character;
            if (player == null || !player.IsAlive)
            {
                if (!playerWasDead && reactToPlayerState)
                {
                    playerWasDead = true;
                    SendChatPrompt("The player just died! React to their death with sympathy or humor.");
                }
                return;
            }
            
            if (playerWasDead)
            {
                playerWasDead = false;
                if (reactToPlayerState)
                {
                    SendChatPrompt("The player respawned. Welcome them back.");
                }
            }
            
            // Check vehicle changes
            CheckVehicleChange(player);
            
            // Check weather changes
            CheckWeatherChange();
            
            // Check time changes
            CheckTimeChange();
            
            // Check location changes
            CheckLocationChange(player);
            
            // Check wanted level changes
            CheckWantedLevel(player);
            
            // Check character switch
            CheckCharacterSwitch(player);
            
            // Check mission state
            CheckMissionState();
        }
        
        private void CheckVehicleChange(Ped player)
        {
            if (!reactToVehicles) return;
            
            Vehicle currentVehicle = player.CurrentVehicle;
            
            if (currentVehicle != null && currentVehicle != lastVehicle)
            {
                if (CanReact(ref lastVehicleReactionTime, FAST_COOLDOWN_MS))
                {
                    lastVehicle = currentVehicle;
                    
                    string vehicleType = GetVehicleTypeString(currentVehicle);
                    string vehicleName = ((VehicleHash)currentVehicle.Model.Hash).ToString();
                    string valueInfo = "";
                    
                    if (reactToVehicleValue && vehicleValues.ContainsKey((VehicleHash)currentVehicle.Model.Hash))
                    {
                        int value = vehicleValues[(VehicleHash)currentVehicle.Model.Hash];
                        valueInfo = $" worth ${value:N0}";
                    }
                    
                    SendChatPrompt($"The player just entered a {vehicleType} called {vehicleName}{valueInfo}. React to this vehicle!");
                }
            }
            else if (currentVehicle == null && lastVehicle != null)
            {
                if (CanReact(ref lastVehicleReactionTime, FAST_COOLDOWN_MS))
                {
                    lastVehicle = null;
                    SendChatPrompt("The player just exited their vehicle. Comment on it.");
                }
            }
        }
        
        private void CheckWeatherChange()
        {
            if (!reactToWeather) return;
            
            Weather currentWeather = World.Weather;
            if (currentWeather != lastWeather)
            {
                if (CanReact(ref lastWeatherReactionTime, COOLDOWN_MS))
                {
                    lastWeather = currentWeather;
                    string weatherName = currentWeather.ToString();
                    SendChatPrompt($"The weather changed to {weatherName}. Comment on the weather!");
                }
            }
        }
        
        private void CheckTimeChange()
        {
            if (!reactToTime) return;
            
            int currentHour = World.CurrentDate.Hour;
            
            // React to major time transitions
            if (lastHour != -1 && lastHour != currentHour)
            {
                bool shouldReact = false;
                string timeDescription = "";
                
                if (currentHour == 0 && lastHour == 23)
                {
                    shouldReact = true;
                    timeDescription = "midnight";
                }
                else if (currentHour == 6 && lastHour == 5)
                {
                    shouldReact = true;
                    timeDescription = "sunrise/morning";
                }
                else if (currentHour == 12 && lastHour == 11)
                {
                    shouldReact = true;
                    timeDescription = "noon";
                }
                else if (currentHour == 18 && lastHour == 17)
                {
                    shouldReact = true;
                    timeDescription = "sunset/evening";
                }
                
                if (shouldReact && CanReact(ref lastReactionTime, COOLDOWN_MS))
                {
                    SendChatPrompt($"It's now {timeDescription} in the game. Comment on the time!");
                }
            }
            
            lastHour = currentHour;
        }
        
        private void CheckLocationChange(Ped player)
        {
            if (!reactToLocation) return;
            
            string currentZone = World.GetZoneLocalizedName(player.Position);
            
            if (!string.IsNullOrEmpty(currentZone) && currentZone != lastZone)
            {
                if (CanReact(ref lastLocationReactionTime, COOLDOWN_MS))
                {
                    lastZone = currentZone;
                    SendChatPrompt($"The player entered {currentZone}. Comment on this location!");
                }
            }
        }
        
        private void CheckWantedLevel(Ped player)
        {
            if (!reactToPlayerState) return;
            
            int currentWantedLevel = Game.Player.WantedLevel;
            
            if (currentWantedLevel != lastWantedLevel)
            {
                if (CanReact(ref lastReactionTime, FAST_COOLDOWN_MS))
                {
                    lastWantedLevel = currentWantedLevel;
                    
                    if (currentWantedLevel > 0)
                    {
                        SendChatPrompt($"The player now has {currentWantedLevel} wanted stars! React to the police chase!");
                    }
                    else
                    {
                        SendChatPrompt("The player escaped the police! Congratulate them!");
                    }
                }
            }
        }
        
        private void CheckCharacterSwitch(Ped player)
        {
            if (!reactToCharacterSwitch) return;
            
            Model currentModel = player.Model;
            
            if (lastCharacterModel.Hash != 0 && currentModel.Hash != lastCharacterModel.Hash)
            {
                if (CanReact(ref lastReactionTime, COOLDOWN_MS))
                {
                    string characterName = GetCharacterName(currentModel);
                    SendChatPrompt($"The player switched to {characterName}. React to this character!");
                }
            }
            
            lastCharacterModel = currentModel;
        }
        
        private void CheckMissionState()
        {
            if (!reactToMissions) return;
            
            // Check if player is in a mission
            // Note: Mission detection in GTA V is complex and limited by ScriptHookV API
            // This is a simplified approach that detects when mission flag changes
            bool isInMission = Function.Call<bool>(Hash.GET_MISSION_FLAG);
            
            // Track mission state changes
            if (isInMission && lastMissionHash == (Hash)0)
            {
                if (CanReact(ref lastReactionTime, COOLDOWN_MS))
                {
                    lastMissionHash = (Hash)1; // Simple flag to indicate mission is active
                    SendChatPrompt("A mission started or progressed. Comment on the mission action!");
                }
            }
            else if (!isInMission && lastMissionHash != (Hash)0)
            {
                lastMissionHash = (Hash)0; // Mission ended
                // Note: We don't react to mission end to avoid spam
            }
        }
        
        private string GetVehicleTypeString(Vehicle vehicle)
        {
            if (vehicle.Model.IsHelicopter) return "helicopter";
            if (vehicle.Model.IsPlane) return "plane";
            if (vehicle.Model.IsBoat) return "boat";
            if (vehicle.Model.IsBike || vehicle.Model.IsBicycle) return "motorcycle";
            if (vehicle.Model.IsCar) return "car";
            return "vehicle";
        }
        
        private string GetCharacterName(Model model)
        {
            // GTA V protagonists (using cached hashes for performance)
            if (model.Hash == michaelHash) return "Michael De Santa";
            if (model.Hash == franklinHash) return "Franklin Clinton";
            if (model.Hash == trevorHash) return "Trevor Philips";
            
            return "a different character";
        }
        
        private bool CanReact(ref DateTime lastTime, int cooldownMs)
        {
            TimeSpan elapsed = DateTime.Now - lastTime;
            if (elapsed.TotalMilliseconds >= cooldownMs)
            {
                lastTime = DateTime.Now;
                return true;
            }
            return false;
        }
        
        private void SendToAgent(string command)
        {
            Task.Run(() =>
            {
                try
                {
                    if (protocol.Equals("TCP", StringComparison.OrdinalIgnoreCase))
                    {
                        // TCP Socket connection
                        using (var client = new TcpClient())
                        {
                            client.ConnectAsync(ipAddress, port).Wait(1000); // 1 second timeout
                            
                            if (!client.Connected)
                                return; // Silently fail if not connected
                            
                            using (var stream = client.GetStream())
                            using (var reader = new StreamReader(stream, Encoding.UTF8))
                            using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                            {
                                writer.WriteLine(command);
                                string response = reader.ReadLine();
                                
                                if (response != null && response.StartsWith("ERROR"))
                                {
                                    GTA.UI.Notification.Show($"~r~MSAgent Error: {response}");
                                }
                            }
                        }
                    }
                    else
                    {
                        // Named Pipe connection
                        using (var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut))
                        {
                            client.Connect(1000); // 1 second timeout
                            
                            using (var reader = new StreamReader(client))
                            using (var writer = new StreamWriter(client) { AutoFlush = true })
                            {
                                writer.WriteLine(command);
                                string response = reader.ReadLine();
                                
                                if (response != null && response.StartsWith("ERROR"))
                                {
                                    GTA.UI.Notification.Show($"~r~MSAgent Error: {response}");
                                }
                            }
                        }
                    }
                }
                catch (TimeoutException)
                {
                    // MSAgent not running, silently ignore
                }
                catch (Exception ex)
                {
                    // Only show error on first connection attempt, then silently fail
                    // This prevents spam if MSAgent-AI isn't running
                    GTA.UI.Notification.Show($"~r~MSAgent connection error: {ex.Message}");
                }
            });
        }
        
        private void SendChatPrompt(string prompt)
        {
            SendToAgent($"CHAT:{prompt}");
        }
        
        private void OnAborted(object sender, EventArgs e)
        {
            SendToAgent("SPEAK:GTA V integration stopped. See you later!");
        }
    }
}
