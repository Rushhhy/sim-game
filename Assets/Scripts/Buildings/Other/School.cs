public class School : Building
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
            BuildingName = "School LVL1";
        }
        if (Level == 2)
        {
            BuildingName = "School LVL2";
        }
        else if (Level == 3)
        {
            BuildingName = "School LVL3";
        }
    }
}
