public static class VectorExtensions
{
    public static System.Numerics.Vector2 ToSystem2(this UnityEngine.Vector2 v) => new System.Numerics.Vector2(v.x, v.y);
    public static System.Numerics.Vector2 ToSystem2(this UnityEngine.Vector3 v) => new System.Numerics.Vector2(v.x, v.y);
    public static System.Numerics.Vector3 ToSystem3(this UnityEngine.Vector3 v) => new System.Numerics.Vector3(v.x, v.y, v.z);

    public static UnityEngine.Vector2 ToUnity2(this System.Numerics.Vector2 v) => new UnityEngine.Vector2(v.X, v.Y);
    public static UnityEngine.Vector2 ToUnity2(this System.Numerics.Vector3 v) => new UnityEngine.Vector2(v.X, v.Y);
    public static UnityEngine.Vector3 ToUnity3(this System.Numerics.Vector3 v) => new UnityEngine.Vector3(v.X, v.Y, v.Z);
}