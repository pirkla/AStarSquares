using UnityEngine;
using Unity.Mathematics;


namespace AStarSquares
{
    public static class Vector3Extensions
    {
        public static Vector3 Flattened(this Vector3 vector)
        {
            return new Vector3(vector.x, 0f, vector.z);
        }

        public static float DistanceFlat(this Vector3 origin, Vector3 destination)
        {
            return Vector3.Distance(origin.Flattened(), destination.Flattened());
        }

        public static int3 asInt3(this Vector3Int vector) {
            return new int3(vector.x, vector.y, vector.z);
        }

        public static Vector3Int asVector3(this int3 vector) {
            return new Vector3Int(vector.x, vector.y, vector.z);
        }
    }
}
