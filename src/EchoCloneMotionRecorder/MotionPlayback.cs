using System;

namespace EchoCloneMotionRecorder
{
    /// <summary>
    /// Game-independent playback clock for a recorded sequence.
    /// A game adapter is responsible for applying the sampled pose to a character rig.
    /// </summary>
    public sealed class MotionPlayback
    {
        private MotionRecording recording;
        private float firstFrameTime;

        public bool IsPlaying { get; private set; }
        public bool Loop { get; private set; }
        public float CurrentTimeSeconds { get; private set; }

        /// <summary>Starts playback from the beginning of a non-empty recording.</summary>
        public bool Start(MotionRecording source, bool loop = false)
        {
            Stop();
            if (source == null || source.FrameCount == 0)
                return false;

            recording = source;
            firstFrameTime = source.Frames[0].TimeSeconds;
            Loop = loop;
            CurrentTimeSeconds = 0f;
            IsPlaying = true;
            return true;
        }

        public void Stop()
        {
            IsPlaying = false;
            recording = null;
            firstFrameTime = 0f;
            CurrentTimeSeconds = 0f;
            Loop = false;
        }

        /// <summary>
        /// Advances playback by elapsed seconds. Negative, NaN, and infinite deltas are rejected.
        /// Non-looping playback stops at the final frame; looping playback wraps to the start.
        /// </summary>
        public bool Advance(float deltaSeconds)
        {
            if (!IsPlaying || recording == null || !IsFinite(deltaSeconds) || deltaSeconds < 0f)
                return false;

            float duration = recording.DurationSeconds;
            if (duration <= 0f)
            {
                CurrentTimeSeconds = 0f;
                if (!Loop)
                    IsPlaying = false;
                return true;
            }

            float next = CurrentTimeSeconds + deltaSeconds;
            if (Loop)
            {
                CurrentTimeSeconds = next % duration;
            }
            else if (next >= duration)
            {
                CurrentTimeSeconds = duration;
                IsPlaying = false;
            }
            else
            {
                CurrentTimeSeconds = next;
            }

            return true;
        }

        /// <summary>Samples the current playback position, including the first-frame timestamp offset.</summary>
        public bool TryGetCurrentFrame(out MotionFrame frame)
        {
            frame = null;
            if (recording == null || recording.FrameCount == 0)
                return false;

            return recording.TrySample(firstFrameTime + CurrentTimeSeconds, out frame);
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
