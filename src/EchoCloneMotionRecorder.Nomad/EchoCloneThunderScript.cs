using System;
using System.Reflection;
using System.Linq.Expressions;
using EchoCloneMotionRecorder;
using ThunderRoad;
using UnityEngine;

namespace EchoCloneMotionRecorder.Nomad
{
    /// <summary>
    /// Nomad prototype. The plugin intentionally uses reflection for game-specific members
    /// so its compile does not require shipping proprietary game assemblies or binding to
    /// field/property implementation details. The only compile-time game contract is the
    /// ThunderScript/ModOptions surface. Runtime compatibility still requires Quest testing.
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

        private object echoClone;
        private Transform cloneRoot;
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
        [ModOption("Playback", "Replay recorded head and hand poses with the spawned NPC.", nameof(actionValues), defaultValueIndex = 0)]
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
            Debug.Log(LogPrefix + "Loaded for mod: " + (modData == null ? "(unknown mod)" : modData.Name));
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

            Transform head;
            Transform left;
            Transform right;
            if (!TryGetPlayerTransforms(out head, out left, out right))
            {
                Debug.LogWarning(LogPrefix + "Cannot spawn: player tracking transforms are not available.");
                return;
            }

            Type catalogType = FindGameType("ThunderRoad.Catalog", "Catalog");
            Type creatureDataType = FindGameType("ThunderRoad.CreatureData", "CreatureData");
            if (catalogType == null || creatureDataType == null)
            {
                Debug.LogError(LogPrefix + "Cannot find Catalog/CreatureData in the loaded game assemblies.");
                return;
            }

            object data;
            try
            {
                data = GetCatalogData(catalogType, creatureDataType, HumanCreatureId);
            }
            catch (Exception exception)
            {
                Debug.LogError(LogPrefix + "Creature catalog lookup failed: " + exception);
                return;
            }

            if (data == null)
            {
                Debug.LogError(LogPrefix + "Creature catalog ID '" + HumanCreatureId + "' was not found.");
                return;
            }

            Vector3 forward = head.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.forward;
            forward.Normalize();

            Vector3 position = head.position + forward * SpawnDistance;
            position.y -= EstimatedHeadHeight;
            float rotationY = head.eulerAngles.y;

            isSpawning = true;
            try
            {
                MethodInfo spawnMethod = FindSpawnMethod(data.GetType());
                if (spawnMethod == null)
                {
                    isSpawning = false;
                    Debug.LogError(LogPrefix + "No compatible SpawnAsync overload was found on CreatureData.");
                    return;
                }

                ParameterInfo[] parameters = spawnMethod.GetParameters();
                object[] arguments = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    ParameterInfo parameter = parameters[i];
                    if (i == 0)
                    {
                        arguments[i] = position;
                    }
                    else if (i == 1)
                    {
                        arguments[i] = rotationY;
                    }
                    else if (typeof(Delegate).IsAssignableFrom(parameter.ParameterType))
                    {
                        arguments[i] = CreateSpawnCallback(parameter.ParameterType);
                    }
                    else if (parameter.HasDefaultValue)
                    {
                        arguments[i] = parameter.DefaultValue;
                    }
                    else if (parameter.ParameterType == typeof(bool))
                    {
                        // CreatureData.SpawnAsync's optional pooled argument defaults to true.
                        arguments[i] = true;
                    }
                    else
                    {
                        arguments[i] = GetDefaultValue(parameter.ParameterType);
                    }
                }

