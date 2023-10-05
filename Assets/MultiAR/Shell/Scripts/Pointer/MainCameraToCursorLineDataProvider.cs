namespace MultiAR.Shell.Scripts.Pointer
{
    using Microsoft.MixedReality.Toolkit.Input;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using UnityEngine;

    public class MainCameraToCursorLineDataProvider : SimpleLineDataProvider
    {
        [Header("MultiAR - Settings")]
        [SerializeField]
        private Vector3 startOffset = Vector3.zero;

        private Transform _cursor;

        private void Start()
        {
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        private void Update()
        {
            if (_cursor == null)
            {
                _cursor = FindObjectOfType<AnimatedCursor>().transform;
            }

            if (_cursor)
            {
                SetPointInternal(0, CameraCache.Main.transform.position + startOffset);
                SetPointInternal(1,  _cursor.position);
            }
        }
    }
}
