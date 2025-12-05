/**
 * MSAgent Character Type Definitions
 * 
 * These types define the structure of a Microsoft Agent character file.
 * MSAgent characters consist of animations, frames, sound data, and metadata.
 */

/**
 * Represents a single frame of animation data
 */
export interface Frame {
  /** Frame duration in milliseconds */
  duration: number;
  /** Image data as base64 or path to image file */
  imageData: string;
  /** X offset for positioning */
  offsetX: number;
  /** Y offset for positioning */
  offsetY: number;
  /** Optional sound ID to play with this frame */
  soundId?: string;
  /** Branching information for interactive animations */
  branching?: FrameBranching;
}

/**
 * Frame branching allows interactive animations
 */
export interface FrameBranching {
  /** Probability of branching (0-100) */
  probability: number;
  /** Target frame index to branch to */
  targetFrame: number;
}

/**
 * Represents a single animation sequence
 */
export interface Animation {
  /** Unique name for the animation */
  name: string;
  /** Human-readable description */
  description?: string;
  /** Animation frames */
  frames: Frame[];
  /** Return animation name (played after this animation) */
  returnAnimation?: string;
  /** Whether this is a looping animation */
  loop?: boolean;
  /** Animation speed multiplier */
  speed?: number;
}

/**
 * Sound data for character audio
 */
export interface Sound {
  /** Unique sound identifier */
  id: string;
  /** Sound name */
  name: string;
  /** Audio data as base64 or path to audio file */
  audioData: string;
  /** Audio format (wav, mp3, etc.) */
  format: 'wav' | 'mp3' | 'ogg';
}

/**
 * Word balloon configuration for character speech
 */
export interface BalloonConfig {
  /** Number of lines in balloon */
  lines: number;
  /** Characters per line */
  charsPerLine: number;
  /** Font name */
  fontName: string;
  /** Font size in points */
  fontSize: number;
  /** Text color (hex) */
  textColor: string;
  /** Background color (hex) */
  backgroundColor: string;
  /** Border color (hex) */
  borderColor: string;
}

/**
 * Character state information
 */
export interface CharacterState {
  /** State name */
  name: string;
  /** Animations associated with this state */
  animations: string[];
}

/**
 * Standard animation types that MSAgent characters typically support
 */
export type StandardAnimation = 
  | 'Idle1'
  | 'Idle2'
  | 'Idle3'
  | 'Show'
  | 'Hide'
  | 'Speak'
  | 'Wave'
  | 'GestureUp'
  | 'GestureDown'
  | 'GestureLeft'
  | 'GestureRight'
  | 'Think'
  | 'Pleased'
  | 'Sad'
  | 'Surprised'
  | 'Alert'
  | 'Acknowledge'
  | 'Decline'
  | 'Explain'
  | 'Suggest'
  | 'Congratulate'
  | 'Greet'
  | 'Goodbye'
  | 'Blink'
  | 'LookUp'
  | 'LookDown'
  | 'LookLeft'
  | 'LookRight'
  | 'Processing'
  | 'Writing'
  | 'Reading'
  | 'GetAttention'
  | 'Hearing'
  | 'DoMagic1'
  | 'DoMagic2';

/**
 * Character metadata
 */
export interface CharacterInfo {
  /** Character name */
  name: string;
  /** Character description */
  description: string;
  /** Author/creator name */
  author: string;
  /** Version string */
  version: string;
  /** Creation date */
  createdAt: Date;
  /** Last modified date */
  modifiedAt: Date;
  /** Character width in pixels */
  width: number;
  /** Character height in pixels */
  height: number;
  /** Default palette for 256-color mode */
  palette?: string[];
  /** Icon data as base64 */
  iconData?: string;
}

/**
 * Main character definition
 */
export interface MSAgentCharacter {
  /** Character metadata */
  info: CharacterInfo;
  /** All animations for this character */
  animations: Animation[];
  /** Sound effects */
  sounds: Sound[];
  /** Word balloon configuration */
  balloon: BalloonConfig;
  /** Character states */
  states: CharacterState[];
  /** Default animation to play */
  defaultAnimation: string;
  /** Voice configuration */
  voice?: VoiceConfig;
}

/**
 * Voice configuration for text-to-speech
 */
export interface VoiceConfig {
  /** Voice engine (SAPI, etc.) */
  engine: string;
  /** Voice name */
  voiceName: string;
  /** Speech rate */
  rate: number;
  /** Voice pitch */
  pitch: number;
}

/**
 * Export format options
 */
export type ExportFormat = 'json' | 'acs' | 'acf' | 'zip';

/**
 * Character creation options
 */
export interface CharacterCreationOptions {
  /** Character name */
  name: string;
  /** Character description */
  description?: string;
  /** Author name */
  author?: string;
  /** Character dimensions */
  width?: number;
  height?: number;
  /** Whether to include standard animations */
  includeStandardAnimations?: boolean;
}
