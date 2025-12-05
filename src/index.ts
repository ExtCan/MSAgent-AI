/**
 * MSAgent-AI - Tool for creating MSAgent characters
 * 
 * This library provides utilities for creating, editing, and exporting
 * Microsoft Agent character definition files.
 */

// Export types
export type {
  MSAgentCharacter,
  Animation,
  Frame,
  FrameBranching,
  Sound,
  BalloonConfig,
  CharacterState,
  CharacterInfo,
  VoiceConfig,
  StandardAnimation,
  ExportFormat,
  CharacterCreationOptions
} from './models/types.js';

// Export builders
export { CharacterBuilder } from './builders/CharacterBuilder.js';
export { AnimationBuilder } from './builders/AnimationBuilder.js';

// Export utilities
export {
  createStandardAnimation,
  createDefaultFrame,
  createFrameSequence,
  createSound,
  calculateAnimationDuration,
  getStandardAnimationTypes,
  isStandardAnimation,
  cloneAnimation,
  mergeAnimations,
  reverseAnimation,
  scaleAnimationTiming
} from './utils/animations.js';

// Export exporter
export {
  CharacterExporter,
  generatePlaceholderImage,
  generateId
} from './export/exporter.js';
export type { ValidationResult } from './export/exporter.js';

import { CharacterBuilder } from './builders/CharacterBuilder.js';
import type { CharacterCreationOptions } from './models/types.js';

/**
 * Quick helper to create a new character
 */
export function createCharacter(
  name: string,
  options?: Partial<CharacterCreationOptions>
): CharacterBuilder {
  return new CharacterBuilder({
    name,
    ...options
  });
}

/**
 * Library version
 */
export const VERSION = '1.0.0';
