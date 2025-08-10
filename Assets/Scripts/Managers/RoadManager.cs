using System;
using System.Collections.Generic;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    [SerializeField] private PlacementSystem placementSystem;

    // Road type constants for better readability
    private const int VERTICAL_ROAD = 0;
    private const int HORIZONTAL_ROAD_LEFT = 1;
    private const int HORIZONTAL_ROAD_RIGHT = 2;
    private const int DEAD_END_UP = 3;
    private const int DEAD_END_DOWN = 4;
    private const int DEAD_END_LEFT = 5;
    private const int DEAD_END_RIGHT = 6;
    private const int CORNER_TOP_RIGHT = 7;
    private const int CORNER_BOTTOM_RIGHT = 8;
    private const int CORNER_BOTTOM_LEFT = 9;
    private const int CORNER_TOP_LEFT = 10;
    private const int THREE_WAY_TOP_RIGHT_LEFT = 11;
    private const int THREE_WAY_BOTTOM_RIGHT_LEFT = 12;
    private const int THREE_WAY_TOP_RIGHT_BOTTOM = 13;
    private const int THREE_WAY_TOP_LEFT_BOTTOM = 14;
    private const int FOUR_WAY_INTERSECTION = 15;

    // Direction indices for neighbor array
    private const int RIGHT = 0;
    private const int LEFT = 1;
    private const int TOP = 2;
    private const int BOTTOM = 3;

    public void FixRoadAt(Vector3Int position, bool[] neighbourRoads)
    {
        int roadCount = CountConnectedRoads(neighbourRoads);
        
        switch (roadCount)
        {
            case 0:
                placementSystem.PlaceStructureAt(VERTICAL_ROAD, position);
                break;
            case 1:
                PlaceDeadEnd(neighbourRoads, position);
                break;
            case 2:
                PlaceTwoWayRoad(neighbourRoads, position);
                break;
            case 3:
                PlaceThreeWay(neighbourRoads, position);
                break;
            case 4:
                placementSystem.PlaceStructureAt(FOUR_WAY_INTERSECTION, position);
                break;
        }
    }

    private static int CountConnectedRoads(bool[] neighbourRoads)
    {
        int count = 0;
        for (int i = 0; i < neighbourRoads.Length; i++)
        {
            if (neighbourRoads[i]) count++;
        }
        return count;
    }

    private void PlaceTwoWayRoad(bool[] neighbourRoads, Vector3Int position)
    {
        // Horizontal road (right and left)
        if (neighbourRoads[RIGHT] && neighbourRoads[LEFT])
        {
            // Choose horizontal road type based on position
            int roadType = position.x < 0 ? HORIZONTAL_ROAD_LEFT : HORIZONTAL_ROAD_RIGHT;
            placementSystem.PlaceStructureAt(roadType, position);
        }
        // Vertical road (top and bottom)
        else if (neighbourRoads[TOP] && neighbourRoads[BOTTOM])
        {
            placementSystem.PlaceStructureAt(VERTICAL_ROAD, position);
        }
        // Corner configurations
        else
        {
            PlaceCorner(neighbourRoads, position);
        }
    }

    private void PlaceThreeWay(bool[] neighbourRoads, Vector3Int position)
    {
        // Use bit pattern matching for cleaner logic
        bool hasRight = neighbourRoads[RIGHT];
        bool hasLeft = neighbourRoads[LEFT];
        bool hasTop = neighbourRoads[TOP];
        bool hasBottom = neighbourRoads[BOTTOM];

        if (hasTop && hasRight && hasLeft)
        {
            placementSystem.PlaceStructureAt(THREE_WAY_TOP_RIGHT_LEFT, position);
        }
        else if (hasTop && hasRight && hasBottom)
        {
            placementSystem.PlaceStructureAt(THREE_WAY_TOP_RIGHT_BOTTOM, position);
        }
        else if (hasTop && hasLeft && hasBottom)
        {
            placementSystem.PlaceStructureAt(THREE_WAY_TOP_LEFT_BOTTOM, position);
        }
        else if (hasBottom && hasRight && hasLeft)
        {
            placementSystem.PlaceStructureAt(THREE_WAY_BOTTOM_RIGHT_LEFT, position);
        }
    }

    private void PlaceCorner(bool[] neighbourRoads, Vector3Int position)
    {
        bool hasRight = neighbourRoads[RIGHT];
        bool hasLeft = neighbourRoads[LEFT];
        bool hasTop = neighbourRoads[TOP];
        bool hasBottom = neighbourRoads[BOTTOM];

        if (hasTop && hasRight)
        {
            placementSystem.PlaceStructureAt(CORNER_TOP_RIGHT, position);
        }
        else if (hasTop && hasLeft)
        {
            placementSystem.PlaceStructureAt(CORNER_TOP_LEFT, position);
        }
        else if (hasBottom && hasRight)
        {
            placementSystem.PlaceStructureAt(CORNER_BOTTOM_RIGHT, position);
        }
        else if (hasBottom && hasLeft)
        {
            placementSystem.PlaceStructureAt(CORNER_BOTTOM_LEFT, position);
        }
    }

    private void PlaceDeadEnd(bool[] neighbourRoads, Vector3Int position)
    {
        if (neighbourRoads[RIGHT])
        {
            placementSystem.PlaceStructureAt(DEAD_END_LEFT, position);
        }
        else if (neighbourRoads[LEFT])
        {
            placementSystem.PlaceStructureAt(DEAD_END_RIGHT, position);
        }
        else if (neighbourRoads[TOP])
        {
            placementSystem.PlaceStructureAt(DEAD_END_DOWN, position);
        }
        else if (neighbourRoads[BOTTOM])
        {
            placementSystem.PlaceStructureAt(DEAD_END_UP, position);
        }
    }

    public void FixNeighbouringRoadsAt(IReadOnlyList<Vector3Int> neighbourPositions)
    {
        // Process each neighbor position
        foreach (var neighbourPosition in neighbourPositions)
        {
            placementSystem.RemoveStructureAt(neighbourPosition);
            var neighbourRoads = placementSystem.gridData.GetNeighbouringRoads(neighbourPosition);
            FixRoadAt(neighbourPosition, neighbourRoads);
        }
    }

    // Overload for backward compatibility with List<Vector3Int>
    public void FixNeighbouringRoadsAt(List<Vector3Int> neighbourPositions)
    {
        FixNeighbouringRoadsAt((IReadOnlyList<Vector3Int>)neighbourPositions);
    }

    // Utility method for debugging road connections
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void LogRoadConnections(Vector3Int position)
    {
        var roads = placementSystem.gridData.GetNeighbouringRoads(position);
        Debug.Log($"Road connections at {position}: Right={roads[RIGHT]}, Left={roads[LEFT]}, Top={roads[TOP]}, Bottom={roads[BOTTOM]}");
    }
}

// Extension methods for better road type handling (optional)
public static class RoadTypeExtensions
{
    public static bool IsDeadEnd(this int roadType)
    {
        return roadType >= 3 && roadType <= 6;
    }

    public static bool IsCorner(this int roadType)
    {
        return roadType >= 7 && roadType <= 10;
    }

    public static bool IsThreeWay(this int roadType)
    {
        return roadType >= 11 && roadType <= 14;
    }

    public static bool IsFourWay(this int roadType)
    {
        return roadType == 15;
    }

    public static bool IsStraight(this int roadType)
    {
        return roadType >= 0 && roadType <= 2;
    }
}