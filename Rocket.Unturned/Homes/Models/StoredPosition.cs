using UnityEngine;

namespace Rocket.Unturned.Homes.Models
{
    public sealed class StoredPosition
    {
        public StoredPosition() { }

        public StoredPosition(Vector3 vector)
        {
            X = vector.x;
            Y = vector.y;
            Z = vector.z;
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }
    }
}
