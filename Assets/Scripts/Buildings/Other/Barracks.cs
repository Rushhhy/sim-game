using System.Collections.Generic;
using UnityEngine;

public class Barracks : Building
{
    protected override void Awake()
    {
        base.Awake();
        workPositions = new List<Vector3>() { new Vector3(0.52f, 1.05f, 0f) };
        width = 3;
    }
    public override void UpgradeBuilding()
    {
        base.UpgradeBuilding();

        if (Level == 1)
        {
            BuildingName = "Barracks LVL1";
        }
        if (Level == 2)
        {
            BuildingName = "Barracks LVL2";
            workPositions.Add(new Vector3(2.43f, 1.05f, 0f));
        }
        else if (Level == 3)
        {
            BuildingName = "Barracks LVL3";
            workPositions.Add(new Vector3(0.21f, 0.56f, 0f));
            workPositions.Add(new Vector3(2.43f, 0.56f, 0f));
        }
    }
}
