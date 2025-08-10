public class Mill : ProductionBuilding
{
    protected override void Awake()
    {
        base.Awake();

        inputMethodBase = new int[] { 0 };
        inputMethodOne = new int[] { 1 };
        inputMethodTwo = new int[] { 2 };
        inputMethodThree = new int[] { 3 };

        outputMethodBase = new int[] { 0 };
        outputMethodOne = new int[] { 1 };
        outputMethodTwo = new int[] { 2 };
        outputMethodThree = new int[] { 3 };

        NeededResourcesID = new int[] { 0 };
        ProducedResourcesID = new int[] { 8 };
        ResourceProductionTime = new float[] { 10f };

        ProductionType = 1;
        width = 2;
    }

    public override void UpgradeBuilding()
    {
        base.UpgradeBuilding();

        if (Level == 2)
        {
            BuildingName = "Windmill LVL2";
        }
        else if (Level == 3)
        {
            BuildingName = "Windmill LVL3";
        }
    }
}
