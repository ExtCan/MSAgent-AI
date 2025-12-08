# Test Script for MSAgent-AI Named Pipe Communication
# This PowerShell script tests the communication between external apps and MSAgent-AI

Write-Host "MSAgent-AI Named Pipe Test Script" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan
Write-Host ""

# Function to send a command to MSAgent-AI
function Send-MSAgentCommand {
    param(
        [string]$Command
    )
    
    try {
        $pipe = New-Object System.IO.Pipes.NamedPipeClientStream(".", "MSAgentAI", [System.IO.Pipes.PipeDirection]::InOut)
        
        Write-Host "Connecting to MSAgent-AI pipe..." -ForegroundColor Yellow
        $pipe.Connect(2000)  # 2 second timeout
        
        $writer = New-Object System.IO.StreamWriter($pipe)
        $reader = New-Object System.IO.StreamReader($pipe)
        $writer.AutoFlush = $true
        
        Write-Host "Sending command: $Command" -ForegroundColor Green
        $writer.WriteLine($Command)
        
        $response = $reader.ReadLine()
        Write-Host "Response: $response" -ForegroundColor Cyan
        
        $pipe.Close()
        return $response
    }
    catch {
        Write-Host "Error: $_" -ForegroundColor Red
        Write-Host ""
        Write-Host "Make sure MSAgent-AI is running before running this test." -ForegroundColor Yellow
        return $null
    }
}

# Test 1: PING
Write-Host "Test 1: PING command" -ForegroundColor Magenta
$response = Send-MSAgentCommand "PING"
if ($response -eq "PONG") {
    Write-Host "✓ PING test passed" -ForegroundColor Green
} else {
    Write-Host "✗ PING test failed" -ForegroundColor Red
}
Write-Host ""

# Test 2: VERSION
Write-Host "Test 2: VERSION command" -ForegroundColor Magenta
$response = Send-MSAgentCommand "VERSION"
Write-Host "✓ VERSION: $response" -ForegroundColor Green
Write-Host ""

# Test 3: SPEAK
Write-Host "Test 3: SPEAK command" -ForegroundColor Magenta
$response = Send-MSAgentCommand "SPEAK:Testing MSAgent integration!"
if ($response -like "OK:*") {
    Write-Host "✓ SPEAK test passed" -ForegroundColor Green
} else {
    Write-Host "✗ SPEAK test failed: $response" -ForegroundColor Red
}
Write-Host ""

# Test 4: Simulated GTA V event
Write-Host "Test 4: Simulated GTA V vehicle event" -ForegroundColor Magenta
$response = Send-MSAgentCommand "CHAT:I just got into a Zentorno (Super car). It's worth about $500000. React to this!"
if ($response -like "OK:*") {
    Write-Host "✓ GTA V simulation test passed" -ForegroundColor Green
    Write-Host "  (Check MSAgent-AI for the AI response)" -ForegroundColor Yellow
} else {
    Write-Host "✗ GTA V simulation test failed: $response" -ForegroundColor Red
}
Write-Host ""

Write-Host "===================================" -ForegroundColor Cyan
Write-Host "Test completed!" -ForegroundColor Cyan
Write-Host ""
Write-Host "If all tests passed, the GTA V integration should work correctly." -ForegroundColor Green
Write-Host "Make sure to:" -ForegroundColor Yellow
Write-Host "  1. Have MSAgent-AI running" -ForegroundColor Yellow
Write-Host "  2. Have Ollama configured for CHAT commands" -ForegroundColor Yellow
Write-Host "  3. Install ScriptHook V for GTA V integration" -ForegroundColor Yellow
