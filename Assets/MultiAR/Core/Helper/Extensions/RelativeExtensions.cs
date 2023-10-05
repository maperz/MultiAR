namespace MultiAR.Core.Helper.Extensions
{
    using Photon.Pun;
    using UnityEngine;

    public static class RelativeExtensions
    {
        public static Vector3 FromRelative(this Vector3 relative, Transform origin)
        {
            return origin.TransformPoint(relative);
        }

        public static Quaternion FromRelative(this Quaternion relative, Transform origin)
        {
            return origin.rotation * relative;
        }

        public static Vector3 ToRelative(this Vector3 position, Transform origin)
        {
            return origin.InverseTransformPoint(position);
        }

        public static Quaternion ToRelative(this Quaternion rotation, Transform origin)
        {
            return Quaternion.Inverse(origin.rotation) * rotation;
        }

        public static void SendNextRelativePosition(this PhotonStream stream, Vector3 position, Transform origin)
        {
            stream.SendNext(position.ToRelative(origin));
        }

        public static void SendNextRelativeRotation(this PhotonStream stream, Quaternion rotation, Transform origin)
        {
            stream.SendNext(rotation.ToRelative(origin));
        }

        public static Vector3 ReceiveNextRelativePosition(this PhotonStream stream, Transform origin)
        {
            var position = (Vector3)stream.ReceiveNext();
            return position.FromRelative(origin);
        }

        public static Quaternion ReceiveNextRelativeRotation(this PhotonStream stream, Transform origin)
        {
            var quaternion = (Quaternion)stream.ReceiveNext();
            return quaternion.FromRelative(origin);
        }
    }
}
