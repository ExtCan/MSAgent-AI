# TCP Pipeline Testing & Setup

This directory contains tools to test and configure the MSAgent-AI TCP pipeline for LAN connectivity.

## Quick Links

- **Main Setup Guide**: See `/tmp/COMPLETE_SETUP_GUIDE.md`
- **Testing Tools Guide**: See `/tmp/TESTING_TOOLS_README.md`
- **Configuration Reference**: See `PIPELINE.md`

## Quick Start

1. **Configure MSAgent-AI**:
   - Settings → Pipeline
   - Protocol: `TCP`
   - IP Address: `0.0.0.0` ⚠️ Required for LAN!
   - Port: `8765`
   - Click Apply

2. **Configure Firewall**:
   ```cmd
   REM Run as Administrator
   netsh advfirewall firewall add rule name="MSAgent-AI" dir=in action=allow protocol=TCP localport=8765
   ```

3. **Test Connection**:
   ```bash
   python /tmp/test_lan_connection.py YOUR_IP_ADDRESS
   ```

## Testing Tools (in /tmp)

- `test_lan_connection.py` - Main diagnostic tool
- `test_tcp_pipeline_detailed.py` - Detailed testing
- `simple_tcp_server.py` - Reference server
- `setup_firewall.bat` - Firewall helper (Windows)
- `TestClient/` - C# test client

## Common Issue: LAN Not Working

**Problem**: Works on localhost but not from other machines.

**Solution**: IP address must be `0.0.0.0`, not `127.0.0.1`!

See PIPELINE.md for complete troubleshooting guide.
