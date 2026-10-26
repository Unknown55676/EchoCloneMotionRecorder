using EchoCloneMotionRecorder;
using System.Collections;

static class Program
{
    private static int checks;

    static int Main()
    {
        Run("cannot add frames before recording", CannotAddBeforeRecording);
        Run("records and samples midpoint", SamplesMidpoint);
        Run("rejects invalid and non-increasing timestamps", RejectsInvalidFrames);
        Run("rejects missing or invalid weapon poses", RejectsInvalidWeaponPose);
        Run("copies weapon poses safely", CopiesWeaponPose);
        Run("copies frames to prevent caller mutation", CopiesFrames);
        Run("exposes a read-only frame collection", FramesAreReadOnly);
        Run("frame snapshots cannot mutate the recording", FrameSnapshotsAreIsolated);
        Run("duration uses the first frame as its origin", DurationUsesFirstTimestamp);
        Run("clears recording", ClearsRecording);
        return 0;
    }

    static void Run(string name, Action check)
    {
        check();
        checks++;
        Console.WriteLine($"PASS: {name}");
    }

    static void CannotAddBeforeRecording()
    {
        var recording = new MotionRecording();
        Assert(!recording.TryAddFrame(Frame(0, 0)), "frame should be rejected while stopped");
        Assert(recording.FrameCount == 0, "no frame should be stored");
    }

    static void SamplesMidpoint()
    {
        var recording = new MotionRecording();
        recording.Start();
        Assert(recording.TryAddFrame(Frame(0, 0)), "first frame accepted");
        Assert(recording.TryAddFrame(Frame(2, 10)), "second frame accepted");
        recording.Stop();

        Assert(recording.TrySample(1, out var sample), "midpoint sample exists");
        Near(sample.Head.PositionX, 5, "position interpolated");
        Near(sample.Head.RotationW, 1, "rotation normalized");
        Near(sample.Weapon.PositionX, 15, "weapon position interpolated");
        Near(recording.DurationSeconds, 2, "duration reflects last timestamp");
        Assert(!recording.TrySample(float.PositiveInfinity, out _), "infinite sample time rejected");
    }

    static void RejectsInvalidFrames()
    {
        var recording = new MotionRecording();
        recording.Start();
        Assert(!recording.TryAddFrame(Frame(float.NaN, 0)), "NaN timestamp rejected");
        Assert(recording.TryAddFrame(Frame(2, 2)), "valid timestamp accepted");
        Assert(!recording.TryAddFrame(Frame(2, 3)), "duplicate timestamp rejected");
        Assert(!recording.TryAddFrame(Frame(1, 1)), "out-of-order timestamp rejected");
        Assert(recording.FrameCount == 1, "invalid frames are not stored");
    }

    static void RejectsInvalidWeaponPose()
    {
        var recording = new MotionRecording();
        recording.Start();
        var missingWeapon = Frame(0, 0);
        missingWeapon.Weapon = null;
        Assert(!recording.TryAddFrame(missingWeapon), "missing weapon pose rejected");

        var invalidWeapon = Frame(1, 0);
        invalidWeapon.Weapon.PositionY = float.NaN;
        Assert(!recording.TryAddFrame(invalidWeapon), "non-finite weapon pose rejected");
        Assert(recording.FrameCount == 0, "invalid weapon poses are not stored");
    }

    static void CopiesWeaponPose()
    {
        var recording = new MotionRecording();
        recording.Start();
        var original = Frame(0, 4);
        Assert(recording.TryAddFrame(original), "frame accepted");
        original.Weapon.PositionX = 999;
        Assert(recording.TrySample(0, out var sample), "sample exists");
        Near(sample.Weapon.PositionX, 14, "stored weapon pose isolated from input mutation");
        sample.Weapon.PositionX = -999;
        Assert(recording.TrySample(0, out var secondSample), "second sample exists");
        Near(secondSample.Weapon.PositionX, 14, "returned weapon pose isolated from stored data");
    }

    static void CopiesFrames()
    {
        var recording = new MotionRecording();
        recording.Start();
        var original = Frame(0, 4);
        Assert(recording.TryAddFrame(original), "frame accepted");
        original.Head.PositionX = 999;
        Assert(recording.TrySample(0, out var sample), "sample exists");
        Near(sample.Head.PositionX, 4, "stored frame is isolated from caller mutation");
        sample.Head.PositionX = -999;
        Assert(recording.TrySample(0, out var secondSample), "second sample exists");
        Near(secondSample.Head.PositionX, 4, "returned sample is isolated from stored data");
    }

    static void FramesAreReadOnly()
    {
        var recording = new MotionRecording();
        recording.Start();
        Assert(recording.TryAddFrame(Frame(0, 4)), "frame accepted");
        Assert(recording.Frames is IList list && list.IsReadOnly, "public frames collection reports itself read-only");
    }

    static void FrameSnapshotsAreIsolated()
    {
        var recording = new MotionRecording();
        recording.Start();
        Assert(recording.TryAddFrame(Frame(5, 7)), "frame accepted");
        var snapshot = recording.Frames[0];
        snapshot.Head.PositionX = 500;
        snapshot.Weapon.PositionX = 600;
        Assert(recording.TrySample(5, out var sampled), "stored frame remains sampleable");
        Near(sampled.Head.PositionX, 7, "frame collection snapshot cannot mutate head pose");
        Near(sampled.Weapon.PositionX, 17, "frame collection snapshot cannot mutate weapon pose");
    }

    static void DurationUsesFirstTimestamp()
    {
        var recording = new MotionRecording();
        recording.Start();
        Assert(recording.TryAddFrame(Frame(5, 0)), "first frame accepted");
        Near(recording.DurationSeconds, 0, "single frame has zero duration");
        Assert(recording.TryAddFrame(Frame(8, 3)), "second frame accepted");
        Near(recording.DurationSeconds, 3, "duration is last timestamp minus first timestamp");
    }

    static void ClearsRecording()
    {
        var recording = new MotionRecording();
        recording.Start();
        Assert(recording.TryAddFrame(Frame(0, 0)), "frame accepted");
        recording.Clear();
        Assert(!recording.IsRecording, "clear stops recording");
        Assert(recording.FrameCount == 0, "clear removes frames");
        Assert(!recording.TrySample(0, out _), "empty recording cannot be sampled");
    }

    static MotionFrame Frame(float time, float x) => new()
    {
        TimeSeconds = time,
        Head = Pose(x, 0, 0, 1),
        LeftHand = Pose(x, 0, 0, 1),
        RightHand = Pose(x, 0, 0, 1),
        Weapon = Pose(x + 10, 0, 0, 1)
    };

    static PoseSample Pose(float x, float y, float z, float w) => new()
    {
        PositionX = x,
        PositionY = y,
        PositionZ = z,
        RotationW = w
    };

    static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    static void Near(float actual, float expected, string message)
    {
        if (MathF.Abs(actual - expected) > 0.001f)
            throw new InvalidOperationException($"{message}: expected {expected}, got {actual}");
    }
}
