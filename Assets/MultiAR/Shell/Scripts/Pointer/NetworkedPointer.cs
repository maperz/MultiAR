namespace MultiAR.Shell.Scripts.Pointer
{
    using Microsoft.MixedReality.Toolkit.Utilities;
    using Core.Helper;
    using Core.Helper.Extensions;
    using Core.Models;
    using Core.Services.Interfaces;
    using Photon.Pun;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UniRx;
    using UnityEngine;
    using UnityEngine.Rendering;

    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(LineRenderer))]
    [DisallowMultipleComponent]
    public class NetworkedPointer : MonoBehaviourPun, IPunObservable, IUserObject
    {
        [SerializeField] public string originGameObjectName = "RoomOrigin";

        [SerializeField]
        private int linePoints = 10;

        private Vector3[] _positions;
        private Vector3[] _targetPositions;
        private float[] _distancesTargetAndCurrent;

        private BaseMixedRealityLineDataProvider _provider;
        private LineRenderer _lineRenderer;

        private bool HasPositionData => _positions != null && _positions.Length > 0;

        private bool CanInterpolatePositionData =>
            _positions != null && _distancesTargetAndCurrent != null && _distancesTargetAndCurrent.Length == _targetPositions.Length;

        private float _serializationFrequency;

        private IDisposable _subscription;

        private IPointerService _pointerService;

        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        public User User { get; private set; }

        public void SetPointerService(IPointerService pointerService)
        {
            _pointerService = pointerService;
            InitializeByOwner();
        }

        public void SetUser(User user)
        {
            User = user;

            if (!photonView.IsMine)
            {
                Debug.LogWarning("Trying to set user without owning network pointer - Ignoring request");
                return;
            }

            photonView.RPC(nameof(OnSetColor), RpcTarget.AllBuffered, new Vector3(user.Color.r, user.Color.g, user.Color.b));
        }

        private void OnEnable()
        {
            _serializationFrequency = (1.0f / PhotonNetwork.SerializationRate);

            _lineRenderer = GetComponent<LineRenderer>();

            _lineRenderer.useWorldSpace = true;
            _lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            _lineRenderer.lightProbeUsage = LightProbeUsage.Off;
            _lineRenderer.enabled = false;
            _lineRenderer.positionCount = 0;

            InitializeByOwner();
        }

        private void InitializeByOwner()
        {
            if (!photonView.IsMine)
            {
                return;
            }

            SetActive(false);

            if (_pointerService != null)
            {
                _subscription?.Dispose();
                _subscription = _pointerService.IsSharedPointerEnabled().Subscribe(this.SetActive);
            }
        }

        private void Update()
        {
            if (!photonView.IsMine)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
               _pointerService.ToggleSharedPointerEnabled();
            }

            // Check double tap with two fingers
            if (Input.touchCount == 2 && Input.GetTouch(0).tapCount == 2 && Input.GetTouch(1).tapCount == 2)
            {
                _pointerService.ToggleSharedPointerEnabled();
            }
        }

        private void FixedUpdate()
        {
            if (Origin == null)
            {
                return;
            }

            if (!photonView.IsMine && _targetPositions != null)
            {
                if (CanInterpolatePositionData)
                {
                    for (var i = 0; i < _targetPositions.Length; i++)
                    {
                        _positions[i] = Vector3.MoveTowards(_positions[i], _targetPositions[i],
                            _distancesTargetAndCurrent[i] * _serializationFrequency);
                    }
                }
                else
                {
                    _positions = _targetPositions;
                }
            }
        }

        private void OnDisable()
        {
            if (!photonView.IsMine)
            {
                return;
            }

            if (_subscription != null)
            {
                _subscription.Dispose();
                _subscription = null;
            }

            SetActive(false);
        }

        public void SetActive(bool active)
        {
            if (!photonView.IsMine)
            {
                Debug.LogWarning("Trying to set active without owning network pointer - Ignoring request");
                return;
            }
            photonView.RPC(nameof(OnSetActive), RpcTarget.AllBuffered, active);
        }

        [PunRPC]
        private void OnSetColor(Vector3 serializedColor)
        {
            var color = new Color(serializedColor.x, serializedColor.y, serializedColor.z);
            var material = _lineRenderer.material;
            material.color = color;
            material.SetColor(EmissionColor, color);
        }

        [PunRPC]
        private void OnSetActive(bool active)
        {
            _lineRenderer.enabled = active;
        }

        public void SetLineDataProvider(BaseMixedRealityLineDataProvider provider)
        {
            _provider = provider;
        }

        private GameObject _origin;

        Transform Origin
        {
            get
            {
                if (_origin == null)
                {
                    var origin = GameObject.Find(originGameObjectName);
                    if (origin == null)
                    {
                        return null;
                    }

                    _origin = origin;
                }

                return (_origin.transform);
            }
        }

        protected void LateUpdate()
        {
            if (_provider)
            {
                UpdatePositionsFromProvider();
            }

            if (HasPositionData)
            {
                _lineRenderer.positionCount = _positions.Length;
                _lineRenderer.SetPositions(_positions);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (Origin == null)
            {
                return;
            }

            if (stream.IsWriting)
            {
                if (_positions == null)
                {
                    return;
                }

                stream.SendNext(ToRelative(_positions));
            }
            else
            {
                _targetPositions = FromRelative((Vector3[])stream.ReceiveNext());
                if (_positions != null && _targetPositions.Length == _positions.Length)
                {
                    _distancesTargetAndCurrent = new float[_positions.Length];
                    for (var i = 0; i < _positions.Length; i++)
                    {
                        _distancesTargetAndCurrent[i] = Vector3.Distance(_targetPositions[i],_positions[i]);
                    }
                }
            }
        }

        private void UpdatePositionsFromProvider()
        {
            if (_positions == null)
            {
                _positions = new Vector3[linePoints];
            }

            var step = 1.0f / (linePoints-1);
            for(var i = 0; i < linePoints; i++)
            {
                _positions[i] = _provider.GetPoint(step * i);
            }

            _targetPositions = _positions;
        }

        private  Vector3[] ToRelative(IEnumerable<Vector3> positions)
        {
            var origin = Origin;
            return positions.Select(position => position.ToRelative(origin)).ToArray();
        }

        private Vector3[] FromRelative(IEnumerable<Vector3> positions)
        {
            var origin = Origin;
            return positions.Select(position => position.FromRelative(origin)).ToArray();
        }

    }
}
