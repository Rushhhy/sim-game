using UnityEngine;
using System;

public class AStarNode : IComparable<AStarNode>
{
    public Vector3Int position;
    public float gCost; // Distance from start
    public float hCost; // Distance to target (heuristic)
    public float fCost => gCost + hCost; // Total cost
    public AStarNode parent;
    public bool isWalkable;

    public AStarNode(Vector3Int position, bool isWalkable = true)
    {
        this.position = position;
        this.isWalkable = isWalkable;
        this.gCost = float.MaxValue;
        this.hCost = 0;
        this.parent = null;
    }

    public void CalculateHCost(Vector3Int target, bool allowDiagonal = true)
    {
        int deltaX = Mathf.Abs(position.x - target.x);
        int deltaY = Mathf.Abs(position.y - target.y);

        if (allowDiagonal)
        {
            // Diagonal distance heuristic
            int diagonalSteps = Mathf.Min(deltaX, deltaY);
            int straightSteps = Mathf.Max(deltaX, deltaY) - diagonalSteps;
            hCost = diagonalSteps * 1.414f + straightSteps;
        }
        else
        {
            // Manhattan distance heuristic
            hCost = deltaX + deltaY;
        }
    }

    public void SetGCost(float newGCost)
    {
        this.gCost = newGCost;
    }

    public int CompareTo(AStarNode other)
    {
        int compare = fCost.CompareTo(other.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(other.hCost);
        }
        return compare;
    }

    public override bool Equals(object obj)
    {
        if (obj is AStarNode other)
        {
            return position.Equals(other.position);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return position.GetHashCode();
    }

    public override string ToString()
    {
        return $"Node({position.x}, {position.y}) G:{gCost:F1} H:{hCost:F1} F:{fCost:F1}";
    }
}
