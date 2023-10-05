#if UNITY_ANDROID || UNITY_IOS || UNITY_WSA
#define HAS_ANCHORS
#endif

#if UNITY_EDITOR
#undef HAS_ANCHORS
#endif

#if HAS_ANCHORS
using Microsoft.Azure.SpatialAnchors.Unity;
#endif

using System;
using Microsoft.MixedReality.Toolkit.Utilities;
using MultiAR.Core.Models;
using MultiAR.Core.Services.Implementations;
using MultiAR.Core.Services.Interfaces;
using Photon.Pun;
using UnityEngine;

namespace MultiAR.Shell.Scripts.Room
{
    public class MultiUserRoomCenter : MonoBehaviour
    {
        private IMultiUserService _multiUserService;
        private ActiveMultiUserRoom _room;

        [SerializeField] private GameObject originAxis;

        public GameObject userIndicatorHoloLensPrefab;
        public GameObject userIndicatorMobilePrefab;
        public GameObject userIndicatorUnknownPrefab;

        private void Start()
        {
            _multiUserService = FindObjectOfType<MultiUserService>();
            if (_multiUserService == null)
            {
                throw new Exception("MultiARSystem could not be found - Please add the prefab to the scene");
            }

            _room = _multiUserService.GetActiveRoom();
            if (_room == null)
            {
                throw new Exception("Active room not found - Room center should not exist without an active room!");
            }

            Move(_room.Pose);

            if (_room.Color.HasValue && originAxis)
            {
                originAxis.GetComponentInChildren<MeshRenderer>().material.color = _room.Color.Value;
            }


            CreateUserIndicator();
        }

        public void Move(Pose pose)
        {
            transform.SetPositionAndRotation(pose.position, pose.rotation);

#if HAS_ANCHORS
            // Set pose to create a local anchor if necessary
           gameObject.SetPose(pose);
#endif
        }

        private void CreateUserIndicator()
        {
            var localUser = _multiUserService.GetLocalUser();
            var indicatorPrefab = GetPrefabForDevice(localUser.DeviceType);
            if (indicatorPrefab)
            {
                var cameraTransform = CameraCache.Main.transform;
                var userIndicator = PhotonNetwork.Instantiate(indicatorPrefab.name, cameraTransform.position,
                    cameraTransform.rotation);
                userIndicator.transform.SetParent(cameraTransform);
                userIndicator.GetComponent<UserIndicator>().SetUser(localUser);
            }
        }

        private GameObject GetPrefabForDevice(Device device)
        {
            switch (device)
            {
                case Device.Mobile:
                    return userIndicatorMobilePrefab;
                case Device.HoloLens:
                    return userIndicatorHoloLensPrefab;
                default:
                    return userIndicatorUnknownPrefab;
            }
        }
    }
}
