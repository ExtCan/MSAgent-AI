/*
	MSAgent-AI GTA V Integration Script
	
	This ScriptHook V script integrates GTA V with MSAgent-AI, allowing the MSAgent character
	to react to in-game events in real-time.
	
	NOTE: This is a demonstration/example implementation. Some native function calls are
	simplified for clarity. For production use, consider:
	- Using UI::_GET_LABEL_TEXT() for proper localized vehicle/zone names
	- Using PLAYER::GET_PLAYER_CHARACTER() for accurate character detection
	- Implementing proper weather hash-to-name conversion
	- Adding error handling and edge case management
	
	Features:
	- Vehicle reactions (entering, exiting, type, value)
	- Mission reactions (start, end, objectives)
	- Character reactions (switch, health, death)
	- Environment reactions (weather, time, area)
	- In-game menu for toggling reaction categories
	
	Installation:
	1. Install ScriptHook V: http://www.dev-c.com/gtav/scripthookv/
	2. Place the compiled .asi file in your GTA V directory
	3. Make sure MSAgent-AI is running
	
	Keybinding: F9 to open the menu
*/

#include <windows.h>
#include <string>
#include <map>
#include <chrono>
#include <sstream>
#include <iomanip>
#include "inc/main.h"
#include "inc/natives.h"
#include "inc/types.h"
#include "inc/enums.h"
#include "keyboard.h"

// Named Pipe Communication
const std::string PIPE_NAME = "\\\\.\\pipe\\MSAgentAI";

// Settings for toggling different reaction types
struct Settings {
	bool vehicleReactions = true;
	bool missionReactions = true;
	bool environmentReactions = true;
	bool characterReactions = true;
	bool generalReactions = true;
	bool enableCommentary = true;
	int menuKey = VK_F9;
};

Settings g_Settings;

// State tracking to avoid duplicate messages
struct GameState {
	int lastVehicle = 0;
	Hash lastVehicleModel = 0;
	int lastWeather = -1;
	int lastHour = -1;
	int lastCharacter = -1;
	bool inMission = false;
	std::string lastZone;
	int lastWantedLevel = 0;
	bool wasInVehicle = false;
	float lastHealth = 0.0f;
	std::chrono::steady_clock::time_point lastCommentTime;
};

GameState g_State;

// Menu state
bool g_MenuOpen = false;
int g_MenuSelection = 0;
const int MENU_ITEMS = 6;

// Forward declarations
void SendToMSAgent(const std::string& command);
void SendSpeakCommand(const std::string& text);
void SendChatCommand(const std::string& prompt);
std::string GetVehicleClassName(int vehicleClass);
std::string GetVehicleName(Hash model);
std::string GetWeatherName(int weather);
std::string GetZoneName(const std::string& zone);
int GetVehicleValue(Hash model, int vehicleClass);
void CheckVehicleChanges();
void CheckEnvironmentChanges();
void CheckCharacterChanges();
void CheckMissionChanges();
void CheckGeneralEvents();
void DrawMenu();
void UpdateMenu();

// Named Pipe Communication
void SendToMSAgent(const std::string& command) {
	HANDLE hPipe = CreateFileA(
		PIPE_NAME.c_str(),
		GENERIC_READ | GENERIC_WRITE,
		0,
		NULL,
		OPEN_EXISTING,
		0,
		NULL
	);

	if (hPipe == INVALID_HANDLE_VALUE) {
		// MSAgent-AI not running or pipe not available
		return;
	}

	DWORD mode = PIPE_READMODE_MESSAGE;
	SetNamedPipeHandleState(hPipe, &mode, NULL, NULL);

	std::string message = command + "\n";
	DWORD bytesWritten;
	WriteFile(hPipe, message.c_str(), (DWORD)message.length(), &bytesWritten, NULL);

	// Read response
	char buffer[1024];
	DWORD bytesRead;
	ReadFile(hPipe, buffer, sizeof(buffer) - 1, &bytesRead, NULL);
	buffer[bytesRead] = '\0';

	CloseHandle(hPipe);
}

void SendSpeakCommand(const std::string& text) {
	SendToMSAgent("SPEAK:" + text);
}

