namespace MultiAR_Examples.Buttons.Scripts
{
    using Photon.Pun;
    using UnityEngine;

    public class NetworkedSpawnObject : MonoBehaviour
    {
        [SerializeField] private GameObject spawnLocation;

        public void Spawn(GameObject go)
        {
            PhotonNetwork.InstantiateRoomObject(go.name, spawnLocation.transform.position, spawnLocation.transform.rotation);
        }
    }
}
