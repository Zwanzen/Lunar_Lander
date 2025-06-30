using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

[Serializable]
public struct GravitySource
{
    public Transform source;
    public float gravity;
    public float range;
    [Range(0f, 1f)]
    public float maxGravityPoint; // Point in range [0-1] where gravity is maximum (0 = at source, 1 = at edge)
}

public class PhysicsManager : MonoBehaviour
{
    // ___ PRIVATE FIELDS ___
    [Header("Physics Settings")]
    [SerializeField] private Dictionary<Transform, GravitySource> gravitySources
        = new Dictionary<Transform, GravitySource>();
    [Space(2)]
    [SerializeField] private Rigidbody[] gravityObjects;

    [Header("Debug Visualization")]
    [SerializeField] private bool unregisterAll = false;
    [SerializeField] private bool showGravityRanges = true;
    [SerializeField] private bool showGravityStrength = true;
    [SerializeField] private int visualizationDetail = 20; // Detail level for visualization
    [SerializeField] private Color rangeColor = new Color(0.2f, 0.6f, 1f, 0.2f);
    [SerializeField] private Color strengthColor = new Color(1f, 0.3f, 0.3f, 0.4f);
    [SerializeField] private bool use2DVisualization = true; // Optimized for 2.5D view

    // ___ INSTANCE ___
    public static PhysicsManager Instance { get; private set; }

    private void Awake()
    {
        // Ensure that there is only one instance of PhysicsManager
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // Destroy duplicate instances
    }

    // ___ Unity Methods ___

    private void OnValidate()
    {
        if(Application.isPlaying)
            return;

        if(unregisterAll)
        {
            gravitySources.Clear();
            unregisterAll = false; // Reset the flag
        }

        // Ensure that there is only one instance of PhysicsManager
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // Destroy duplicate instances
    }

    private void FixedUpdate()
    {
        ApplyGravity();
    }

    // ___ PRIVATE METHODS ___
    private void ApplyGravity()
    {
        if (gravityObjects.Length == 0 || gravitySources.Count == 0)
        {
            return; // No objects to apply gravity to or no gravity sources
        }

        foreach (Rigidbody rb in gravityObjects)
        {
            if (rb == null || rb.isKinematic)
            {
                continue; // Skip if the Rigidbody is null or kinematic
            }

            // Apply gravity from each source
            foreach (KeyValuePair<Transform, GravitySource> pair in gravitySources)
            {
                var source = pair.Value;
                // Calculate direction and distance to source
                Vector3 direction = source.source.position - rb.position;
                float distance = direction.magnitude;

                // Check if object is within range
                if (distance <= source.range)
                {
                    // Normalize direction
                    direction.Normalize();

                    // Calculate gravity strength based on distance and maxGravityPoint
                    float gravityStrength;
                    float maxGravityDistance = source.range * source.maxGravityPoint;

                    if (distance <= maxGravityDistance)
                    {
                        // Before the max gravity point - increasing gravity
                        float t = distance / maxGravityDistance;
                        gravityStrength = source.gravity * t;
                    }
                    else
                    {
                        // After the max gravity point - decreasing gravity
                        float t = (distance - maxGravityDistance) / (source.range - maxGravityDistance);
                        gravityStrength = source.gravity * (1f - t);
                    }

                    // Apply the gravitational force
                    rb.AddForce(direction * gravityStrength, ForceMode.Acceleration);
                }
            }
        }
    }

    // ___ PUBLIC METHODS ___
    public void AddGravitySource(GravitySource moon)
    {
        // Check if it already exists
        if(gravitySources.ContainsKey(moon.source))
        {
            GravitySource existingSource = gravitySources[moon.source];
            existingSource.gravity = moon.gravity;
            existingSource.range = moon.range;
            existingSource.maxGravityPoint = Mathf.Clamp01(moon.maxGravityPoint); // Ensure it's between 0 and 1
            gravitySources[moon.source] = existingSource; // Update the dictionary entry
            return;
        }

        gravitySources.Add(moon.source, moon);
    }

    public void RemoveGravitySource(Transform Source)
    {
        if (gravitySources.ContainsKey(Source))
        {
            gravitySources.Remove(Source);
        }
        else
        {
            Debug.LogWarning($"Gravity source {Source.name} does not exist.");
        }
    }

    // ___ Gizmo Visualization ___
    private void OnDrawGizmos()
    {
        if (!showGravityRanges && !showGravityStrength)
            return;

        foreach (KeyValuePair<Transform, GravitySource> pair in gravitySources)
        {

            var source = pair.Value;

            Vector3 position = source.source.position;

            // Draw range circle
            if (showGravityRanges)
            {
                Gizmos.color = rangeColor;

                if (use2DVisualization)
                {
                    // For 2.5D games, draw a circle on the XY plane
                    DrawCircleXY(position, source.range, visualizationDetail);
                }
                else
                {
                    // Draw a traditional circle on XZ plane
                    DrawCircleXZ(position, source.range, visualizationDetail);
                }
            }

            // Draw strength point circle
            if (showGravityStrength)
            {
                Gizmos.color = strengthColor;
                float strengthRadius = source.range * source.maxGravityPoint;

                if (use2DVisualization)
                {
                    // For 2.5D games, draw a circle on the XY plane
                    DrawCircleXY(position, strengthRadius, visualizationDetail);
                }
                else
                {
                    // Draw a traditional circle on XZ plane
                    DrawCircleXZ(position, strengthRadius, visualizationDetail);
                }
            }
        }
    }

    // Helper method to draw a circle using Gizmos on XZ plane (traditional top-down view)
    private void DrawCircleXZ(Vector3 center, float radius, int segments)
    {
        if (segments < 3)
            segments = 3;

        float angleStep = 360f / segments;

        // Draw circle segments
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = ((i + 1) % segments) * angleStep * Mathf.Deg2Rad;

            Vector3 point1 = new Vector3(
                center.x + Mathf.Cos(angle1) * radius,
                center.y,
                center.z + Mathf.Sin(angle1) * radius
            );

            Vector3 point2 = new Vector3(
                center.x + Mathf.Cos(angle2) * radius,
                center.y,
                center.z + Mathf.Sin(angle2) * radius
            );

            Gizmos.DrawLine(point1, point2);
        }
    }

    // Helper method to draw a circle using Gizmos on XY plane (for 2.5D side view)
    private void DrawCircleXY(Vector3 center, float radius, int segments)
    {
        if (segments < 3)
            segments = 3;

        float angleStep = 360f / segments;

        // Draw circle segments
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = ((i + 1) % segments) * angleStep * Mathf.Deg2Rad;

            Vector3 point1 = new Vector3(
                center.x + Mathf.Cos(angle1) * radius,
                center.y + Mathf.Sin(angle1) * radius,
                center.z
            );

            Vector3 point2 = new Vector3(
                center.x + Mathf.Cos(angle2) * radius,
                center.y + Mathf.Sin(angle2) * radius,
                center.z
            );

            Gizmos.DrawLine(point1, point2);
        }
    }
}
