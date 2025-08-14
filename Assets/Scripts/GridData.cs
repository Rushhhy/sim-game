using System;
using System.Collections.Generic;
using UnityEngine;

public class GridData
{
    Dictionary<Vector3Int, PlacementData> placedObjects = new();
    Dictionary<Vector3Int, PlacementData> placedDecorations = new();

    // Road ID range constants
    private const int MIN_ROAD_ID = 0;
    private const int MAX_ROAD_ID = 15;

    // Can place - space that area could not fill
    public HashSet<Vector3Int> mapBoundaries = new HashSet<Vector3Int>()
    {
        new Vector3Int(1, 29, 0), new Vector3Int(0, 31, 0), new Vector3Int(1, 31, 0), new Vector3Int(2, 31, 0), new Vector3Int(-5, 31, 0), new Vector3Int(-5, 30, 0), new Vector3Int(-5, 29, 0), new Vector3Int(7, 31, 0),
        new Vector3Int(8, 31, 0), new Vector3Int(19, 34, 0), new Vector3Int(19, 35, 0), new Vector3Int(22, 40, 0), new Vector3Int(22, 41, 0), new Vector3Int(22, 42, 0), new Vector3Int(19, 51, 0), new Vector3Int(19, 52, 0), new Vector3Int(8, 56, 0), new Vector3Int(9, 56, 0),
        new Vector3Int(10, 56, 0), new Vector3Int(11, 56, 0), new Vector3Int(12, 56, 0), new Vector3Int(1, 56, 0), new Vector3Int(2, 56, 0), new Vector3Int(3, 56, 0), new Vector3Int(-11, 56, 0), new Vector3Int(-10, 56, 0), new Vector3Int(-9, 56, 0), new Vector3Int(-20, 56, 0),
        new Vector3Int(-19, 56, 0), new Vector3Int(-18, 56, 0), new Vector3Int(-17, 56, 0), new Vector3Int(-16, 56, 0), new Vector3Int(-23, 54, 0), new Vector3Int(-27, 53, 0), new Vector3Int(-26, 53, 0), new Vector3Int(-29, 53, 0), new Vector3Int(-29, 52, 0), new Vector3Int(-31, 30, 0),
        new Vector3Int(-31, 29, 0), new Vector3Int(-31, 28, 0), new Vector3Int(-31, 27, 0), new Vector3Int(-31, 26, 0), new Vector3Int(-32, 28, 0), new Vector3Int(-32, 29, 0), new Vector3Int(-24, 24, 0), new Vector3Int(-23, 24, 0), new Vector3Int(-22, 24, 0), new Vector3Int(-21, 24, 0),
        new Vector3Int(-20, 24, 0), new Vector3Int(-19, 24, 0), new Vector3Int(-18, 24, 0), new Vector3Int(-28, 36, 0), new Vector3Int(-23, 53, 0), new Vector3Int(13, 33, 0), new Vector3Int(8, 28, 0), new Vector3Int(9, 28, 0),  new Vector3Int(10, 28, 0), new Vector3Int(11, 28, 0), 
        new Vector3Int(9, 31, 0), new Vector3Int(0, 30, 0), new Vector3Int(2, 30, 0), new Vector3Int(7, 30, 0), new Vector3Int(9, 30, 0)
    };
    // Cannot place - obstacles that area filled
    public HashSet<Vector3Int> positionsToRemove = new HashSet<Vector3Int>()
    {
        new Vector3Int(-5, 29, 0), new Vector3Int(-24, 32, 0), new Vector3Int(-23, 32, 0), new Vector3Int(-19, 32, 0), new Vector3Int(-18, 32, 0), new Vector3Int(12, 35, 0), new Vector3Int(13, 35, 0),
        new Vector3Int(17, 35, 0), new Vector3Int(18, 35, 0), new Vector3Int(-24, 53, 0), new Vector3Int(-23, 54, 0), new Vector3Int(-22, 54, 0), new Vector3Int(-21, 54, 0), new Vector3Int(-20, 54, 0),
        new Vector3Int(-20, 53, 0), new Vector3Int(-18, 56, 0), new Vector3Int(-19, 55, 0), new Vector3Int(-19, 54, 0), new Vector3Int(-20, 55, 0), new Vector3Int(-20, 56, 0), new Vector3Int(-19 , 56, 0),
        new Vector3Int(-20, 57, 0), new Vector3Int(-21, 56, 0), new Vector3Int(-22, 56, 0), new Vector3Int(-23, 56, 0), new Vector3Int(-24, 55, 0), new Vector3Int(-24, 56, 0), new Vector3Int(9, 56, 0),
        new Vector3Int(9, 55, 0), new Vector3Int(9, 54, 0), new Vector3Int(10, 56, 0), new Vector3Int(10, 55, 0), new Vector3Int(10, 54, 0), new Vector3Int(10, 53, 0), new Vector3Int(11, 56, 0), 
        new Vector3Int(11, 55, 0), new Vector3Int(12, 56, 0), new Vector3Int(12, 55, 0), new Vector3Int(13, 56, 0), new Vector3Int(13, 55, 0), new Vector3Int(14, 56, 0), new Vector3Int(14, 55, 0),
        new Vector3Int(14, 54, 0), new Vector3Int(14 ,53, 0), new Vector3Int(11, 53, 0), new Vector3Int(12, 54, 0), new Vector3Int(11, 54, 0), new Vector3Int(8, 56, 0), new Vector3Int(14, 35, 0), 
        new Vector3Int(-23, 53, 0), new Vector3Int(-21, 53, 0), new Vector3Int(-22, 32, 0), new Vector3Int(-20, 32, 0), new Vector3Int(16, 35, 0), new Vector3Int(-8, 54, 0), new Vector3Int(-7, 54, 0),
        new Vector3Int(-6, 54, 0), new Vector3Int(-4, 54, 0), new Vector3Int(-3, 54, 0), new Vector3Int(-2, 54, 0), new Vector3Int(10, 32, 0), new Vector3Int(11, 32, 0), new Vector3Int(9, 29, 0), new Vector3Int(7, 29, 0)
    };

