using System;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Class that is similar to MRTK's InBetween but with a fixed (absolute) distance along the direction line
/// </summary>
public class InBetweenFixed : Solver
{
    [SerializeField]
    [Tooltip(
        "Distance along the center line the object will be located. This is in absolute values and not like the regular in between with relative values.")]
    private float fixedOffset = 0.0f;

    /// <summary>
    /// Distance along the center line the object will be located. 0.5 is halfway, 1.0 is at the first transform, 0.0 is at the second transform.
    /// </summary>
    public float FixedOffset
    {
        get => fixedOffset;
        set => fixedOffset = value;
    }

    [SerializeField]
    [Tooltip(
        "Tracked object to calculate position and orientation for the second object. If you want to manually override and use a scene object, use the TransformTarget field.")]
    [HideInInspector]
    [FormerlySerializedAs("trackedObjectForSecondTransform")]
    private TrackedObjectType secondTrackedObjectType = TrackedObjectType.Head;

    public bool invertTargets = false;

    /// <summary>
    /// Tracked object to calculate position and orientation for the second object. If you want to manually override and use a scene object, use the TransformTarget field.
    /// </summary>
    public TrackedObjectType SecondTrackedObjectType
    {
        get => secondTrackedObjectType;
        set
        {
            if (secondTrackedObjectType == value) return;
            secondTrackedObjectType = value;
            UpdateSecondSolverHandler();
        }
    }

    /// <summary>
    /// Tracked object to calculate position and orientation for the second object. If you want to manually override and use a scene object, use the TransformTarget field.
    /// </summary>
    [Obsolete("Use SecondTrackedObjectType property instead")]
    public TrackedObjectType TrackedObjectForSecondTransform
    {
        get => secondTrackedObjectType;
        set
        {
            if (secondTrackedObjectType == value) return;
            secondTrackedObjectType = value;
            UpdateSecondSolverHandler();
        }
    }

    [SerializeField]
    [Tooltip(
        "This transform overrides any Tracked Object as the second point for the In Between with fixed distances.")]
    [HideInInspector]
    private Transform secondTransformOverride = null;

    /// <summary>
    /// This transform overrides any Tracked Object as the second point for the In Between
    /// </summary>
    public Transform SecondTransformOverride
    {
        get => secondTransformOverride;
        set
        {
            if (secondTransformOverride == value) return;
            secondTransformOverride = value;
            UpdateSecondSolverHandler();
        }
    }

    private SolverHandler _secondSolverHandler;

    protected void OnValidate()
    {
        UpdateSecondSolverHandler();
    }

    protected override void Start()
    {
        base.Start();

        // We need to get the secondSolverHandler ready before we tell them both to seek a tracked object.
        _secondSolverHandler = gameObject.AddComponent<SolverHandler>();
        _secondSolverHandler.UpdateSolvers = false;

        UpdateSecondSolverHandler();
    }

    /// <inheritdoc />
    public override void SolverUpdate()
    {
        if (SolverHandler == null || _secondSolverHandler == null) return;

        if (SolverHandler.TransformTarget != null && _secondSolverHandler.TransformTarget != null)
        {
            AdjustPositionForOffset(SolverHandler.TransformTarget, _secondSolverHandler.TransformTarget);
        }
    }

    private void AdjustPositionForOffset(Transform targetTransform, Transform secondTransform)
    {
        if (targetTransform == null || secondTransform == null) return;


        var start = !invertTargets ? targetTransform.position : secondTransform.position;
        var end = !invertTargets ? secondTransform.position : targetTransform.position;

        var targetVector = (end - start).normalized;

        GoalPosition = start + (targetVector * fixedOffset);
    }

    private void UpdateSecondSolverHandler()
    {
        if (_secondSolverHandler == null) return;

        _secondSolverHandler.TrackedTargetType = secondTrackedObjectType;

        if (secondTrackedObjectType == TrackedObjectType.CustomOverride && secondTransformOverride != null)
        {
            _secondSolverHandler.TransformOverride = secondTransformOverride;
        }
    }
}