                spawnMethod.Invoke(data, arguments);
            }
            catch (Exception exception)
            {
                isSpawning = false;
                Debug.LogError(LogPrefix + "Spawn failed: " + UnwrapException(exception));
            }
        }

        private void OnSpawnCompleted(object creature)
        {
            isSpawning = false;
            if (creature == null)
            {
                Debug.LogError(LogPrefix + "Game failed to spawn the vanilla human creature.");
                return;
            }

            Transform root = GetTransform(creature);
            if (root == null)
            {
                Debug.LogError(LogPrefix + "Spawn callback returned an object without a Transform.");
                return;
            }

            echoClone = creature;
            cloneRoot = root;
            cloneStartPosition = root.position;
            cloneStartRotation = root.rotation;
            CacheCloneBones(creature);
            Debug.Log(LogPrefix + "Spawned vanilla human NPC. Recorded frames: " + recording.FrameCount);
        }

        private void StartRecording()
        {
            Transform head;
            Transform left;
            Transform right;
            if (!TryGetPlayerTransforms(out head, out left, out right))
            {
                Debug.LogWarning(LogPrefix + "Cannot record: player tracking transforms are not ready.");
                return;
            }

            playback.Stop();
            recording.Start();
            elapsedSinceSample = 0f;
            recordingClock = 0f;
            recordedStartHeadPosition = head.position;
            recordedStartHeadRotation = head.rotation;
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

            float sampleDelta = elapsedSinceSample;
            elapsedSinceSample = 0f;
            recordingClock += sampleDelta;

            Transform head;
            Transform left;
            Transform right;
            if (!TryGetPlayerTransforms(out head, out left, out right))
                return;

            MotionFrame frame = new MotionFrame
            {
                TimeSeconds = recordingClock,
                Head = ToPose(head),
                LeftHand = ToPose(left),
                RightHand = ToPose(right),
                // Actual held-weapon capture is still unfinished; this is only a placeholder.
                Weapon = ToPose(right)
            };

            if (!recording.TryAddFrame(frame))
                Debug.LogWarning(LogPrefix + "Dropped a frame because its timestamp or pose was invalid.");
        }

        private void StartPlayback()
        {
            if (echoClone == null || cloneRoot == null)
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
            cloneStartPosition = cloneRoot.position;
            cloneStartRotation = Quaternion.Euler(0f, cloneRoot.eulerAngles.y, 0f);
            Debug.Log(LogPrefix + "Playback started.");
        }

        private void PlaybackUpdate()
        {
            if (echoClone == null || cloneRoot == null)
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
            cloneRoot.position = cloneStartPosition + cloneStartRotation * localOffset;

            Quaternion currentHeadRotation = new Quaternion(
                frame.Head.RotationX, frame.Head.RotationY, frame.Head.RotationZ, frame.Head.RotationW);
            Quaternion rotationDelta = Quaternion.Inverse(recordedStartHeadRotation) * currentHeadRotation;
            Vector3 euler = rotationDelta.eulerAngles;
            cloneRoot.rotation = cloneStartRotation * Quaternion.Euler(0f, euler.y, 0f);

            ApplyRecordedBonePose(cloneHeadBone, frame.Head, recordedStartHeadPosition,
                recordedStartHeadRotation, cloneStartPosition, cloneStartRotation);
            ApplyRecordedBonePose(cloneLeftHandBone, frame.LeftHand, recordedStartHeadPosition,
                recordedStartHeadRotation, cloneStartPosition, cloneStartRotation);
            ApplyRecordedBonePose(cloneRightHandBone, frame.RightHand, recordedStartHeadPosition,
                recordedStartHeadRotation, cloneStartPosition, cloneStartRotation);

            if (!playback.IsPlaying)
                Debug.Log(LogPrefix + "Playback finished.");
        }

        private void CacheCloneBones(object creature)
        {
            cloneHeadBone = null;
            cloneLeftHandBone = null;
            cloneRightHandBone = null;

            object animator = ReadMemberValue(creature, "animator");
            if (animator == null)
            {
                Debug.LogWarning(LogPrefix + "Spawned NPC has no Animator; body retargeting is unavailable.");
                return;
            }

            object isHuman = ReadMemberValue(animator, "isHuman");
            if (isHuman is bool && !(bool)isHuman)
            {
                Debug.LogWarning(LogPrefix + "Spawned NPC Animator is not humanoid; body retargeting is unavailable.");
                return;
            }

            MethodInfo getBoneTransform = FindMethod(animator.GetType(), "GetBoneTransform", 1, false);
            if (getBoneTransform == null || !getBoneTransform.GetParameters()[0].ParameterType.IsEnum)
            {
                Debug.LogWarning(LogPrefix + "Animator.GetBoneTransform was not found; only root motion will replay.");
                return;
            }

            Type boneEnumType = getBoneTransform.GetParameters()[0].ParameterType;
            cloneHeadBone = GetBoneTransform(animator, getBoneTransform, boneEnumType, "Head");
            cloneLeftHandBone = GetBoneTransform(animator, getBoneTransform, boneEnumType, "LeftHand");
            cloneRightHandBone = GetBoneTransform(animator, getBoneTransform, boneEnumType, "RightHand");
            if (cloneHeadBone == null || cloneLeftHandBone == null || cloneRightHandBone == null)
                Debug.LogWarning(LogPrefix + "One or more humanoid bones were not found; available bones only will be retargeted.");
        }

        private static Transform GetBoneTransform(object animator, MethodInfo method, Type enumType, string boneName)
        {
            try
            {
                object bone = Enum.Parse(enumType, boneName);
                return method.Invoke(animator, new[] { bone }) as Transform;
            }
            catch
            {
                return null;
            }
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

        private static bool TryGetPlayerTransforms(out Transform head, out Transform left, out Transform right)
        {
            head = null;
            left = null;
            right = null;

            Type playerType = FindGameType("ThunderRoad.Player", "Player");
            if (playerType == null)
                return false;

            object player = ReadMemberValue(playerType, "local");
            if (player == null)
                return false;

            head = GetTransform(ReadMemberValue(player, "head"));
            left = GetTransform(ReadMemberValue(player, "handLeft"));
            right = GetTransform(ReadMemberValue(player, "handRight"));
            return head != null && left != null && right != null;
        }

        private static object GetCatalogData(Type catalogType, Type creatureDataType, string id)
        {
            MethodInfo getData = null;
            MethodInfo[] methods = catalogType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            for (int i = 0; i < methods.Length; i++)
            {
                ParameterInfo[] parameters = methods[i].GetParameters();
                if (methods[i].Name == "GetData"
                    && methods[i].IsGenericMethodDefinition
                    && parameters.Length >= 1
                    && parameters[0].ParameterType == typeof(string))
                {
                    getData = methods[i];
                    break;
                }
            }

            if (getData == null)
                throw new MissingMethodException(catalogType.FullName, "GetData<T>(string)");

            ParameterInfo[] getDataParameters = getData.GetParameters();
            object[] arguments = new object[getDataParameters.Length];
            arguments[0] = id;
            for (int i = 1; i < arguments.Length; i++)
            {
                arguments[i] = getDataParameters[i].HasDefaultValue
                    ? getDataParameters[i].DefaultValue
                    : GetDefaultValue(getDataParameters[i].ParameterType);
            }

            return getData.MakeGenericMethod(creatureDataType).Invoke(null, arguments);
        }

        private Delegate CreateSpawnCallback(Type delegateType)
        {
            Delegate covariantCallback = new Action<object>(OnSpawnCompleted);
            if (delegateType.IsInstanceOfType(covariantCallback))
                return covariantCallback;

            MethodInfo invoke = delegateType.GetMethod("Invoke");
            if (invoke == null)
                return null;

            ParameterInfo[] callbackParameters = invoke.GetParameters();
            if (callbackParameters.Length != 1)
                return null;

            ParameterExpression parameter = Expression.Parameter(callbackParameters[0].ParameterType, "spawnedCreature");
            MethodInfo handler = GetType().GetMethod("OnSpawnCompleted", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodCallExpression call = Expression.Call(
                Expression.Constant(this),
                handler,
                Expression.Convert(parameter, typeof(object)));
            return Expression.Lambda(delegateType, call, parameter).Compile();
        }

        private static MethodInfo FindSpawnMethod(Type creatureDataType)
        {
            MethodInfo selected = null;
            MethodInfo[] methods = creatureDataType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < methods.Length; i++)
            {
                if (methods[i].Name != "SpawnAsync")
                    continue;

                ParameterInfo[] parameters = methods[i].GetParameters();
                if (parameters.Length < 3
                    || parameters[0].ParameterType != typeof(Vector3)
                    || parameters[1].ParameterType != typeof(float)
                    || !typeof(Delegate).IsAssignableFrom(parameters[parameters.Length - 1].ParameterType))
                    continue;

                if (selected == null || parameters.Length > selected.GetParameters().Length)
                    selected = methods[i];
            }

            return selected;
        }

        private static MethodInfo FindMethod(Type type, string name, int parameterCount, bool isStatic)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic
                | (isStatic ? BindingFlags.Static : BindingFlags.Instance);
            MethodInfo[] methods = type.GetMethods(flags);
            for (int i = 0; i < methods.Length; i++)
            {
                if (methods[i].Name == name && methods[i].GetParameters().Length == parameterCount)
                    return methods[i];
            }
            return null;
        }

        private static object GetDefaultValue(Type type)
        {
            if (type == null || !type.IsValueType)
                return null;
            return Activator.CreateInstance(type);
        }

        private static Exception UnwrapException(Exception exception)
        {
            TargetInvocationException invocation = exception as TargetInvocationException;
            return invocation != null && invocation.InnerException != null
                ? invocation.InnerException
                : exception;
        }

        private static Transform GetTransform(object component)
        {
            if (component == null)
                return null;
            Transform directTransform = component as Transform;
            if (directTransform != null)
                return directTransform;
            return ReadMemberValue(component, "transform") as Transform;
        }

        private static Type FindGameType(string fullName, string simpleName)
        {
            Type result = Type.GetType(fullName + ", ThunderRoad", false);
            if (result != null)
                return result;

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                try
                {
                    result = assemblies[i].GetType(fullName, false);
                    if (result != null)
                        return result;
                }
                catch
                {
                    // A single uninspectable assembly should not prevent trying the others.
                }
            }

            for (int i = 0; i < assemblies.Length; i++)
            {
                try
                {
                    Type[] types = assemblies[i].GetTypes();
                    for (int j = 0; j < types.Length; j++)
                    {
                        if (types[j].Name == simpleName
                            && (types[j].Namespace == "ThunderRoad" || types[j].Namespace == null))
                            return types[j];
                    }
                }
                catch (ReflectionTypeLoadException exception)
                {
                    Type[] types = exception.Types;
                    for (int j = 0; j < types.Length; j++)
                    {
                        if (types[j] != null && types[j].Name == simpleName
                            && (types[j].Namespace == "ThunderRoad" || types[j].Namespace == null))
                            return types[j];
                    }
                }
                catch
                {
                    // Ignore noncritical reflection failures from other assemblies.
                }
            }

            return null;
        }

        private static object ReadMemberValue(object target, string name)
        {
            if (target == null)
                return null;

            bool staticTarget = target is Type;
            Type type = staticTarget ? (Type)target : target.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy
                | (staticTarget ? BindingFlags.Static : BindingFlags.Instance);

            try
            {
                FieldInfo field = type.GetField(name, flags);
                if (field != null)
                    return field.GetValue(staticTarget ? null : target);

                PropertyInfo property = type.GetProperty(name, flags);
                if (property != null)
                {
                    MethodInfo getter = property.GetGetMethod(true);
                    if (getter != null)
                        return property.GetValue(staticTarget ? null : target, null);
                }
            }
            catch
            {
                // Return null when a runtime member is unavailable/inaccessible.
            }

            return null;
        }
    }
}
