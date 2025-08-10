public class Logger : ProductionBuilding
{
    protected override void Awake()
    {
        base.Awake();

        inputMethodBase = new int[] {  };
        inputMethodOne = new int[] { };
        inputMethodTwo = new int[] { };
        inputMethodThree = new int[] { };

        outputMethodBase = new int[] { 0 };
        outputMethodOne = new int[] { 1 };
        outputMethodTwo = new int[] { 2 };
        outputMethodThree = new int[] { 3 };

        NeededResourcesID = new int[] { };
        ProducedResourcesID = new int[] { 1 };
        ResourceProductionTime = new float[] { 10f };

        ProductionType = 0;
        width = 3;
    }

    public override void UpgradeBuilding()
    {
        base.UpgradeBuilding();

        if (Level == 2)
        {
            BuildingName = "Logger LVL2";
        }
        else if (Level == 3)
        {
            BuildingName = "Logger LVL3";
        }
    }
}
