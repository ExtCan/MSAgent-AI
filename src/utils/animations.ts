import type { Animation, Frame, Sound, StandardAnimation } from '../models/types.js';

/**
 * Create a standard animation template
 */
export function createStandardAnimation(
  type: StandardAnimation,
  frames: Frame[] = []
): Animation {
  const defaultFrames = frames.length > 0 ? frames : [createDefaultFrame()];
  
  const animationTemplates: Record<StandardAnimation, Partial<Animation>> = {
    Idle1: { loop: true, description: 'Primary idle animation' },
    Idle2: { loop: true, description: 'Secondary idle animation' },
    Idle3: { loop: true, description: 'Tertiary idle animation' },
    Show: { loop: false, description: 'Character appears' },
    Hide: { loop: false, description: 'Character disappears' },
    Speak: { loop: true, description: 'Speaking animation' },
    Wave: { loop: false, description: 'Wave greeting' },
    GestureUp: { loop: false, description: 'Point up gesture' },
    GestureDown: { loop: false, description: 'Point down gesture' },
    GestureLeft: { loop: false, description: 'Point left gesture' },
    GestureRight: { loop: false, description: 'Point right gesture' },
    Think: { loop: true, description: 'Thinking pose' },
    Pleased: { loop: false, description: 'Happy expression' },
    Sad: { loop: false, description: 'Sad expression' },
    Surprised: { loop: false, description: 'Surprised expression' },
    Alert: { loop: false, description: 'Alert/attention' },
    Acknowledge: { loop: false, description: 'Acknowledgment nod' },
    Decline: { loop: false, description: 'Decline/disagree' },
    Explain: { loop: true, description: 'Explaining gesture' },
    Suggest: { loop: false, description: 'Suggesting pose' },
    Congratulate: { loop: false, description: 'Congratulatory gesture' },
    Greet: { loop: false, description: 'Greeting gesture' },
    Goodbye: { loop: false, description: 'Farewell gesture' },
    Blink: { loop: false, description: 'Eye blink' },
    LookUp: { loop: false, description: 'Looking up' },
    LookDown: { loop: false, description: 'Looking down' },
    LookLeft: { loop: false, description: 'Looking left' },
    LookRight: { loop: false, description: 'Looking right' },
    Processing: { loop: true, description: 'Working/processing' },
    Writing: { loop: true, description: 'Writing animation' },
    Reading: { loop: true, description: 'Reading animation' },
    GetAttention: { loop: false, description: 'Getting user attention' },
    Hearing: { loop: true, description: 'Listening animation' },
    DoMagic1: { loop: false, description: 'Magic trick animation 1' },
    DoMagic2: { loop: false, description: 'Magic trick animation 2' }
  };

  const template = animationTemplates[type] ?? {};
  
  return {
    name: type,
    frames: defaultFrames,
    loop: template.loop ?? false,
    description: template.description,
    returnAnimation: template.loop ? undefined : 'Idle1',
    speed: 1.0
  };
}

/**
 * Create a default placeholder frame
 */
export function createDefaultFrame(duration: number = 100): Frame {
  return {
    duration,
    imageData: '',
    offsetX: 0,
    offsetY: 0
  };
}

/**
 * Create multiple frames from an image sequence
 */
export function createFrameSequence(
  images: string[],
  duration: number = 100
): Frame[] {
  return images.map(imageData => ({
    duration,
    imageData,
    offsetX: 0,
    offsetY: 0
  }));
}

/**
 * Create a sound object
 */
export function createSound(
  id: string,
  name: string,
  audioData: string,
  format: 'wav' | 'mp3' | 'ogg' = 'wav'
): Sound {
  return {
    id,
    name,
    audioData,
    format
  };
}

/**
 * Calculate animation duration in milliseconds
 */
export function calculateAnimationDuration(animation: Animation): number {
  return animation.frames.reduce((sum, frame) => sum + frame.duration, 0);
}

/**
 * Get all standard animation types
 */
export function getStandardAnimationTypes(): StandardAnimation[] {
  return [
    'Idle1', 'Idle2', 'Idle3',
    'Show', 'Hide',
    'Speak', 'Wave',
    'GestureUp', 'GestureDown', 'GestureLeft', 'GestureRight',
    'Think', 'Pleased', 'Sad', 'Surprised', 'Alert',
    'Acknowledge', 'Decline', 'Explain', 'Suggest',
    'Congratulate', 'Greet', 'Goodbye',
    'Blink', 'LookUp', 'LookDown', 'LookLeft', 'LookRight',
    'Processing', 'Writing', 'Reading',
    'GetAttention', 'Hearing',
    'DoMagic1', 'DoMagic2'
  ];
}

/**
 * Check if a string is a valid standard animation name
 */
export function isStandardAnimation(name: string): name is StandardAnimation {
  return getStandardAnimationTypes().includes(name as StandardAnimation);
}

/**
 * Deep clone an animation
 */
export function cloneAnimation(animation: Animation): Animation {
  return {
    ...animation,
    frames: animation.frames.map(frame => ({ ...frame }))
  };
}

/**
 * Merge two animations (append frames from second to first)
 */
export function mergeAnimations(first: Animation, second: Animation): Animation {
  return {
    ...first,
    frames: [...first.frames, ...second.frames]
  };
}

/**
 * Reverse animation frames
 */
export function reverseAnimation(animation: Animation): Animation {
  return {
    ...animation,
    frames: [...animation.frames].reverse()
  };
}

/**
 * Scale animation timing
 */
export function scaleAnimationTiming(animation: Animation, factor: number): Animation {
  return {
    ...animation,
    frames: animation.frames.map(frame => ({
      ...frame,
      duration: Math.round(frame.duration * factor)
    }))
  };
}
