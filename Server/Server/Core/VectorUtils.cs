using network;
using System.Numerics;

namespace Server.Core
{
    public static class VectorUtils
    {
        public static readonly Vector3 Zero = new Vector3(0, 0, 0);
        public static readonly Vector3 One = new Vector3(1, 1, 1);

        public static readonly Vector3 Down = new Vector3(0, -1, 0);
        public static readonly Vector3 Up = new Vector3(0, 1, 0);

        public static readonly Vector3 Forward = new Vector3(0, 0, 1);
        public static readonly Vector3 Back = new Vector3(0, 0, -1);
        public static readonly Vector3 Left = new Vector3(-1, 0, 0);
        public static readonly Vector3 Right = new Vector3(1, 0, 0);

        public static NetworkVector3 ToNetworkVector(this Vector3 vector)
        {
            return new NetworkVector3
            {
                X = vector.X,
                Y = vector.Y,
                Z = vector.Z,
            };
        }

        public static Vector3 ToVector3(this NetworkVector3 networkVector)
        {
            return new Vector3
            {
                X = networkVector.X,
                Y = networkVector.Y,
                Z = networkVector.Z,
            };
        }
    }
}