using Microsoft.MixedReality.Toolkit.Utilities;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace MultiAR.Shell.Scripts.Room
{
    public class RoomSounds : MonoBehaviourPunCallbacks
    {
        public AudioClip userJoined;
        public AudioClip userLeft;

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            AudioSource.PlayClipAtPoint(userJoined, CameraCache.Main.transform.position);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            AudioSource.PlayClipAtPoint(userLeft, CameraCache.Main.transform.position);
        }
    }
}
