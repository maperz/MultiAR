namespace MultiAR_Examples.Cubes.Scripts
{
    using Photon.Pun;
    using System;
    using UnityEngine;

    public class CubeSpawner : MonoBehaviour
    {
        public GameObject cube;

        public void Spawn()
        {
            if (cube == null)
            {
                throw new ArgumentException("Cube Prefab to be spawned has to be set!");
            }

            var cubeLocation = new Vector3(0, 0, 0.5f);

            PhotonNetwork.InstantiateRoomObject(cube.name, cubeLocation, Quaternion.identity);
        }
    }
}
