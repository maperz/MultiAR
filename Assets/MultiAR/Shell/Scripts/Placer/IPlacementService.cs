namespace MultiAR.Shell.Scripts.Placer
{
    using System;
    using UnityEngine;

    public interface IPlacementService
    {
        public void StartPlacing(GameObject go);

        public void StopPlacing();

        public bool IsPlacing();

        public IObservable<GameObject> PlacingObject();

        public GameObject CurrentPlacingObject();
    }
}
