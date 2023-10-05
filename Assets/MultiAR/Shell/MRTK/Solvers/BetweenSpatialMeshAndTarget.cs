using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Physics;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;

public class BetweenSpatialMeshAndTarget : Solver
{
    private const int SpatialAwarenessLayer = 31;

    [SerializeField] [Tooltip("Offset to the normal vector of the found spatial mesh")]
    private float normalOffset = 0.03f;

    [SerializeField] [Tooltip("Minimum distance to reposition")]
    private float minDistanceToReposition = 0.1f;

    [SerializeField] [Tooltip("Minimum angle to re-orientate")]
    private float minAngleToReOrientate = 5f;

    [SerializeField] [Tooltip("Max distance for raycast to check for surfaces")]
    private float maxRaycastDistance = 100.0f;

    [SerializeField] [Tooltip("Layer mask of the spatial mesh")]
    private LayerMask spatialMeshLayer = SpatialAwarenessLayer;

    private Vector3 RaycastEndPoint => transform.position;

    private Vector3 RaycastStartPoint =>
        SolverHandler.TransformTarget == null ? Vector3.zero : SolverHandler.TransformTarget.position;


    public override void SolverUpdate()
    {
        if (SolverHandler == null || SolverHandler.TransformTarget == null) return;

        var direction = RaycastEndPoint - RaycastStartPoint;

        Debug.DrawLine(RaycastStartPoint, RaycastEndPoint);

        // Check if there is a spatial mesh between the observer and the object
        var isHit = Physics.Raycast(RaycastStartPoint, direction, out var result, maxRaycastDistance, spatialMeshLayer);
        if (isHit)
        {
            var targetPosition = result.point + normalOffset * result.normal;
            var distance = Vector3.Distance(GoalPosition, targetPosition);

            if (distance < minDistanceToReposition)
            {
                return;
            }

            var targetOrientation = CalculateOrientation(result.normal);
            ;
            var angle = Quaternion.Angle(targetOrientation, GoalRotation);

            if (angle < minAngleToReOrientate)
            {
                return;
            }

            GoalPosition = targetPosition;
            GoalRotation = targetOrientation;
        }
    }

    private Quaternion CalculateOrientation(Vector3 surfaceNormal)
    {
        var currentUpVector = WorkingRotation * Vector3.up;
        var surfaceReferenceRotation = Quaternion.LookRotation(-surfaceNormal, currentUpVector);
        var surfaceReferenceUp = surfaceReferenceRotation * Vector3.up;
        surfaceReferenceRotation = Quaternion.FromToRotation(surfaceReferenceUp, Vector3.up) * surfaceReferenceRotation;
        return surfaceReferenceRotation;
    }
}
