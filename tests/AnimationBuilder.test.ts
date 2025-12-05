import { AnimationBuilder } from '../src/builders/AnimationBuilder';

describe('AnimationBuilder', () => {
  describe('constructor', () => {
    it('should create an animation with given name', () => {
      const builder = new AnimationBuilder('TestAnimation');
      const animation = builder.build();
      
      expect(animation.name).toBe('TestAnimation');
      expect(animation.frames).toHaveLength(0);
      expect(animation.loop).toBe(false);
    });
  });

  describe('setDescription', () => {
    it('should set the description', () => {
      const builder = new AnimationBuilder('Test')
        .setDescription('A test animation');
      
      expect(builder.build().description).toBe('A test animation');
    });
  });

  describe('setLoop', () => {
    it('should set loop to true', () => {
      const builder = new AnimationBuilder('Test')
        .setLoop(true);
      
      expect(builder.build().loop).toBe(true);
    });

    it('should set loop to false', () => {
      const builder = new AnimationBuilder('Test')
        .setLoop(true)
        .setLoop(false);
      
      expect(builder.build().loop).toBe(false);
    });
  });

  describe('setSpeed', () => {
    it('should set speed within valid range', () => {
      const builder = new AnimationBuilder('Test')
        .setSpeed(2.0);
      
      expect(builder.build().speed).toBe(2.0);
    });

    it('should clamp speed to minimum', () => {
      const builder = new AnimationBuilder('Test')
        .setSpeed(0.01);
      
      expect(builder.build().speed).toBe(0.1);
    });

    it('should clamp speed to maximum', () => {
      const builder = new AnimationBuilder('Test')
        .setSpeed(100);
      
      expect(builder.build().speed).toBe(10);
    });
  });

  describe('setReturnAnimation', () => {
    it('should set return animation', () => {
      const builder = new AnimationBuilder('Test')
        .setReturnAnimation('Idle1');
      
      expect(builder.build().returnAnimation).toBe('Idle1');
    });
  });

  describe('createFrame', () => {
    it('should add a frame with default values', () => {
      const builder = new AnimationBuilder('Test')
        .createFrame('imageData');
      
      const animation = builder.build();
      expect(animation.frames).toHaveLength(1);
      expect(animation.frames[0]?.imageData).toBe('imageData');
      expect(animation.frames[0]?.duration).toBe(100);
      expect(animation.frames[0]?.offsetX).toBe(0);
      expect(animation.frames[0]?.offsetY).toBe(0);
    });

    it('should add a frame with custom values', () => {
      const builder = new AnimationBuilder('Test')
        .createFrame('imageData', 200, 10, 20);
      
      const animation = builder.build();
      expect(animation.frames[0]?.duration).toBe(200);
      expect(animation.frames[0]?.offsetX).toBe(10);
      expect(animation.frames[0]?.offsetY).toBe(20);
    });
  });

  describe('createFrameWithSound', () => {
    it('should add a frame with sound', () => {
      const builder = new AnimationBuilder('Test')
        .createFrameWithSound('imageData', 'soundId');
      
      const animation = builder.build();
      expect(animation.frames[0]?.soundId).toBe('soundId');
    });
  });

  describe('addFrame', () => {
    it('should add a pre-built frame', () => {
      const frame = {
        duration: 150,
        imageData: 'data',
        offsetX: 5,
        offsetY: 10
      };
      
      const builder = new AnimationBuilder('Test')
        .addFrame(frame);
      
      expect(builder.build().frames[0]).toEqual(frame);
    });
  });

  describe('addFrames', () => {
    it('should add multiple frames', () => {
      const frames = [
        { duration: 100, imageData: 'data1', offsetX: 0, offsetY: 0 },
        { duration: 100, imageData: 'data2', offsetX: 0, offsetY: 0 }
      ];
      
      const builder = new AnimationBuilder('Test')
        .addFrames(frames);
      
      expect(builder.build().frames).toHaveLength(2);
    });
  });

  describe('removeFrame', () => {
    it('should remove a frame by index', () => {
      const builder = new AnimationBuilder('Test')
        .createFrame('data1')
        .createFrame('data2')
        .createFrame('data3')
        .removeFrame(1);
      
      const animation = builder.build();
      expect(animation.frames).toHaveLength(2);
      expect(animation.frames[1]?.imageData).toBe('data3');
    });

    it('should do nothing for invalid index', () => {
      const builder = new AnimationBuilder('Test')
        .createFrame('data1')
        .removeFrame(10);
      
      expect(builder.build().frames).toHaveLength(1);
    });
  });

  describe('moveFrame', () => {
    it('should move a frame to a new position', () => {
      const builder = new AnimationBuilder('Test')
        .createFrame('data1')
        .createFrame('data2')
        .createFrame('data3')
        .moveFrame(0, 2);
      
      const animation = builder.build();
      expect(animation.frames[0]?.imageData).toBe('data2');
      expect(animation.frames[2]?.imageData).toBe('data1');
    });
  });

  describe('duplicateFrame', () => {
    it('should duplicate a frame', () => {
      const builder = new AnimationBuilder('Test')
        .createFrame('data1')
        .duplicateFrame(0);
      
      const animation = builder.build();
      expect(animation.frames).toHaveLength(2);
      expect(animation.frames[1]?.imageData).toBe('data1');
    });
  });

  describe('setAllFramesDuration', () => {
    it('should set duration for all frames', () => {
      const builder = new AnimationBuilder('Test')
        .createFrame('data1', 100)
        .createFrame('data2', 200)
        .setAllFramesDuration(150);
      
      const animation = builder.build();
      expect(animation.frames[0]?.duration).toBe(150);
      expect(animation.frames[1]?.duration).toBe(150);
    });
  });

  describe('getTotalDuration', () => {
    it('should calculate total duration', () => {
      const builder = new AnimationBuilder('Test')
        .createFrame('data1', 100)
        .createFrame('data2', 200)
        .createFrame('data3', 150);
      
      expect(builder.getTotalDuration()).toBe(450);
    });
  });

  describe('getFrameCount', () => {
    it('should return frame count', () => {
      const builder = new AnimationBuilder('Test')
        .createFrame('data1')
        .createFrame('data2');
      
      expect(builder.getFrameCount()).toBe(2);
    });
  });

  describe('clearFrames', () => {
    it('should remove all frames', () => {
      const builder = new AnimationBuilder('Test')
        .createFrame('data1')
        .createFrame('data2')
        .clearFrames();
      
      expect(builder.getFrameCount()).toBe(0);
    });
  });

  describe('addBranchingToLastFrame', () => {
    it('should add branching to the last frame', () => {
      const builder = new AnimationBuilder('Test')
        .createFrame('data1')
        .addBranchingToLastFrame({ probability: 50, targetFrame: 0 });
      
      const animation = builder.build();
      expect(animation.frames[0]?.branching).toEqual({
        probability: 50,
        targetFrame: 0
      });
    });
  });

  describe('clone', () => {
    it('should create an independent copy', () => {
      const original = new AnimationBuilder('Test')
        .createFrame('data1');
      
      const clone = original.clone();
      clone.createFrame('data2');
      
      expect(original.getFrameCount()).toBe(1);
      expect(clone.getFrameCount()).toBe(2);
    });
  });

  describe('fluent API', () => {
    it('should support method chaining', () => {
      const animation = new AnimationBuilder('ComplexAnimation')
        .setDescription('A complex animation')
        .setLoop(true)
        .setSpeed(1.5)
        .createFrame('frame1', 100)
        .createFrame('frame2', 100)
        .createFrame('frame3', 100)
        .setReturnAnimation('Idle1')
        .build();
      
      expect(animation.name).toBe('ComplexAnimation');
      expect(animation.description).toBe('A complex animation');
      expect(animation.loop).toBe(true);
      expect(animation.speed).toBe(1.5);
      expect(animation.frames).toHaveLength(3);
      expect(animation.returnAnimation).toBe('Idle1');
    });
  });
});
