using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class PathfindingSettings
{
    [Header("A* Pathfinding Settings")]
    [Tooltip("Maximum number of nodes to search before giving up")]
    public int maxSearchNodes = 1000;
    [Tooltip("Use diagonal movement in pathfinding")]
    public bool allowDiagonalMovement = true;
    [Tooltip("Cost multiplier for diagonal movement (should be ~1.414 for realistic diagonal cost)")]
    public float diagonalCost = 1.414f;
    [Tooltip("How close to get to target before considering path complete")]
    public float pathCompletionDistance = 0.1f;
    [Tooltip("How often to check for path invalidation due to blocked waypoints")]
    public float pathValidationInterval = 0.5f;
    [Tooltip("Distance from blocked waypoint to trigger path recalculation")]
    public float blockedWaypointThreshold = 1.0f;

    [Header("Path Following")]
    [Tooltip("How far ahead to look when following path")]
    public float lookAheadDistance = 1.0f;
    [Tooltip("Smooth path following instead of rigid node-to-node movement")]
    public bool enablePathSmoothing = true;

    [Header("Legacy Settings")]
    public int maxPathfindingAttempts = 10;
    public bool allowWalkingOnRoads = true;
    [Tooltip("Allow walking through nature elements like trees and rocks")]
    public bool allowWalkingThroughNature = true;
    [Tooltip("Enable detailed pathfinding logs for debugging")]
    public bool enablePathfindingDebug = false;
    [Tooltip("Maximum radius to search for alternative walkable positions")]
    public int maxRepositionSearchRadius = 10;

    [Header("Fallback Obstacle Avoidance")]
    [Tooltip("How far ahead to check for obstacles (in Unity units)")]
    public float obstacleDetectionDistance = 1.5f;
    [Tooltip("How sharply to steer around obstacles (0-1)")]
    public float steeringForce = 0.7f;
    [Tooltip("Maximum attempts to find alternative direction")]
    public int maxAvoidanceAttempts = 6;

    [Header("Special Walkable Areas")]
    [Tooltip("Enable special walkable corridors that ignore all obstacles")]
    public bool enableWalkableCorridors = true;

    // Static method to get the predefined walkable corridor positions
    public static HashSet<Vector3Int> GetWalkableCorridorPositions()
    {
        var corridorPositions = new HashSet<Vector3Int>();

        // Row at y=31: from (-5, 31, 0) to (9, 31, 0)
        for (int x = -5; x <= 9; x++)
        {
            corridorPositions.Add(new Vector3Int(x, 31, 0));
        }

        // Row at y=28: from (-4, 28, 0) to (11, 28, 0)
        for (int x = -4; x <= 11; x++)
        {
            corridorPositions.Add(new Vector3Int(x, 28, 0));
        }

        // Individual positions
        corridorPositions.Add(new Vector3Int(1, 29, 0));
        corridorPositions.Add(new Vector3Int(1, 30, 0));
        corridorPositions.Add(new Vector3Int(11, 29, 0));
        corridorPositions.Add(new Vector3Int(8, 29, 0));
        corridorPositions.Add(new Vector3Int(8, 30, 0));

        return corridorPositions;
    }
}
public class GridPathfinder
{
    private GridData gridData;
    private PathfindingSettings settings;
    private string debugName;
    private HashSet<Vector3Int> walkableCorridors;

    // A* specific data structures
    private Dictionary<Vector3Int, AStarNode> nodeCache;
    private List<Vector3Int> currentPath;
    private int currentPathIndex;
    private Vector3Int lastTarget;
    private float lastPathValidationTime;
    private Vector3 lastKnownPosition;

    // Neighbor directions for 4-directional and 8-directional movement
    private static readonly Vector3Int[] CardinalDirections = {
        Vector3Int.right,   // (1, 0, 0)
        Vector3Int.left,    // (-1, 0, 0)
        Vector3Int.up,      // (0, 1, 0)
        Vector3Int.down     // (0, -1, 0)
    };

    private static readonly Vector3Int[] DiagonalDirections = {
        new Vector3Int(1, 1, 0),   // Northeast
        new Vector3Int(-1, 1, 0),  // Northwest
        new Vector3Int(1, -1, 0),  // Southeast
        new Vector3Int(-1, -1, 0)  // Southwest
    };

    public GridPathfinder(GridData gridData, PathfindingSettings settings, string debugName = "GridPathfinder")
    {
        this.gridData = gridData;
        this.settings = settings;
        this.debugName = debugName;
        this.walkableCorridors = PathfindingSettings.GetWalkableCorridorPositions();
        this.nodeCache = new Dictionary<Vector3Int, AStarNode>();
        this.currentPath = new List<Vector3Int>();
        this.currentPathIndex = 0;
        this.lastTarget = Vector3Int.zero;
        this.lastPathValidationTime = 0f;
        this.lastKnownPosition = Vector3.zero;
    }

