using System;
using MultiAR.Core.Services.Implementations;
using UnityEngine;

namespace MultiAR.Shell.Scripts.Room
{
    using Core.Services.Interfaces;

    public class LeaveRoomScript : MonoBehaviour
    {
        public void OnLeaveRoom()
        {
            Debug.Log("On leave room called");
            IMultiUserService multiUserService = FindObjectOfType<MultiUserService>();
            if (multiUserService == null)
            {
                throw new Exception("MultiARSystem could not be found - Please add the prefab to the scene");
            }

            multiUserService.LeaveRoom();
        }
    }
}
