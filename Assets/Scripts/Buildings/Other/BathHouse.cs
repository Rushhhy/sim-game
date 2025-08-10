using System.Collections.Generic;
using UnityEngine;
public class BathHouse : Building
{
    protected override void Awake()
    {
        base.Awake();
        workPositions = new List<Vector3>() { new Vector3(0.58f, 1f, 0) };
        width = 3;
    }
    public override void UpgradeBuilding()
    {
        base.UpgradeBuilding();

        if (Level == 1)
        {
            BuildingName = "Bath House LVL1";
        }
        if (Level == 2)
        {
            BuildingName = "Bath House LVL2";
            workPositions.Add(new Vector3(2.38f, 1f, 0));
        }
        else if (Level == 3)
        {
            BuildingName = "Bath House LVL3";
            workPositions.Add(new Vector3(0.58f, 0.44f, 0));
            workPositions.Add(new Vector3(2.38f, 0.44f, 0));
        }
    }
}
