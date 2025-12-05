#!/usr/bin/env node

import * as fs from 'fs';
import * as path from 'path';
import * as readline from 'readline';
import { CharacterBuilder } from './builders/CharacterBuilder.js';
import { AnimationBuilder } from './builders/AnimationBuilder.js';
import { CharacterExporter } from './export/exporter.js';
import { createStandardAnimation, getStandardAnimationTypes } from './utils/animations.js';
import type { StandardAnimation, CharacterCreationOptions } from './models/types.js';

/**
 * Interactive CLI for creating MSAgent characters
 */
class MSAgentCLI {
  private rl: readline.Interface;

  constructor() {
    this.rl = readline.createInterface({
      input: process.stdin,
      output: process.stdout
    });
  }

  /**
   * Prompt user for input
   */
  private async prompt(question: string): Promise<string> {
    return new Promise((resolve) => {
      this.rl.question(question, (answer) => {
        resolve(answer.trim());
      });
    });
  }

  /**
   * Display main menu
   */
  private displayMainMenu(): void {
    console.log('\n=================================');
    console.log('   MSAgent Character Creator');
    console.log('=================================\n');
    console.log('1. Create new character');
    console.log('2. Load existing character');
    console.log('3. Quick create (with defaults)');
    console.log('4. List standard animations');
    console.log('5. Help');
    console.log('6. Exit');
    console.log('');
  }

  /**
   * Display character edit menu
   */
  private displayEditMenu(characterName: string): void {
    console.log(`\n--- Editing: ${characterName} ---\n`);
    console.log('1. Edit character info');
    console.log('2. Add animation');
    console.log('3. Remove animation');
    console.log('4. Add standard animations');
    console.log('5. List animations');
    console.log('6. Configure balloon');
    console.log('7. Validate character');
    console.log('8. Export character');
    console.log('9. Save and return to main menu');
    console.log('0. Discard and return to main menu');
    console.log('');
  }

  /**
   * Create a new character interactively
   */
  async createNewCharacter(): Promise<CharacterBuilder | null> {
    console.log('\n--- Create New Character ---\n');
    
    const name = await this.prompt('Character name: ');
    if (!name) {
      console.log('Name is required.');
      return null;
    }

    const description = await this.prompt('Description (optional): ');
    const author = await this.prompt('Author (optional): ');
    
    const widthStr = await this.prompt('Width in pixels (default 128): ');
    const heightStr = await this.prompt('Height in pixels (default 128): ');
    
    const width = parseInt(widthStr, 10) || 128;
    const height = parseInt(heightStr, 10) || 128;

    const includeStandard = await this.prompt('Include standard animations? (y/n): ');

    const options: CharacterCreationOptions = {
      name,
      description: description || undefined,
      author: author || undefined,
      width,
      height,
      includeStandardAnimations: includeStandard.toLowerCase() === 'y'
    };

    const builder = new CharacterBuilder(options);
    console.log(`\nCharacter "${name}" created successfully!`);
    
    return builder;
  }

  /**
   * Quick create a character with defaults
   */
  async quickCreate(): Promise<CharacterBuilder | null> {
    console.log('\n--- Quick Create Character ---\n');
    
    const name = await this.prompt('Character name: ');
    if (!name) {
      console.log('Name is required.');
      return null;
    }

    const builder = new CharacterBuilder({
      name,
      description: `${name} - MSAgent character`,
      author: 'MSAgent Creator',
      width: 128,
      height: 128,
      includeStandardAnimations: true
    });

    console.log(`\nCharacter "${name}" created with default settings and standard animations!`);
    return builder;
  }

  /**
   * Load character from file
   */
  async loadCharacter(): Promise<CharacterBuilder | null> {
    console.log('\n--- Load Character ---\n');
    
    const filePath = await this.prompt('Enter file path: ');
    if (!filePath) {
      console.log('File path is required.');
      return null;
    }

    if (!fs.existsSync(filePath)) {
      console.log('File not found.');
      return null;
    }

    try {
      const json = fs.readFileSync(filePath, 'utf-8');
      const builder = CharacterBuilder.fromJSON(json);
      console.log(`\nCharacter "${builder.getName()}" loaded successfully!`);
      return builder;
    } catch (error) {
      console.log(`Error loading character: ${error instanceof Error ? error.message : 'Unknown error'}`);
      return null;
    }
  }

  /**
   * Edit character interactively
   */
  async editCharacter(builder: CharacterBuilder): Promise<void> {
    let editing = true;

    while (editing) {
      this.displayEditMenu(builder.getName());
      const choice = await this.prompt('Choose option: ');

      switch (choice) {
        case '1':
          await this.editCharacterInfo(builder);
          break;
        case '2':
          await this.addAnimation(builder);
          break;
        case '3':
          await this.removeAnimation(builder);
          break;
        case '4':
          await this.addStandardAnimations(builder);
          break;
        case '5':
          this.listAnimations(builder);
          break;
        case '6':
          await this.configureBalloon(builder);
          break;
        case '7':
          this.validateCharacter(builder);
          break;
        case '8':
          await this.exportCharacter(builder);
          break;
        case '9':
          await this.saveCharacter(builder);
          editing = false;
          break;
        case '0':
          editing = false;
          break;
        default:
          console.log('Invalid option.');
      }
    }
  }