void SendChatCommand(const std::string& prompt) {
	SendToMSAgent("CHAT:" + prompt);
}

// Vehicle utilities
std::string GetVehicleClassName(int vehicleClass) {
	static const std::map<int, std::string> classNames = {
		{0, "Compacts"}, {1, "Sedans"}, {2, "SUVs"}, {3, "Coupes"},
		{4, "Muscle"}, {5, "Sports Classics"}, {6, "Sports"},
		{7, "Super"}, {8, "Motorcycles"}, {9, "Off-road"},
		{10, "Industrial"}, {11, "Utility"}, {12, "Vans"},
		{13, "Cycles"}, {14, "Boats"}, {15, "Helicopters"},
		{16, "Planes"}, {17, "Service"}, {18, "Emergency"},
		{19, "Military"}, {20, "Commercial"}, {21, "Trains"}
	};
	
	auto it = classNames.find(vehicleClass);
	return it != classNames.end() ? it->second : "Unknown";
}

std::string GetVehicleName(Hash model) {
	// NOTE: In actual implementation, use UI::_GET_LABEL_TEXT() to convert
	// the display name key to a user-friendly name
	// Example: UI::_GET_LABEL_TEXT(VEHICLE::GET_DISPLAY_NAME_FROM_VEHICLE_MODEL(model))
	const char* displayName = VEHICLE::GET_DISPLAY_NAME_FROM_VEHICLE_MODEL(model);
	return displayName ? std::string(displayName) : "Unknown Vehicle";
}

std::string GetWeatherName(int weather) {
	static const std::map<int, std::string> weatherNames = {
		{0, "Extra Sunny"}, {1, "Clear"}, {2, "Clouds"},
		{3, "Smog"}, {4, "Foggy"}, {5, "Overcast"},
		{6, "Raining"}, {7, "Thunderstorm"}, {8, "Light Rain"},
		{9, "Smoggy"}, {10, "Snowing"}, {11, "Blizzard"},
		{12, "Light Snow"}, {13, "Christmas"}
	};
	
	auto it = weatherNames.find(weather);
	return it != weatherNames.end() ? it->second : "Unknown";
}

