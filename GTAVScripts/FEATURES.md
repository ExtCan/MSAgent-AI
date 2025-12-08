# MSAgent-AI GTA V Integration - Feature Overview

## What This Script Does

The MSAgent-AI GTA V integration brings your Microsoft Agent desktop character into Grand Theft Auto V, providing live commentary and reactions to everything happening in the game.

## Visual Feature Demonstration

### Menu System (Press F9)

When you press F9 in-game, a menu appears with the following options:

```
┌─────────────────────────────────────┐
│ MSAgent-AI                          │
│ GTA V Integration Settings          │
├─────────────────────────────────────┤
│ [✓] React to Vehicles               │
│ [✓] React to Missions               │
│ [✓] React to Weather                │
│ [✓] React to Time                   │
│ [✓] React to Location               │
│ [✓] React to Player State           │
│ [✓] React to Character Switch       │
│ [✓] React to Vehicle Value          │
└─────────────────────────────────────┘
```

**How to use:**
- Navigate with Arrow Keys
- Toggle with Enter
- Close with F9 or Esc

### Reaction Examples by Category

#### 1. Vehicle Reactions

**Trigger:** Entering or exiting a vehicle

**Examples:**

| Vehicle Type | MSAgent Says (with AI) |
|-------------|------------------------|
| Zentorno (supercar) | "Whoa! A Zentorno worth $725,000? Someone's living large! Try not to scratch the paint!" |
| Faggio (scooter) | "A scooter? Really? Well, at least it gets good gas mileage!" |
| Buzzard (helicopter) | "Now we're talking! A military helicopter! Time to see Los Santos from above!" |
| Marquis (yacht) | "A yacht worth $413,000! Fancy! Let's take her for a spin on the ocean!" |
| BMX (bicycle) | "Going eco-friendly with a bicycle? Respect! But maybe not the best for a getaway..." |

**With Vehicle Value enabled:**
- Cheap vehicles: Comments on affordability
- Expensive vehicles: Impressed reactions
- Free vehicles: Jokes about getting a good deal

#### 2. Mission Reactions

**Trigger:** Mission starts or progresses

**Examples:**

| Event | MSAgent Says |
|-------|-------------|
| Mission start | "Oh! A new mission! This should be interesting! What's the plan?" |
| Mission progress | "Things are heating up! The mission is getting intense!" |
| (Note: Full mission dialog tracking limited by API) | |

#### 3. Weather Reactions

**Trigger:** Weather changes

**Examples:**

| Weather | MSAgent Says |
|---------|-------------|
| Sunny → Rainy | "Uh oh, it's starting to rain! Better turn on those windshield wipers!" |
| Clear → Foggy | "Wow, this fog is thick! I can barely see anything. Drive carefully!" |
| Any → Thunderstorm | "Whoa! Thunder and lightning! Nature's putting on a show!" |
| Any → Clear | "The weather's clearing up! What a beautiful day in Los Santos!" |

#### 4. Time of Day Reactions

**Trigger:** Specific hours (sunrise, noon, sunset, midnight)

**Examples:**

| Time | MSAgent Says |
|------|-------------|
| 6:00 AM | "Good morning! The sun is rising over Los Santos! What a beautiful sunrise!" |
| 12:00 PM | "It's noon! The sun is directly overhead. Perfect time for lunch!" |
| 6:00 PM | "Look at that sunset! The sky is turning orange and pink. Gorgeous!" |
| 12:00 AM | "It's midnight! The witching hour! Los Santos looks different at night..." |

#### 5. Location Reactions

**Trigger:** Entering a new area/zone

**Examples:**

| Location | MSAgent Says |
|----------|-------------|
| Vinewood Hills | "Welcome to Vinewood Hills! This is where the rich and famous live!" |
| Vespucci Beach | "Ah, Vespucci Beach! Time to relax by the ocean!" |
| Downtown | "We're in downtown Los Santos! Look at all these skyscrapers!" |
| Sandy Shores | "Sandy Shores... Not the nicest part of town, but it has character!" |

#### 6. Player State Reactions

**Trigger:** Health, wanted level, or death events

**Examples:**

| Event | MSAgent Says |
|-------|-------------|
| 1 Star | "Uh oh! One wanted star! The cops are watching you now!" |
| 3 Stars | "THREE STARS! This is serious! The cops are NOT messing around!" |
| 5 Stars | "FIVE STARS?! The whole city is after you! RUN!" |
| Wanted cleared | "Phew! You lost the cops! Nice driving!" |
| Death | "Ouch! That looked painful! Better luck next time, champ!" |
| Respawn | "Back from the dead! Ready for round two?" |

#### 7. Character Switch Reactions

**Trigger:** Switching between Michael, Franklin, and Trevor

**Examples:**

| Character | MSAgent Says |
|-----------|-------------|
| → Michael | "Switching to Michael De Santa! The retired bank robber himself!" |
| → Franklin | "Franklin Clinton is on the scene! Time for some repo work!" |
| → Trevor | "Oh boy, Trevor Philips! Things are about to get CRAZY!" |

#### 8. Vehicle Value Reactions

**Trigger:** Same as vehicle reactions, but includes price commentary

**Examples:**

