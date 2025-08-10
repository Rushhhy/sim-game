public class Hospital : Building
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
            BuildingName = "Hospital LVL1";
        }
        if (Level == 2)
        {
            BuildingName = "Hospital LVL2";
        }
        else if (Level == 3)
        {
            BuildingName = "Hospital LVL3";
        }
    }
}
