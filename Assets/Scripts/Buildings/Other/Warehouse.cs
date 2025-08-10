public class Warehouse : Building
{
    protected override void Awake()
    {
        base.Awake();
        width = 2;
    }
    public override void UpgradeBuilding()
    {
        base.UpgradeBuilding();

        if (Level == 1)
        {
            BuildingName = "Warehouse LVL1";
        }
        if (Level == 2)
        {
            BuildingName = "Warehouse LVL2";
        }
        else if (Level == 3)
        {
            BuildingName = "Warehouse LVL3";
        }
    }
}
