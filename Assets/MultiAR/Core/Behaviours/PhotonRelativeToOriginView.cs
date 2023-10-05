using Photon.Pun;
using UnityEngine;

namespace MultiAR.Core.Behaviours
{
    using Helper.Extensions;

    [RequireComponent(typeof(PhotonView))]
    public class PhotonRelativeToOriginView : MonoBehaviourPun, IPunObservable
    {
        [SerializeField] public string originGameObjectName = "RoomOrigin";

        public bool includeScale = true;
        public float teleportDistance = 2;

        private Vector3 _targetScale;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;

        private float _angle;
        private float _scaleDistance;
        private float _positionDistance;

        private float _serializationFrequency;

        private void Start()
        {
            _serializationFrequency = (1.0f / PhotonNetwork.SerializationRate);
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

        bool IsReady()
        {
            try
            {
                return !ReferenceEquals(Origin, null);
            }
            catch
            {
                return false;
            }
        }

        private void FixedUpdate()
        {
            if (Origin == null)
            {
                return;
            }

            if (!photonView.IsMine)
            {
                var transformCache = transform;
                var currentRotation = transformCache.rotation;
                transform.rotation =
                    Quaternion.RotateTowards(currentRotation, _targetRotation, _angle * _serializationFrequency);

                var currentPosition = transformCache.position;
                transform.position = Vector3.MoveTowards(currentPosition, _targetPosition,
                    _positionDistance * _serializationFrequency);

                if (includeScale)
                {
                    var currentScale = transformCache.localScale;
                    transform.localScale = Vector3.MoveTowards(currentScale, _targetScale,
                        _scaleDistance * _serializationFrequency);
                }
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (Origin == null)
            {
                return;
            }

            if (!IsReady())
            {
                Debug.LogWarning("Not ready - skipping serialization");
                return;
            }

            if (stream.IsWriting)
            {
                //Debug.Log(this.name + "(Sending) -> Position and Relative Rotation are: " + RelativePosition + " / " + RelativeRotation);
                stream.SendNext(transform.position.ToRelative(Origin));
                stream.SendNext(transform.rotation.ToRelative(Origin));
                if (includeScale)
                {
                    stream.SendNext(transform.localScale);
                }
            }
            else
            {
                //Debug.Log(this.name + "(Recv) -> Position and Relative Rotation are: " + RelativePosition + " / " + RelativeRotation);
                var relativePosition = (Vector3)stream.ReceiveNext();
                var relativeRotation = (Quaternion)stream.ReceiveNext();

                _targetPosition = relativePosition.FromRelative(Origin);
                _positionDistance = Vector3.Distance(_targetPosition, transform.position);

                _targetRotation = relativeRotation.FromRelative(Origin);
                _angle = Quaternion.Angle(transform.rotation, _targetRotation);

                if (Vector3.Distance(transform.position, _targetPosition) >= teleportDistance)
                {
                    transform.position = _targetPosition;
                }

                if (includeScale)
                {
                    _targetScale = (Vector3)stream.ReceiveNext();
                    _scaleDistance = Vector3.Distance(_targetScale, transform.localScale);
                }
            }
        }
    }
}
