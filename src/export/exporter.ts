import * as fs from 'fs';
import * as path from 'path';
import type { MSAgentCharacter, ExportFormat } from '../models/types.js';

/**
 * Character exporter utilities
 */
export class CharacterExporter {
  /**
   * Export character to JSON file
   */
  static exportToJSON(character: MSAgentCharacter, filePath: string): void {
    const json = JSON.stringify(character, null, 2);
    fs.writeFileSync(filePath, json, 'utf-8');
  }

  /**
   * Export character to JSON string
   */
  static toJSONString(character: MSAgentCharacter): string {
    return JSON.stringify(character, null, 2);
  }

  /**
   * Import character from JSON file
   */
  static importFromJSON(filePath: string): MSAgentCharacter {
    const json = fs.readFileSync(filePath, 'utf-8');
    return CharacterExporter.parseJSON(json);
  }

  /**
   * Parse character from JSON string
   */
  static parseJSON(json: string): MSAgentCharacter {
    const data = JSON.parse(json) as MSAgentCharacter;
    
    // Convert date strings back to Date objects
    data.info.createdAt = new Date(data.info.createdAt);
    data.info.modifiedAt = new Date(data.info.modifiedAt);
    
    return data;
  }

  /**
   * Export character as a package (directory with assets)
   */
  static exportAsPackage(character: MSAgentCharacter, dirPath: string): void {
    // Create directory if it doesn't exist
    if (!fs.existsSync(dirPath)) {
      fs.mkdirSync(dirPath, { recursive: true });
    }

    // Create subdirectories
    const animationsDir = path.join(dirPath, 'animations');
    const soundsDir = path.join(dirPath, 'sounds');
    
    if (!fs.existsSync(animationsDir)) {
      fs.mkdirSync(animationsDir, { recursive: true });
    }
    if (!fs.existsSync(soundsDir)) {
      fs.mkdirSync(soundsDir, { recursive: true });
    }

    // Export animations metadata
    for (const animation of character.animations) {
      const animPath = path.join(animationsDir, `${animation.name}.json`);
      fs.writeFileSync(animPath, JSON.stringify(animation, null, 2), 'utf-8');
    }

    // Export sounds metadata
    for (const sound of character.sounds) {
      const soundPath = path.join(soundsDir, `${sound.id}.json`);
      fs.writeFileSync(soundPath, JSON.stringify(sound, null, 2), 'utf-8');
    }

    // Export main character file
    const mainFilePath = path.join(dirPath, 'character.json');
    const charData = {
      ...character,
      animations: character.animations.map(a => a.name),
      sounds: character.sounds.map(s => s.id)
    };
    fs.writeFileSync(mainFilePath, JSON.stringify(charData, null, 2), 'utf-8');

    // Create manifest
    const manifest = {
      name: character.info.name,
      version: character.info.version,
      author: character.info.author,
      animationCount: character.animations.length,
      soundCount: character.sounds.length,
      createdAt: character.info.createdAt,
      modifiedAt: character.info.modifiedAt
    };
    const manifestPath = path.join(dirPath, 'manifest.json');
    fs.writeFileSync(manifestPath, JSON.stringify(manifest, null, 2), 'utf-8');
  }

  /**
   * Get export format from file extension
   */
  static getFormatFromExtension(filePath: string): ExportFormat {
    const ext = path.extname(filePath).toLowerCase();
    switch (ext) {
      case '.json':
        return 'json';
      case '.acs':
        return 'acs';
      case '.acf':
        return 'acf';
      case '.zip':
        return 'zip';
      default:
        return 'json';
    }
  }

  /**
   * Validate character data
   */
  static validate(character: MSAgentCharacter): ValidationResult {
    const errors: string[] = [];
    const warnings: string[] = [];

    // Check required fields
    if (!character.info.name) {
      errors.push('Character name is required');
    }

    if (character.animations.length === 0) {
      errors.push('Character must have at least one animation');
    }

    // Check for Idle animation
    const hasIdle = character.animations.some(a => a.name.startsWith('Idle'));
    if (!hasIdle) {
      warnings.push('Character should have an Idle animation');
    }

    // Check for Show/Hide animations
    const hasShow = character.animations.some(a => a.name === 'Show');
    const hasHide = character.animations.some(a => a.name === 'Hide');
    if (!hasShow) {
      warnings.push('Character should have a Show animation');
    }
    if (!hasHide) {
      warnings.push('Character should have a Hide animation');
    }

    // Check default animation exists
    const defaultExists = character.animations.some(
      a => a.name === character.defaultAnimation
    );
    if (!defaultExists) {
      errors.push(`Default animation '${character.defaultAnimation}' does not exist`);
    }

    // Check animations have frames
    for (const animation of character.animations) {
      if (animation.frames.length === 0) {
        warnings.push(`Animation '${animation.name}' has no frames`);
      }
    }

    // Check dimensions
    if (character.info.width <= 0 || character.info.height <= 0) {
      errors.push('Character dimensions must be positive');
    }

    return {
      valid: errors.length === 0,
      errors,
      warnings
    };
  }
}

/**
 * Validation result
 */
export interface ValidationResult {
  valid: boolean;
  errors: string[];
  warnings: string[];
}

/**
 * Generate a simple PNG placeholder image as base64
 */
export function generatePlaceholderImage(
  width: number,
  height: number,
  color: string = '#808080'
): string {
  // This is a placeholder - in a real implementation, you'd use a canvas library
  // For now, return an empty string to indicate placeholder
  return `placeholder:${width}x${height}:${color}`;
}

/**
 * Generate unique ID for resources
 */
export function generateId(prefix: string = ''): string {
  const timestamp = Date.now().toString(36);
  const randomPart = Math.random().toString(36).substring(2, 8);
  return prefix ? `${prefix}_${timestamp}${randomPart}` : `${timestamp}${randomPart}`;
}
