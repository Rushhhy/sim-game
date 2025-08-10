using System.Collections.Generic;
using UnityEngine;

public class Mine : ProductionBuilding
{
    protected override void Awake()
    {
        base.Awake();

        workPositions = new List<Vector3>() { new Vector3(-0.85f, 1.1f, 0), new Vector3(3.05f, 1.1f, 0) };

        inputMethodBase = new int[] { 0, 0 };
        inputMethodOne = new int[] { 1, 1 };
        inputMethodTwo = new int[] { 2, 2 };
        inputMethodThree = new int[] { 3, 3 };

        outputMethodBase = new int[] { 0, 0 };
        outputMethodOne = new int[] { 2, 2 };
        outputMethodTwo = new int[] { 3, 3 };
        outputMethodThree = new int[] { 4, 4 };

        NeededResourcesID = new int[] { 2, 2 };
        ProducedResourcesID = new int[] { 3, 4 };
        ResourceProductionTime = new float[] { 10f, 10f };

        ProductionType = 3;
        width = 3;
    }

    protected override void StartBuildOrUpgrade(int level)
    {
        base.StartBuildOrUpgrade(level);
        transform.position += new Vector3(0.15f, 0f, 0f);
        currentProgressBar.gameObject.transform.position += new Vector3(0.15f, 0f, 0f);
        finishConstructionObj.transform.position += new Vector3(0.15f, 0f, 0f);
        hammerObj.transform.position += new Vector3(0.15f, 0f, 0f);
    }

    public override void UpgradeBuilding()
    {
        base.UpgradeBuilding();

        if (Level == 1)
        {
            BuildingName = "Mine LVL1";
        }
        else if (Level == 2)
        {
            BuildingName = "Mine LVL2";
        }
        else if (Level == 3)
        {
            BuildingName = "Mine LVL3";
        }
    }

    public override void FinishConstruction()
    {
        base.FinishConstruction();
        transform.position -= new Vector3(0.15f, 0f, 0f);
    }
}
