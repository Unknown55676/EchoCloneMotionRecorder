using System;
using EchoCloneMotionRecorder;
using ThunderRoad;
using UnityEngine;

namespace EchoCloneMotionRecorder.Nomad
{
    /// <summary>
    /// First gameplay integration for Nomad:
    /// - spawns a vanilla human creature,
    /// - records player head and hand world poses,
    /// - replays the recorded head path by moving the spawned creature root.
    ///
    /// This is intentionally not described as full body retargeting: the NPC's arms,
    /// weapon handling, and combat AI are not driven by the recording yet.
    /// </summary>
    public sealed class EchoCloneThunderScript : ThunderScript
    {
        private const string LogPrefix = "[EchoClone] ";
        private const string HumanCreatureId = "HumanMale";
        private const float RecordIntervalSeconds = 0.05f;
        private const float SpawnDistance = 1.8f;
        private const float EstimatedHeadHeight = 1.55f;

        public static EchoCloneThunderScript Instance { get; private set; }

        public static ModOptionString[] actionValues =
        {
            new ModOptionString("Run", "run")
        };

        private readonly MotionRecording recording = new MotionRecording();
        private readonly MotionPlayback playback = new MotionPlayback();

        private Creature echoClone;
        private float elapsedSinceSample;
        private Vector3 recordedStartHeadPosition;
        private Quaternion recordedStartHeadRotation = Quaternion.identity;
        private Vector3 cloneStartPosition;
        private Quaternion cloneStartRotation = Quaternion.identity;
        private bool hostileMode;
        private bool isSpawning;
        private float lastErrorTime = -100f;

        public bool IsRecording => recording.IsRecording;
        public bool IsPlaying => playback.IsPlaying;
        public int RecordedFrameCount => recording.FrameCount;

        [ModOptionCategory("EchoClone", 0)]
        [ModOptionDontSave]
        [ModOption("Spawn EchoClone", "Spawn a vanilla human NPC for playback.", nameof(actionValues), defaultValueIndex = 0)]
        [ModOptionButton]
        public static void SpawnEchoCloneOption(string _)
        {
            Instance?.SpawnEchoClone();
        }

        [ModOptionCategory("EchoClone", 0)]
        [ModOptionDontSave]
        [ModOption("Record Motion", "Record head and both hand poses. Choose Yes to start recording.", nameof(actionValues), defaultValueIndex = 0)]
        [ModOptionButton]
        public static void RecordMotionOption(string _)
        {
            Instance?.StartRecording();
        }

        [ModOptionCategory("EchoClone", 0)]
        [ModOptionDontSave]
        [ModOption("Stop Recording", "Stop the current recording.", nameof(actionValues), defaultValueIndex = 0)]
        [ModOptionButton]
        public static void StopRecordingOption(string _)
        {
            Instance?.StopRecording();
        }

        [ModOptionCategory("EchoClone", 0)]
        [ModOptionDontSave]
        [ModOption("Playback", "Replay the recorded head path with the spawned NPC.", nameof(actionValues), defaultValueIndex = 0)]
        [ModOptionButton]
        public static void PlaybackOption(string _)
        {
            Instance?.StartPlayback();
        }

