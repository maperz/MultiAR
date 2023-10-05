using UnityEngine;
using UnityEngine.Events;

namespace MultiAR.Shell.Scripts.Placer
{
    public class PlacedListener : MonoBehaviour
    {
        public UnityEvent onPlaced = new UnityEvent();

        public void OnPlaced()
        {
            onPlaced?.Invoke();
        }
    }
}
