import type {
  MSAgentCharacter,
  Animation,
  Frame,
  Sound,
  BalloonConfig,
  CharacterState,
  CharacterInfo,
  CharacterCreationOptions,
  VoiceConfig,
  StandardAnimation
} from '../models/types.js';

/**
 * Default balloon configuration
 */
const DEFAULT_BALLOON: BalloonConfig = {
  lines: 4,
  charsPerLine: 40,
  fontName: 'Arial',
  fontSize: 12,
  textColor: '#000000',
  backgroundColor: '#FFFFCC',
  borderColor: '#000000'
};

/**
 * Standard animation names that MSAgent characters typically support
 */
const STANDARD_ANIMATIONS: StandardAnimation[] = [
  'Idle1',
  'Show',
  'Hide',
  'Speak',
  'Wave',
  'GestureUp',
  'GestureDown',
  'GestureLeft',
  'GestureRight',
  'Think',
  'Pleased',
  'Sad',
  'Surprised',
  'Alert',
  'Blink'
];

/**
 * CharacterBuilder provides a fluent API for creating MSAgent characters
 */
export class CharacterBuilder {
  private character: MSAgentCharacter;

  constructor(options: CharacterCreationOptions) {
    const now = new Date();
    
    this.character = {
      info: {
        name: options.name,
        description: options.description ?? '',
        author: options.author ?? 'Unknown',
        version: '1.0.0',
        createdAt: now,
        modifiedAt: now,
        width: options.width ?? 128,
        height: options.height ?? 128
      },
      animations: [],
      sounds: [],
      balloon: { ...DEFAULT_BALLOON },
      states: [],
      defaultAnimation: 'Idle1'
    };

    if (options.includeStandardAnimations) {
      this.addStandardAnimations();
    }
  }

  /**
   * Add standard placeholder animations
   */
  private addStandardAnimations(): void {
    for (const animName of STANDARD_ANIMATIONS) {
      this.addAnimation({
        name: animName,
        description: `Standard ${animName} animation`,
        frames: [this.createPlaceholderFrame()],
        loop: animName.startsWith('Idle')
      });
    }
  }

  /**
   * Create a placeholder frame for animations
   */
  private createPlaceholderFrame(): Frame {
    return {
      duration: 100,
      imageData: '',
      offsetX: 0,
      offsetY: 0
    };
  }

  /**
   * Get the character name
   */
  getName(): string {
    return this.character.info.name;
  }

  /**
   * Set character name
   */
  setName(name: string): this {
    this.character.info.name = name;
    this.character.info.modifiedAt = new Date();
    return this;
  }

  /**
   * Set character description
   */
  setDescription(description: string): this {
    this.character.info.description = description;
    this.character.info.modifiedAt = new Date();
    return this;
  }

  /**
   * Set character author
   */
  setAuthor(author: string): this {
    this.character.info.author = author;
    this.character.info.modifiedAt = new Date();
    return this;
  }

  /**
   * Set character dimensions
   */
  setDimensions(width: number, height: number): this {
    this.character.info.width = width;
    this.character.info.height = height;
    this.character.info.modifiedAt = new Date();
    return this;
  }

  /**
   * Set character icon
   */
  setIcon(iconData: string): this {
    this.character.info.iconData = iconData;
    this.character.info.modifiedAt = new Date();
    return this;
  }

  /**
   * Add an animation to the character
   */
  addAnimation(animation: Animation): this {
    const existingIndex = this.character.animations.findIndex(
      a => a.name === animation.name
    );
    
    if (existingIndex >= 0) {
      this.character.animations[existingIndex] = animation;
    } else {
      this.character.animations.push(animation);
    }
    
    this.character.info.modifiedAt = new Date();
    return this;
  }

  /**
   * Remove an animation by name
   */
  removeAnimation(name: string): this {
    this.character.animations = this.character.animations.filter(
      a => a.name !== name
    );
    this.character.info.modifiedAt = new Date();
    return this;
  }

