import type { Animation, Frame, FrameBranching } from '../models/types.js';

/**
 * AnimationBuilder provides a fluent API for creating animations
 */
export class AnimationBuilder {
  private animation: Animation;

  constructor(name: string) {
    this.animation = {
      name,
      frames: [],
      loop: false,
      speed: 1.0
    };
  }

  /**
   * Set animation description
   */
  setDescription(description: string): this {
    this.animation.description = description;
    return this;
  }

  /**
   * Set the return animation (animation to play after this one completes)
   */
  setReturnAnimation(animationName: string): this {
    this.animation.returnAnimation = animationName;
    return this;
  }

  /**
   * Set whether this animation loops
   */
  setLoop(loop: boolean): this {
    this.animation.loop = loop;
    return this;
  }

  /**
   * Set animation speed multiplier
   */
  setSpeed(speed: number): this {
    this.animation.speed = Math.max(0.1, Math.min(10, speed));
    return this;
  }

  /**
   * Add a frame to the animation
   */
  addFrame(frame: Frame): this {
    this.animation.frames.push(frame);
    return this;
  }

  /**
   * Add multiple frames at once
   */
  addFrames(frames: Frame[]): this {
    this.animation.frames.push(...frames);
    return this;
  }

  /**
   * Create and add a frame with the given parameters
   */
  createFrame(
    imageData: string,
    duration: number = 100,
    offsetX: number = 0,
    offsetY: number = 0
  ): this {
    this.animation.frames.push({
      imageData,
      duration,
      offsetX,
      offsetY
    });
    return this;
  }

  /**
   * Add a frame with sound
   */
  createFrameWithSound(
    imageData: string,
    soundId: string,
    duration: number = 100,
    offsetX: number = 0,
    offsetY: number = 0
  ): this {
    this.animation.frames.push({
      imageData,
      duration,
      offsetX,
      offsetY,
      soundId
    });
    return this;
  }

  /**
   * Add branching to the last frame
   */
  addBranchingToLastFrame(branching: FrameBranching): this {
    const lastFrame = this.animation.frames[this.animation.frames.length - 1];
    if (lastFrame) {
      lastFrame.branching = branching;
    }
    return this;
  }

  /**
   * Remove a frame by index
   */
  removeFrame(index: number): this {
    if (index >= 0 && index < this.animation.frames.length) {
      this.animation.frames.splice(index, 1);
    }
    return this;
  }

  /**
   * Reorder a frame
   */
  moveFrame(fromIndex: number, toIndex: number): this {
    if (
      fromIndex >= 0 && fromIndex < this.animation.frames.length &&
      toIndex >= 0 && toIndex < this.animation.frames.length
    ) {
      const frame = this.animation.frames[fromIndex];
      if (frame) {
        this.animation.frames.splice(fromIndex, 1);
        this.animation.frames.splice(toIndex, 0, frame);
      }
    }
    return this;
  }

  /**
   * Duplicate a frame
   */
  duplicateFrame(index: number): this {
    const frame = this.animation.frames[index];
    if (frame) {
      this.animation.frames.splice(index + 1, 0, { ...frame });
    }
    return this;
  }

  /**
   * Set duration for all frames
   */
  setAllFramesDuration(duration: number): this {
    for (const frame of this.animation.frames) {
      frame.duration = duration;
    }
    return this;
  }

  /**
   * Get the total duration of the animation
   */
  getTotalDuration(): number {
    return this.animation.frames.reduce((sum, frame) => sum + frame.duration, 0);
  }

  /**
   * Get frame count
   */
  getFrameCount(): number {
    return this.animation.frames.length;
  }

  /**
   * Clear all frames
   */
  clearFrames(): this {
    this.animation.frames = [];
    return this;
  }

  /**
   * Build and return the animation
   */
  build(): Animation {
    return { ...this.animation, frames: [...this.animation.frames] };
  }

  /**
   * Create a copy of this builder
   */
  clone(): AnimationBuilder {
    const builder = new AnimationBuilder(this.animation.name);
    builder.animation = {
      ...this.animation,
      frames: this.animation.frames.map(f => ({ ...f }))
    };
    return builder;
  }
}
