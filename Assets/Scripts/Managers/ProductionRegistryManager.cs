using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductionRegistryManager : MonoBehaviour
{
    [SerializeField]
    private GameObject goodsListObj;
    private List<GameObject> entryList = new List<GameObject>();
    [SerializeField]
    private ResourceSO resourceSO;
    [SerializeField]
    private GameObject entryPrefab;
    private ResourceManager resourceManager;

    private Color red = new Color(0.8f, 0, 0);
    private Color green = new Color(0, 0.8f, 0f);

    private void Awake()
    {
        resourceManager = GameObject.Find("ResourceManager").GetComponent<ResourceManager>();
        foreach (ResourceData resource in resourceSO.resourcesData)
        {
            if (resource.ID == 12)
                continue;
            GameObject newEntry = Instantiate(entryPrefab, goodsListObj.transform);
            GameObject iconObj = newEntry.transform.Find("ItemIcon/Image").gameObject;
            Image iconImage = iconObj.GetComponent<Image>();
            iconImage.sprite = resource.Icon;
            iconImage.SetNativeSize();
            if (resource.ID == 11)
                iconObj.transform.localScale = new Vector3(0.5f, 0.5f, 0);

            entryList.Add(newEntry);
        }
    }

    public void UpdateTotalOfResourceWithID(int resourceID)
    {
        GameObject resourceEntry = entryList[resourceID];
        GameObject totalObj = resourceEntry.transform.Find("TotalCount/Text").gameObject;
        TextMeshProUGUI totalText = totalObj.GetComponent<TextMeshProUGUI>();
        totalText.text = resourceManager.resourceTotals[resourceID].ToString();
    }
    public void UpdateConsumptionRateOfResourceWithID(int resourceID)
    {
        GameObject resourceEntry = entryList[resourceID];
        GameObject consumptionObj = resourceEntry.transform.Find("ConsumptionCount/Text").gameObject;
        TextMeshProUGUI consumptionText = consumptionObj.GetComponent<TextMeshProUGUI>();

        float epsilon = 0.0001f;
        float consumption = resourceManager.resourceConsumptionTotals[resourceID];
        if (Mathf.Abs(consumption) < epsilon)
        {
            consumptionText.text = "0";
        }
        else
        {
            consumptionText.text = consumption.ToString("F2");
        }
        
        UpdateSurplusOfResourceWithID(resourceID);
    }
    public void UpdateProductionRateOfResourceWithID(int resourceID)
    {
        GameObject resourceEntry = entryList[resourceID];
        GameObject productionObj = resourceEntry.transform.Find("ProductionCount/Text").gameObject;
        TextMeshProUGUI productionText = productionObj.GetComponent<TextMeshProUGUI>();

        float epsilon = 0.0001f;
        float production = resourceManager.resourceProductionTotals[resourceID];

        if (Mathf.Abs(production) < epsilon)
        {
            productionText.text = "0";
        }
        else
        {
            productionText.text = production.ToString("F2");
        }

        UpdateSurplusOfResourceWithID(resourceID);
    }
    private void UpdateSurplusOfResourceWithID(int resourceID)
    {
        GameObject resourceEntry = entryList[resourceID];
        GameObject surplusObj = resourceEntry.transform.Find("SurplusCount/Text").gameObject;
        TextMeshProUGUI surplusText = surplusObj.GetComponent<TextMeshProUGUI>();
        float surplus = resourceManager.resourceProductionTotals[resourceID] - resourceManager.resourceConsumptionTotals[resourceID];
        surplusText.text = surplus.ToString("F2");

        float epsilon = 0.0001f;

        if (surplus < -epsilon)
            surplusText.color = red;
        else if (surplus > epsilon)
            surplusText.color = green;
        else
        {
            surplusText.color = Color.white;
            surplusText.text = "0";
        }
    }
}