    // this is filled in TreeLayerOrderSetter class - Scan all trees and rocks on map and add
    public HashSet<Vector3Int> positionHasNature = new HashSet<Vector3Int>()
    {

    };

    private static readonly Vector3Int[] NeighborDirections = {
        Vector3Int.right,   // Right
        Vector3Int.left,    // Left
        Vector3Int.up,      // Top
        Vector3Int.down     // Bottom
    };

    // Right, Left, Top, Bottom
    public bool[] GetNeighbouringRoads(Vector3Int position)
    {
        var roads = new bool[4];

        for (int i = 0; i < NeighborDirections.Length; i++)
        {
            var neighborPos = position + NeighborDirections[i];
            var id = GetIDAtPosition(neighborPos);
            roads[i] = IsRoadID(id);
        }

        return roads;
    }

    public List<Vector3Int> GetNeighbourRoadPositions(Vector3Int position)
    {
        var roadPositions = new List<Vector3Int>(4); // Pre-allocate for max 4 neighbors

        for (int i = 0; i < NeighborDirections.Length; i++)
        {
            var neighborPos = position + NeighborDirections[i];
            var id = GetIDAtPosition(neighborPos);

            if (IsRoadID(id))
            {
                roadPositions.Add(neighborPos);
            }
        }

        return roadPositions;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static bool IsRoadID(int id) => id >= MIN_ROAD_ID && id <= MAX_ROAD_ID;

    public int GetIDAtPosition(Vector3Int position)
    {
        if (placedDecorations.TryGetValue(position, out var decorData))
            return decorData.ID;

        if (placedObjects.TryGetValue(position, out var objectData))
            return objectData.ID;

        return -1;
    }

    public int GetIndexAtPosition(Vector3Int position)
    {
        if (placedObjects.TryGetValue(position, out var objectData))
            return objectData.PlaceObjectIndex;

        if (placedDecorations.TryGetValue(position, out var decorData))
            return decorData.PlaceObjectIndex;

        return -1;
    }

    public PlacementData GetPlacementDataAt(Vector3Int position)
    {
        if (placedObjects.TryGetValue(position, out var objectData))
            return objectData;

        if (placedDecorations.TryGetValue(position, out var decorData))
            return decorData;

        return null;
    }

    public void AddObjectAt(Vector3Int gridPosition, Vector2Int objectSize, int id, int placedObjectIndex, ObjectType type)
    {
        var positionsToOccupy = CalculatePositions(gridPosition, objectSize);
        var data = new PlacementData(positionsToOccupy, id, placedObjectIndex, type);

        var targetDictionary = type == ObjectType.Object ? placedObjects : placedDecorations;

        // Validate all positions first
        foreach (var pos in positionsToOccupy)
        {
            if (targetDictionary.ContainsKey(pos))
            {
                throw new InvalidOperationException($"Position {pos} is already occupied by a {type}");
            }
        }

        // Add all positions
        foreach (var pos in positionsToOccupy)
        {
            targetDictionary[pos] = data;
        }
    }

    public void AddFixedObjects(IReadOnlyList<Vector3Int> positionsToOccupy, int id, int placedObjectIndex)
    {
        var data = new PlacementData(positionsToOccupy as List<Vector3Int> ?? new List<Vector3Int>(positionsToOccupy),
                                   id, placedObjectIndex, ObjectType.Object);

        // Validate all positions first
        foreach (var pos in positionsToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
            {
                throw new InvalidOperationException($"Position {pos} is already occupied");
            }
        }

        // Add all positions
        foreach (var pos in positionsToOccupy)
        {
            placedObjects[pos] = data;
        }
    }

    // Remove structure from grid data
    public bool RemoveObjectAt(Vector3Int position)
    {
        if (placedObjects.TryGetValue(position, out var objectData))
        {
            foreach (var pos in objectData.occupiedPositions)
            {
                placedObjects.Remove(pos);
            }
            return true;
        }

        if (placedDecorations.TryGetValue(position, out var decorData))
        {
            foreach (var pos in decorData.occupiedPositions)
            {
                placedDecorations.Remove(pos);
            }
            return true;
        }

        return false;
    }

    private static List<Vector3Int> CalculatePositions(Vector3Int gridPosition, Vector2Int objectSize)
    {
        var positions = new List<Vector3Int>(objectSize.x * objectSize.y);

        for (int x = 0; x < objectSize.x; x++)
        {
            for (int y = 0; y < objectSize.y; y++)
            {
                positions.Add(gridPosition + new Vector3Int(x, y, 0));
            }
        }

        return positions;
    }

    public bool CanPlaceObjectAt(Vector3Int gridPosition, Vector2Int objectSize)
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize);
        foreach(var pos in positionToOccupy)
        {
            if (!mapBoundaries.Contains(pos))
            {
                return false;
            }

            if (positionHasNature.Contains(pos))
            {
                return false;
            }

            if (placedObjects.ContainsKey(pos))
            {
                return false;
            }
            if (placedDecorations.ContainsKey(pos))
            {
                return false;
            }
        }

