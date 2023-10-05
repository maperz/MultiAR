using System;
using UnityEngine;

namespace MultiAR.Core.Models
{
    public class MultiUserRoomSettings
    {
        public Pose LocalOrigin { get; set; }

        public Vector3? Boundaries { get; set; }

        public byte? UserLimit { get; set; }

        public TimeSpan? EmptyTimeToLive { get; set; }

        public Color? Color { get; set; }
    }
}
