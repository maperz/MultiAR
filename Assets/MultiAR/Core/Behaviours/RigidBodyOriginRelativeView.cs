namespace MultiAR.Core.Behaviours
{
    using Helper;
    using Helper.Extensions;
    using Photon.Pun;
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    public class RigidBodyOriginRelativeView : MonoBehaviourPun, IPunObservable
    {
        [SerializeField] public string originGameObjectName = "RoomOrigin";

        private float _distance;
        private float _angle;

        private Rigidbody _rigidBody;

        private Vector3 _networkPosition;
        private Quaternion _networkRotation;

        private float _teleportDistance = 3.0f;

        public void Awake()
        {
            this._rigidBody = GetComponent<Rigidbody>();
            this._networkPosition = new Vector3();
            this._networkRotation = new Quaternion();
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

        public void FixedUpdate()
        {
            if (!this.photonView.IsMine)
            {
                this._rigidBody.position = Vector3.MoveTowards(this._rigidBody.position, this._networkPosition, this._distance * (1.0f / PhotonNetwork.SerializationRate));
                this._rigidBody.rotation = Quaternion.RotateTowards(this._rigidBody.rotation, this._networkRotation, this._angle * (1.0f / PhotonNetwork.SerializationRate));
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNextRelativePosition(this._rigidBody.position, Origin);
                stream.SendNextRelativeRotation(this._rigidBody.rotation, Origin);
                stream.SendNextRelativePosition(this._rigidBody.velocity, Origin);
                stream.SendNextRelativePosition(this._rigidBody.angularVelocity, Origin);
            }
            else
            {
                float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));

                this._networkPosition = stream.ReceiveNextRelativePosition(Origin);
                this._networkRotation = stream.ReceiveNextRelativeRotation(Origin);

                if (Vector3.Distance(this._rigidBody.position, this._networkPosition) > this._teleportDistance)
                {
                    this._rigidBody.position = this._networkPosition;
                }

                this._rigidBody.velocity = stream.ReceiveNextRelativePosition(Origin);

                this._networkPosition += this._rigidBody.velocity * lag;

                this._distance = Vector3.Distance(this._rigidBody.position, this._networkPosition);

                this._rigidBody.angularVelocity = stream.ReceiveNextRelativePosition(Origin);

                this._networkRotation = Quaternion.Euler(this._rigidBody.angularVelocity * lag) * this._networkRotation;

                this._angle = Quaternion.Angle(this._rigidBody.rotation, this._networkRotation);
            }
        }
    }
}
