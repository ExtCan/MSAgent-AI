"""
Test script for BeamNG Bridge Server (Mock version for non-Windows)
"""

import sys
import json

# Check if we're on Windows
try:
    import win32pipe
    IS_WINDOWS = True
except ImportError:
    IS_WINDOWS = False
    print("Not on Windows - testing with mock Named Pipe client")

if IS_WINDOWS:
    from bridge import app
else:
    # Mock version for testing on non-Windows
    from flask import Flask, request, jsonify
    from flask_cors import CORS
    
    app = Flask(__name__)
    CORS(app)
    
    def send_to_msagent(command):
        """Mock function for testing"""
        print(f"[MOCK] Would send to MSAgent-AI: {command}")
        return "OK:MOCK"
    
    @app.route('/health', methods=['GET'])
    def health():
        return jsonify({
            'status': 'ok',
            'msagent_connected': False,
            'msagent_response': 'MOCK:Not on Windows',
            'note': 'This is a mock server for testing on non-Windows systems'
        })
    
    @app.route('/vehicle', methods=['POST'])
    def comment_on_vehicle():
        data = request.json
        print(f"[MOCK] Vehicle: {data}")
        return jsonify({'status': 'ok'})
    
    @app.route('/crash', methods=['POST'])
    def comment_on_crash():
        data = request.json
        print(f"[MOCK] Crash: {data}")
        return jsonify({'status': 'ok'})
    
    @app.route('/dent', methods=['POST'])
    def comment_on_dent():
        data = request.json
        print(f"[MOCK] Dent: {data}")
        return jsonify({'status': 'ok'})
    
    @app.route('/scratch', methods=['POST'])
    def comment_on_scratch():
        data = request.json
        print(f"[MOCK] Scratch: {data}")
        return jsonify({'status': 'ok'})
    
    @app.route('/surroundings', methods=['POST'])
    def comment_on_surroundings():
        data = request.json
        print(f"[MOCK] Surroundings: {data}")
        return jsonify({'status': 'ok'})

# Test client
import requests
import time

def test_endpoints():
    """Test all bridge endpoints"""
    base_url = "http://localhost:5000"
    
    tests = [
        ("Health Check", "GET", "/health", None),
        ("Vehicle", "POST", "/vehicle", {"vehicle_name": "ETK 800-Series", "vehicle_model": "2.0T"}),
        ("Crash", "POST", "/crash", {"vehicle_name": "D-Series", "speed_before": 80, "damage_level": 0.5}),
        ("Dent", "POST", "/dent", {"vehicle_name": "Pessima", "damage_amount": 0.2, "total_damage": 0.5}),
        ("Scratch", "POST", "/scratch", {"vehicle_name": "Covet", "damage_amount": 0.01, "total_damage": 0.05}),
        ("Surroundings", "POST", "/surroundings", {"vehicle_name": "ETK K-Series", "location": "Italy", "speed": 75})
    ]
    
    print("Testing Bridge Server Endpoints")
    print("=" * 60)
    
    results = []
    for test_name, method, endpoint, data in tests:
        try:
            url = base_url + endpoint
            if method == "GET":
                response = requests.get(url, timeout=5)
            else:
                response = requests.post(url, json=data, timeout=5)
            
            success = response.status_code == 200
            results.append((test_name, success, response.json()))
            
            status = "✓ PASS" if success else "✗ FAIL"
            print(f"{test_name}: {status}")
            if not success:
                print(f"  Status: {response.status_code}")
            print(f"  Response: {response.json()}")
            
        except Exception as e:
            results.append((test_name, False, str(e)))
            print(f"{test_name}: ✗ FAIL - {e}")
        
        time.sleep(0.2)
    
    print("\n" + "=" * 60)
    passed = sum(1 for _, success, _ in results if success)
    total = len(results)
    print(f"Results: {passed}/{total} tests passed")
    
    return all(success for _, success, _ in results)

if __name__ == '__main__':
    if len(sys.argv) > 1 and sys.argv[1] == 'test':
        # Run tests
        time.sleep(2)  # Wait for server to start
        success = test_endpoints()
        sys.exit(0 if success else 1)
    else:
        # Run server
        print("Starting BeamNG Bridge Server (Mock Mode)" if not IS_WINDOWS else "Starting BeamNG Bridge Server")
        print("Server running on http://localhost:5000")
        print("\nTo test, run: python test_bridge.py test")
        app.run(host='0.0.0.0', port=5000, debug=False)