    public void UpdateSettings(PathfindingSettings newSettings)
    {
        this.settings = newSettings;
        this.walkableCorridors = PathfindingSettings.GetWalkableCorridorPositions();
    }

    public void UpdateGridData(GridData newGridData)
    {
        this.gridData = newGridData;
        ClearCache();
    }

    /// <summary>
    /// Main pathfinding method - finds complete path from start to target using A*
    /// </summary>
    public List<Vector3Int> FindPath(Vector3 startWorld, Vector3 targetWorld)
    {
        Vector3Int start = WorldToGridPosition(startWorld);
        Vector3Int target = WorldToGridPosition(targetWorld);

        if (settings.enablePathfindingDebug)
            Debug.Log($"{debugName}: Finding path from {start} to {target}");

        // Quick validation
        if (!IsPositionWalkable(target))
        {
            if (settings.enablePathfindingDebug)
                Debug.Log($"{debugName}: Target position {target} is not walkable");
            return new List<Vector3Int>();
        }

        return FindPathAStar(start, target);
    }

    /// <summary>
    /// A* pathfinding algorithm implementation
    /// </summary>
    private List<Vector3Int> FindPathAStar(Vector3Int start, Vector3Int target)
    {
        // Initialize data structures
        var openSet = new SortedSet<AStarNode>();
        var closedSet = new HashSet<Vector3Int>();
        var inOpenSet = new HashSet<Vector3Int>();

        // Create start node
        AStarNode startNode = GetOrCreateNode(start);
        startNode.SetGCost(0);
        startNode.CalculateHCost(target, settings.allowDiagonalMovement);

        openSet.Add(startNode);
        inOpenSet.Add(start);

        int searchedNodes = 0;

        while (openSet.Count > 0 && searchedNodes < settings.maxSearchNodes)
        {
            searchedNodes++;

            // Get the node with lowest F cost
            AStarNode currentNode = openSet.Min;
            openSet.Remove(currentNode);
            inOpenSet.Remove(currentNode.position);
            closedSet.Add(currentNode.position);

            // Check if we've reached the target
            if (currentNode.position == target)
            {
                if (settings.enablePathfindingDebug)
                    Debug.Log($"{debugName}: Path found! Searched {searchedNodes} nodes");
                return ReconstructPath(currentNode);
            }

            // Explore neighbors
            foreach (var neighbor in GetNeighbors(currentNode.position))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                if (!IsPositionWalkable(neighbor))
                    continue;

                AStarNode neighborNode = GetOrCreateNode(neighbor);

                // Calculate tentative G cost
                float movementCost = GetMovementCost(currentNode.position, neighbor);
                float tentativeGCost = currentNode.gCost + movementCost;

                // If this path to neighbor is better than any previous one
                if (tentativeGCost < neighborNode.gCost)
                {
                    neighborNode.parent = currentNode;
                    neighborNode.SetGCost(tentativeGCost);
                    neighborNode.CalculateHCost(target, settings.allowDiagonalMovement);

                    if (!inOpenSet.Contains(neighbor))
                    {
                        openSet.Add(neighborNode);
                        inOpenSet.Add(neighbor);
                    }
                }
            }
        }

        if (settings.enablePathfindingDebug)
            Debug.Log($"{debugName}: No path found after searching {searchedNodes} nodes");

