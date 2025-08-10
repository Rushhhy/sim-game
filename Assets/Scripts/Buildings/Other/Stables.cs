public class Stables : Building
{
    protected override void Awake()
    {
        base.Awake();
        width = 3;
    }
    public override void UpgradeBuilding()
    {
        base.UpgradeBuilding();

        if (Level == 1)
        {
            BuildingName = "Stables LVL1";
        }
        if (Level == 2)
        {
            BuildingName = "Stables LVL2";
        }
        else if (Level == 3)
        {
            BuildingName = "Stables LVL3";
        }
    }
}