std::string GetZoneName(const std::string& zone) {
	// Basic zone name mapping - can be expanded
	static const std::map<std::string, std::string> zoneNames = {
		{"AIRP", "Los Santos Airport"},
		{"ALAMO", "Alamo Sea"},
		{"ALTA", "Alta"},
		{"ARMYB", "Fort Zancudo"},
		{"BEACH", "Vespucci Beach"},
		{"BHAMCA", "Banham Canyon"},
		{"BRADP", "Braddock Pass"},
		{"BRADT", "Braddock Tunnel"},
		{"BURTON", "Burton"},
		{"CALAFB", "Calafia Bridge"},
		{"CANNY", "Raton Canyon"},
		{"CCREAK", "Cassidy Creek"},
		{"CHAMH", "Chamberlain Hills"},
		{"CHIL", "Vinewood Hills"},
		{"CHU", "Chumash"},
		{"CMSW", "Chiliad Mountain State Wilderness"},
		{"CYPRE", "Cypress Flats"},
		{"DAVIS", "Davis"},
		{"DELBE", "Del Perro Beach"},
		{"DELPE", "Del Perro"},
		{"DELSOL", "La Puerta"},
		{"DESRT", "Grand Senora Desert"},
		{"DOWNT", "Downtown"},
		{"DTVINE", "Downtown Vinewood"},
		{"EAST_V", "East Vinewood"},
		{"EBURO", "El Burro Heights"},
		{"ELGORL", "El Gordo Lighthouse"},
		{"ELYSIAN", "Elysian Island"},
		{"GALFISH", "Galilee"},
		{"GOLF", "GWC and Golfing Society"},
		{"GRAPES", "Grapeseed"},
		{"GREATC", "Great Chaparral"},
		{"HARMO", "Harmony"},
		{"HAWICK", "Hawick"},
		{"HORS", "Vinewood Racetrack"},
		{"HUMLAB", "Humane Labs and Research"},
		{"JAIL", "Bolingbroke Penitentiary"},
		{"KOREAT", "Little Seoul"},
		{"LACT", "Land Act Reservoir"},
		{"LAGO", "Lago Zancudo"},
		{"LDAM", "Land Act Dam"},
		{"LEGSQU", "Legion Square"},
		{"LMESA", "La Mesa"},
		{"LOSPUER", "La Puerta"},
		{"MIRR", "Mirror Park"},
		{"MORN", "Morningwood"},
		{"MOVIE", "Richards Majestic"},
		{"MTCHIL", "Mount Chiliad"},
		{"MTGORDO", "Mount Gordo"},
		{"MTJOSE", "Mount Josiah"},
		{"MURRI", "Murrieta Heights"},
		{"NCHU", "North Chumash"},
		{"NOOSE", "N.O.O.S.E"},
		{"OCEANA", "Pacific Ocean"},
		{"PALCOV", "Paleto Cove"},
		{"PALETO", "Paleto Bay"},
		{"PALFOR", "Paleto Forest"},
		{"PALHIGH", "Palomino Highlands"},
		{"PALMPOW", "Palmer-Taylor Power Station"},
		{"PBLUFF", "Pacific Bluffs"},
		{"PBOX", "Pillbox Hill"},
		{"PROCOB", "Procopio Beach"},
		{"RANCHO", "Rancho"},
		{"RGLEN", "Richman Glen"},
		{"RICHM", "Richman"},
		{"ROCKF", "Rockford Hills"},
		{"RTRAK", "Redwood Lights Track"},
		{"SANAND", "San Andreas"},
		{"SANCHIA", "San Chianski Mountain Range"},
		{"SANDY", "Sandy Shores"},
		{"SKID", "Mission Row"},
		{"SLAB", "Stab City"},
		{"STAD", "Maze Bank Arena"},
		{"STRAW", "Strawberry"},
		{"TATAMO", "Tataviam Mountains"},
		{"TERMINA", "Terminal"},
		{"TEXTI", "Textile City"},
		{"TONGVAH", "Tongva Hills"},
		{"TONGVAV", "Tongva Valley"},
		{"VCANA", "Vespucci Canals"},
		{"VESP", "Vespucci"},
		{"VINE", "Vinewood"},
		{"WINDF", "Ron Alternates Wind Farm"},
		{"WVINE", "West Vinewood"},
		{"ZANCUDO", "Zancudo River"},
		{"ZP_ORT", "Port of South Los Santos"},
		{"ZQ_UAR", "Davis Quartz"}
	};
	
	auto it = zoneNames.find(zone);
	return it != zoneNames.end() ? it->second : zone;
}

int GetVehicleValue(Hash model, int vehicleClass) {
	// Estimate vehicle value based on class (simplified)
	static const std::map<int, int> classValues = {
		{0, 15000},   // Compacts
		{1, 25000},   // Sedans
		{2, 35000},   // SUVs
		{3, 45000},   // Coupes
		{4, 50000},   // Muscle
		{5, 100000},  // Sports Classics
		{6, 150000},  // Sports
		{7, 500000},  // Super
		{8, 20000},   // Motorcycles
		{9, 30000},   // Off-road
		{10, 25000},  // Industrial
		{11, 20000},  // Utility
		{12, 18000},  // Vans
		{13, 500},    // Cycles
		{14, 75000},  // Boats
		{15, 250000}, // Helicopters
		{16, 500000}, // Planes
		{17, 15000},  // Service
		{18, 35000},  // Emergency
		{19, 150000}, // Military
		{20, 40000},  // Commercial
		{21, 100000}  // Trains
	};
	
	auto it = classValues.find(vehicleClass);
	return it != classValues.end() ? it->second : 25000;
}

