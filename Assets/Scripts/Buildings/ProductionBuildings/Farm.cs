using System.Collections.Generic;
using UnityEngine;

public class Farm : ProductionBuilding
{
    protected override void Update()
    {
        base.Update();
        SwitchHarvestSprite();
    }

    protected override void Awake()
    {
        base.Awake();

        workPositions = new List<Vector3>() { new Vector3(0.6f, 1f, 0) };

        inputMethodBase = new int[] { };
        inputMethodOne = new int[] { };
        inputMethodTwo = new int[] { };
        inputMethodThree = new int[] { };

        outputMethodBase = new int[] { 0 };
        outputMethodOne = new int[] { 1 };
        outputMethodTwo = new int[] { 2 };
        outputMethodThree = new int[] { 3 };

        NeededResourcesID = new int[] { };
        ProducedResourcesID = new int[] { 0 };
        ResourceProductionTime = new float[] { 10f };

        ProductionType = 0;
        width = 3;
    }

    public override void UpgradeBuilding()
    {
        base.UpgradeBuilding();

        if (Level == 2)
        {
            BuildingName = "Farm LVL2";
            workPositions.Add(new Vector3(2.55f, 1f, 0));
        }
        else if (Level == 3)
        {
            BuildingName = "Farm LVL3";
            workPositions.Add(new Vector3(0.6f, 0.455f, 0f));
            workPositions.Add(new Vector3(2.55f, 0.455f, 0f));
        }

        animator.SetInteger("Growth", 1);
    }

    private void SwitchHarvestSprite()
    {
        // Calculate the ProductionStage based on ProductionTimer
        if (individualTimers[0] <= ResourceProductionTime[0] / 3)
        {
            animator.SetInteger("Growth", 1);
        }
        else if (individualTimers[0] <= ResourceProductionTime[0] / 3 * 2)
        {
            animator.SetInteger("Growth", 2);
        }
        else
        {
            animator.SetInteger("Growth", 3);
        }
    }
 }

