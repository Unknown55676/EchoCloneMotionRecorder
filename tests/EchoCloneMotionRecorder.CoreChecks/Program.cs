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
        Run("copies frames to prevent caller mutation", CopiesFrames);
        Run("exposes a read-only frame collection", FramesAreReadOnly);
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
        Assert(recording.Frames is not IList, "public frames collection is not a mutable IList");
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
        RightHand = Pose(x, 0, 0, 1)
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
