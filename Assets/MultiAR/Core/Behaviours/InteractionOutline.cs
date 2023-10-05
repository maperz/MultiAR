using Microsoft.MixedReality.Toolkit.Utilities;
using MultiAR.Core.Models;
using UnityEngine;

namespace MultiAR.Core.Behaviours
{
    using Helper;
    using System;
    using System.Linq;
    using UniRx;

    public class InteractionOutline : MonoBehaviour
    {
        public NetworkedObject networkedObject;
        public MeshOutline outline;
        private Material _outlineMaterial;

        private void OnEnable()
        {
            if (!outline)
            {
                outline = GetComponent<MeshOutline>();
            }

            if (outline != null)
            {
                outline.enabled = false;
            }

            if (networkedObject == null)
            {
                networkedObject = GetComponent<NetworkedObject>();
            }

            if (networkedObject != null)
            {
                networkedObject.TransformInteraction.CombineLatest(networkedObject.FocusInteractions, ((interaction, set) =>
                            new Tuple<NetworkObjectInteraction, ImmutableInteractionSet>(interaction, set)))
                    .Subscribe(InteractionsChanged).AddTo(this);
            }
        }

        private void StartDrawOutline(NetworkObjectInteraction interaction)
        {
            // Debug.Log("Starting outline");
            if (outline != null)
            {
                if (!_outlineMaterial)
                {
                    _outlineMaterial = new Material(outline.OutlineMaterial);
                    outline.OutlineMaterial = _outlineMaterial;
                }

                _outlineMaterial.color = interaction.User.Color;
                outline.enabled = true;
            }
        }

        private void OnDestroy()
        {
            if (_outlineMaterial)
            {
                Destroy(_outlineMaterial);
            }
        }

        private void StopDrawOutline()
        {
            //Debug.Log("Stopping outline");
            if (outline != null)
            {
                outline.enabled = false;
            }
        }

        private void InteractionsChanged(Tuple<NetworkObjectInteraction, ImmutableInteractionSet> interactions)
        {
            var transformInteraction = interactions.Item1;
            var focusInteractions = interactions.Item2;

            if (transformInteraction != null)
            {
                // Debug.Log("Start draw outline: transform interaction");
                StartDrawOutline(transformInteraction);
                return;
            }

            var local = focusInteractions.FindLocal();
            if (local != null)
            {
                // Debug.Log("Start draw outline: local focus interaction");
                StartDrawOutline(local);
                return;
            }

            if (focusInteractions.Count() > 0)
            {
                // Debug.Log("Start draw outline: first focus interaction");
                var focusInteraction = focusInteractions.Data.First();
                StartDrawOutline(focusInteraction);
            }
            else
            {
                // Debug.Log("Stop draw outline.");
                StopDrawOutline();
            }
        }
    }
}
