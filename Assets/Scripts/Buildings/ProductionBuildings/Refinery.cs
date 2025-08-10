public class Refinery : ProductionBuilding
{
    
    protected override void Awake()
    {
        base.Awake();

        inputMethodBase = new int[] { };
        inputMethodOne = new int[] { };
        inputMethodTwo = new int[] { };
        inputMethodThree = new int[] { };

        outputMethodBase = new int[] { 0 };
        outputMethodOne = new int[] { 1 };
        outputMethodTwo = new int[] { 2 };
        outputMethodThree = new int[] { 3 };

        NeededResourcesID = new int[] { };
        ProducedResourcesID = new int[] { 12 };
        ResourceProductionTime = new float[] { 10f };

        ProductionType = 0;
        Level = 0;
        width = 2;
    }

    public override void UpgradeBuilding()
    {
        base.UpgradeBuilding();

        if (Level == 1)
        {
            BuildingName = "Refinery LVL1";
        }
        else if (Level == 2)
        {
            BuildingName = "Refinery LVL2";
        }
        else if (Level == 3)
        {
            BuildingName = "Refinery LVL3";
        }
    }
}
