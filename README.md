# MSAgent-AI

A comprehensive tool for creating Microsoft Agent (MSAgent) characters.

## Overview

MSAgent-AI provides a complete toolkit for creating, editing, and exporting MSAgent character definitions. Microsoft Agent characters are animated assistants (like Clippy, Merlin, or Bonzi Buddy) that can appear on the desktop and interact with users through speech and animations.

## Features

- **Character Builder**: Fluent API for creating characters programmatically
- **Animation Builder**: Easy creation of animation sequences with frames
- **Standard Animations**: Templates for common MSAgent animations (Idle, Wave, Speak, etc.)
- **CLI Tool**: Interactive command-line interface for character creation
- **Export Options**: Export to JSON or directory package format
- **Validation**: Built-in validation for character definitions
- **TypeScript Support**: Full TypeScript definitions included

## Installation

```bash
npm install msagent-ai
```

Or clone and build from source:

```bash
git clone https://github.com/ExtCan/MSAgent-AI.git
cd MSAgent-AI
npm install
npm run build
```

## Quick Start

### Using the CLI

```bash
npm run cli
```

The interactive CLI will guide you through creating a character:

1. Choose "Quick create" for a character with default settings
2. Edit animations, balloon configuration, etc.
3. Export your character to JSON

### Programmatic Usage

```typescript
import { CharacterBuilder, AnimationBuilder, createStandardAnimation } from 'msagent-ai';

// Create a new character
const builder = new CharacterBuilder({
  name: 'MyAgent',
  description: 'My custom MSAgent character',
  author: 'Your Name',
  width: 128,
  height: 128,
  includeStandardAnimations: true
});

// Add custom animation
const waveAnimation = new AnimationBuilder('CustomWave')
  .setDescription('A custom wave animation')
  .setLoop(false)
  .createFrame('base64ImageData1', 100)
  .createFrame('base64ImageData2', 100)
  .createFrame('base64ImageData3', 100)
  .build();

builder.addAnimation(waveAnimation);

// Configure balloon
builder.setBalloonConfig({
  fontName: 'Comic Sans MS',
  fontSize: 14,
  backgroundColor: '#FFFFCC'
});

// Build the character
const character = builder.build();

// Export to JSON
const json = builder.toJSON();
```

## Character Structure

An MSAgent character consists of:

### Character Info
- Name, description, author
- Dimensions (width/height in pixels)
- Version information
- Optional icon

### Animations
Each animation contains:
- Name (e.g., 'Idle1', 'Wave', 'Speak')
- Frames with image data
- Duration settings
- Loop configuration
- Optional return animation

### Standard Animations

| Animation | Description |
|-----------|-------------|
| Idle1, Idle2, Idle3 | Idle/resting animations |
| Show | Character appears |
| Hide | Character disappears |
| Speak | Speaking animation |
| Wave | Wave greeting |
| GestureUp/Down/Left/Right | Pointing gestures |
| Think | Thinking pose |
| Pleased, Sad, Surprised | Emotional expressions |
| Acknowledge, Decline | Response animations |
| Greet, Goodbye | Greeting animations |
| Blink | Eye blink |
| LookUp/Down/Left/Right | Looking animations |
| Processing, Writing, Reading | Activity animations |

### Sounds
Audio files for character sounds:
- Format: WAV, MP3, or OGG
- Can be linked to specific animation frames

### Balloon Configuration
Speech bubble settings:
- Lines and characters per line
- Font name and size
- Colors (text, background, border)

## API Reference

### CharacterBuilder

```typescript
const builder = new CharacterBuilder(options);

// Set character properties
builder.setName('MyAgent')
       .setDescription('Description')
       .setAuthor('Author')
       .setDimensions(128, 128)
       .setVersion('1.0.0');

// Add/remove animations
builder.addAnimation(animation)
       .removeAnimation('animationName');

// Add/remove sounds
builder.addSound(sound)
       .removeSound('soundId');

// Configure balloon
builder.setBalloonConfig({...});

// Set default animation
builder.setDefaultAnimation('Idle1');

// Build character
const character = builder.build();

// Export to JSON
const json = builder.toJSON();

// Load from JSON
const loadedBuilder = CharacterBuilder.fromJSON(json);
```

### AnimationBuilder

```typescript
const animation = new AnimationBuilder('AnimationName')
  .setDescription('Description')
  .setLoop(true)
  .setSpeed(1.0)
  .createFrame(imageData, duration, offsetX, offsetY)
  .createFrameWithSound(imageData, soundId, duration)
  .setReturnAnimation('Idle1')
  .build();
```

### CharacterExporter

```typescript
import { CharacterExporter } from 'msagent-ai';

// Export to JSON file
CharacterExporter.exportToJSON(character, 'character.json');

// Export as package
CharacterExporter.exportAsPackage(character, './my-character/');

// Import from JSON
const character = CharacterExporter.importFromJSON('character.json');

// Validate character
const result = CharacterExporter.validate(character);
if (!result.valid) {
  console.log('Errors:', result.errors);
  console.log('Warnings:', result.warnings);
}
```

### Utility Functions

```typescript
import {
  createStandardAnimation,
  getStandardAnimationTypes,
  calculateAnimationDuration,
  cloneAnimation,
  reverseAnimation,
  scaleAnimationTiming
} from 'msagent-ai';

// Create a standard animation template
const idle = createStandardAnimation('Idle1');

// Get list of standard animation types
const types = getStandardAnimationTypes();

// Calculate total duration
const duration = calculateAnimationDuration(animation);

// Clone, reverse, or scale animations
const reversed = reverseAnimation(animation);
const scaled = scaleAnimationTiming(animation, 0.5);
```

## Export Formats

### JSON Format

Single file containing all character data:

```json
{
  "info": {
    "name": "MyAgent",
    "description": "...",
    "author": "...",
    "version": "1.0.0",
    "width": 128,
    "height": 128,
    "createdAt": "...",
    "modifiedAt": "..."
  },
  "animations": [...],
  "sounds": [...],
  "balloon": {...},
  "states": [...],
  "defaultAnimation": "Idle1"
}
```

### Package Format

Directory structure:
```
my-character/
├── manifest.json
├── character.json
├── animations/
│   ├── Idle1.json
│   ├── Wave.json
│   └── ...
└── sounds/
    ├── sound1.json
    └── ...
```

## Standalone Executables

Pre-built executables are available for download from the [Actions](../../actions) tab:

- **msagent-windows-x64** - Windows executable (.exe)
- **msagent-linux-x64** - Linux executable
- **msagent-macos-x64** - macOS executable

Or build them yourself:

```bash
npm run build:exe
```

This creates standalone executables in the `bin/` directory that can run without Node.js installed.

## Known Limitations

- **Placeholder Image Generation**: The `generatePlaceholderImage` function currently returns a placeholder string rather than actual image data. To generate real images, you'll need to integrate an image processing library like `canvas` or `sharp`.

- **Binary ACS/ACF Export**: Export to native MSAgent binary formats (.acs, .acf) is not yet implemented. Currently only JSON export is fully supported.

## Development

```bash
# Install dependencies
npm install

# Build
npm run build

# Build standalone executables
npm run build:exe

# Run CLI in development
npm run cli

# Run tests
npm test

# Type checking
npm run lint
```

## License

MIT License

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Related

- [Microsoft Agent Documentation](https://docs.microsoft.com/en-us/previous-versions/windows/desktop/msagent/msagent)
- [Double Agent](http://doubleagent.sourceforge.net/) - Open source MSAgent implementation

