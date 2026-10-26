// Build-only API contract for EchoClone's compile check.
// This assembly is NOT shipped. It intentionally provides only the stable Unity
// surface used by the plugin; the real game assembly must supply runtime types.
namespace UnityEngine
{
    public class Object
    {
    }

    public class Component : Object
    {
        public Transform transform { get; }
    }

    public class Behaviour : Component
    {
    }

    public class MonoBehaviour : Behaviour
    {
    }

    public class Transform : Component
    {
        public Vector3 position { get; set; }
        public Quaternion rotation { get; set; }
        public Vector3 forward { get; }
        public Vector3 eulerAngles { get { return default(Vector3); } }
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float sqrMagnitude { get { return 0f; } }
        public void Normalize() { }
        public static Vector3 forward { get; }

        public static Vector3 operator +(Vector3 left, Vector3 right) { return default(Vector3); }
        public static Vector3 operator -(Vector3 left, Vector3 right) { return default(Vector3); }
        public static Vector3 operator *(Vector3 vector, float scalar) { return default(Vector3); }
        public static Vector3 operator *(float scalar, Vector3 vector) { return default(Vector3); }
    }

    public struct Quaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static Quaternion identity { get; }
        public Vector3 eulerAngles { get { return default(Vector3); } }
        public static Quaternion Euler(float x, float y, float z) { return default(Quaternion); }
        public static Quaternion Inverse(Quaternion rotation) { return default(Quaternion); }
        public static Quaternion operator *(Quaternion left, Quaternion right) { return default(Quaternion); }
        public static Vector3 operator *(Quaternion rotation, Vector3 point) { return default(Vector3); }
    }

    public static class Time
    {
        public static float time { get; }
        public static float deltaTime { get; }
    }

    public static class Debug
    {
        public static void Log(object message) { }
        public static void LogWarning(object message) { }
        public static void LogError(object message) { }
    }
}
