using MultiAR.Core.Helper;
using Photon.Realtime;
using UnityEngine;

namespace MultiAR.Core.Models
{
    public class User
    {
        /// <summary>
        /// Private constructor to prevent accidental user creation.
        /// Please use a static constructing method for this.
        /// </summary>
        private User() { }

        /// <summary>
        /// An Identifier for the current user starting at 1.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// A name that is assigned to the current user.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// An individual color assigned to every user.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// A device type for the user. This includes all devices supported by MultiAR.
        /// </summary>
        public Device DeviceType { get; set; }

        /// <summary>
        /// A field that indicates if the user is co-located to the local user.
        /// This means the user is in the same physical room as the local user.
        /// Note: This will also be true for the local user.
        /// </summary>
        public bool Colocated { get; set; }

        /// <summary>
        /// A field that indicates if the user is the current user on this local device.
        /// </summary>
        public bool IsLocal { get; set; }

        internal static User FromPhotonPlayer(Player player)
        {
            var properties = player.CustomProperties;
            var hasType = properties.TryGetValue("deviceType", out var deviceType);
            var hasColocated = properties.TryGetValue("colocated", out var colocated);

            return new User()
            {
                Id = player.ActorNumber,
                Name = player.NickName,
                DeviceType = hasType ? (Device)deviceType : Device.Unknown,
                Color = ColorHelper.GetColorForId(player.ActorNumber),
                Colocated =  player.IsLocal || hasColocated && (bool)colocated,
                IsLocal = player.IsLocal
            };
        }

        public override bool Equals(object other)
        {
            return Equals(other as User);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Color.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) DeviceType;
                hashCode = (hashCode * 397) ^ Colocated.GetHashCode();
                return hashCode;
            }
        }

        private bool Equals(User other)
        {
            return other != null && this.Id == other.Id && this.Colocated == other.Colocated && this.DeviceType == other.DeviceType;
        }
    }
}
