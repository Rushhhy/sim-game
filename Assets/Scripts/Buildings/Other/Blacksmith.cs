public class Blacksmith : Building
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
            BuildingName = "Blacksmith LVL1";
        }
        if (Level == 2)
        {
            BuildingName = "Blacksmith LVL2";
        }
        else if (Level == 3)
        {
            BuildingName = "Blacksmith LVL3";
        }
    }
}
