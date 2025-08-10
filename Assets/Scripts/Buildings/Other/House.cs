public class House : Building
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
            BuildingName = "House LVL1";
        }
        if (Level == 2)
        {
            BuildingName = "House LVL2";
        }
        else if (Level == 3)
        {
            BuildingName = "House LVL3";
        }
    }
}