        return true;
    }

    // Used in PlacementSystem class only to create the boundaries of the map
    public void AddPositionsInArea(Vector3Int bottomLeft, Vector3Int topRight)
    {
    for (int x = bottomLeft.x; x <= topRight.x; x++)
    {
        for (int y = bottomLeft.y; y <= topRight.y; y++)
        {
            Vector3Int pos = new Vector3Int(x, y, bottomLeft.z); // Assuming z stays constant

            if (!mapBoundaries.Contains(pos)) // Check if position is already in the list
            {
                mapBoundaries.Add(pos);
            }
        }
    }
    }
    // Used in PlacementSystem class only to create the boundaries of the map
    public void RemovePositionsInArea(Vector3Int bottomLeft, Vector3Int topRight)
    {
        for (int x = bottomLeft.x; x <= topRight.x; x++)
        {
            for (int y = bottomLeft.y; y <= topRight.y; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, bottomLeft.z);
                mapBoundaries.Remove(pos); // HashSet automatically ignores if the item isn't in the set
            }
        }
    }
}

public enum ObjectType
{
    Object = 0,
    Decoration = 1
}

public class PlacementData
{
    public List<Vector3Int> occupiedPositions { get; }
    public int ID { get; }
    public int PlaceObjectIndex { get; set; }
    public ObjectType Type { get; }

    public PlacementData(List<Vector3Int> occupiedPositions, int id, int index, ObjectType type)
    {
        this.occupiedPositions = occupiedPositions ?? throw new ArgumentNullException(nameof(occupiedPositions));
        ID = id;
        PlaceObjectIndex = index;
        Type = type;
    }
}
