using UnityEngine;

namespace MultiAR.Core.Helper
{
    using Shell.Scripts.Room;

    public class MoveRoomOrigin : MonoBehaviour
    {
        [SerializeField] public string originGameObjectName = "RoomOrigin";

        public void MoveInstantly()
        {
            var origin = GameObject.Find(originGameObjectName)?.GetComponent<MultiUserRoomCenter>();
            if (!origin)
            {
                Debug.LogError("Tried to move room origin. Room origin was not found.");
                return;
            }

            origin.Move(new Pose(transform.position, transform.rotation));
        }
    }
}
