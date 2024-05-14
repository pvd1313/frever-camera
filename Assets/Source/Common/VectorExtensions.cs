using UnityEngine;

namespace Source.Common
{
    public static class VectorExtensions
    {
        public static Vector3 XY0(this Vector2 vector)
        {
            return new Vector3(vector.x, vector.y, 0);
        }
    }
}