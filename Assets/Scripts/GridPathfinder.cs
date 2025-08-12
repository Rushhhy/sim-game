using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathfindingSettings
{
    [Header("Pathfinding Settings")]
    public int maxPathfindingAttempts = 10;
    public bool allowWalkingOnRoads = true;
    [Tooltip("Enable detailed pathfinding logs for debugging")]
    public bool enablePathfindingDebug = true;
    [Tooltip("Maximum radius to search for alternative walkable positions")]
    public int maxRepositionSearchRadius = 10;

    [Header("Obstacle Avoidance")]
    [Tooltip("How far ahead to check for obstacles (in Unity units)")]
    public float obstacleDetectionDistance = 1.5f;
    [Tooltip("How sharply to steer around obstacles (0-1)")]
    public float steeringForce = 0.7f;
    [Tooltip("Maximum attempts to find alternative direction")]
    public int maxAvoidanceAttempts = 6;
}

public class GridPathfinder
{
    private GridData gridData;
    private PathfindingSettings settings;
    private string debugName;

    public GridPathfinder(GridData gridData, PathfindingSettings settings, string debugName = "GridPathfinder")
    {
        this.gridData = gridData;
        this.settings = settings;
        this.debugName = debugName;
    }

    public void UpdateSettings(PathfindingSettings newSettings)
    {
        this.settings = newSettings;
    }

    public void UpdateGridData(GridData newGridData)
    {
        this.gridData = newGridData;
    }

    /// <summary>
    /// Gets the next movement direction with obstacle avoidance
    /// Returns the direction to move in, or Vector3.zero if no valid direction found
    /// </summary>
    public Vector3 GetAvoidanceDirection(Vector3 currentPos, Vector3 targetPos)
    {
        Vector3 desiredDirection = (targetPos - currentPos).normalized;

        if (settings.enablePathfindingDebug)
            Debug.Log($"{debugName}: Checking direction from {currentPos} to {targetPos}");

        // Check if direct path is clear
        if (IsDirectionClear(currentPos, desiredDirection))
        {
            if (settings.enablePathfindingDebug)
                Debug.Log($"{debugName}: Direct path clear, using desired direction");
            return desiredDirection;
        }

        if (settings.enablePathfindingDebug)
            Debug.Log($"{debugName}: Direct path blocked, searching for alternative");

        // Try to find alternative direction
        Vector3 avoidanceDirection = FindAvoidanceDirection(currentPos, desiredDirection);

        if (avoidanceDirection != Vector3.zero)
        {
            if (settings.enablePathfindingDebug)
                Debug.Log($"{debugName}: Found alternative direction: {avoidanceDirection}");
            return avoidanceDirection;
        }

        if (settings.enablePathfindingDebug)
            Debug.Log($"{debugName}: No valid direction found - all paths blocked");
        return Vector3.zero;
    }

    /// <summary>
    /// Checks if movement in a direction is clear of obstacles
    /// </summary>
    private bool IsDirectionClear(Vector3 position, Vector3 direction)
    {
        Vector3 checkPosition = position + direction * settings.obstacleDetectionDistance;
        bool isWalkable = IsPositionWalkable(checkPosition);

        if (settings.enablePathfindingDebug && !isWalkable)
        {
            Debug.Log($"{debugName}: Direction blocked at {checkPosition}");
        }

        return isWalkable;
    }

    /// <summary>
    /// Finds an alternative direction to avoid obstacles
    /// </summary>
    private Vector3 FindAvoidanceDirection(Vector3 currentPos, Vector3 desiredDirection)
    {
        // Try different angles around the desired direction
        float[] angleOffsets = { 45f, -45f, 90f, -90f, 135f, -135f };

        for (int i = 0; i < angleOffsets.Length && i < settings.maxAvoidanceAttempts; i++)
        {
            Vector3 testDirection = RotateVector(desiredDirection, angleOffsets[i]);

            if (settings.enablePathfindingDebug)
                Debug.Log($"{debugName}: Testing {angleOffsets[i]}° angle: {testDirection}");

            if (IsDirectionClear(currentPos, testDirection))
            {
                if (settings.enablePathfindingDebug)
                    Debug.Log($"{debugName}: Found clear path at {angleOffsets[i]}°");

                // Blend with desired direction for smoother movement
                Vector3 blendedDirection = Vector3.Lerp(desiredDirection, testDirection, settings.steeringForce).normalized;

                // Check if blended direction is also clear
                if (IsDirectionClear(currentPos, blendedDirection))
                {
                    if (settings.enablePathfindingDebug)
                        Debug.Log($"{debugName}: Using blended direction: {blendedDirection}");
                    return blendedDirection;
                }
                else
                {
                    if (settings.enablePathfindingDebug)
                        Debug.Log($"{debugName}: Blended blocked, using pure avoidance: {testDirection}");
                    return testDirection;
                }
            }
        }

        if (settings.enablePathfindingDebug)
            Debug.Log($"{debugName}: All tested angles are blocked");
        return Vector3.zero;
    }