| Scenario | MSAgent Says |
|----------|-------------|
| Expensive car | "A $2.2 million T20?! That's more than most houses! Don't crash it!" |
| Free car | "The Elegy is free! Best deal in Los Santos!" |
| Cheap vehicle | "A $9,000 scooter. Well, it's cheap and cheerful!" |
| Mid-range | "Nice! A $195,000 Carbonizzare. Classy choice!" |

## Cooldown System Explanation

To prevent MSAgent from talking constantly, the script uses cooldowns:

### Standard Cooldown (10 seconds)
Used for:
- Weather changes
- Time of day transitions
- Location changes
- Character switches
- Mission events

### Fast Cooldown (3 seconds)
Used for:
- Vehicle entry/exit
- Wanted level changes

**Visual Timeline:**
```
Time: 0s    3s    6s    9s    12s   15s
      |     |     |     |     |     |
Event: [Vehicle] ⏱️⏱️  [Vehicle] ⏱️⏱️  [Vehicle]
       "Nice car!"  ❌Too soon "Cool bike!"

Event: [Weather]     ⏱️⏱️⏱️⏱️⏱️⏱️⏱️⏱️⏱️⏱️  [Weather]
       "It's raining!"              ❌Too soon
```

## Communication Flow

```
┌─────────────┐         Named Pipe          ┌──────────────┐
│  GTA V      │    \\.\pipe\MSAgentAI      │  MSAgent-AI  │
│  (Game)     │ ──────────────────────────> │ (Desktop App)│
│             │                              │              │
│ Player      │  CHAT:The player entered    │ [AI Thinks]  │
│ enters car  │  a Zentorno worth $725k     │              │
│             │                              │ Agent speaks │
│             │                              │ with voice   │
└─────────────┘                              └──────────────┘
```

### Command Types Sent:

1. **SPEAK:text** - Direct speech (rare, only for system messages)
   - Example: `SPEAK:GTA V integration loaded!`

2. **CHAT:prompt** - AI-powered response (most reactions)
   - Example: `CHAT:The player just crashed. React with sympathy or humor.`
   - MSAgent uses Ollama to generate a contextual response

## Typical Play Session Example

**Player starts GTA V:**
```
MSAgent: "GTA V integration loaded! I'm ready to commentate!"
```

**Player gets in a Zentorno:**
```
MSAgent: "Whoa! A Zentorno worth $725,000? That's one expensive ride! 
         Try not to scratch it!"
```

**Player drives around, enters Vinewood:**
```
[10 seconds pass]
MSAgent: "Welcome to Vinewood Hills! Home of the stars! Fancy neighborhood!"
```

**Weather changes to rain:**
```
[10 seconds pass]
MSAgent: "Oh great, it's starting to rain! Better slow down, the roads 
         get slippery!"
```

**Player gets 3 wanted stars:**
```
[3 seconds pass]
MSAgent: "THREE WANTED STARS! The cops are serious now! Floor it!"
```

**Player crashes:**
```
[Player exits damaged vehicle]
MSAgent: "Yikes! That car is wrecked! Time for a new one!"
```

**Player switches to Trevor:**
```
[10 seconds pass]
MSAgent: "Switching to Trevor Philips! Oh boy, things are about to 
         get interesting... and probably violent!"
```

**Player presses F9:**
```
[Menu appears on screen]
MSAgent: [Silent - player is in menu]
```

**Player disables "React to Weather":**
```
[In-game notification: "MSAgent: React to Weather disabled"]
```

**Player closes menu and continues:**
```
[Weather changes - no reaction because disabled]
[Player enters new car - MSAgent reacts because vehicle reactions enabled]
MSAgent: "A Faggio scooter? Really? Well, it's fuel-efficient!"
```

## Frequency of Reactions

With all options enabled, expect reactions:

| Situation | Frequency |
|-----------|-----------|
| Normal driving around city | ~2-3 reactions per minute |
| Staying in one area | ~1 reaction per minute |
| Active chaos (changing vehicles, etc.) | ~4-6 reactions per minute |
| Stationary | 0 reactions |

**Tips to reduce chattiness:**
- Disable location reactions (most frequent in city)
- Disable vehicle reactions (frequent if you switch cars often)
- Keep weather/time only (least frequent but interesting)

**Tips to increase commentary:**
- Enable all reaction types
- Drive around different neighborhoods
- Switch vehicles frequently
- Switch characters
- Change weather with cheats/mods

## Compatibility Notes

**Works with:**
- ✅ Story Mode
- ✅ All three characters
- ✅ All vehicles in the game
- ✅ Weather mods
- ✅ Vehicle mods (won't know value, but will detect type)
- ✅ Trainer mods

**Does NOT work with:**
- ❌ Online mode (designed for single-player only)
- ❌ Some mission mods (may have limited mission detection)

## Performance Impact

**Minimal impact on game performance:**
- Script runs very efficiently
- Named pipe communication is lightweight
- Most processing happens in MSAgent-AI app, not GTA V
- No graphics rendering or heavy computation

**MSAgent-AI app impact:**
- Depends on Ollama model (faster models = faster responses)
- GPU acceleration recommended for Ollama
- Typically <1 second response time with llama3.2

## Customization Freedom

Everything can be customized:
- Change F9 to any key
- Modify all prompts
- Add new reaction types
- Adjust cooldown times
- Change personality style
- Add easter eggs

See **ADVANCED.md** for detailed customization guide.

## Summary

This integration turns GTA V into a narrated experience where your MSAgent character becomes your commentary companion, reacting to your adventures in Los Santos with personality and humor!