  /**
   * Edit character info
   */
  private async editCharacterInfo(builder: CharacterBuilder): Promise<void> {
    console.log('\n--- Edit Character Info ---\n');
    
    const name = await this.prompt(`Name (current: ${builder.getName()}): `);
    if (name) builder.setName(name);

    const character = builder.build();
    
    const description = await this.prompt(`Description (current: ${character.info.description}): `);
    if (description) builder.setDescription(description);

    const author = await this.prompt(`Author (current: ${character.info.author}): `);
    if (author) builder.setAuthor(author);

    const version = await this.prompt(`Version (current: ${character.info.version}): `);
    if (version) builder.setVersion(version);

    console.log('\nCharacter info updated!');
  }

  /**
   * Add a custom animation
   */
  private async addAnimation(builder: CharacterBuilder): Promise<void> {
    console.log('\n--- Add Animation ---\n');
    
    const name = await this.prompt('Animation name: ');
    if (!name) {
      console.log('Name is required.');
      return;
    }

    const description = await this.prompt('Description (optional): ');
    const loopStr = await this.prompt('Loop animation? (y/n): ');
    const frameCountStr = await this.prompt('Number of placeholder frames (default 1): ');
    const frameCount = parseInt(frameCountStr, 10) || 1;
    const durationStr = await this.prompt('Frame duration in ms (default 100): ');
    const duration = parseInt(durationStr, 10) || 100;

    const animBuilder = new AnimationBuilder(name)
      .setDescription(description || name)
      .setLoop(loopStr.toLowerCase() === 'y');

    for (let i = 0; i < frameCount; i++) {
      animBuilder.createFrame('', duration);
    }

    builder.addAnimation(animBuilder.build());
    console.log(`\nAnimation "${name}" added with ${frameCount} frame(s)!`);
  }

  /**
   * Remove an animation
   */
  private async removeAnimation(builder: CharacterBuilder): Promise<void> {
    console.log('\n--- Remove Animation ---\n');
    this.listAnimations(builder);
    
    const name = await this.prompt('Animation name to remove: ');
    if (!name) {
      return;
    }

    if (builder.getAnimation(name)) {
      builder.removeAnimation(name);
      console.log(`\nAnimation "${name}" removed!`);
    } else {
      console.log(`Animation "${name}" not found.`);
    }
  }

  /**
   * Add standard animations
   */
  private async addStandardAnimations(builder: CharacterBuilder): Promise<void> {
    console.log('\n--- Add Standard Animations ---\n');
    console.log('Available standard animations:');
    
    const types = getStandardAnimationTypes();
    types.forEach((type, index) => {
      const exists = builder.getAnimation(type) ? ' [exists]' : '';
      console.log(`  ${index + 1}. ${type}${exists}`);
    });

    const choice = await this.prompt('\nEnter animation number (or "all" for all): ');
    
    if (choice.toLowerCase() === 'all') {
      for (const type of types) {
        if (!builder.getAnimation(type)) {
          builder.addAnimation(createStandardAnimation(type));
        }
      }
      console.log('\nAll standard animations added!');
    } else {
      const index = parseInt(choice, 10) - 1;
      const type = types[index];
      if (type) {
        builder.addAnimation(createStandardAnimation(type));
        console.log(`\nAnimation "${type}" added!`);
      } else {
        console.log('Invalid selection.');
      }
    }
  }

  /**
   * List all animations
   */
  private listAnimations(builder: CharacterBuilder): void {
    const character = builder.build();
    console.log('\n--- Animations ---\n');
    
    if (character.animations.length === 0) {
      console.log('No animations.');
      return;
    }

    for (const anim of character.animations) {
      const loop = anim.loop ? ' [loop]' : '';
      console.log(`  - ${anim.name}: ${anim.frames.length} frame(s)${loop}`);
    }
  }

