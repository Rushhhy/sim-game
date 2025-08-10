using System.Collections.Generic;
using UnityEngine;

public class Workshop : ProductionBuilding
{
    protected override void Awake()
    {
        base.Awake();

        workPositions = new List<Vector3>() { new Vector3(0.655f, 1.1f, 0f) };

        inputMethodBase = new int[] { 0, 0 };
        inputMethodOne = new int[] { 1, 1 };
        inputMethodTwo = new int[] { 2, 2 };
        inputMethodThree = new int[] { 3, 3 };

        outputMethodBase = new int[] { 0 };
        outputMethodOne = new int[] { 2 };
        outputMethodTwo = new int[] { 3 };
        outputMethodThree = new int[] { 4 };

        NeededResourcesID = new int[] { 1, 4 };
        ProducedResourcesID = new int[] { 2 };
        ResourceProductionTime = new float[] { 10f };

        ProductionType = 2;
        width = 3;
    }

    public override void UpgradeBuilding()
    {
        base.UpgradeBuilding();

        if (Level == 2)
        {
            BuildingName = "Workshop LVL2";
        }
        else if (Level == 3)
        {
            BuildingName = "Workshop LVL3";
            workPositions.Add(new Vector3(0.655f, 0.53f, 0f));
        }
    }
}
