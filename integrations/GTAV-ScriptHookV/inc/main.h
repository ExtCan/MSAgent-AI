/*
	THIS IS A PLACEHOLDER FILE
	
	The actual ScriptHook V SDK files should be downloaded from:
	http://www.dev-c.com/gtav/scripthookv/
	
	Required SDK files to place in this 'inc' folder:
	- main.h       (ScriptHook V main header)
	- natives.h    (Native function declarations)
	- types.h      (Type definitions)
	- enums.h      (Game enumerations)
	
	After downloading the SDK, extract and copy the SDK files here.
*/

#pragma once

// This is a placeholder - download the actual ScriptHook V SDK
// Note: The actual SDK contains thousands of native function declarations

#ifndef SCRIPTHOOK_SDK_PLACEHOLDER
#define SCRIPTHOOK_SDK_PLACEHOLDER

#include <windows.h>

// Placeholder types - actual SDK has more complete definitions
typedef DWORD Void;
typedef DWORD Any;
typedef DWORD uint;
typedef DWORD Hash;
typedef int Entity;
typedef int Player;
typedef int FireId;
typedef int Ped;
typedef int Vehicle;
typedef int Cam;
typedef int CarGenerator;
typedef int Group;
typedef int Train;
typedef int Pickup;
typedef int Object;
typedef int Weapon;
typedef int Interior;
typedef int Blip;
typedef int Texture;
typedef int TextureDict;
typedef int CoverPoint;
typedef int Camera;
typedef int TaskSequence;
typedef int ColourIndex;
typedef int Sphere;
typedef int ScrHandle;

struct Vector3 {
	float x;
	float y;
	float z;
};

// Main ScriptHook V function
void WAIT(DWORD ms);

#endif // SCRIPTHOOK_SDK_PLACEHOLDER