        return new List<Vector3Int>();
    }

    /// <summary>
    /// Gets movement cost between two adjacent positions
    /// </summary>
    private float GetMovementCost(Vector3Int from, Vector3Int to)
    {
        Vector3Int delta = to - from;

        // Check if it's a diagonal move
        if (Mathf.Abs(delta.x) == 1 && Mathf.Abs(delta.y) == 1)
        {
            return settings.diagonalCost;
        }

        return 1.0f; // Cardinal movement cost
    }

    /// <summary>
    /// Gets all valid neighbors for a position
    /// </summary>
    private List<Vector3Int> GetNeighbors(Vector3Int position)
    {
        var neighbors = new List<Vector3Int>();

        // Add cardinal directions
        foreach (var direction in CardinalDirections)
        {
            neighbors.Add(position + direction);
        }

        // Add diagonal directions if enabled
        if (settings.allowDiagonalMovement)
        {
            foreach (var direction in DiagonalDirections)
            {
                Vector3Int diagonalPos = position + direction;

                // Check if diagonal movement is valid (not cutting corners)
                Vector3Int horizontal = position + new Vector3Int(direction.x, 0, 0);
                Vector3Int vertical = position + new Vector3Int(0, direction.y, 0);

                if (IsPositionWalkable(horizontal) && IsPositionWalkable(vertical))
                {
                    neighbors.Add(diagonalPos);
                }
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Reconstructs the path from the target node back to start
    /// </summary>
    private List<Vector3Int> ReconstructPath(AStarNode targetNode)
    {
        var path = new List<Vector3Int>();
        AStarNode current = targetNode;

        while (current != null)
        {
            path.Add(current.position);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    /// Gets or creates a node for the given position
    /// </summary>
    private AStarNode GetOrCreateNode(Vector3Int position)
    {
        if (!nodeCache.TryGetValue(position, out AStarNode node))
        {
            node = new AStarNode(position, IsPositionWalkable(position));
            nodeCache[position] = node;
        }
        return node;
    }

    /// <summary>
    /// Clears the node cache (call when grid data changes)
    /// </summary>
    public void ClearCache()
    {
        nodeCache.Clear();
        currentPath.Clear();
        currentPathIndex = 0;
        lastTarget = Vector3Int.zero;
        lastPathValidationTime = 0f;
    }

    /// <summary>
    /// Gets the next movement direction using A* pathfinding
    /// This replaces the old obstacle avoidance method
    /// </summary>
    public Vector3 GetAvoidanceDirection(Vector3 currentPos, Vector3 targetPos)
    {
        return GetPathDirection(currentPos, targetPos);
    }

    /// <summary>
    /// Gets the direction to move along the calculated path
    /// </summary>
    public Vector3 GetPathDirection(Vector3 currentWorld, Vector3 targetWorld)
    {
        Vector3Int currentGrid = WorldToGridPosition(currentWorld);
        Vector3Int targetGrid = WorldToGridPosition(targetWorld);

        // Check if we need to recalculate path
        if (ShouldRecalculatePath(currentWorld, targetWorld))
        {
            if (settings.enablePathfindingDebug)
                Debug.Log($"{debugName}: Recalculating path from {currentGrid} to {targetGrid}");

            currentPath = FindPath(currentWorld, targetWorld);
            currentPathIndex = 0;
            lastTarget = targetGrid;
        }

        // Validate path periodically for dynamic obstacles
        if (Time.time - lastPathValidationTime > settings.pathValidationInterval)
        {
            if (ValidateCurrentPath(currentWorld))
            {
                lastPathValidationTime = Time.time;
            }
            else
            {
                if (settings.enablePathfindingDebug)
                    Debug.Log($"{debugName}: Path invalidated due to blocked waypoints, recalculating");

                currentPath = FindPath(currentWorld, targetWorld);
                currentPathIndex = 0;
                lastPathValidationTime = Time.time;
            }
        }

        if (currentPath.Count == 0)
        {
            if (settings.enablePathfindingDebug)
                Debug.Log($"{debugName}: No path available, using fallback");
            return GetFallbackDirection(currentWorld, targetWorld);
        }

        // Update path index based on current position
        UpdatePathIndex(currentWorld);

        // Check if we're stuck (not making progress)
        if (IsStuck(currentWorld))
        {
            if (settings.enablePathfindingDebug)
                Debug.Log($"{debugName}: Villager appears stuck, forcing path recalculation");

            currentPath = FindPath(currentWorld, targetWorld);
            currentPathIndex = 0;
        }

        // Update last known position
        lastKnownPosition = currentWorld;

        // Get next waypoint
        if (currentPathIndex >= currentPath.Count - 1)
        {
            // We've reached the end of the path
            Vector3 finalTarget = GridToWorldPosition(currentPath[currentPath.Count - 1]);
            Vector3 toTarget = (finalTarget - currentWorld).normalized;

            if (Vector3.Distance(currentWorld, finalTarget) < settings.pathCompletionDistance)
            {
                return Vector3.zero; // We've arrived
            }

            return toTarget;
        }

        // Get direction to next waypoint
        Vector3Int nextWaypoint = currentPath[currentPathIndex + 1];
        Vector3 nextWorldPos = GridToWorldPosition(nextWaypoint);

        // Check if the next waypoint is blocked
        if (!IsPositionWalkable(nextWaypoint))
        {
            if (settings.enablePathfindingDebug)
                Debug.Log($"{debugName}: Next waypoint {nextWaypoint} is blocked, recalculating path");

            currentPath = FindPath(currentWorld, targetWorld);
            currentPathIndex = 0;

            if (currentPath.Count == 0)
            {
                return GetFallbackDirection(currentWorld, targetWorld);
            }

            if (currentPathIndex + 1 < currentPath.Count)
            {
                nextWaypoint = currentPath[currentPathIndex + 1];
                nextWorldPos = GridToWorldPosition(nextWaypoint);
            }
        }

        if (settings.enablePathSmoothing)
        {
            // Look ahead for smoother movement
            Vector3Int lookAheadWaypoint = GetLookAheadWaypoint(currentWorld);
            nextWorldPos = GridToWorldPosition(lookAheadWaypoint);
        }

        Vector3 direction = (nextWorldPos - currentWorld).normalized;

        if (settings.enablePathfindingDebug)
            Debug.Log($"{debugName}: Following path - current waypoint: {currentPathIndex}, direction: {direction}, next pos: {nextWorldPos}");

        return direction;
    }

    /// <summary>
    /// Determines if we should recalculate the path
    /// </summary>
    private bool ShouldRecalculatePath(Vector3 currentWorld, Vector3 targetWorld)
    {
        Vector3Int currentGrid = WorldToGridPosition(currentWorld);
        Vector3Int targetGrid = WorldToGridPosition(targetWorld);

        // Recalculate if no path exists
        if (currentPath.Count == 0)
            return true;

        // Recalculate if target has changed significantly
        if (Vector3Int.Distance(lastTarget, targetGrid) > 0.5f)
            return true;

        // Recalculate if current position is far from path
        if (!IsOnPath(currentGrid))
        {
            if (settings.enablePathfindingDebug)
                Debug.Log($"{debugName}: Current position {currentGrid} is not on path, recalculating");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Updates the current path index based on position
    /// </summary>
    private void UpdatePathIndex(Vector3 currentWorld)
    {
        Vector3Int currentGrid = WorldToGridPosition(currentWorld);

        for (int i = currentPathIndex; i < currentPath.Count; i++)
        {
            if (currentPath[i] == currentGrid)
            {
                currentPathIndex = i;
                return;
            }

            Vector3 waypointWorld = GridToWorldPosition(currentPath[i]);
            if (Vector3.Distance(currentWorld, waypointWorld) < settings.pathCompletionDistance)
            {
                currentPathIndex = i;
                return;
            }
        }
    }

    /// <summary>
    /// Checks if current position is reasonably close to the path
    /// </summary>
    private bool IsOnPath(Vector3Int position)
    {
        if (currentPath.Count == 0) return false;

        // Check if we're close to any waypoint in a reasonable range around current index
        int searchStart = Mathf.Max(0, currentPathIndex - 2);
        int searchEnd = Mathf.Min(currentPath.Count, currentPathIndex + 4);

        for (int i = searchStart; i < searchEnd; i++)
        {
            if (Vector3Int.Distance(position, currentPath[i]) <= 1.5f)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Validates the current path to see if any upcoming waypoints are blocked
    /// </summary>
    private bool ValidateCurrentPath(Vector3 currentWorld)
    {
        if (currentPath.Count == 0) return false;

        // Check the next few waypoints to see if they're still walkable
        int checkAhead = Mathf.Min(3, currentPath.Count - currentPathIndex - 1);

        for (int i = 1; i <= checkAhead; i++)
        {
            int waypointIndex = currentPathIndex + i;
            if (waypointIndex >= currentPath.Count) break;

            Vector3Int waypoint = currentPath[waypointIndex];
            Vector3 waypointWorld = GridToWorldPosition(waypoint);

            // If we're close to this waypoint and it's blocked, path is invalid
            if (Vector3.Distance(currentWorld, waypointWorld) < settings.blockedWaypointThreshold)
            {
                if (!IsPositionWalkable(waypoint))
                {
                    if (settings.enablePathfindingDebug)
                        Debug.Log($"{debugName}: Waypoint {waypoint} at index {waypointIndex} is blocked and we're close to it");
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if the villager appears to be stuck (not making progress)
    /// </summary>
    private bool IsStuck(Vector3 currentWorld)
    {
        // If we don't have a previous position, we're not stuck
        if (lastKnownPosition == Vector3.zero)
            return false;

        // Check if we've moved very little since last frame
        float distanceMoved = Vector3.Distance(currentWorld, lastKnownPosition);

        // If we're barely moving and have a path, we might be stuck
        if (distanceMoved < 0.01f && currentPath.Count > 0 && currentPathIndex < currentPath.Count - 1)
        {
            // Check if the next waypoint is actually reachable
            Vector3Int nextWaypoint = currentPath[currentPathIndex + 1];
            if (!IsPositionWalkable(nextWaypoint))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets a waypoint that's further ahead for smoother movement
    /// </summary>
    private Vector3Int GetLookAheadWaypoint(Vector3 currentWorld)
    {
        if (currentPathIndex >= currentPath.Count - 1)
            return currentPath[currentPath.Count - 1];

        Vector3 currentPos = currentWorld;

        for (int i = currentPathIndex + 1; i < currentPath.Count; i++)
        {
            Vector3 waypointWorld = GridToWorldPosition(currentPath[i]);
            if (Vector3.Distance(currentPos, waypointWorld) >= settings.lookAheadDistance)
            {
                return currentPath[i];
            }
        }

        return currentPath[currentPath.Count - 1];
    }

    /// <summary>
    /// Fallback direction calculation using simple obstacle avoidance
    /// </summary>
    private Vector3 GetFallbackDirection(Vector3 currentPos, Vector3 targetPos)
    {
        Vector3 desiredDirection = (targetPos - currentPos).normalized;

        // Check if direct path is clear
        if (IsDirectionClear(currentPos, desiredDirection))
        {
            return desiredDirection;
        }

        // Try to find alternative direction using old obstacle avoidance
        return FindAvoidanceDirection(currentPos, desiredDirection);
    }

    /// <summary>
    /// Legacy obstacle avoidance for fallback
    /// </summary>
    private Vector3 FindAvoidanceDirection(Vector3 currentPos, Vector3 desiredDirection)
    {
        float[] angleOffsets = { 45f, -45f, 90f, -90f, 135f, -135f };

        for (int i = 0; i < angleOffsets.Length && i < settings.maxAvoidanceAttempts; i++)
        {
            Vector3 testDirection = RotateVector(desiredDirection, angleOffsets[i]);

            if (IsDirectionClear(currentPos, testDirection))
            {
                Vector3 blendedDirection = Vector3.Lerp(desiredDirection, testDirection, settings.steeringForce).normalized;

                if (IsDirectionClear(currentPos, blendedDirection))
                {
                    return blendedDirection;
                }
                else
                {
                    return testDirection;
                }
            }
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Checks if movement in a direction is clear of obstacles
    /// </summary>
    private bool IsDirectionClear(Vector3 position, Vector3 direction)
    {
        Vector3 checkPosition = position + direction * settings.obstacleDetectionDistance;
        return IsPositionWalkable(checkPosition);
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

        // CHECK FIRST: Is this position in a walkable corridor? If so, always allow walking
        if (settings.enableWalkableCorridors && walkableCorridors.Contains(gridPosition))
        {
            if (settings.enablePathfindingDebug)
                Debug.Log($"{debugName}: Position {gridPosition} is in walkable corridor - always accessible");
            return true;
        }

        // Check if position is within map boundaries
        if (!gridData.mapBoundaries.Contains(gridPosition))
        {
            if (settings.enablePathfindingDebug)
                Debug.Log($"{debugName}: Position {gridPosition} outside map boundaries");
            return false;
        }

        // Check if position has nature (trees, rocks, etc.) - NOW OPTIONAL
        if (!settings.allowWalkingThroughNature && gridData.positionHasNature.Contains(gridPosition))
        {
            if (settings.enablePathfindingDebug)
                Debug.Log($"{debugName}: Position {gridPosition} has nature elements (walking through nature disabled)");
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

        // Otherwise, position is blocked by a building/structure
        if (settings.enablePathfindingDebug)
            Debug.Log($"{debugName}: Position {gridPosition} blocked by object ID {id}");
        return false;
    }

    /// <summary>
    /// Checks if a position is in a walkable corridor (useful for external systems)
    /// </summary>
    public bool IsInWalkableCorridor(Vector3Int gridPosition)
    {
        return settings.enableWalkableCorridors && walkableCorridors.Contains(gridPosition);
    }

    /// <summary>
    /// Checks if a world position is in a walkable corridor
    /// </summary>
    public bool IsInWalkableCorridor(Vector3 worldPosition)
    {
        return IsInWalkableCorridor(WorldToGridPosition(worldPosition));
    }

    /// <summary>
    /// Gets all walkable corridor positions (useful for debugging or visualization)
    /// </summary>
    public HashSet<Vector3Int> GetWalkableCorridorPositions()
    {
        return new HashSet<Vector3Int>(walkableCorridors);
    }

    /// <summary>
    /// Gets the current calculated path (useful for debugging)
    /// </summary>
    public List<Vector3Int> GetCurrentPath()
    {
        return new List<Vector3Int>(currentPath);
    }

    /// <summary>
    /// Forces a path recalculation on next movement request
    /// </summary>
    public void InvalidatePath()
    {
        currentPath.Clear();
        currentPathIndex = 0;
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