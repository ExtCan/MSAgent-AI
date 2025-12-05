import {
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
} from '../src/utils/animations';
import type { Animation, Frame } from '../src/models/types';

describe('Animation Utilities', () => {
  describe('createStandardAnimation', () => {
    it('should create an Idle1 animation with loop=true', () => {
      const animation = createStandardAnimation('Idle1');

      expect(animation.name).toBe('Idle1');
      expect(animation.loop).toBe(true);
      expect(animation.frames).toHaveLength(1);
    });

    it('should create a Show animation with loop=false', () => {
      const animation = createStandardAnimation('Show');

      expect(animation.name).toBe('Show');
      expect(animation.loop).toBe(false);
    });

    it('should use provided frames', () => {
      const customFrames: Frame[] = [
        { duration: 100, imageData: 'data1', offsetX: 0, offsetY: 0 },
        { duration: 100, imageData: 'data2', offsetX: 0, offsetY: 0 }
      ];
      
      const animation = createStandardAnimation('Wave', customFrames);

      expect(animation.frames).toHaveLength(2);
      expect(animation.frames[0]?.imageData).toBe('data1');
    });
  });

  describe('createDefaultFrame', () => {
    it('should create a frame with default duration', () => {
      const frame = createDefaultFrame();

      expect(frame.duration).toBe(100);
      expect(frame.imageData).toBe('');
      expect(frame.offsetX).toBe(0);
      expect(frame.offsetY).toBe(0);
    });

    it('should create a frame with custom duration', () => {
      const frame = createDefaultFrame(200);

      expect(frame.duration).toBe(200);
    });
  });

  describe('createFrameSequence', () => {
    it('should create frames from image array', () => {
      const images = ['img1', 'img2', 'img3'];
      
      const frames = createFrameSequence(images);

      expect(frames).toHaveLength(3);
      expect(frames[0]?.imageData).toBe('img1');
      expect(frames[1]?.imageData).toBe('img2');
      expect(frames[2]?.imageData).toBe('img3');
    });

    it('should use custom duration', () => {
      const images = ['img1', 'img2'];
      
      const frames = createFrameSequence(images, 150);

      expect(frames[0]?.duration).toBe(150);
      expect(frames[1]?.duration).toBe(150);
    });
  });

  describe('createSound', () => {
    it('should create a sound object', () => {
      const sound = createSound('id1', 'Test Sound', 'audioData', 'wav');

      expect(sound.id).toBe('id1');
      expect(sound.name).toBe('Test Sound');
      expect(sound.audioData).toBe('audioData');
      expect(sound.format).toBe('wav');
    });

    it('should default to wav format', () => {
      const sound = createSound('id1', 'Test', 'data');

      expect(sound.format).toBe('wav');
    });
  });

  describe('calculateAnimationDuration', () => {
    it('should calculate total duration of animation', () => {
      const animation: Animation = {
        name: 'Test',
        frames: [
          { duration: 100, imageData: '', offsetX: 0, offsetY: 0 },
          { duration: 200, imageData: '', offsetX: 0, offsetY: 0 },
          { duration: 150, imageData: '', offsetX: 0, offsetY: 0 }
        ]
      };

      expect(calculateAnimationDuration(animation)).toBe(450);
    });

    it('should return 0 for empty frames', () => {
      const animation: Animation = { name: 'Test', frames: [] };

      expect(calculateAnimationDuration(animation)).toBe(0);
    });
  });

  describe('getStandardAnimationTypes', () => {
    it('should return array of standard animation names', () => {
      const types = getStandardAnimationTypes();

      expect(types).toContain('Idle1');
      expect(types).toContain('Show');
      expect(types).toContain('Hide');
      expect(types).toContain('Wave');
      expect(types).toContain('Speak');
    });
  });

  describe('isStandardAnimation', () => {
    it('should return true for standard animation names', () => {
      expect(isStandardAnimation('Idle1')).toBe(true);
      expect(isStandardAnimation('Wave')).toBe(true);
      expect(isStandardAnimation('Speak')).toBe(true);
    });

    it('should return false for non-standard names', () => {
      expect(isStandardAnimation('CustomAnimation')).toBe(false);
      expect(isStandardAnimation('MyIdle')).toBe(false);
    });
  });

  describe('cloneAnimation', () => {
    it('should create independent copy of animation', () => {
      const original: Animation = {
        name: 'Test',
        frames: [
          { duration: 100, imageData: 'data', offsetX: 0, offsetY: 0 }
        ]
      };

      const clone = cloneAnimation(original);
      clone.frames[0]!.duration = 200;

      expect(original.frames[0]?.duration).toBe(100);
      expect(clone.frames[0]?.duration).toBe(200);
    });
  });

  describe('mergeAnimations', () => {
    it('should combine frames from two animations', () => {
      const first: Animation = {
        name: 'First',
        frames: [{ duration: 100, imageData: 'a', offsetX: 0, offsetY: 0 }]
      };
      const second: Animation = {
        name: 'Second',
        frames: [{ duration: 100, imageData: 'b', offsetX: 0, offsetY: 0 }]
      };

      const merged = mergeAnimations(first, second);

      expect(merged.frames).toHaveLength(2);
      expect(merged.name).toBe('First');
    });
  });

  describe('reverseAnimation', () => {
    it('should reverse frame order', () => {
      const animation: Animation = {
        name: 'Test',
        frames: [
          { duration: 100, imageData: 'a', offsetX: 0, offsetY: 0 },
          { duration: 100, imageData: 'b', offsetX: 0, offsetY: 0 },
          { duration: 100, imageData: 'c', offsetX: 0, offsetY: 0 }
        ]
      };

      const reversed = reverseAnimation(animation);

      expect(reversed.frames[0]?.imageData).toBe('c');
      expect(reversed.frames[1]?.imageData).toBe('b');
      expect(reversed.frames[2]?.imageData).toBe('a');
    });

    it('should not modify original animation', () => {
      const animation: Animation = {
        name: 'Test',
        frames: [
          { duration: 100, imageData: 'a', offsetX: 0, offsetY: 0 },
          { duration: 100, imageData: 'b', offsetX: 0, offsetY: 0 }
        ]
      };

      reverseAnimation(animation);

      expect(animation.frames[0]?.imageData).toBe('a');
    });
  });

  describe('scaleAnimationTiming', () => {
    it('should scale frame durations', () => {
      const animation: Animation = {
        name: 'Test',
        frames: [
          { duration: 100, imageData: '', offsetX: 0, offsetY: 0 },
          { duration: 200, imageData: '', offsetX: 0, offsetY: 0 }
        ]
      };

      const scaled = scaleAnimationTiming(animation, 0.5);

      expect(scaled.frames[0]?.duration).toBe(50);
      expect(scaled.frames[1]?.duration).toBe(100);
    });

    it('should round durations', () => {
      const animation: Animation = {
        name: 'Test',
        frames: [
          { duration: 100, imageData: '', offsetX: 0, offsetY: 0 }
        ]
      };

      const scaled = scaleAnimationTiming(animation, 0.33);

      expect(scaled.frames[0]?.duration).toBe(33);
    });
  });
});
