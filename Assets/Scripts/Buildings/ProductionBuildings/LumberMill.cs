using System.Collections.Generic;
using UnityEngine;

public class LumberMill : ProductionBuilding
{
    protected override void Awake()
    {
        base.Awake();

        workPositionsOne = new List<Vector3>() { new Vector3(1.835f, 0.6f, 0) };
        workPositionsTwo = new List<Vector3>() { new Vector3(0.778f, 0.55f, 0), new Vector3(2.373f, 0.6f, 0) };
        workPositionsThree = new List<Vector3>() { new Vector3(2.265f, 0.78f, 0f), new Vector3(0.66f, 0.78f, 0f), new Vector3(0.66f, 0f, 0f), new Vector3(2.265f, 0.1f, 0f) };
        workPositions = workPositionsOne;

        inputMethodBase = new int[] { 0, 0 };
        inputMethodOne = new int[] { 1, 1 };
        inputMethodTwo = new int[] { 2, 2 };
        inputMethodThree = new int[] { 3, 3 };

        outputMethodBase = new int[] { 0 };
        outputMethodOne = new int[] { 1 };
        outputMethodTwo = new int[] { 2 };
        outputMethodThree = new int[] { 3 };

        NeededResourcesID = new int[] {1, 2};
        ProducedResourcesID = new int[] { 5 };
        ResourceProductionTime = new float[] { 10f};

        ProductionType = 2;
        width = 3;
    }

    public override void UpgradeBuilding()
    {
        base.UpgradeBuilding();

        if (Level == 2)
        {
            BuildingName = "Lumber Mill LVL2";
            workPositions = workPositionsTwo;
        }
        else if (Level == 3)
        {
            BuildingName = "Lumber Mill LVL3";
            workPositions = workPositionsThree;
        }
    }
}
