import { CharacterBuilder } from '../src/builders/CharacterBuilder';
import type { Animation, Frame } from '../src/models/types';

describe('CharacterBuilder', () => {
  describe('constructor', () => {
    it('should create a character with basic options', () => {
      const builder = new CharacterBuilder({
        name: 'TestAgent'
      });
      
      const character = builder.build();
      
      expect(character.info.name).toBe('TestAgent');
      expect(character.info.width).toBe(128);
      expect(character.info.height).toBe(128);
      expect(character.animations.length).toBeGreaterThan(0);
    });

    it('should create a character with custom dimensions', () => {
      const builder = new CharacterBuilder({
        name: 'CustomAgent',
        width: 256,
        height: 256
      });
      
      const character = builder.build();
      
      expect(character.info.width).toBe(256);
      expect(character.info.height).toBe(256);
    });

    it('should include standard animations when requested', () => {
      const builder = new CharacterBuilder({
        name: 'StandardAgent',
        includeStandardAnimations: true
      });
      
      const character = builder.build();
      
      expect(character.animations.some(a => a.name === 'Idle1')).toBe(true);
      expect(character.animations.some(a => a.name === 'Show')).toBe(true);
      expect(character.animations.some(a => a.name === 'Hide')).toBe(true);
    });
  });

  describe('setName', () => {
    it('should update the character name', () => {
      const builder = new CharacterBuilder({ name: 'Original' });
      builder.setName('Updated');
      
      expect(builder.getName()).toBe('Updated');
      expect(builder.build().info.name).toBe('Updated');
    });
  });

  describe('setDescription', () => {
    it('should update the character description', () => {
      const builder = new CharacterBuilder({ name: 'Test' });
      builder.setDescription('A test character');
      
      expect(builder.build().info.description).toBe('A test character');
    });
  });

  describe('setAuthor', () => {
    it('should update the author', () => {
      const builder = new CharacterBuilder({ name: 'Test' });
      builder.setAuthor('Test Author');
      
      expect(builder.build().info.author).toBe('Test Author');
    });
  });

  describe('addAnimation', () => {
    it('should add a new animation', () => {
      const builder = new CharacterBuilder({ name: 'Test' });
      const animation: Animation = {
        name: 'CustomAnimation',
        frames: [{ duration: 100, imageData: '', offsetX: 0, offsetY: 0 }]
      };
      
      builder.addAnimation(animation);
      
      expect(builder.getAnimation('CustomAnimation')).toBeDefined();
    });

    it('should replace existing animation with same name', () => {
      const builder = new CharacterBuilder({ name: 'Test' });
      
      const animation1: Animation = {
        name: 'Test',
        frames: [{ duration: 100, imageData: 'data1', offsetX: 0, offsetY: 0 }]
      };
      const animation2: Animation = {
        name: 'Test',
        frames: [{ duration: 200, imageData: 'data2', offsetX: 0, offsetY: 0 }]
      };
      
      builder.addAnimation(animation1);
      builder.addAnimation(animation2);
      
      const result = builder.getAnimation('Test');
      expect(result?.frames[0]?.duration).toBe(200);
    });
  });

  describe('removeAnimation', () => {
    it('should remove an animation by name', () => {
      const builder = new CharacterBuilder({
        name: 'Test',
        includeStandardAnimations: true
      });
      
      expect(builder.getAnimation('Wave')).toBeDefined();
      
      builder.removeAnimation('Wave');
      
      expect(builder.getAnimation('Wave')).toBeUndefined();
    });
  });

  describe('addSound', () => {
    it('should add a sound', () => {
      const builder = new CharacterBuilder({ name: 'Test' });
      
      builder.addSound({
        id: 'sound1',
        name: 'Test Sound',
        audioData: 'base64data',
        format: 'wav'
      });
      
      const character = builder.build();
      expect(character.sounds).toHaveLength(1);
      expect(character.sounds[0]?.id).toBe('sound1');
    });
  });

  describe('removeSound', () => {
    it('should remove a sound by id', () => {
      const builder = new CharacterBuilder({ name: 'Test' });
      
      builder.addSound({
        id: 'sound1',
        name: 'Test Sound',
        audioData: 'base64data',
        format: 'wav'
      });
      
      builder.removeSound('sound1');
      
      const character = builder.build();
      expect(character.sounds).toHaveLength(0);
    });
  });

  describe('setBalloonConfig', () => {
    it('should update balloon configuration', () => {
      const builder = new CharacterBuilder({ name: 'Test' });
      
      builder.setBalloonConfig({
        fontName: 'Comic Sans MS',
        fontSize: 14
      });
      
      const character = builder.build();
      expect(character.balloon.fontName).toBe('Comic Sans MS');
      expect(character.balloon.fontSize).toBe(14);
    });
  });

  describe('setDefaultAnimation', () => {
    it('should set default animation', () => {
      const builder = new CharacterBuilder({
        name: 'Test',
        includeStandardAnimations: true
      });
      
      builder.setDefaultAnimation('Wave');
      
      expect(builder.build().defaultAnimation).toBe('Wave');
    });

    it('should not set default if animation does not exist', () => {
      const builder = new CharacterBuilder({ name: 'Test' });
      const originalDefault = builder.build().defaultAnimation;
      
      builder.setDefaultAnimation('NonExistent');
      
      expect(builder.build().defaultAnimation).toBe(originalDefault);
    });
  });

  describe('toJSON / fromJSON', () => {
    it('should serialize and deserialize correctly', () => {
      const builder = new CharacterBuilder({
        name: 'TestAgent',
        description: 'Test description',
        author: 'Test Author',
        width: 200,
        height: 200,
        includeStandardAnimations: true
      });
      
      const json = builder.toJSON();
      const restored = CharacterBuilder.fromJSON(json);
      
      expect(restored.getName()).toBe('TestAgent');
      const restoredChar = restored.build();
      expect(restoredChar.info.description).toBe('Test description');
      expect(restoredChar.info.author).toBe('Test Author');
      expect(restoredChar.info.width).toBe(200);
      expect(restoredChar.info.height).toBe(200);
    });
  });

  describe('build', () => {
    it('should ensure at least one animation exists', () => {
      const builder = new CharacterBuilder({ name: 'Test' });
      // Remove all animations
      const character = builder.build();
      for (const anim of character.animations) {
        builder.removeAnimation(anim.name);
      }
      
      // Build should add default Idle1
      const rebuilt = builder.build();
      expect(rebuilt.animations.length).toBeGreaterThan(0);
    });
  });
});
