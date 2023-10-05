using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;

namespace MultiAR.Shell.Menu
{
    using UniRx;

    [RequireComponent(typeof(HandConstraint))]
    public class HandMenuActivator : MonoBehaviour
    {
        private HandConstraint _handConstraint;

        void Start()
        {
            _handConstraint = GetComponent<HandConstraint>();

            _handConstraint.OnHandActivate.AsObservable().Subscribe((_) => OnActivateChanged(true)).AddTo(this);
            _handConstraint.OnHandDeactivate.AsObservable().Subscribe((_) => OnActivateChanged(false)).AddTo(this);
        }

        private void OnActivateChanged(bool activated)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(activated);
            }
        }
    }
}
