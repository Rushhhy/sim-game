using UnityEngine;

public class Market : Building
{
    [SerializeField]
    private GameObject marketOne, marketTwo, marketThree;
    protected override void Awake()
    {
        base.Awake();
        width = 4;
    }
    public override void SetBuildingSprite()
    {
        Transform childToRemove = null;
        GameObject newMarketPrefab = null;

        if (Level == 1)
        {
            childToRemove = transform.Find("MarketBroken");
            newMarketPrefab = marketOne;
        }
        else if (Level == 2)
        {
            childToRemove = transform.Find("MarketOne(Clone)");
            newMarketPrefab = marketTwo;
        }
        else if (Level == 3)
        {
            childToRemove = transform.Find("MarketTwo(Clone)");
            newMarketPrefab = marketThree;
        }

        if (childToRemove != null)
        {
            Destroy(childToRemove.gameObject);
        }

        if (newMarketPrefab != null)
        {
            GameObject newMarket = Instantiate(newMarketPrefab, transform);
        }
    }

    protected override void StartBuildOrUpgrade(int level)
    {
        base.StartBuildOrUpgrade(level);
        transform.position += new Vector3(0f, 0.7f, 0f);
        currentProgressBar.gameObject.transform.position += new Vector3(0f, 0.7f, 0f);
        finishConstructionObj.transform.position += new Vector3(0f, 0.7f, 0f);
        hammerObj.transform.position += new Vector3(0.5f, 1.5f, 0f);
    }

    public override void UpgradeBuilding()
    {
        if (Level == 0)
        {
            transform.Find("MarketBroken").gameObject.SetActive(false);
        }
        else if (Level == 1)
        {
            transform.Find("MarketOne(Clone)").gameObject.SetActive(false);
        }
        else if (Level == 2)
        {
            transform.Find("MarketTwo(Clone)").gameObject.SetActive(false);
        }

        Level++;
        StartBuildOrUpgrade(Level);

        if (Level == 1)
        {
            BuildingName = "Market LVL1";
        }
        if (Level == 2)
        {
            BuildingName = "Market LVL2";
        }
        else if (Level == 3)
        {
            BuildingName = "Market LVL3";
        }
    }

    public override void FinishConstruction()
    {
        base.FinishConstruction();
        if (Index == 0 || Index == 1)
        {
            SpriteRenderer.sprite = null;
        }
        transform.position -= new Vector3(0f, 0.7f, 0f);
    }
}
