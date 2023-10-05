namespace MultiAR.Shell.Scripts.Placer
{
    using Core.Models;
    using Core.Services.Interfaces;
    using Microsoft.MixedReality.Toolkit;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
    using System;
    using System.Numerics;
    using UniRx;
    using UnityEngine;
    using Zenject;
    using Vector3 = UnityEngine.Vector3;

    [RequireComponent(typeof(TapToPlace))]
    public class PlaceObjectWithTap : MonoBehaviour
    {
        public GameObject Slot;

        private GameObject _currentPlacedObject;
        private TapToPlace _tapToPlace;

        [Inject] private IPlacementService _placementService;

        public GameObject groundIndicator;

        void Start()
        {
            _tapToPlace = GetComponent<TapToPlace>();
            _tapToPlace.AutoStart = true;

            _placementService.PlacingObject().Subscribe(OnNewPlacingObject).AddTo(this);

            var deviceTypeService = MixedRealityToolkit.Instance.GetService<IDeviceTypeService>();
            var solverHandler = GetComponent<SolverHandler>();
            if (deviceTypeService.GetDeviceType() != Device.HoloLens)
            {
                Debug.Log("Using Mobile device: Setting TrackedObjectType.Head!");
                solverHandler.TrackedTargetType = TrackedObjectType.Head;
            }
        }

        private void OnNewPlacingObject(GameObject go)
        {
            _currentPlacedObject = go;
            _tapToPlace.enabled = _currentPlacedObject != null;
            if (_currentPlacedObject != null)
            {
                _currentPlacedObject.transform.parent = Slot.transform;
                _currentPlacedObject.transform.localPosition = Vector3.zero;

                var scale = Vector3.one;
                var objectCollider = _currentPlacedObject.GetComponent<Collider>();
                if (objectCollider)
                {
                    var size = objectCollider.bounds.size;
                    var largestSide = Math.Max(size.x, size.z);
                    if (largestSide > 0)
                    {
                        scale = new Vector3(largestSide, 1, largestSide);
                    }
                }

                if (groundIndicator)
                {
                    groundIndicator.transform.localScale = scale;
                }

                _tapToPlace.StartPlacement();
                Slot.SetActive(true);
            }
        }

        public void OnDetachAndPlaceObject()
        {
            if (_currentPlacedObject != null)
            {
                _currentPlacedObject.transform.parent = null;
                _currentPlacedObject.GetComponent<PlacedListener>()?.OnPlaced();
                _placementService.StopPlacing();
            }

            Slot.SetActive(false);
        }
    }
}