// Game state monitoring
void CheckVehicleChanges() {
	if (!g_Settings.vehicleReactions) return;

	Player player = PLAYER::PLAYER_ID();
	Ped playerPed = PLAYER::PLAYER_PED_ID();
	
	bool inVehicle = PED::IS_PED_IN_ANY_VEHICLE(playerPed, false);
	
	if (inVehicle && !g_State.wasInVehicle) {
		// Just entered a vehicle
		Vehicle vehicle = PED::GET_VEHICLE_PED_IS_IN(playerPed, false);
		Hash model = ENTITY::GET_ENTITY_MODEL(vehicle);
		int vehicleClass = VEHICLE::GET_VEHICLE_CLASS(vehicle);
		
		std::string vehicleName = GetVehicleName(model);
		std::string className = GetVehicleClassName(vehicleClass);
		int value = GetVehicleValue(model, vehicleClass);
		
		std::ostringstream prompt;
		prompt << "I just got into a " << vehicleName << " (" << className << "). ";
		prompt << "It's worth about $" << value << ". React to this!";
		
		SendChatCommand(prompt.str());
		
		g_State.lastVehicle = vehicle;
		g_State.lastVehicleModel = model;
	}
	else if (!inVehicle && g_State.wasInVehicle) {
		// Just exited a vehicle
		if (g_State.lastVehicleModel != 0) {
			std::string vehicleName = GetVehicleName(g_State.lastVehicleModel);
			SendChatCommand("I just got out of the " + vehicleName + ". Say something about it.");
		}
		g_State.lastVehicle = 0;
		g_State.lastVehicleModel = 0;
	}
	
	g_State.wasInVehicle = inVehicle;
}

void CheckEnvironmentChanges() {
	if (!g_Settings.environmentReactions) return;

	// Check weather changes
	// NOTE: This is simplified - actual implementation should use proper weather detection
	// and handle hash-to-index conversion correctly
	int currentWeather = GAMEPLAY::GET_PREV_WEATHER_TYPE_HASH_NAME();
	if (currentWeather != g_State.lastWeather && g_State.lastWeather != -1) {
		std::string weatherName = GetWeatherName(currentWeather);
		SendChatCommand("The weather just changed to " + weatherName + ". Comment on it!");
		g_State.lastWeather = currentWeather;
	}
	else if (g_State.lastWeather == -1) {
		g_State.lastWeather = currentWeather;
	}
	
	// Check time changes (hourly)
	int hour, minute;
	TIME::GET_TIME_OF_DAY(&hour, &minute);
	
	if (hour != g_State.lastHour && g_State.lastHour != -1) {
		std::ostringstream prompt;
		prompt << "It's now " << hour << ":00 in the game. ";
		if (hour >= 6 && hour < 12) {
			prompt << "It's morning. ";
		} else if (hour >= 12 && hour < 18) {
			prompt << "It's afternoon. ";
		} else if (hour >= 18 && hour < 22) {
			prompt << "It's evening. ";
		} else {
			prompt << "It's night time. ";
		}
		prompt << "Say something about the time of day.";
		
		SendChatCommand(prompt.str());
		g_State.lastHour = hour;
	}
	else if (g_State.lastHour == -1) {
		g_State.lastHour = hour;
	}
	
	// Check zone changes
	Player player = PLAYER::PLAYER_ID();
	Ped playerPed = PLAYER::PLAYER_PED_ID();
	Vector3 coords = ENTITY::GET_ENTITY_COORDS(playerPed, true);
	
	// NOTE: ZONE::GET_NAME_OF_ZONE returns internal codes like "AIRP", "DOWNT"
	// GetZoneName() function below maps these to friendly names
	// Alternatively, use UI::_GET_LABEL_TEXT() for proper localized names
	const char* zoneName = ZONE::GET_NAME_OF_ZONE(coords.x, coords.y, coords.z);
	std::string currentZone = zoneName ? std::string(zoneName) : "";
	
	if (!currentZone.empty() && currentZone != g_State.lastZone && !g_State.lastZone.empty()) {
		std::string friendlyName = GetZoneName(currentZone);
		SendChatCommand("I'm now in " + friendlyName + ". Tell me something about this area!");
		g_State.lastZone = currentZone;
	}
	else if (g_State.lastZone.empty()) {
		g_State.lastZone = currentZone;
	}
}

