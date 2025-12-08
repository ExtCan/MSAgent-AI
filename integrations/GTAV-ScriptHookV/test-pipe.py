#!/usr/bin/env python3
"""
Test Script for MSAgent-AI Named Pipe Communication
This script tests the communication between external apps and MSAgent-AI

Requirements: pywin32 (install with: pip install pywin32)
"""

import sys

try:
    import win32pipe
    import win32file
    import pywintypes
except ImportError:
    print("ERROR: pywin32 is not installed.")
    print("Install it with: pip install pywin32")
    sys.exit(1)

def send_msagent_command(command):
    """Send a command to MSAgent-AI via named pipe"""
    try:
        pipe = win32file.CreateFile(
            r'\\.\pipe\MSAgentAI',
            win32file.GENERIC_READ | win32file.GENERIC_WRITE,
            0,
            None,
            win32file.OPEN_EXISTING,
            0,
            None
        )
        
        # Send command
        message = (command + '\n').encode('utf-8')
        win32file.WriteFile(pipe, message)
        
        # Read response
        result, data = win32file.ReadFile(pipe, 1024)
        response = data.decode('utf-8').strip()
        
        win32file.CloseHandle(pipe)
        return response
    except pywintypes.error as e:
        print(f"ERROR: {e}")
        print("\nMake sure MSAgent-AI is running before running this test.")
        return None

def main():
    print("=" * 50)
    print("MSAgent-AI Named Pipe Test Script")
    print("=" * 50)
    print()
    
    # Test 1: PING
    print("Test 1: PING command")
    response = send_msagent_command("PING")
    if response == "PONG":
        print("✓ PING test passed")
    else:
        print(f"✗ PING test failed: {response}")
    print()
    
    # Test 2: VERSION
    print("Test 2: VERSION command")
    response = send_msagent_command("VERSION")
    print(f"✓ VERSION: {response}")
    print()
    
    # Test 3: SPEAK
    print("Test 3: SPEAK command")
    response = send_msagent_command("SPEAK:Testing MSAgent integration from Python!")
    if response and response.startswith("OK:"):
        print("✓ SPEAK test passed")
    else:
        print(f"✗ SPEAK test failed: {response}")
    print()
    
    # Test 4: ANIMATION
    print("Test 4: ANIMATION command")
    response = send_msagent_command("ANIMATION:Wave")
    if response and response.startswith("OK:"):
        print("✓ ANIMATION test passed")
    else:
        print(f"✗ ANIMATION test failed: {response}")
    print()
    
    # Test 5: Simulated GTA V event
    print("Test 5: Simulated GTA V vehicle event")
    response = send_msagent_command(
        "CHAT:I just got into a Zentorno (Super car). It's worth about $500000. React to this!"
    )
    if response and response.startswith("OK:"):
        print("✓ GTA V simulation test passed")
        print("  (Check MSAgent-AI for the AI response)")
    else:
        print(f"✗ GTA V simulation test failed: {response}")
    print()
    
    # Test 6: HIDE/SHOW
    print("Test 6: HIDE/SHOW commands")
    response = send_msagent_command("HIDE")
    if response and response.startswith("OK:"):
        print("✓ HIDE command passed")
        
        import time
        time.sleep(1)
        
        response = send_msagent_command("SHOW")
        if response and response.startswith("OK:"):
            print("✓ SHOW command passed")
        else:
            print(f"✗ SHOW test failed: {response}")
    else:
        print(f"✗ HIDE test failed: {response}")
    print()
    
    print("=" * 50)
    print("Test completed!")
    print()
    print("If all tests passed, the GTA V integration should work correctly.")
    print("Make sure to:")
    print("  1. Have MSAgent-AI running")
    print("  2. Have Ollama configured for CHAT commands")
    print("  3. Install ScriptHook V for GTA V integration")

if __name__ == "__main__":
    main()
