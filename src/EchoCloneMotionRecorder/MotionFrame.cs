using System;

namespace EchoCloneMotionRecorder
{
    /// <summary>
    /// Timestamped sample of tracked head, hand, and weapon poses.
    /// This file intentionally has no Unity or Blade & Sorcery references.
    /// The integration layer must document and normalize its coordinate space.
    /// </summary>
    [Serializable]
    public sealed class MotionFrame
    {
        public float TimeSeconds;
        public PoseSample Head = new PoseSample();
        public PoseSample LeftHand = new PoseSample();
        public PoseSample RightHand = new PoseSample();
        public PoseSample Weapon = new PoseSample();
    }

    [Serializable]
    public sealed class PoseSample
    {
        public float PositionX;
        public float PositionY;
        public float PositionZ;

        // Quaternion orientation.
        public float RotationX;
        public float RotationY;
        public float RotationZ;
        public float RotationW = 1f;
    }
}
