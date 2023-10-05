namespace MultiAR.Shell.Scripts.Placer
{
    using System;
    using UniRx;
    using UnityEngine;

    public class PlacementService : MonoBehaviour, IPlacementService
    {
        private readonly BehaviorSubject<GameObject> _placingObject = new BehaviorSubject<GameObject>(null);

        public void StartPlacing(GameObject go)
        {
            if (_placingObject.Value != null)
            {
                Destroy(_placingObject.Value);
            }

            _placingObject.OnNext(go);
        }

        public void StopPlacing()
        {
            _placingObject.OnNext(null);
        }

        public bool IsPlacing()
        {
            return _placingObject.Value != null;
        }

        public IObservable<GameObject> PlacingObject()
        {
            return _placingObject;
        }

        public GameObject CurrentPlacingObject()
        {
            return _placingObject.Value;
        }
    }
}
