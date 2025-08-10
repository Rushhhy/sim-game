using System.Collections.Generic;
using UnityEngine;

public class Tailor : ProductionBuilding
{
    protected override void Awake()
    {
        base.Awake();

        workPositions = new List<Vector3>() { new Vector3(2.16f, 1f, 0) };

        inputMethodBase = new int[] { 0, 0 };
        inputMethodOne = new int[] { 1, 1 };
        inputMethodTwo = new int[] { 2, 2 };
        inputMethodThree = new int[] { 3, 3 };

        outputMethodBase = new int[] { 0 };
        outputMethodOne = new int[] { 1 };
        outputMethodTwo = new int[] { 2 };
        outputMethodThree = new int[] { 3 };

        NeededResourcesID = new int[] { 6, 2 };
        ProducedResourcesID = new int[] { 10 };
        ResourceProductionTime = new float[] { 10f };

        ProductionType = 2;
        width = 3;
    }

    public override void UpgradeBuilding()
    {
        base.UpgradeBuilding();

        if (Level == 2)
        {
            BuildingName = "Tailor LVL2";
        }
        else if (Level == 3)
        {
            BuildingName = "Tailor LVL3";
            workPositions.Add(new Vector3(2.16f, 0.43f, 0));
        }
    }
}
