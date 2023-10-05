using UnityEngine;

namespace MultiAR.Core.Models
{
    public class ActiveMultiUserRoom : MultiUserRoom
    {
        public ActiveMultiUserRoom(MultiUserRoom room, Pose pose, bool colocated)
        {
            this.Boundaries = room.Boundaries;
            this.Color = room.Color;
            this.AnchorId = room.AnchorId;
            this.Name = room.Name;
            this.UserCount = room.UserCount;
            this.UserLimit = room.UserLimit;
            this.TypeId = room.TypeId;
            this.Pose = pose;
            this.Colocated = colocated;
        }

        public Pose Pose { get; }

        public bool Colocated { get; }
    }
}
