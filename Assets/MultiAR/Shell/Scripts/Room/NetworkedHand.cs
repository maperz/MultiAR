using Photon.Pun;
using UnityEngine;

namespace MultiAR.Shell.Scripts.Room
{
    using Core.Helper;
    using Core.Helper.Extensions;
    using Core.Models;
    using Core.Services.Interfaces;
    using Microsoft.MixedReality.Toolkit;
    using Microsoft.MixedReality.Toolkit.Input;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using System;
    using System.Collections.Generic;

    [RequireComponent(typeof(PhotonView))]
    public class NetworkedHand : MonoBehaviourPun, IPunObservable,
        IMixedRealitySourceStateHandler, IMixedRealityHandJointHandler, IUserObject
    {
        public Handedness handedness = Handedness.None;

        [SerializeField]
        [Tooltip("Hand material to use for hand tracking hand mesh.")]
        private Material handMaterial;

        public bool hideLocal = true;

        private bool _isActive;

        public User User { get; private set; }

        private bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                handRenderer.enabled = ComputeEnabledHelper(_isActive, photonView.IsMine, User?.Colocated ?? false);
            }
        }

        private bool ComputeEnabledHelper(bool isActive, bool isMine, bool isUserColocated)
        {
            return isActive && (!hideLocal || (!isMine && !isUserColocated));
        }

        /// <summary>
        /// Wrist Transform
        /// </summary>
        public Transform Wrist;

        /// <summary>
        /// Palm transform
        /// </summary>
        public Transform Palm;

        /// <summary>
        /// Thumb metacarpal transform  (thumb root)
        /// </summary>
        public Transform ThumbRoot;

        /// <summary>
        /// Index metacarpal transform (index finger root)
        /// </summary>
        public Transform IndexRoot;

        /// <summary>
        /// Middle metacarpal transform (middle finger root)
        /// </summary>
        public Transform MiddleRoot;

        /// <summary>
        /// Ring metacarpal transform (ring finger root)
        /// </summary>
        public Transform RingRoot;

        /// <summary>
        /// Pinky metacarpal transform (pinky finger root)
        /// </summary>
        public Transform PinkyRoot;


        [Tooltip("If non-zero, this vector and the modelPalmFacing vector " +
                 "will be used to re-orient the Transform bones in the hand rig, to " +
                 "compensate for bone axis discrepancies between Leap Bones and model " +
                 "bones.")]
        public Vector3 ModelFingerPointing = new Vector3(0, 0, 0);

        [Tooltip("If non-zero, this vector and the modelFingerPointing vector " +
                 "will be used to re-orient the Transform bones in the hand rig, to " +
                 "compensate for bone axis discrepancies between Leap Bones and model " +
                 "bones.")]
        public Vector3 ModelPalmFacing = new Vector3(0, 0, 0);

        [SerializeField]
        [Tooltip("Renderer of the hand mesh")]
        private SkinnedMeshRenderer handRenderer;

        /// <summary>
        /// flag checking that the handRenderer was initialized with its own material
        /// </summary>
        private bool _handRendererInitialized;

        /// <summary>
        /// Renderer of the hand mesh.
        /// </summary>
        public SkinnedMeshRenderer HandRenderer => handRenderer;


        /// <summary>
        /// Property name for modifying the mesh's appearance based on pinch strength
        /// </summary>
        private const string pinchStrengthMaterialProperty = "_PressIntensity";

        /// <summary>
        /// Property name for modifying the mesh's appearance based on pinch strength
        /// </summary>
        public string PinchStrengthMaterialProperty => pinchStrengthMaterialProperty;

        private float _pinchStrength;


        /// <summary>
        /// Precalculated values for LeapMotion testhand fingertip lengths
        /// </summary>
        private const float ThumbFingerTipLength = 0.02167f;
        private const float IndexingerTipLength = 0.01582f;
        private const float MiddleFingerTipLength = 0.0174f;
        private const float RingFingerTipLength = 0.0173f;
        private const float PinkyFingerTipLength = 0.01596f;

        /// <summary>
        /// Precalculated fingertip lengths used for scaling the fingertips of the skinnedmesh
        /// to match with tracked hand fingertip size
        /// </summary>
        private readonly Dictionary<TrackedHandJoint, float> _fingerTipLengths = new Dictionary<TrackedHandJoint, float>()
        {
            {TrackedHandJoint.ThumbTip, ThumbFingerTipLength },
            {TrackedHandJoint.IndexTip, IndexingerTipLength },
            {TrackedHandJoint.MiddleTip, MiddleFingerTipLength },
            {TrackedHandJoint.RingTip, RingFingerTipLength },
            {TrackedHandJoint.PinkyTip, PinkyFingerTipLength }
        };


        [SerializeField] public string originGameObjectName = "RoomOrigin";
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

        public float PinchStrength
        {
            get => _pinchStrength;
            set
            {
                _pinchStrength = value;
                if (handRenderer.sharedMaterial.HasProperty(pinchStrengthMaterialProperty))
                {
                    handRenderer.sharedMaterial.SetFloat(PressIntensity, _pinchStrength);
                }
            }
        }


        /// <summary>
        /// Rotation derived from the `modelFingerPointing` and
        /// `modelPalmFacing` vectors in the RiggedHand inspector.
        /// </summary>
        private Quaternion UserBoneRotation
        {
            get
            {
                if (ModelFingerPointing == Vector3.zero || ModelPalmFacing == Vector3.zero)
                {
                    return Quaternion.identity;
                }

                return Quaternion.Inverse(Quaternion.LookRotation(ModelFingerPointing, -ModelPalmFacing));
            }
        }

        private readonly Dictionary<TrackedHandJoint, Transform>
            _joints = new Dictionary<TrackedHandJoint, Transform>();

        private static readonly int PressIntensity = Shader.PropertyToID(pinchStrengthMaterialProperty);

        private void Start()
        {

            // Find user in Hierarchy
            User = HierarchyLookup.FindUserInHierarchy(this);

            // Ensure hand is not visible until we can update position first time.
            HandRenderer.enabled = false;

            if (photonView.IsMine || (User?.IsLocal ?? false))
            {
                InitJoints();
            }

            // Give the hand mesh its own material to avoid modifying both hand materials when making property changes
            var handMaterialInstance = new Material(handMaterial);
            handRenderer.sharedMaterial = handMaterialInstance;
            _handRendererInitialized = true;
        }

        private void InitJoints()
        {
            // Initialize joint dictionary with their corresponding joint transforms
            _joints[TrackedHandJoint.Wrist] = Wrist;
            _joints[TrackedHandJoint.Palm] = Palm;

            // Thumb joints, first node is user assigned, note that there are only 4 joints in the thumb
            if (ThumbRoot)
            {
                _joints[TrackedHandJoint.ThumbMetacarpalJoint] = ThumbRoot;
                _joints[TrackedHandJoint.ThumbProximalJoint] = RetrieveChild(TrackedHandJoint.ThumbMetacarpalJoint);
                _joints[TrackedHandJoint.ThumbDistalJoint] = RetrieveChild(TrackedHandJoint.ThumbProximalJoint);
                _joints[TrackedHandJoint.ThumbTip] = RetrieveChild(TrackedHandJoint.ThumbDistalJoint);
            }

            // Look up index finger joints below the index finger root joint
            if (IndexRoot)
            {
                _joints[TrackedHandJoint.IndexMetacarpal] = IndexRoot;
                _joints[TrackedHandJoint.IndexKnuckle] = RetrieveChild(TrackedHandJoint.IndexMetacarpal);

                _joints[TrackedHandJoint.IndexMiddleJoint] = RetrieveChild(TrackedHandJoint.IndexKnuckle);
                _joints[TrackedHandJoint.IndexDistalJoint] = RetrieveChild(TrackedHandJoint.IndexMiddleJoint);
                _joints[TrackedHandJoint.IndexTip] = RetrieveChild(TrackedHandJoint.IndexDistalJoint);
            }

            // Look up middle finger joints below the middle finger root joint
            if (MiddleRoot)
            {
                _joints[TrackedHandJoint.MiddleMetacarpal] = MiddleRoot;
                _joints[TrackedHandJoint.MiddleKnuckle] = RetrieveChild(TrackedHandJoint.MiddleMetacarpal);
                _joints[TrackedHandJoint.MiddleMiddleJoint] = RetrieveChild(TrackedHandJoint.MiddleKnuckle);
                _joints[TrackedHandJoint.MiddleDistalJoint] = RetrieveChild(TrackedHandJoint.MiddleMiddleJoint);
                _joints[TrackedHandJoint.MiddleTip] = RetrieveChild(TrackedHandJoint.MiddleDistalJoint);
            }

            // Look up ring finger joints below the ring finger root joint
            if (RingRoot)
            {
                _joints[TrackedHandJoint.RingMetacarpal] = RingRoot;
                _joints[TrackedHandJoint.RingKnuckle] = RetrieveChild(TrackedHandJoint.RingMetacarpal);

                _joints[TrackedHandJoint.RingMiddleJoint] = RetrieveChild(TrackedHandJoint.RingKnuckle);
                _joints[TrackedHandJoint.RingDistalJoint] = RetrieveChild(TrackedHandJoint.RingMiddleJoint);
                _joints[TrackedHandJoint.RingTip] = RetrieveChild(TrackedHandJoint.RingDistalJoint);
            }

            // Look up pinky joints below the pinky root joint
            if (PinkyRoot)
            {
                _joints[TrackedHandJoint.PinkyMetacarpal] = PinkyRoot;
                _joints[TrackedHandJoint.PinkyKnuckle] = RetrieveChild(TrackedHandJoint.PinkyMetacarpal);

                _joints[TrackedHandJoint.PinkyMiddleJoint] = RetrieveChild(TrackedHandJoint.PinkyKnuckle);
                _joints[TrackedHandJoint.PinkyDistalJoint] = RetrieveChild(TrackedHandJoint.PinkyMiddleJoint);
                _joints[TrackedHandJoint.PinkyTip] = RetrieveChild(TrackedHandJoint.PinkyDistalJoint);
            }

        }

        private Transform RetrieveChild(TrackedHandJoint parentJoint)
        {
            if (_joints[parentJoint] != null && _joints[parentJoint].childCount > 0)
            {
                return _joints[parentJoint].GetChild(0);
            }

            return null;
        }

        private void OnEnable()
        {
            if (photonView.IsMine)
            {
                CoreServices.InputSystem?.RegisterHandler<IMixedRealitySourceStateHandler>(this);
                CoreServices.InputSystem?.RegisterHandler<IMixedRealityHandJointHandler>(this);
            }
        }

        private void OnDisable()
        {
            if (photonView.IsMine)
            {
                CoreServices.InputSystem?.UnregisterHandler<IMixedRealitySourceStateHandler>(this);
                CoreServices.InputSystem?.UnregisterHandler<IMixedRealityHandJointHandler>(this);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(IsActive);
                if (IsActive)
                {
                    stream.SendNextRelativePosition(Palm.position, Origin);
                    stream.SendNextRelativeRotation(Palm.rotation, Origin);
                    stream.SendNext(PinchStrength);
                }
            }
            else
            {
                IsActive = (bool)stream.ReceiveNext();
                if (IsActive)
                {
                    Palm.position = stream.ReceiveNextRelativePosition(Origin);
                    Palm.rotation = stream.ReceiveNextRelativeRotation(Origin);
                    PinchStrength = (float)stream.ReceiveNext();
                }
            }
        }

        /// <inheritdoc/>
        void IMixedRealitySourceStateHandler.OnSourceDetected(SourceStateEventData eventData)
        {
            if (!(eventData.Controller is IMixedRealityHand hand))
            {
                return;
            }

            if (!hand.ControllerHandedness.IsMatch(handedness) || !photonView.IsMine)
            {
                return;
            }

            IsActive = true;
        }

        /// <inheritdoc/>
        void IMixedRealitySourceStateHandler.OnSourceLost(SourceStateEventData eventData)
        {
            if (!(eventData.Controller is IMixedRealityHand hand))
            {
                return;
            }

            if (!hand.ControllerHandedness.IsMatch(handedness) || !photonView.IsMine)
            {
                return;
            }

            IsActive = false;
        }

        /// <inheritdoc/>
        void IMixedRealityHandJointHandler.OnHandJointsUpdated(
            InputEventData<IDictionary<TrackedHandJoint, MixedRealityPose>> eventData)
        {
            if (!eventData.Handedness.IsMatch(handedness) || !photonView.IsMine)
            {
                return;
            }

            // Apply updated TrackedHandJoint pose data to the assigned transforms
            foreach (TrackedHandJoint handJoint in eventData.InputData.Keys)
            {
                if (_joints.TryGetValue(handJoint, out var jointTransform))
                {
                    if (jointTransform != null)
                    {
                        if (handJoint == TrackedHandJoint.Palm)
                        {
                            Palm.position = eventData.InputData[TrackedHandJoint.Wrist].Position;
                            Palm.rotation = eventData.InputData[TrackedHandJoint.Palm].Rotation * UserBoneRotation;
                        }
                        else if (handJoint == TrackedHandJoint.Wrist)
                        {
                            //Wrist.position = eventData.InputData[TrackedHandJoint.Wrist].Position;
                        }
                        else
                        {
                            jointTransform.rotation = eventData.InputData[handJoint].Rotation * Reorientation();

                            if ((handJoint == TrackedHandJoint.ThumbDistalJoint ||
                                 handJoint == TrackedHandJoint.IndexDistalJoint ||
                                 handJoint == TrackedHandJoint.MiddleDistalJoint ||
                                 handJoint == TrackedHandJoint.RingDistalJoint ||
                                 handJoint == TrackedHandJoint.PinkyDistalJoint))
                            {
                                ScaleFingerTip(eventData, jointTransform, handJoint + 1, jointTransform.position);
                            }
                        }
                    }
                }

                // Update the hand material
                float calculateIndexPinch = HandPoseUtils.CalculateIndexPinch(handedness);

                // Hand Curl Properties:
                float indexFingerCurl = HandPoseUtils.IndexFingerCurl(handedness);
                float middleFingerCurl = HandPoseUtils.MiddleFingerCurl(handedness);
                float ringFingerCurl = HandPoseUtils.RingFingerCurl(handedness);
                float pinkyFingerCurl = HandPoseUtils.PinkyFingerCurl(handedness);

                if (handMaterial != null && _handRendererInitialized)
                {
                    float gripStrength = indexFingerCurl + middleFingerCurl + ringFingerCurl + pinkyFingerCurl;
                    gripStrength /= 4.0f;
                    gripStrength = gripStrength > 0.8f ? 1.0f : gripStrength;

                    PinchStrength = Mathf.Pow(Mathf.Max(calculateIndexPinch, gripStrength), 2.0f);
                }
            }
        }

        private void ScaleFingerTip(InputEventData<IDictionary<TrackedHandJoint, MixedRealityPose>> eventData, Transform jointTransform, TrackedHandJoint fingerTipJoint, Vector3 boneRootPos)
        {
            // Set fingertip base bone scale to match the bone length to the fingertip.
            // This will only scale correctly if the model was constructed to match
            // the standard "test" edit-time hand model from the LeapMotion TestHandFactory.
            var boneTipPos = eventData.InputData[fingerTipJoint].Position;
            var boneVec = boneTipPos - boneRootPos;

            if (transform.lossyScale.x != 0f && transform.lossyScale.x != 1f)
            {
                boneVec /= transform.lossyScale.x;
            }
            var newScale = jointTransform.transform.localScale;
            var lengthComponentIdx = GetLargestComponentIndex(ModelFingerPointing);
            newScale[lengthComponentIdx] = boneVec.magnitude / _fingerTipLengths[fingerTipJoint];
            jointTransform.transform.localScale = newScale;
        }


        private int GetLargestComponentIndex(Vector3 pointingVector)
        {
            var largestValue = 0f;
            var largestIdx = 0;
            for (int i = 0; i < 3; i++)
            {
                var testValue = pointingVector[i];
                if (Mathf.Abs(testValue) > largestValue)
                {
                    largestIdx = i;
                    largestValue = Mathf.Abs(testValue);
                }
            }
            return largestIdx;
        }

        private Quaternion Reorientation()
        {
            return Quaternion.Inverse(Quaternion.LookRotation(ModelFingerPointing, -ModelPalmFacing));
        }
    }
}
