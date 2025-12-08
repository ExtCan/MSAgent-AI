/*
	THIS IS A PLACEHOLDER FILE
	
	The actual natives.h file from ScriptHook V SDK contains thousands of native function declarations.
	Download from: http://www.dev-c.com/gtav/scripthookv/
	
	This file provides placeholder declarations for the functions used in our script.
*/

#pragma once

#ifndef SCRIPTHOOK_NATIVES_PLACEHOLDER
#define SCRIPTHOOK_NATIVES_PLACEHOLDER

#include "types.h"

// Namespaces for game natives (placeholders - actual SDK has full implementations)

namespace PLAYER {
	Player PLAYER_ID();
	Ped PLAYER_PED_ID();
	int GET_PLAYER_WANTED_LEVEL(Player player);
	int GET_PLAYER_SWITCH_TYPE();
}

namespace PED {
	BOOL IS_PED_IN_ANY_VEHICLE(Ped ped, BOOL atGetIn);
	Vehicle GET_VEHICLE_PED_IS_IN(Ped ped, BOOL lastVehicle);
}

namespace VEHICLE {
	Hash GET_DISPLAY_NAME_FROM_VEHICLE_MODEL(Hash model);
	int GET_VEHICLE_CLASS(Vehicle vehicle);
}

namespace ENTITY {
	Hash GET_ENTITY_MODEL(Entity entity);
	Vector3 GET_ENTITY_COORDS(Entity entity, BOOL alive);
	float GET_ENTITY_HEALTH(Entity entity);
	float GET_ENTITY_MAX_HEALTH(Entity entity);
	BOOL IS_ENTITY_DEAD(Entity entity);
}

namespace GAMEPLAY {
	int GET_PREV_WEATHER_TYPE_HASH_NAME();
	BOOL GET_MISSION_FLAG();
}

namespace TIME {
	void GET_TIME_OF_DAY(int* hour, int* minute);
}

namespace ZONE {
	const char* GET_NAME_OF_ZONE(float x, float y, float z);
}

namespace UI {
	void SET_TEXT_FONT(int fontType);
	void SET_TEXT_SCALE(float scale, float size);
	void SET_TEXT_COLOUR(int red, int green, int blue, int alpha);
	void SET_TEXT_CENTRE(BOOL align);
	void SET_TEXT_DROPSHADOW(int distance, int r, int g, int b);
	void SET_TEXT_EDGE(int p1, int r, int g, int b, int a);
	void _SET_TEXT_ENTRY(const char* text);
	void _ADD_TEXT_COMPONENT_STRING(const char* text);
	void _DRAW_TEXT(float x, float y);
}

namespace GRAPHICS {
	void DRAW_RECT(float x, float y, float width, float height, int r, int g, int b, int a);
}

#endif // SCRIPTHOOK_NATIVES_PLACEHOLDER
