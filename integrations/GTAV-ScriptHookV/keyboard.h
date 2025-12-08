/*
	Keyboard input handling for ScriptHook V
	Based on ScriptHook V SDK sample
*/

#pragma once

#include <windows.h>

#define KEYS_SIZE 255

// Key states
static bool g_KeyStates[KEYS_SIZE];
static bool g_PrevKeyStates[KEYS_SIZE];

// Update key states
void OnKeyboardMessage(DWORD key, WORD repeats, BYTE scanCode, BOOL isExtended, BOOL isWithAlt, BOOL wasDownBefore, BOOL isUpNow) {
	if (key < KEYS_SIZE) {
		g_KeyStates[key] = !isUpNow;
	}
}

// Check if key is currently pressed
bool IsKeyDown(DWORD key) {
	return (key < KEYS_SIZE) ? g_KeyStates[key] : false;
}

// Check if key was just pressed (not held)
bool IsKeyJustUp(DWORD key) {
	if (key >= KEYS_SIZE) return false;
	
	bool result = g_PrevKeyStates[key] && !g_KeyStates[key];
	g_PrevKeyStates[key] = g_KeyStates[key];
	
	return result;
}

// Reset all key states
void ResetKeyStates() {
	for (int i = 0; i < KEYS_SIZE; i++) {
		g_KeyStates[i] = false;
		g_PrevKeyStates[i] = false;
	}
}
