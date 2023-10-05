namespace MultiAR.Core.Models
{
    using Photon.Realtime;
    using Services.Interfaces;

    public class NetworkObjectInteraction : IUserObject
    {
        public User User { get; set; }

        public bool IsLocal { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as NetworkObjectInteraction);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((User != null ? User.GetHashCode() : 0) * 397) ^ IsLocal.GetHashCode();
            }
        }

        private bool Equals(NetworkObjectInteraction other)
        {
            return other != null && this.IsLocal == other.IsLocal && this.User.Equals(other.User);
        }

        public static NetworkObjectInteraction From(User user)
        {
            return new NetworkObjectInteraction()
            {
                User = user, IsLocal = user.IsLocal,
            };
        }

        public static NetworkObjectInteraction From(Player player)
        {
            return From(Models.User.FromPhotonPlayer(player));
        }
    }
}
