// MSAgent-AI GTA V Integration Script
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.UI;
using GTA.Native;

namespace MSAgentGTA
{
    /// <summary>
    /// MSAgent-AI GTA V Integration Script for ScriptHookDotNet
    /// 
    /// This script integrates GTA V with MSAgent-AI, allowing the MSAgent character
    /// to react to in-game events in real-time through AI-powered commentary.
    /// 
    /// Features:
    /// - Vehicle reactions (entering, exiting, type, value)
    /// - Mission reactions (start, end, objectives)
    /// - Character reactions (switch, health)
    /// - Environment reactions (weather, time, area)
    /// - In-game menu for toggling reaction categories
    /// 
    /// Installation:
    /// 1. Install ScriptHookDotNet: https://github.com/scripthookvdotnet/scripthookvdotnet
    /// 2. Place MSAgentGTA.dll in your GTA V/scripts folder
    /// 3. Make sure MSAgent-AI is running
    /// 
    /// Keybinding: [ (left bracket) to open the menu
    /// </summary>
    public class MSAgentGTA : Script
    {
        // Named Pipe Communication
        private const string PIPE_NAME = "MSAgentAI";

        // Settings for toggling different reaction types
        private class Settings
        {
            public bool VehicleReactions = true;
            public bool MissionReactions = true;
            public bool EnvironmentReactions = true;
            public bool CharacterReactions = true;
            public bool GeneralReactions = true;
            public bool EnableCommentary = true;
        }

        private Settings _settings = new Settings();

        // State tracking to avoid duplicate messages
        private class GameState
        {
            public Vehicle LastVehicle = null;
            public int LastVehicleModel = 0;
            public Weather LastWeather = Weather.Unknown;
            public int LastHour = -1;
            public string LastZone = "";
            public int LastWantedLevel = 0;
            public bool WasInVehicle = false;
            public float LastHealth = 0.0f;
            public DateTime LastCommentTime = DateTime.Now;
        }

        private GameState _state = new GameState();

        // Menu state
        private bool _menuOpen = false;
        private int _menuSelection = 0;
        private const int MENU_ITEMS = 6;

        // Menu items
        private readonly string[] _menuItemNames = {
            "Vehicle Reactions",
            "Mission Reactions",
            "Environment Reactions",
            "Character Reactions",
            "General Reactions",
            "Live Commentary"
        };

        public MSAgentGTA()
        {
            // Initialize script
            Tick += OnTick;
            KeyDown += OnKeyDown;

            // Send initial connection message
            SendSpeakCommand("GTA 5 MSAgent integration is now active!");

            // Initialize last comment time
            _state.LastCommentTime = DateTime.Now;
        }

        private void OnTick(object sender, EventArgs e)
        {
            // Update menu if open
            if (_menuOpen)
            {
                DrawMenu();
            }
            else
            {
                // Check game state changes only when menu is closed
                CheckVehicleChanges();
                CheckEnvironmentChanges();
                CheckCharacterChanges();
                CheckGeneralEvents();
            }

            Wait(100); // Check every 100ms
        }

        private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            // Menu key: [ (left bracket) = OemOpenBrackets
            if (e.KeyCode == System.Windows.Forms.Keys.OemOpenBrackets)
            {
                _menuOpen = !_menuOpen;
                if (_menuOpen)
                {
                    SendSpeakCommand("Opening MSAgent reactions menu!");
                }
            }

            if (!_menuOpen) return;

            // Navigation
            if (e.KeyCode == System.Windows.Forms.Keys.Up)
            {
                _menuSelection = (_menuSelection - 1 + MENU_ITEMS) % MENU_ITEMS;
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.Down)
            {
                _menuSelection = (_menuSelection + 1) % MENU_ITEMS;
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                ToggleMenuSetting(_menuSelection);
                string status = GetMenuSetting(_menuSelection) ? "enabled" : "disabled";
                SendSpeakCommand($"Setting {status}!");
            }
        }

