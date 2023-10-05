using System;
using MultiAR.Core.Services.Implementations;
using MultiAR.Core.Services.Interfaces;
using TMPro;
using UnityEngine;

namespace MultiAR.Shell.Scripts.Lobby
{
    using UniRx;
    using Zenject;

    /// <summary>
    /// A class that enables two objects to be shown / hidden depending if creation of room is allowed.
    /// </summary>
    public class CanCreateRoomSwitcher : MonoBehaviour
    {
        [Tooltip("The object that shall be  active when creation is ready")]
        public GameObject readyForCreate;

        [Tooltip("The object that shall be active while waiting creation to be ready")]
        public GameObject waitForCreate;

        [Tooltip("A progress text object that is updated while waiting creation to be ready")]
        public TextMeshPro progressText;

        private IColocateService<string> _colocateService;

        private bool _isReady;

        void Start()
        {
            SetReady(false);

            _colocateService = FindObjectOfType<AsaColocateService>();
            if (_colocateService == null)
            {
                throw new Exception("AsaColocateService could not be found - Please add the prefab to the scene");
            }

            if (!_colocateService.IsSupported())
            {
                Debug.Log("Co-location is not supported - Rooms can be created immediately");
                SetReady(true);
                enabled = false;
            }
            else
            {
                _colocateService.CanCreateProgress().Subscribe(OnProgressUpdate).AddTo(this);
            }
        }

        private void OnProgressUpdate(float progress)
        {
            if (progress >= 1.0 && !_isReady)
            {
                SetReady(true);
            }
            else if (progressText != null)
            {
                progressText.SetText($"{Math.Round(progress * 100)}%");
            }
        }

        private void SetReady(bool ready)
        {
            if (waitForCreate != null)
            {
                waitForCreate.SetActive(!ready);
            }

            if (readyForCreate != null)
            {
                readyForCreate.SetActive(ready);
            }

            this._isReady = ready;
        }
    }
}