  /**
   * Get an animation by name
   */
  getAnimation(name: string): Animation | undefined {
    return this.character.animations.find(a => a.name === name);
  }

  /**
   * Add a frame to an animation
   */
  addFrameToAnimation(animationName: string, frame: Frame): this {
    const animation = this.getAnimation(animationName);
    if (animation) {
      animation.frames.push(frame);
      this.character.info.modifiedAt = new Date();
    }
    return this;
  }

  /**
   * Add a sound to the character
   */
  addSound(sound: Sound): this {
    const existingIndex = this.character.sounds.findIndex(
      s => s.id === sound.id
    );
    
    if (existingIndex >= 0) {
      this.character.sounds[existingIndex] = sound;
    } else {
      this.character.sounds.push(sound);
    }
    
    this.character.info.modifiedAt = new Date();
    return this;
  }

  /**
   * Remove a sound by ID
   */
  removeSound(id: string): this {
    this.character.sounds = this.character.sounds.filter(s => s.id !== id);
    this.character.info.modifiedAt = new Date();
    return this;
  }

  /**
   * Configure the word balloon
   */
  setBalloonConfig(config: Partial<BalloonConfig>): this {
    this.character.balloon = { ...this.character.balloon, ...config };
    this.character.info.modifiedAt = new Date();
    return this;
  }

  /**
   * Add a character state
   */
  addState(state: CharacterState): this {
    const existingIndex = this.character.states.findIndex(
      s => s.name === state.name
    );
    
    if (existingIndex >= 0) {
      this.character.states[existingIndex] = state;
    } else {
      this.character.states.push(state);
    }
    
    this.character.info.modifiedAt = new Date();
    return this;
  }

  /**
   * Remove a state by name
   */
  removeState(name: string): this {
    this.character.states = this.character.states.filter(s => s.name !== name);
    this.character.info.modifiedAt = new Date();
    return this;
  }

  /**
   * Set the default animation
   */
  setDefaultAnimation(name: string): this {
    if (this.getAnimation(name)) {
      this.character.defaultAnimation = name;
      this.character.info.modifiedAt = new Date();
    }
    return this;
  }

  /**
   * Configure voice settings
   */
  setVoice(config: VoiceConfig): this {
    this.character.voice = config;
    this.character.info.modifiedAt = new Date();
    return this;
  }

  /**
   * Set version string
   */
  setVersion(version: string): this {
    this.character.info.version = version;
    this.character.info.modifiedAt = new Date();
    return this;
  }

  /**
   * Build and return the complete character
   */
  build(): MSAgentCharacter {
    // Ensure at least one animation exists
    if (this.character.animations.length === 0) {
      this.addAnimation({
        name: 'Idle1',
        description: 'Default idle animation',
        frames: [this.createPlaceholderFrame()],
        loop: true
      });
    }

    // Ensure default animation exists
    if (!this.getAnimation(this.character.defaultAnimation)) {
      const firstAnimation = this.character.animations[0];
      if (firstAnimation) {
        this.character.defaultAnimation = firstAnimation.name;
      }
    }

    return { ...this.character };
  }

  /**
   * Export character to JSON
   */
  toJSON(): string {
    return JSON.stringify(this.build(), null, 2);
  }

  /**
   * Load character from JSON
   */
  static fromJSON(json: string): CharacterBuilder {
    const data = JSON.parse(json) as MSAgentCharacter;
    const builder = new CharacterBuilder({
      name: data.info.name,
      description: data.info.description,
      author: data.info.author,
      width: data.info.width,
      height: data.info.height
    });

    // Restore all properties
    builder.character = {
      ...data,
      info: {
        ...data.info,
        createdAt: new Date(data.info.createdAt),
        modifiedAt: new Date(data.info.modifiedAt)
      }
    };

    return builder;
  }
}
