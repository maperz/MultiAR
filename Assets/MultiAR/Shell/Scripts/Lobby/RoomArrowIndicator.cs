using MultiAR.Core.Models;
using UnityEngine;

namespace MultiAR.Shell.Scripts.Lobby
{
    using Core.Behaviours;

    public class RoomArrowIndicator : RoomMonoBehaviour
    {
        public Renderer arrowRenderer;

        protected override void Start()
        {
            base.Start();
            SyncRoomWithParent();
        }

        protected override void OnRoomUpdate(MultiUserRoom room)
        {
            if (room != null && room.Color != null && arrowRenderer != null)
            {
                arrowRenderer.material.color = room.Color.Value;
            }
        }
    }
}