  /**
   * Configure balloon settings
   */
  private async configureBalloon(builder: CharacterBuilder): Promise<void> {
    console.log('\n--- Configure Word Balloon ---\n');
    const character = builder.build();
    const current = character.balloon;

    const lines = await this.prompt(`Lines (current: ${current.lines}): `);
    const charsPerLine = await this.prompt(`Chars per line (current: ${current.charsPerLine}): `);
    const fontName = await this.prompt(`Font name (current: ${current.fontName}): `);
    const fontSize = await this.prompt(`Font size (current: ${current.fontSize}): `);
    const textColor = await this.prompt(`Text color hex (current: ${current.textColor}): `);
    const bgColor = await this.prompt(`Background color hex (current: ${current.backgroundColor}): `);

    builder.setBalloonConfig({
      lines: parseInt(lines, 10) || current.lines,
      charsPerLine: parseInt(charsPerLine, 10) || current.charsPerLine,
      fontName: fontName || current.fontName,
      fontSize: parseInt(fontSize, 10) || current.fontSize,
      textColor: textColor || current.textColor,
      backgroundColor: bgColor || current.backgroundColor
    });

    console.log('\nBalloon configuration updated!');
  }

  /**
   * Validate character
   */
  private validateCharacter(builder: CharacterBuilder): void {
    console.log('\n--- Validation Results ---\n');
    const character = builder.build();
    const result = CharacterExporter.validate(character);

    if (result.valid) {
      console.log('✓ Character is valid!');
    } else {
      console.log('✗ Character has errors:');
      for (const error of result.errors) {
        console.log(`  ERROR: ${error}`);
      }
    }

    if (result.warnings.length > 0) {
      console.log('\nWarnings:');
      for (const warning of result.warnings) {
        console.log(`  WARNING: ${warning}`);
      }
    }
  }

  /**
   * Export character
   */
  private async exportCharacter(builder: CharacterBuilder): Promise<void> {
    console.log('\n--- Export Character ---\n');
    console.log('Export formats:');
    console.log('  1. JSON file');
    console.log('  2. Package directory');
    
    const choice = await this.prompt('Choose format (1-2): ');
    const outputPath = await this.prompt('Output path: ');
    
    if (!outputPath) {
      console.log('Output path is required.');
      return;
    }

    const character = builder.build();

    try {
      if (choice === '1') {
        CharacterExporter.exportToJSON(character, outputPath);
        console.log(`\nCharacter exported to ${outputPath}`);
      } else if (choice === '2') {
        CharacterExporter.exportAsPackage(character, outputPath);
        console.log(`\nCharacter package exported to ${outputPath}/`);
      } else {
        console.log('Invalid format choice.');
      }
    } catch (error) {
      console.log(`Export failed: ${error instanceof Error ? error.message : 'Unknown error'}`);
    }
  }

  /**
   * Save character to file
   */
  private async saveCharacter(builder: CharacterBuilder): Promise<void> {
    const outputPath = await this.prompt('Save to file (leave empty to skip): ');
    
    if (outputPath) {
      try {
        const character = builder.build();
        CharacterExporter.exportToJSON(character, outputPath);
        console.log(`\nCharacter saved to ${outputPath}`);
      } catch (error) {
        console.log(`Save failed: ${error instanceof Error ? error.message : 'Unknown error'}`);
      }
    }
  }

  /**
   * Display help
   */
  private displayHelp(): void {
    console.log(`
MSAgent Character Creator - Help
=================================

This tool helps you create MSAgent character definition files.

MSAgent characters consist of:
  - Character Info: Name, author, dimensions, etc.
  - Animations: Sequences of frames for different actions
  - Sounds: Audio files for character sounds
  - Balloon: Word balloon configuration for speech

Standard Animations:
  MSAgent characters typically include animations like:
  Idle, Show, Hide, Speak, Wave, Think, Pleased, Sad, etc.

Export Formats:
  - JSON: Single file with all character data
  - Package: Directory structure with separate files

Quick Start:
  1. Use 'Quick create' for a character with default settings
  2. Add/edit animations as needed
  3. Export your character

For more information, visit: https://github.com/ExtCan/MSAgent-AI
`);
  }

  /**
   * Run the CLI
   */
  async run(): Promise<void> {
    console.log('Welcome to MSAgent Character Creator!');
    let running = true;

    while (running) {
      this.displayMainMenu();
      const choice = await this.prompt('Choose option: ');

      switch (choice) {
        case '1': {
          const builder = await this.createNewCharacter();
          if (builder) {
            await this.editCharacter(builder);
          }
          break;
        }
        case '2': {
          const builder = await this.loadCharacter();
          if (builder) {
            await this.editCharacter(builder);
          }
          break;
        }
        case '3': {
          const builder = await this.quickCreate();
          if (builder) {
            await this.editCharacter(builder);
          }
          break;
        }
        case '4':
          console.log('\n--- Standard Animation Types ---\n');
          getStandardAnimationTypes().forEach(type => console.log(`  - ${type}`));
          break;
        case '5':
          this.displayHelp();
          break;
        case '6':
          running = false;
          break;
        default:
          console.log('Invalid option. Please try again.');
      }
    }

    console.log('\nGoodbye!');
    this.rl.close();
  }
}

// Run CLI if this is the main module
const cli = new MSAgentCLI();
cli.run().catch(console.error);