    /// <summary>
    /// Rotates a 2D vector by the specified angle in degrees
    /// </summary>
    private Vector3 RotateVector(Vector3 vector, float angleDegrees)
    {
        float angleRad = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);

        return new Vector3(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos,
            vector.z
        );
    }

    /// <summary>
    /// Finds a random walkable position within the specified radius from the center point
    /// </summary>
    public Vector3? FindRandomWalkablePosition(Vector3 centerWorldPos, float radius)
    {
        for (int attempt = 0; attempt < settings.maxPathfindingAttempts; attempt++)
        {
            Vector2 randomDirection = Random.insideUnitCircle * radius;
            Vector3 potentialTarget = centerWorldPos + new Vector3(randomDirection.x, randomDirection.y, 0);

            if (IsPositionWalkable(potentialTarget))
            {
                if (settings.enablePathfindingDebug)
                    Debug.Log($"{debugName}: Found random walkable position at {potentialTarget}");
                return potentialTarget;
            }
        }

        if (settings.enablePathfindingDebug)
            Debug.Log($"{debugName}: No random walkable position found within radius {radius}");
        return null;
    }

    /// <summary>
    /// Finds the nearest walkable position to a blocked position
    /// </summary>
    public Vector3? FindNearestWalkablePosition(Vector3 blockedWorldPos)
    {
        Vector3Int blockedGridPos = WorldToGridPosition(blockedWorldPos);
        Vector3Int? walkableGridPos = FindNearestWalkableGridPosition(blockedGridPos);

        if (walkableGridPos.HasValue)
        {
            return GridToWorldPosition(walkableGridPos.Value);
        }

        return null;
    }

    /// <summary>
    /// Finds the nearest walkable grid position to a blocked grid position
    /// </summary>
    private Vector3Int? FindNearestWalkableGridPosition(Vector3Int blockedPosition)
    {
        // Spiral search outward from the blocked position
        for (int radius = 1; radius <= settings.maxRepositionSearchRadius; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    // Only check positions on the edge of the current radius
                    if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius) continue;

                    Vector3Int checkPos = blockedPosition + new Vector3Int(x, y, 0);
                    if (IsPositionWalkable(checkPos))
                    {
                        return checkPos;
                    }
                }
            }
        }

        if (settings.enablePathfindingDebug)
            Debug.Log($"{debugName}: No walkable position found within {settings.maxRepositionSearchRadius} cells");
        return null;
    }

    /// <summary>
    /// Checks if a world position is walkable
    /// </summary>
    public bool IsPositionWalkable(Vector3 worldPosition)
    {
        return IsPositionWalkable(WorldToGridPosition(worldPosition));
    }

    /// <summary>
    /// Checks if a grid position is walkable
    /// </summary>
    public bool IsPositionWalkable(Vector3Int gridPosition)
    {
        if (gridData == null)
        {
            if (settings.enablePathfindingDebug)
                Debug.LogWarning($"{debugName}: GridData is null, assuming position is walkable");
            return true;
        }

        // Check if position is within map boundaries
        if (!gridData.mapBoundaries.Contains(gridPosition))
        {
            if (settings.enablePathfindingDebug)
                Debug.Log($"{debugName}: Position {gridPosition} outside map boundaries");
            return false;
        }

        // Check if position has nature (trees, rocks, etc.)
        if (gridData.positionHasNature.Contains(gridPosition))
        {
            if (settings.enablePathfindingDebug)
                Debug.Log($"{debugName}: Position {gridPosition} has nature elements");
            return false;
        }

        // Check for placed objects
        int id = gridData.GetIDAtPosition(gridPosition);

        // If no object at position, it's walkable
        if (id == -1)
        {
            return true;
        }

        // If roads are walkable and this is a road, allow it
        if (settings.allowWalkingOnRoads && IsRoadID(id))
        {
            return true;
        }

        // Otherwise, position is blocked
        if (settings.enablePathfindingDebug)
            Debug.Log($"{debugName}: Position {gridPosition} blocked by object ID {id}");
        return false;
    }

    /// <summary>
    /// Converts world position to grid position
    /// </summary>
    public Vector3Int WorldToGridPosition(Vector3 worldPosition)
    {
        return new Vector3Int(
            Mathf.FloorToInt(worldPosition.x),
            Mathf.FloorToInt(worldPosition.y),
            0
        );
    }

    /// <summary>
    /// Converts grid position to world position (center of cell)
    /// </summary>
    public Vector3 GridToWorldPosition(Vector3Int gridPosition)
    {
        return new Vector3(
            gridPosition.x + 0.5f,
            gridPosition.y + 0.5f,
            0
        );
    }

    private bool IsRoadID(int id)
    {
        // Road ID range - adjust this based on your GridData class
        return id >= 0 && id <= 15;
    }
}