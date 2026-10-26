using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EchoCloneMotionRecorder
{
    /// <summary>
    /// Stores a sequence of tracked poses. This is game-independent logic only;
    /// a Nomad integration layer must feed it poses from the supported game/XR APIs.
    /// </summary>
    [Serializable]
    public sealed class MotionRecording
    {
        private readonly List<MotionFrame> frames = new List<MotionFrame>();
        private readonly ReadOnlyCollection<MotionFrame> readOnlyFrames;

        public MotionRecording()
        {
            readOnlyFrames = frames.AsReadOnly();
        }

        /// <summary>
        /// Read-only view of recorded frames. Each accepted frame is copied on input;
        /// samples returned by TrySample are also copies.
        /// </summary>
        public IReadOnlyList<MotionFrame> Frames => readOnlyFrames;
        public bool IsRecording { get; private set; }
        public int FrameCount => frames.Count;
        public float DurationSeconds => frames.Count == 0 ? 0f : frames[frames.Count - 1].TimeSeconds;

        public void Start()
        {
            frames.Clear();
            IsRecording = true;
        }

        public void Stop()
        {
            IsRecording = false;
        }

        public bool TryAddFrame(MotionFrame frame)
        {
            if (!IsRecording || frame == null || !IsValid(frame))
                return false;

            // Strictly increasing timestamps avoid ambiguous samples and zero-length intervals.
            if (frames.Count > 0 && frame.TimeSeconds <= frames[frames.Count - 1].TimeSeconds)
                return false;

            frames.Add(CloneFrame(frame));
            return true;
        }

        public void Clear()
        {
            IsRecording = false;
            frames.Clear();
        }

        /// <summary>
        /// Returns an interpolated pose sample between stored frames.
        /// Returns false when there are no frames. Rotation uses normalized
        /// linear interpolation; a game adapter may replace this with quaternion slerp.
        /// </summary>
        public bool TrySample(float timeSeconds, out MotionFrame sample)
        {
            sample = null;
            if (frames.Count == 0 || !IsFinite(timeSeconds))
                return false;

            if (timeSeconds <= frames[0].TimeSeconds)
            {
                sample = CloneFrame(frames[0]);
                return true;
            }

            int last = frames.Count - 1;
            if (timeSeconds >= frames[last].TimeSeconds)
            {
                sample = CloneFrame(frames[last]);
                return true;
            }

            for (int i = 1; i < frames.Count; i++)
            {
                MotionFrame right = frames[i];
                if (timeSeconds > right.TimeSeconds)
                    continue;

                MotionFrame left = frames[i - 1];
                float span = right.TimeSeconds - left.TimeSeconds;
                float t = (timeSeconds - left.TimeSeconds) / span;
                sample = new MotionFrame
                {
                    TimeSeconds = timeSeconds,
                    Head = Interpolate(left.Head, right.Head, t),
                    LeftHand = Interpolate(left.LeftHand, right.LeftHand, t),
                    RightHand = Interpolate(left.RightHand, right.RightHand, t)
                };
                return true;
            }

            return false;
        }

        private static PoseSample Interpolate(PoseSample a, PoseSample b, float t)
        {
            PoseSample p = new PoseSample
            {
                PositionX = Lerp(a.PositionX, b.PositionX, t),
                PositionY = Lerp(a.PositionY, b.PositionY, t),
                PositionZ = Lerp(a.PositionZ, b.PositionZ, t),
                RotationX = Lerp(a.RotationX, b.RotationX, t),
                RotationY = Lerp(a.RotationY, b.RotationY, t),
                RotationZ = Lerp(a.RotationZ, b.RotationZ, t),
                RotationW = Lerp(a.RotationW, b.RotationW, t)
            };

            float magnitude = (float)Math.Sqrt(
                p.RotationX * p.RotationX + p.RotationY * p.RotationY +
                p.RotationZ * p.RotationZ + p.RotationW * p.RotationW);

            if (magnitude > 0.00001f)
            {
                p.RotationX /= magnitude;
                p.RotationY /= magnitude;
                p.RotationZ /= magnitude;
                p.RotationW /= magnitude;
            }
            else
            {
                p.RotationX = 0f;
                p.RotationY = 0f;
                p.RotationZ = 0f;
                p.RotationW = 1f;
            }

            return p;
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;

        private static MotionFrame CloneFrame(MotionFrame source) => new MotionFrame
        {
            TimeSeconds = source.TimeSeconds,
            Head = ClonePose(source.Head),
            LeftHand = ClonePose(source.LeftHand),
            RightHand = ClonePose(source.RightHand)
        };

        private static PoseSample ClonePose(PoseSample source) => new PoseSample
        {
            PositionX = source.PositionX,
            PositionY = source.PositionY,
            PositionZ = source.PositionZ,
            RotationX = source.RotationX,
            RotationY = source.RotationY,
            RotationZ = source.RotationZ,
            RotationW = source.RotationW
        };

        private static bool IsValid(MotionFrame f) =>
            IsFinite(f.TimeSeconds) && f.TimeSeconds >= 0f &&
            IsValid(f.Head) && IsValid(f.LeftHand) && IsValid(f.RightHand);

        private static bool IsValid(PoseSample p) =>
            p != null &&
            IsFinite(p.PositionX) && IsFinite(p.PositionY) && IsFinite(p.PositionZ) &&
            IsFinite(p.RotationX) && IsFinite(p.RotationY) &&
            IsFinite(p.RotationZ) && IsFinite(p.RotationW);

        private static bool IsFinite(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
