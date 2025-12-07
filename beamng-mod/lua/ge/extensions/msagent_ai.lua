-- MSAgent AI Commentary Extension for BeamNG.drive
-- Monitors vehicle events and sends them to AI server for commentary

local M = {}

-- Configuration
local serverUrl = "http://localhost:5000"
local updateInterval = 2.0  -- seconds between environment updates
local damageThreshold = 0.01  -- minimum damage to trigger commentary

-- State tracking
local lastUpdate = 0
local lastDamage = 0
local lastVehicleId = nil
local commentaryCooldown = 5.0  -- minimum seconds between comments
local lastCommentTime = 0
local previousSpeed = 0
local hasCommentedOnCar = false

-- Initialize the extension
local function onExtensionLoaded()
  log('I', 'msagent_ai', 'MSAgent AI Commentary extension loaded')
  log('I', 'msagent_ai', 'Server URL: ' .. serverUrl)
end

-- Send HTTP request to AI server
local function sendToAI(endpoint, data)
  local currentTime = os.time()
  
  -- Check cooldown
  if currentTime - lastCommentTime < commentaryCooldown then
    return
  end
  
  local url = serverUrl .. endpoint
  local jsonData = jsonEncode(data)
  
  -- Send async HTTP POST request
  local headers = {
    ["Content-Type"] = "application/json"
  }
  
  -- Using BeamNG's HTTP library
  local function onResponse(response)
    if response and response.responseData then
      local success, result = pcall(jsonDecode, response.responseData)
      if success and result.commentary then
        log('I', 'msagent_ai', 'AI Commentary: ' .. result.commentary)
        -- Display commentary on screen
        ui_message(result.commentary, 10, "msagent_ai")
        lastCommentTime = currentTime
      end
    end
  end
  
  -- Make HTTP request
  local request = {
    url = url,
    method = "POST",
    headers = headers,
    postData = jsonData,
    callback = onResponse
  }
  
  -- Using BeamNG's network module
  pcall(function()
    extensions.core_online.httpRequest(request)
  end)
end

-- Get current vehicle information
local function getVehicleInfo()
  local vehicle = be:getPlayerVehicle(0)
  if not vehicle then return nil end
  
  local vehicleObj = scenetree.findObjectById(vehicle:getID())
  if not vehicleObj then return nil end
  
  return {
    id = vehicle:getID(),
    name = vehicleObj.jbeam or "Unknown Vehicle",
    model = vehicleObj.partConfig or "Unknown Model"
  }
end

-- Get damage information
local function getDamageInfo()
  local vehicle = be:getPlayerVehicle(0)
  if not vehicle then return nil end
  
  local damage = vehicle:getObjectInitialNodePositions()
  local beamDamage = vehicle:getBeamDamage() or 0
  
  return {
    beamDamage = beamDamage,
    deformation = vehicle:getDeformationEnergy() or 0
  }
end

-- Get environment/surroundings information
local function getEnvironmentInfo()
  local vehicle = be:getPlayerVehicle(0)
  if not vehicle then return nil end
  
  local pos = vehicle:getPosition()
  local vel = vehicle:getVelocity()
  local speed = vel:length() * 3.6  -- Convert to km/h
  
  return {
    position = {x = pos.x, y = pos.y, z = pos.z},
    speed = speed,
    level = getMissionFilename() or "Unknown Location"
  }
end

-- Check for crash event
local function checkForCrash(env)
  if not env then return false end
  
  local speedDelta = math.abs(previousSpeed - env.speed)
  previousSpeed = env.speed
  
  -- Detect sudden deceleration (crash)
  if speedDelta > 30 and env.speed < 10 then  -- Lost 30+ km/h and now moving slowly
    return true
  end
  
  return false
end

-- Check for new damage
local function checkForDamage(damage)
  if not damage then return false end
  
  local newDamage = damage.beamDamage - lastDamage
  lastDamage = damage.beamDamage
  
  if newDamage > damageThreshold then
    return true, newDamage
  end
  
  return false, 0
end

-- Main update function
local function onUpdate(dt)
  lastUpdate = lastUpdate + dt
  
  if lastUpdate < updateInterval then
    return
  end
  
  lastUpdate = 0
  
  local vehicleInfo = getVehicleInfo()
  if not vehicleInfo then return end
  
  -- Check if vehicle changed
  if vehicleInfo.id ~= lastVehicleId then
    lastVehicleId = vehicleInfo.id
    lastDamage = 0
    hasCommentedOnCar = false
    previousSpeed = 0
    
    -- Comment on new vehicle
    sendToAI("/vehicle", {
      vehicle_name = vehicleInfo.name,
      vehicle_model = vehicleInfo.model
    })
    hasCommentedOnCar = true
    return
  end
  
  -- Get current state
  local damage = getDamageInfo()
  local env = getEnvironmentInfo()
  
  -- Check for crash
  if checkForCrash(env) then
    sendToAI("/crash", {
      vehicle_name = vehicleInfo.name,
      speed_before = previousSpeed,
      damage_level = damage.beamDamage
    })
    return
  end
  
  -- Check for damage (dent/scratch)
  local hasDamage, damageAmount = checkForDamage(damage)
  if hasDamage then
    if damageAmount > 0.1 then
      sendToAI("/dent", {
        vehicle_name = vehicleInfo.name,
        damage_amount = damageAmount,
        total_damage = damage.beamDamage
      })
    else
      sendToAI("/scratch", {
        vehicle_name = vehicleInfo.name,
        damage_amount = damageAmount,
        total_damage = damage.beamDamage
      })
    end
    return
  end
  
  -- Periodic environment commentary
  if not hasCommentedOnCar then
    sendToAI("/surroundings", {
      vehicle_name = vehicleInfo.name,
      location = env.level,
      speed = env.speed
    })
    hasCommentedOnCar = true
  end
end

-- Extension interface
M.onExtensionLoaded = onExtensionLoaded
M.onUpdate = onUpdate

return M
