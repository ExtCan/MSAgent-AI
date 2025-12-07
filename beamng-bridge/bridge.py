"""
BeamNG to MSAgent-AI Bridge Server
Receives HTTP requests from BeamNG mod and forwards them to MSAgent-AI via Named Pipe
"""

import win32pipe
import win32file
import pywintypes
from flask import Flask, request, jsonify
from flask_cors import CORS
import logging
import os
import random

app = Flask(__name__)
CORS(app)

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

PIPE_NAME = r'\\.\pipe\MSAgentAI'
PIPE_TIMEOUT = 5000  # 5 seconds

def send_to_msagent(command):
    """Send a command to MSAgent-AI via Named Pipe"""
    try:
        # Try to connect to the pipe
        handle = win32file.CreateFile(
            PIPE_NAME,
            win32file.GENERIC_READ | win32file.GENERIC_WRITE,
            0,
            None,
            win32file.OPEN_EXISTING,
            0,
            None
        )
        
        # Send command
        command_bytes = (command + '\n').encode('utf-8')
        win32file.WriteFile(handle, command_bytes)
        
        # Read response
        result, data = win32file.ReadFile(handle, 1024)
        response = data.decode('utf-8').strip()
        
        win32file.CloseHandle(handle)
        
        logger.info(f"Sent: {command}, Received: {response}")
        return response
        
    except pywintypes.error as e:
        logger.error(f"Named pipe error: {e}")
        return f"ERROR:Could not connect to MSAgent-AI. Is it running?"
    except Exception as e:
        logger.error(f"Error sending to MSAgent-AI: {e}")
        return f"ERROR:{str(e)}"

@app.route('/health', methods=['GET'])
def health():
    """Health check - also checks if MSAgent-AI is running"""
    response = send_to_msagent("PING")
    is_connected = "PONG" in response
    
    return jsonify({
        'status': 'ok',
        'msagent_connected': is_connected,
        'msagent_response': response
    })

@app.route('/vehicle', methods=['POST'])
def comment_on_vehicle():
    """Comment on the current vehicle"""
    data = request.json
    vehicle_name = data.get('vehicle_name', 'Unknown')
    vehicle_model = data.get('vehicle_model', '')
    
    # Send to MSAgent-AI with context for AI commentary
    prompt = f"I just spawned a {vehicle_name} {vehicle_model} in BeamNG! Make an excited comment about this vehicle."
    send_to_msagent(f"CHAT:{prompt}")
    
    return jsonify({'status': 'ok'})

@app.route('/crash', methods=['POST'])
def comment_on_crash():
    """Comment on a crash event"""
    data = request.json
    vehicle_name = data.get('vehicle_name', 'Unknown')
    speed_before = data.get('speed_before', 0)
    damage_level = data.get('damage_level', 0)
    
    prompt = f"I just crashed my {vehicle_name} at {speed_before:.0f} km/h! The damage is pretty bad ({damage_level:.1f}). React dramatically!"
    send_to_msagent(f"CHAT:{prompt}")
    
    return jsonify({'status': 'ok'})

@app.route('/dent', methods=['POST'])
def comment_on_dent():
    """Comment on a dent/major damage"""
    data = request.json
    vehicle_name = data.get('vehicle_name', 'Unknown')
    damage_amount = data.get('damage_amount', 0)
    
    prompt = f"My {vehicle_name} just got a big dent! Make a comment about the damage."
    send_to_msagent(f"CHAT:{prompt}")
    
    return jsonify({'status': 'ok'})

@app.route('/scratch', methods=['POST'])
def comment_on_scratch():
    """Comment on a scratch/minor damage"""
    data = request.json
    vehicle_name = data.get('vehicle_name', 'Unknown')
    
    prompt = f"Just scratched the paint on my {vehicle_name}. Make a light comment."
    send_to_msagent(f"CHAT:{prompt}")
    
    return jsonify({'status': 'ok'})

@app.route('/surroundings', methods=['POST'])
def comment_on_surroundings():
    """Comment on the surroundings/environment"""
    data = request.json
    vehicle_name = data.get('vehicle_name', 'Unknown')
    location = data.get('location', 'Unknown')
    speed = data.get('speed', 0)
    
    prompt = f"I'm driving my {vehicle_name} at {speed:.0f} km/h in {location}. Comment on the scene!"
    send_to_msagent(f"CHAT:{prompt}")
    
    return jsonify({'status': 'ok'})

if __name__ == '__main__':
    port = int(os.getenv('PORT', 5000))
    logger.info(f"Starting BeamNG to MSAgent-AI Bridge on port {port}")
    logger.info(f"Connecting to Named Pipe: {PIPE_NAME}")
    logger.info("Make sure MSAgent-AI is running!")
    
    app.run(host='0.0.0.0', port=port, debug=False)
