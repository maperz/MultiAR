namespace MultiAR.Core.Helper
{
    using Photon.Pun;

    public static class PhotonHelper
    {
        public static T FindNetworkedComponent<T>(int viewId)
        {
            if (viewId <= 0)
            {
                return default;
            }

            var photonView = PhotonView.Find(viewId);
            if (photonView != null)
            {
                return photonView.GetComponent<T>();
            }

            return default;
        }
    }
}