void CheckCharacterChanges() {
	if (!g_Settings.characterReactions) return;

	Player player = PLAYER::PLAYER_ID();
	Ped playerPed = PLAYER::PLAYER_PED_ID();
	
	// Check character switch
	// NOTE: GET_PLAYER_SWITCH_TYPE returns animation type, not character
	// For accurate character detection, use PLAYER::GET_PLAYER_CHARACTER() or track
	// the actual Ped model hash to detect Michael/Franklin/Trevor
	int currentChar = PLAYER::GET_PLAYER_SWITCH_TYPE();
	if (currentChar != g_State.lastCharacter && g_State.lastCharacter != -1) {
		SendChatCommand("The player just switched to a different character. React to the character switch!");
		g_State.lastCharacter = currentChar;
	}
	else if (g_State.lastCharacter == -1) {
		g_State.lastCharacter = currentChar;
	}
	
	// Check health status
	float health = ENTITY::GET_ENTITY_HEALTH(playerPed);
	float maxHealth = ENTITY::GET_ENTITY_MAX_HEALTH(playerPed);
	float healthPercent = (health / maxHealth) * 100.0f;
	
	// Check for low health (not already low)
	if (healthPercent < 30.0f && g_State.lastHealth >= 30.0f) {
		SendChatCommand("The player's health is really low! Say something concerned!");
	}
	
	g_State.lastHealth = healthPercent;
}

void CheckMissionChanges() {
	if (!g_Settings.missionReactions) return;

	// Check if in mission
	bool currentlyInMission = GAMEPLAY::GET_MISSION_FLAG();
	
	if (currentlyInMission && !g_State.inMission) {
		SendChatCommand("A mission just started! Get excited!");
		g_State.inMission = true;
	}
	else if (!currentlyInMission && g_State.inMission) {
		SendChatCommand("The mission ended. Comment on how it went!");
		g_State.inMission = false;
	}
}

void CheckGeneralEvents() {
	if (!g_Settings.generalReactions) return;

	Player player = PLAYER::PLAYER_ID();
	Ped playerPed = PLAYER::PLAYER_PED_ID();
	
	// Check wanted level changes
	int wantedLevel = PLAYER::GET_PLAYER_WANTED_LEVEL(player);
	if (wantedLevel != g_State.lastWantedLevel) {
		if (wantedLevel > g_State.lastWantedLevel) {
			std::ostringstream prompt;
			prompt << "The player's wanted level just increased to " << wantedLevel << " stars! React to the police chase!";
			SendChatCommand(prompt.str());
		}
		else if (wantedLevel == 0 && g_State.lastWantedLevel > 0) {
			SendChatCommand("The wanted level is gone! The player escaped the cops!");
		}
		g_State.lastWantedLevel = wantedLevel;
	}
	
	// Periodic commentary (every 5 minutes)
	if (g_Settings.enableCommentary) {
		auto now = std::chrono::steady_clock::now();
		auto elapsed = std::chrono::duration_cast<std::chrono::minutes>(now - g_State.lastCommentTime);
		
		if (elapsed.count() >= 5) {
			SendChatCommand("Make a random observation or comment about what's happening in GTA V right now.");
			g_State.lastCommentTime = now;
		}
	}
}

