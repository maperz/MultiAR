namespace MultiAR.Core.Helper
{
    using Photon.Pun;
    using System;

    public class PhotonMessageQueueBreak : IDisposable
    {
        public PhotonMessageQueueBreak()
        {
            PhotonNetwork.IsMessageQueueRunning = false;
        }

        public void Dispose()
        {
            PhotonNetwork.IsMessageQueueRunning = true;
        }
    }
}