        private void DrawMenu()
        {
            const float menuX = 0.1f;
            const float menuY = 0.2f;
            const float lineHeight = 0.035f;
            const float menuWidth = 0.25f;

            // Draw background
            var bgPos = new PointF(menuX + menuWidth / 2, menuY + lineHeight * 4);
            var bgSize = new SizeF(menuWidth, lineHeight * 9);
            new UIRectangle(bgPos, bgSize, Color.FromArgb(200, 0, 0, 0)).Draw();

            // Draw title
            var title = new UIText("MSAgent-AI Reactions", new PointF(menuX, menuY), 0.5f, Color.White, GTA.UI.Font.ChaletLondon, Alignment.Left);
            title.Shadow = true;
            title.Draw();

            // Draw menu items
            for (int i = 0; i < MENU_ITEMS; i++)
            {
                float itemY = menuY + lineHeight * (i + 2);

                // Highlight selected item
                if (i == _menuSelection)
                {
                    var highlightPos = new PointF(menuX + menuWidth / 2, itemY + lineHeight / 2);
                    var highlightSize = new SizeF(menuWidth - 0.01f, lineHeight);
                    new UIRectangle(highlightPos, highlightSize, Color.FromArgb(100, 255, 255, 255)).Draw();
                }

                // Draw item text
                bool isOn = GetMenuSetting(i);
                string itemText = $"{_menuItemNames[i]}: {(isOn ? "ON" : "OFF")}";
                var item = new UIText(itemText, new PointF(menuX + 0.01f, itemY), 0.35f, Color.White, GTA.UI.Font.ChaletLondon, Alignment.Left);
                item.Shadow = true;
                item.Draw();
            }

            // Draw instructions
            var instructions = new UIText("Arrow Keys: Navigate | Enter: Toggle | [: Close", 
                new PointF(menuX, menuY + lineHeight * 8.5f), 0.3f, Color.FromArgb(255, 200, 200, 200), GTA.UI.Font.ChaletLondon, Alignment.Left);
            instructions.Shadow = true;
            instructions.Draw();
        }

        private bool GetMenuSetting(int index)
        {
            switch (index)
            {
                case 0: return _settings.VehicleReactions;
                case 1: return _settings.MissionReactions;
                case 2: return _settings.EnvironmentReactions;
                case 3: return _settings.CharacterReactions;
                case 4: return _settings.GeneralReactions;
                case 5: return _settings.EnableCommentary;
                default: return false;
            }
        }

        private void ToggleMenuSetting(int index)
        {
            switch (index)
            {
                case 0: _settings.VehicleReactions = !_settings.VehicleReactions; break;
                case 1: _settings.MissionReactions = !_settings.MissionReactions; break;
                case 2: _settings.EnvironmentReactions = !_settings.EnvironmentReactions; break;
                case 3: _settings.CharacterReactions = !_settings.CharacterReactions; break;
                case 4: _settings.GeneralReactions = !_settings.GeneralReactions; break;
                case 5: _settings.EnableCommentary = !_settings.EnableCommentary; break;
            }
        }

        #region Game State Monitoring

        private void CheckVehicleChanges()
        {
            if (!_settings.VehicleReactions) return;

            Ped player = Game.Player.Character;
            bool inVehicle = player.IsInVehicle();

            if (inVehicle && !_state.WasInVehicle)
            {
                // Just entered a vehicle
                Vehicle vehicle = player.CurrentVehicle;
                if (vehicle != null)
                {
                    string vehicleName = vehicle.LocalizedName;
                    string className = vehicle.ClassType.ToString();
                    int value = EstimateVehicleValue(vehicle.ClassType);

                    string prompt = $"I just got into a {vehicleName} ({className}). It's worth about ${value}. React to this!";
                    SendChatCommand(prompt);

                    _state.LastVehicle = vehicle;
                    _state.LastVehicleModel = vehicle.Model.Hash;
                }
            }
            else if (!inVehicle && _state.WasInVehicle)
            {
                // Just exited a vehicle
                if (_state.LastVehicle != null)
                {
                    string vehicleName = _state.LastVehicle.LocalizedName;
                    SendChatCommand($"I just got out of the {vehicleName}. Say something about it.");
                }
                _state.LastVehicle = null;
                _state.LastVehicleModel = 0;
            }

            _state.WasInVehicle = inVehicle;
        }

        private void CheckEnvironmentChanges()
        {
            if (!_settings.EnvironmentReactions) return;

            // Check weather changes
            Weather currentWeather = World.Weather;
            if (currentWeather != _state.LastWeather && _state.LastWeather != Weather.Unknown)
            {
                string weatherName = currentWeather.ToString();
                SendChatCommand($"The weather just changed to {weatherName}. Comment on it!");
            }
            _state.LastWeather = currentWeather;

            // Check time changes (hourly)
            int hour = Function.Call<int>(Hash.GET_CLOCK_HOURS);
            if (hour != _state.LastHour && _state.LastHour != -1)
            {
                string timeOfDay;
                if (hour >= 6 && hour < 12)
                    timeOfDay = "morning";
                else if (hour >= 12 && hour < 18)
                    timeOfDay = "afternoon";
                else if (hour >= 18 && hour < 22)
                    timeOfDay = "evening";
                else
                    timeOfDay = "night time";

                SendChatCommand($"It's now {hour}:00 in the game. It's {timeOfDay}. Say something about the time of day.");
            }
            _state.LastHour = hour;

            // Check zone changes
            string currentZone = World.GetZoneLocalizedName(Game.Player.Character.Position);
            if (!string.IsNullOrEmpty(currentZone) && currentZone != _state.LastZone && !string.IsNullOrEmpty(_state.LastZone))
            {
                SendChatCommand($"I'm now in {currentZone}. Tell me something about this area!");
            }
            _state.LastZone = currentZone;
        }