// Menu System
void DrawMenu() {
	const float menuX = 0.1f;
	const float menuY = 0.2f;
	const float lineHeight = 0.035f;
	const float menuWidth = 0.25f;
	
	// Draw background
	GRAPHICS::DRAW_RECT(menuX + menuWidth / 2, menuY + lineHeight * 4, menuWidth, lineHeight * 9, 0, 0, 0, 200);
	
	// Draw title
	UI::SET_TEXT_FONT(1);
	UI::SET_TEXT_SCALE(0.5f, 0.5f);
	UI::SET_TEXT_COLOUR(255, 255, 255, 255);
	UI::SET_TEXT_CENTRE(false);
	UI::SET_TEXT_DROPSHADOW(2, 2, 0, 0, 0);
	UI::SET_TEXT_EDGE(1, 0, 0, 0, 205);
	UI::_SET_TEXT_ENTRY("STRING");
	UI::_ADD_TEXT_COMPONENT_STRING("MSAgent-AI Reactions");
	UI::_DRAW_TEXT(menuX, menuY);
	
	// Draw menu items
	const char* menuItems[] = {
		"Vehicle Reactions",
		"Mission Reactions",
		"Environment Reactions",
		"Character Reactions",
		"General Reactions",
		"Live Commentary"
	};
	
	bool* menuStates[] = {
		&g_Settings.vehicleReactions,
		&g_Settings.missionReactions,
		&g_Settings.environmentReactions,
		&g_Settings.characterReactions,
		&g_Settings.generalReactions,
		&g_Settings.enableCommentary
	};
	
	for (int i = 0; i < MENU_ITEMS; i++) {
		float itemY = menuY + lineHeight * (i + 2);
		
		// Highlight selected item
		if (i == g_MenuSelection) {
			GRAPHICS::DRAW_RECT(menuX + menuWidth / 2, itemY + lineHeight / 2, menuWidth - 0.01f, lineHeight, 255, 255, 255, 100);
		}
		
		// Draw item text
		UI::SET_TEXT_FONT(0);
		UI::SET_TEXT_SCALE(0.35f, 0.35f);
		UI::SET_TEXT_COLOUR(255, 255, 255, 255);
		UI::SET_TEXT_CENTRE(false);
		UI::SET_TEXT_DROPSHADOW(2, 2, 0, 0, 0);
		UI::SET_TEXT_EDGE(1, 0, 0, 0, 205);
		UI::_SET_TEXT_ENTRY("STRING");
		
		std::string itemText = std::string(menuItems[i]) + ": " + (*menuStates[i] ? "ON" : "OFF");
		UI::_ADD_TEXT_COMPONENT_STRING(itemText.c_str());
		UI::_DRAW_TEXT(menuX + 0.01f, itemY);
	}
	
	// Draw instructions
	UI::SET_TEXT_FONT(0);
	UI::SET_TEXT_SCALE(0.3f, 0.3f);
	UI::SET_TEXT_COLOUR(200, 200, 200, 255);
	UI::SET_TEXT_CENTRE(false);
	UI::SET_TEXT_DROPSHADOW(2, 2, 0, 0, 0);
	UI::SET_TEXT_EDGE(1, 0, 0, 0, 205);
	UI::_SET_TEXT_ENTRY("STRING");
	UI::_ADD_TEXT_COMPONENT_STRING("Arrow Keys: Navigate | Enter: Toggle | F9: Close");
	UI::_DRAW_TEXT(menuX, menuY + lineHeight * 8.5f);
}

void UpdateMenu() {
	// Check for menu key
	if (IsKeyJustUp(g_Settings.menuKey)) {
		g_MenuOpen = !g_MenuOpen;
		if (g_MenuOpen) {
			SendSpeakCommand("Opening MSAgent reactions menu!");
		}
	}
	
	if (!g_MenuOpen) return;
	
	// Navigation
	if (IsKeyJustUp(VK_UP)) {
		g_MenuSelection = (g_MenuSelection - 1 + MENU_ITEMS) % MENU_ITEMS;
	}
	if (IsKeyJustUp(VK_DOWN)) {
		g_MenuSelection = (g_MenuSelection + 1) % MENU_ITEMS;
	}
	
	// Toggle setting
	if (IsKeyJustUp(VK_RETURN)) {
		bool* menuStates[] = {
			&g_Settings.vehicleReactions,
			&g_Settings.missionReactions,
			&g_Settings.environmentReactions,
			&g_Settings.characterReactions,
			&g_Settings.generalReactions,
			&g_Settings.enableCommentary
		};
		
		*menuStates[g_MenuSelection] = !*menuStates[g_MenuSelection];
		
		std::string status = *menuStates[g_MenuSelection] ? "enabled" : "disabled";
		SendSpeakCommand("Setting " + status + "!");
	}
	
	DrawMenu();
}

// Main script loop
void ScriptMain() {
	// Initialize
	g_State.lastCommentTime = std::chrono::steady_clock::now();
	
	// Send initial connection message
	SendSpeakCommand("GTA 5 MSAgent integration is now active!");
	
	while (true) {
		// Update menu
		UpdateMenu();
		
		// Check game state changes (only when menu is closed to avoid spam)
		if (!g_MenuOpen) {
			CheckVehicleChanges();
			CheckEnvironmentChanges();
			CheckCharacterChanges();
			CheckMissionChanges();
			CheckGeneralEvents();
		}
		
		WAIT(0);
	}
}