        [ModOptionCategory("EchoClone", 0)]
        [ModOptionDontSave]
        [ModOption("Hostile Mode", "Prototype flag only; this does not yet change NPC faction or combat AI.", nameof(actionValues), defaultValueIndex = 0)]
        [ModOptionButton]
        public static void HostileModeOption(string _)
        {
            Instance?.ToggleHostileMode();
        }

        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);
            Instance = this;
            Debug.Log(LogPrefix + "Loaded for mod: " + modData.Name);
        }

        public override void ScriptUpdate()
        {
            base.ScriptUpdate();

            try
            {
                if (recording.IsRecording)
                    RecordUpdate();

                if (playback.IsPlaying)
                    PlaybackUpdate();
            }
            catch (Exception exception)
            {
                if (Time.time - lastErrorTime > 5f)
                {
                    lastErrorTime = Time.time;
                    Debug.LogError(LogPrefix + "Update failed: " + exception);
                }
            }
        }

        private void SpawnEchoClone()
        {
            if (isSpawning)
            {
                Debug.LogWarning(LogPrefix + "A clone is already being spawned.");
                return;
            }

            Player player = Player.local;
            if (player == null || player.head == null)
            {
                Debug.LogWarning(LogPrefix + "Cannot spawn: player/head is not ready.");
                return;
            }

            CreatureData data = Catalog.GetData<CreatureData>(HumanCreatureId);
            if (data == null)
            {
                Debug.LogError(LogPrefix + "Creature catalog ID '" + HumanCreatureId + "' was not found.");
                return;
            }

            Vector3 forward = player.head.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.forward;
            forward.Normalize();

            Vector3 position = player.head.transform.position + forward * SpawnDistance;
            position.y -= EstimatedHeadHeight;
            float rotationY = player.head.transform.eulerAngles.y;
            isSpawning = true;

            try
            {
                data.SpawnAsync(position, rotationY, callback: delegate(Creature creature)
                {
                    isSpawning = false;
                    if (creature == null)
                    {
                        Debug.LogError(LogPrefix + "Game failed to spawn the vanilla human creature.");
                        return;
                    }

                    echoClone = creature;
                    cloneStartPosition = creature.transform.position;
                    cloneStartRotation = creature.transform.rotation;
                    Debug.Log(LogPrefix + "Spawned vanilla human NPC. Recorded frames: " + recording.FrameCount);
                });
            }
            catch (Exception exception)
            {
                isSpawning = false;
                Debug.LogError(LogPrefix + "Spawn failed: " + exception);
            }
        }

        private void StartRecording()
        {
            Player player = Player.local;
            if (player == null || player.head == null || player.handLeft == null || player.handRight == null)
            {
                Debug.LogWarning(LogPrefix + "Cannot record: player tracking transforms are not ready.");
                return;
            }

            playback.Stop();
            recording.Start();
            elapsedSinceSample = RecordIntervalSeconds;
            recordedStartHeadPosition = player.head.transform.position;
            recordedStartHeadRotation = player.head.transform.rotation;
            Debug.Log(LogPrefix + "Recording started.");
        }

        private void StopRecording()
        {
            recording.Stop();
            Debug.Log(LogPrefix + "Recording stopped. Frames: " + recording.FrameCount
                + ", duration: " + recording.DurationSeconds.ToString("0.00") + "s");
        }

        private void RecordUpdate()
        {
            elapsedSinceSample += Time.deltaTime;
            if (elapsedSinceSample < RecordIntervalSeconds)
                return;

            // Sample once per update, but keep timestamps tied to elapsed game time.
            float sampleDelta = elapsedSinceSample;
            elapsedSinceSample = 0f;

            Player player = Player.local;
            if (player == null || player.head == null || player.handLeft == null || player.handRight == null)
                return;

            Transform head = player.head.transform;
            Transform left = player.handLeft.transform;
            Transform right = player.handRight.transform;
            float time = recording.FrameCount == 0
                ? 0f
                : recording.DurationSeconds + sampleDelta;

            MotionFrame frame = new MotionFrame
            {
                TimeSeconds = time,
                Head = ToPose(head),
                LeftHand = ToPose(left),
                RightHand = ToPose(right),
                // No reliable held-weapon transform is wired yet; use right hand as a placeholder.
                Weapon = ToPose(right)
            };

            if (!recording.TryAddFrame(frame))
            {
                // DurationSeconds is based on first/last timestamps; use a monotonic
                // clock fallback when a frame was rejected due to a timestamp collision.
                float nextTime = recording.FrameCount == 0 ? 0f : recording.Frames[recording.FrameCount - 1].TimeSeconds + 0.001f;
                frame.TimeSeconds = nextTime;
                recording.TryAddFrame(frame);
            }
        }

        private void StartPlayback()
        {
            if (echoClone == null)
            {
                Debug.LogWarning(LogPrefix + "Spawn EchoClone first.");
                return;
            }

            if (!playback.Start(recording, false))
            {
                Debug.LogWarning(LogPrefix + "No recording to play. Record some motion, then stop recording.");
                return;
            }

            MotionFrame first;
            if (!playback.TryGetCurrentFrame(out first))
            {
                playback.Stop();
                Debug.LogWarning(LogPrefix + "Could not sample the recording.");
                return;
            }

            recordedStartHeadPosition = new Vector3(first.Head.PositionX, first.Head.PositionY, first.Head.PositionZ);
            recordedStartHeadRotation = new Quaternion(first.Head.RotationX, first.Head.RotationY, first.Head.RotationZ, first.Head.RotationW);
            cloneStartPosition = echoClone.transform.position;
            cloneStartRotation = Quaternion.Euler(0f, echoClone.transform.eulerAngles.y, 0f);
            Debug.Log(LogPrefix + "Playback started.");
        }

        private void PlaybackUpdate()
        {
            if (echoClone == null)
            {
                playback.Stop();
                return;
            }

            playback.Advance(Time.deltaTime);
            MotionFrame frame;
            if (!playback.TryGetCurrentFrame(out frame))
                return;

            Vector3 recordedHead = new Vector3(frame.Head.PositionX, frame.Head.PositionY, frame.Head.PositionZ);
            Vector3 relative = recordedHead - recordedStartHeadPosition;
            Quaternion yawOnly = Quaternion.Euler(0f, recordedStartHeadRotation.eulerAngles.y, 0f);
            Vector3 localOffset = Quaternion.Inverse(yawOnly) * relative;
            echoClone.transform.position = cloneStartPosition + cloneStartRotation * localOffset;

            Quaternion currentHeadRotation = new Quaternion(
                frame.Head.RotationX, frame.Head.RotationY, frame.Head.RotationZ, frame.Head.RotationW);
            Quaternion rotationDelta = Quaternion.Inverse(yawOnly) * currentHeadRotation;
            Vector3 euler = rotationDelta.eulerAngles;
            echoClone.transform.rotation = cloneStartRotation * Quaternion.Euler(0f, euler.y, 0f);

            if (!playback.IsPlaying)
                Debug.Log(LogPrefix + "Playback finished.");
        }

        private void ToggleHostileMode()
        {
            hostileMode = !hostileMode;
            Debug.Log(LogPrefix + "Hostile mode flag is now " + hostileMode
                + ". Faction/AI behavior is not yet connected.");
        }

        private static PoseSample ToPose(Transform transform)
        {
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;
            return new PoseSample
            {
                PositionX = position.x,
                PositionY = position.y,
                PositionZ = position.z,
                RotationX = rotation.x,
                RotationY = rotation.y,
                RotationZ = rotation.z,
                RotationW = rotation.w
            };
        }
    }
}