        private void CheckCharacterChanges()
        {
            if (!_settings.CharacterReactions) return;

            Ped player = Game.Player.Character;

            // Check health status
            float health = player.Health;
            float maxHealth = player.MaxHealth;
            float healthPercent = (health / maxHealth) * 100.0f;

            if (healthPercent < 30.0f && _state.LastHealth >= 30.0f)
            {
                SendChatCommand("The player's health is really low! Say something concerned!");
            }

            _state.LastHealth = healthPercent;
        }

        private void CheckGeneralEvents()
        {
            if (!_settings.GeneralReactions) return;

            // Check wanted level changes
            int wantedLevel = Game.Player.WantedLevel;
            if (wantedLevel != _state.LastWantedLevel)
            {
                if (wantedLevel > _state.LastWantedLevel)
                {
                    SendChatCommand($"The player's wanted level just increased to {wantedLevel} stars! React to the police chase!");
                }
                else if (wantedLevel == 0 && _state.LastWantedLevel > 0)
                {
                    SendChatCommand("The wanted level is gone! The player escaped the cops!");
                }
                _state.LastWantedLevel = wantedLevel;
            }

            // Periodic commentary (every 5 minutes)
            if (_settings.EnableCommentary)
            {
                TimeSpan elapsed = DateTime.Now - _state.LastCommentTime;
                if (elapsed.TotalMinutes >= 5)
                {
                    SendChatCommand("Make a random observation or comment about what's happening in GTA V right now.");
                    _state.LastCommentTime = DateTime.Now;
                }
            }
        }

        #endregion

        #region Utility Methods

        private int EstimateVehicleValue(VehicleClass vehicleClass)
        {
            Dictionary<VehicleClass, int> classValues = new Dictionary<VehicleClass, int>
            {
                { VehicleClass.Compacts, 15000 },
                { VehicleClass.Sedans, 25000 },
                { VehicleClass.SUVs, 35000 },
                { VehicleClass.Coupes, 45000 },
                { VehicleClass.Muscle, 50000 },
                { VehicleClass.SportsClassics, 100000 },
                { VehicleClass.Sports, 150000 },
                { VehicleClass.Super, 500000 },
                { VehicleClass.Motorcycles, 20000 },
                { VehicleClass.OffRoad, 30000 },
                { VehicleClass.Industrial, 25000 },
                { VehicleClass.Utility, 20000 },
                { VehicleClass.Vans, 18000 },
                { VehicleClass.Cycles, 500 },
                { VehicleClass.Boats, 75000 },
                { VehicleClass.Helicopters, 250000 },
                { VehicleClass.Planes, 500000 },
                { VehicleClass.Service, 15000 },
                { VehicleClass.Emergency, 35000 },
                { VehicleClass.Military, 150000 },
                { VehicleClass.Commercial, 40000 }
            };

            return classValues.ContainsKey(vehicleClass) ? classValues[vehicleClass] : 25000;
        }

        #endregion

        #region Named Pipe Communication

        private void SendToMSAgent(string command)
        {
            try
            {
                using (var client = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.InOut))
                {
                    client.Connect(2000); // 2 second timeout

                    using (var reader = new StreamReader(client))
                    using (var writer = new StreamWriter(client) { AutoFlush = true })
                    {
                        writer.WriteLine(command);
                        string response = reader.ReadLine();
                        // Optionally log response
                    }
                }
            }
            catch (Exception ex)
            {
                // MSAgent-AI not running or pipe not available
                // Silently ignore to avoid spam
            }
        }

        private void SendSpeakCommand(string text)
        {
            SendToMSAgent($"SPEAK:{text}");
        }

        private void SendChatCommand(string prompt)
        {
            SendToMSAgent($"CHAT:{prompt}");
        }

        #endregion
    }
}
