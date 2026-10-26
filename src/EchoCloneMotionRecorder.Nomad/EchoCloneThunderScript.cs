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
    /// - replays the recorded head path by moving the spawned creature root,
    /// - attempts humanoid head/hand pose retargeting when the spawned rig exposes those bones.
    ///
    /// Bone retargeting is a prototype: it must be tested against Nomad's animation/physics
    /// update order. Held-weapon tracking and hostile combat remain separate unfinished work.
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
        private float recordingClock;
        private Transform cloneHeadBone;
        private Transform cloneLeftHandBone;
        private Transform cloneRightHandBone;
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
                    CacheCloneBones(creature);
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
            elapsedSinceSample = 0f;
            recordingClock = 0f;
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

            // Advance a monotonic clock by the accumulated time since the previous sample.
            // Do not derive timestamps from DurationSeconds, which is a property of stored frames.
            float sampleDelta = elapsedSinceSample;
            elapsedSinceSample = 0f;
            recordingClock += sampleDelta;

            Player player = Player.local;
            if (player == null || player.head == null || player.handLeft == null || player.handRight == null)
                return;

            Transform head = player.head.transform;
            Transform left = player.handLeft.transform;
            Transform right = player.handRight.transform;
            float time = recordingClock;

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
                Debug.LogWarning(LogPrefix + "Dropped a frame because its timestamp or pose was invalid.");
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
            Quaternion rotationDelta = Quaternion.Inverse(recordedStartHeadRotation) * currentHeadRotation;
            Vector3 euler = rotationDelta.eulerAngles;
            echoClone.transform.rotation = cloneStartRotation * Quaternion.Euler(0f, euler.y, 0f);

            // Best-effort humanoid bone retargeting. The NPC root follows the recorded head
            // translation; the hand/head bones follow their recorded offsets in that same frame.
            // Nomad's Animator/ragdoll update order must be verified in headset testing.
            ApplyRecordedBonePose(cloneHeadBone, frame.Head, recordedStartHeadPosition,
                recordedStartHeadRotation, cloneStartPosition, cloneStartRotation);
            ApplyRecordedBonePose(cloneLeftHandBone, frame.LeftHand, recordedStartHeadPosition,
                recordedStartHeadRotation, cloneStartPosition, cloneStartRotation);
            ApplyRecordedBonePose(cloneRightHandBone, frame.RightHand, recordedStartHeadPosition,
                recordedStartHeadRotation, cloneStartPosition, cloneStartRotation);

            if (!playback.IsPlaying)
                Debug.Log(LogPrefix + "Playback finished.");
        }

        private void CacheCloneBones(Creature creature)
        {
            cloneHeadBone = null;
            cloneLeftHandBone = null;
            cloneRightHandBone = null;

            if (creature == null || creature.animator == null || !creature.animator.isHuman)
            {
                Debug.LogWarning(LogPrefix + "Spawned NPC has no humanoid Animator; body retargeting is unavailable.");
                return;
            }

            cloneHeadBone = creature.animator.GetBoneTransform(HumanBodyBones.Head);
            cloneLeftHandBone = creature.animator.GetBoneTransform(HumanBodyBones.LeftHand);
            cloneRightHandBone = creature.animator.GetBoneTransform(HumanBodyBones.RightHand);
            if (cloneHeadBone == null || cloneLeftHandBone == null || cloneRightHandBone == null)
                Debug.LogWarning(LogPrefix + "One or more humanoid bones were not found; only available bones will be retargeted.");
        }

        private static void ApplyRecordedBonePose(
            Transform target,
            PoseSample pose,
            Vector3 recordedOrigin,
            Quaternion recordedOriginRotation,
            Vector3 cloneOrigin,
            Quaternion cloneOriginRotation)
        {
            if (target == null || pose == null)
                return;

            Vector3 recordedPosition = new Vector3(pose.PositionX, pose.PositionY, pose.PositionZ);
            Quaternion recordedRotation = new Quaternion(
                pose.RotationX, pose.RotationY, pose.RotationZ, pose.RotationW);
            Vector3 localOffset = Quaternion.Inverse(recordedOriginRotation) * (recordedPosition - recordedOrigin);
            Quaternion localRotation = Quaternion.Inverse(recordedOriginRotation) * recordedRotation;
            target.position = cloneOrigin + cloneOriginRotation * localOffset;
            target.rotation = cloneOriginRotation * localRotation;
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
