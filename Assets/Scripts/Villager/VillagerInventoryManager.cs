using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class VillagerInventoryManager : MonoBehaviour
{
    [SerializeField]
    private VillagersDataSO villagerData;

    [SerializeField]
    private VillagerManager villagerManager;
    [SerializeField]
    private GameObject allVillagers, tierOneVillagers, tierTwoVillagers, tierThreeVillagers;

    [SerializeField]
    private GameObject villagerPrefab, availableVillagerPrefab;
    [SerializeField]
    private GameObject villagerRow;

    private List<List<GameObject>> modifyAddVillagerList = new List<List<GameObject>>();
    private List<List<GameObject>> modifyAvailableVillagerList = new List<List<GameObject>>();

    private List<GameObject> allVillagerItems = new List<GameObject>();
    private List<GameObject> allVillagerRows = new List<GameObject>();

    private List<GameObject> tierOneVillagerItems = new List<GameObject>();
    private List<GameObject> tierOneVillagerRows = new List<GameObject>();

    private List<GameObject> tierTwoVillagerItems = new List<GameObject>();
    private List<GameObject> tierTwoVillagerRows = new List<GameObject>();

    private List<GameObject> tierThreeVillagerItems = new List<GameObject>();
    private List<GameObject> tierThreeVillagerRows = new List<GameObject>();

    [SerializeField]
    private GameObject allDisplay, tierOneDisplay, tierTwoDisplay, tierThreeDisplay, allSelected, oneSelected, twoSelected, threeSelected, noVillagers, allInventory;
    [SerializeField]
    private GameObject availableInventory, allAvailableDisplay, UnemployedAvailableDisplay, allAvailableSelected, unemployedSelected, noVillagersTwo;

    private List<GameObject> allAvailableVillagers = new List<GameObject>();
    private List<GameObject> allAvailableVillagerRows = new List<GameObject>();

    private List<GameObject> unemployedItems = new List<GameObject>();
    private List<GameObject> unemployedRows = new List<GameObject>();

    [SerializeField]
    private GameObject unemployedScroller;

    private void Start()
    {
        villagerManager.OnVillagerBought += AddVillagertoInventory;
        villagerManager.OnVillagerHoused += AddtoAvailableInventory;
        villagerManager.OnVillagerRemoved += RemoveFromAvailableInventory;
        villagerManager.OnVillagerEmployed += UpdateAvailableInventory;
        villagerManager.OnVillagerRemovedWithoutReplacement += AddToUnemployedInventoryNoReturn;
    }

    private void UpdateAvailableInventory(int inVillageIndex, int prevInVillageIndex, int prevVillagerIndex, string buildingName)
    {
        if (prevVillagerIndex != -1)
        {
            GameObject prevWorkStatusObj = allAvailableVillagers[prevInVillageIndex].transform.Find("MainPanel/WorkStatus/WorkStatusText").gameObject;
            prevWorkStatusObj.GetComponent<TextMeshProUGUI>().text = "Unemployed";
            prevWorkStatusObj.GetComponent<TextMeshProUGUI>().color = new Color(1, 0, 0.15f);

            GameObject unemployedObj = AddToUnemployedInventory(prevVillagerIndex);
            modifyAvailableVillagerList[prevInVillageIndex][1] = unemployedObj;

            GameObject prevWorkDescriptionObj = allAvailableVillagers[prevInVillageIndex].transform.Find("VillagerDescription").gameObject;
            prevWorkDescriptionObj.GetComponent<TextMeshProUGUI>().text = "Currently Not Working";
        }

        GameObject currWorkStatusObj = allAvailableVillagers[inVillageIndex].transform.Find("MainPanel/WorkStatus/WorkStatusText").gameObject;
        currWorkStatusObj.GetComponent<TextMeshProUGUI>().text = "Employed";
        currWorkStatusObj.GetComponent<TextMeshProUGUI>().color = Color.white;

        if (modifyAvailableVillagerList[inVillageIndex][1] != null)
        {
            RemoveFromUnemployedInventory(inVillageIndex);
            modifyAvailableVillagerList[inVillageIndex][1] = null;
        }

        GameObject currWorkDescriptionObj = allAvailableVillagers[inVillageIndex].transform.Find("VillagerDescription").gameObject;
        currWorkDescriptionObj.GetComponent<TextMeshProUGUI>().text = "Currently at " + buildingName;

        availableInventory.SetActive(false);
    }

    private void AddVillagertoInventory(int villagerID)
    {
        if (allVillagerItems.Count % 4 == 0)
        {
            GameObject newRow = Instantiate(villagerRow, allVillagers.transform);
            allVillagerRows.Add(newRow);
            if (allVillagerRows.Count > 2)
            {
                RectTransform panelRect = allVillagers.GetComponent<RectTransform>();
                panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, panelRect.sizeDelta.y + 95);
            }
        }

        GameObject newVillager = Instantiate(villagerPrefab, allVillagerRows[^1].transform);
        allVillagerItems.Add(newVillager);

        VillagerData villagerInfo = villagerData.GetVillagerDataByID(villagerID);
        string villagerName = villagerInfo.Name;
        Sprite villagerImage = villagerInfo.villagerIcon;

        newVillager.transform.Find("VillagerName").GetComponent<TextMeshProUGUI>().text = villagerName;
        newVillager.transform.Find("MainPanel/VillagerImage").GetComponent<Image>().sprite = villagerImage;
        newVillager.transform.Find("MainPanel/VillagerImage").GetComponent<Image>().SetNativeSize();

        int villagerTier = villagerInfo.tier;
        GameObject tierParent = null;
        List<GameObject> tierRows = null;
        List<GameObject> tierItems = null;

        switch (villagerTier)
        {
            case 1:
                tierParent = tierOneVillagers;
                tierRows = tierOneVillagerRows;
                tierItems = tierOneVillagerItems;
                break;
            case 2:
                tierParent = tierTwoVillagers;
                tierRows = tierTwoVillagerRows;
                tierItems = tierTwoVillagerItems;
                break;
            case 3:
                tierParent = tierThreeVillagers;
                tierRows = tierThreeVillagerRows;
                tierItems = tierThreeVillagerItems;
                break;
            default:
                return;
        }

        if (tierItems.Count % 4 == 0)
        {
            GameObject newTierRow = Instantiate(villagerRow, tierParent.transform);
            tierRows.Add(newTierRow);
            if (tierRows.Count > 2)
            {
                RectTransform tierPanelRect = tierParent.GetComponent<RectTransform>();
                tierPanelRect.sizeDelta = new Vector2(tierPanelRect.sizeDelta.x, tierPanelRect.sizeDelta.y + 95);
            }
        }

        GameObject tierVillager = Instantiate(villagerPrefab, tierRows[^1].transform);
        tierItems.Add(tierVillager);

        tierVillager.transform.Find("VillagerName").GetComponent<TextMeshProUGUI>().text = villagerName;
        tierVillager.transform.Find("MainPanel/VillagerImage").GetComponent<Image>().sprite = villagerImage;
        tierVillager.transform.Find("MainPanel/VillagerImage").GetComponent<Image>().SetNativeSize();

        List <GameObject> villagerPair = new List<GameObject> { newVillager, tierVillager };
        modifyAddVillagerList.Add(villagerPair);

        int villagerIndex = allVillagerItems.Count - 1;
        newVillager.transform.Find("SelectButton").GetComponent<Button>().onClick.AddListener(() => villagerManager.AssignVillagertoBuilding(villagerIndex));
        tierVillager.transform.Find("SelectButton").GetComponent<Button>().onClick.AddListener(() => villagerManager.AssignVillagertoBuilding(villagerIndex));
    }

    private void AddtoAvailableInventory(int villagerIndex)
    {
        if (allAvailableVillagers.Count % 4 == 0)
        {
            GameObject newRow = Instantiate(villagerRow, allAvailableDisplay.transform);
            allAvailableVillagerRows.Add(newRow);
            if (allAvailableVillagerRows.Count > 2)
            {
                RectTransform panelRect = allAvailableDisplay.GetComponent<RectTransform>();
                panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, panelRect.sizeDelta.y + 95);
            }
        }

        GameObject newVillager = Instantiate(availableVillagerPrefab, allAvailableVillagerRows[^1].transform);
        allAvailableVillagers.Add(newVillager);

        int villagerID = villagerManager.villagersHeldID[villagerIndex];
        VillagerData villagerInfo = villagerData.GetVillagerDataByID(villagerID);
        string villagerName = villagerInfo.Name;
        Sprite villagerImage = villagerInfo.villagerIcon;

        newVillager.transform.Find("VillagerName").GetComponent<TextMeshProUGUI>().text = villagerName;
        newVillager.transform.Find("MainPanel/VillagerImage").GetComponent<Image>().sprite = villagerImage;
        newVillager.transform.Find("MainPanel/VillagerImage").GetComponent<Image>().SetNativeSize();

        GameObject allVillagerEntry = modifyAddVillagerList[villagerIndex][0];
        GameObject tierVillagerEntry = modifyAddVillagerList[villagerIndex][1];
        allVillagerEntry.transform.Find("Added").gameObject.SetActive(true);
        tierVillagerEntry.transform.Find("Added").gameObject.SetActive(true);

        GameObject unemployedVillager = AddToUnemployedInventory(villagerIndex);

        newVillager.transform.Find("SelectButton").GetComponent<Button>().onClick.AddListener(() => villagerManager.AssignVillagertoBuilding(villagerIndex));
        unemployedVillager.transform.Find("SelectButton").GetComponent<Button>().onClick.AddListener(() => villagerManager.AssignVillagertoBuilding(villagerIndex));

        modifyAvailableVillagerList.Add(new List<GameObject> { newVillager, unemployedVillager });

        allInventory.SetActive(false);
    }

    public void RemoveFromAvailableInventory(int villagerIndex, int inVillageIndex, bool isEmployed)
    {
        // Store the references first
        GameObject allVillagerEntry = modifyAddVillagerList[villagerIndex][0];
        GameObject tierVillagerEntry = modifyAddVillagerList[villagerIndex][1];
        GameObject availableVillagerEntry = allAvailableVillagers[inVillageIndex];

        allVillagerEntry.transform.Find("Added").gameObject.SetActive(false);
        tierVillagerEntry.transform.Find("Added").gameObject.SetActive(false);

        if (!isEmployed)
        {
            GameObject unemployedObj = modifyAvailableVillagerList[inVillageIndex][1];
            if (unemployedObj != null)
            {
                // Remove from unemployed list and reorganize
                unemployedItems.Remove(unemployedObj);
                Destroy(unemployedObj);
                ReorganizeItems(unemployedItems, unemployedRows, UnemployedAvailableDisplay.transform);
            }
        }

        // Remove from available villagers list
        allAvailableVillagers.RemoveAt(inVillageIndex);
        modifyAvailableVillagerList.RemoveAt(inVillageIndex);
        Destroy(availableVillagerEntry);

        // Reorganize available villagers to remove gaps
        ReorganizeItems(allAvailableVillagers, allAvailableVillagerRows, allAvailableDisplay.transform);
    }


    public GameObject AddToUnemployedInventory(int villagerIndex)
    {
        int villagerID = villagerManager.villagersHeldID[villagerIndex];
        VillagerData villagerInfo = villagerData.GetVillagerDataByID(villagerID);
        string villagerName = villagerInfo.Name;
        Sprite villagerImage = villagerInfo.villagerIcon;

        if (unemployedItems.Count % 4 == 0)
        {
            GameObject newTierRow = Instantiate(villagerRow, UnemployedAvailableDisplay.transform);
            unemployedRows.Add(newTierRow);
            if (unemployedRows.Count > 2)
            {
                RectTransform tierPanelRect = UnemployedAvailableDisplay.GetComponent<RectTransform>();
                tierPanelRect.sizeDelta = new Vector2(tierPanelRect.sizeDelta.x, tierPanelRect.sizeDelta.y + 95);
            }
        }

        GameObject unemployedVillager = Instantiate(availableVillagerPrefab, unemployedRows[^1].transform);
        unemployedVillager.transform.Find("SelectButton").GetComponent<Button>().onClick.AddListener(() => villagerManager.AssignVillagertoBuilding(villagerIndex));
        unemployedItems.Add(unemployedVillager);

        unemployedVillager.transform.Find("VillagerName").GetComponent<TextMeshProUGUI>().text = villagerName;
        unemployedVillager.transform.Find("MainPanel/VillagerImage").GetComponent<Image>().sprite = villagerImage;
        unemployedVillager.transform.Find("MainPanel/VillagerImage").GetComponent<Image>().SetNativeSize();

        return unemployedVillager;
    }

    public void AddToUnemployedInventoryNoReturn(int villagerIndex)
    {
        int villagerID = villagerManager.villagersHeldID[villagerIndex];
        VillagerData villagerInfo = villagerData.GetVillagerDataByID(villagerID);
        string villagerName = villagerInfo.Name;
        Sprite villagerImage = villagerInfo.villagerIcon;

        if (unemployedItems.Count % 4 == 0)
        {
            GameObject newTierRow = Instantiate(villagerRow, UnemployedAvailableDisplay.transform);
            unemployedRows.Add(newTierRow);
            if (unemployedRows.Count > 2)
            {
                RectTransform tierPanelRect = UnemployedAvailableDisplay.GetComponent<RectTransform>();
                tierPanelRect.sizeDelta = new Vector2(tierPanelRect.sizeDelta.x, tierPanelRect.sizeDelta.y + 95);
            }
        }

        GameObject unemployedVillager = Instantiate(availableVillagerPrefab, unemployedRows[^1].transform);
        unemployedVillager.transform.Find("SelectButton").GetComponent<Button>().onClick.AddListener(() => villagerManager.AssignVillagertoBuilding(villagerIndex));
        unemployedItems.Add(unemployedVillager);

        unemployedVillager.transform.Find("VillagerName").GetComponent<TextMeshProUGUI>().text = villagerName;
        unemployedVillager.transform.Find("MainPanel/VillagerImage").GetComponent<Image>().sprite = villagerImage;
        unemployedVillager.transform.Find("MainPanel/VillagerImage").GetComponent<Image>().SetNativeSize();

        GameObject currWorkStatusObj = modifyAvailableVillagerList[villagerIndex][0].transform.Find("MainPanel/WorkStatus/WorkStatusText").gameObject;
        currWorkStatusObj.GetComponent<TextMeshProUGUI>().text = "Unemployed";
        currWorkStatusObj.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0f, 0.15f);

        GameObject currWorkDescriptionObj = modifyAvailableVillagerList[villagerIndex][0].transform.Find("VillagerDescription").gameObject;
        currWorkDescriptionObj.GetComponent<TextMeshProUGUI>().text = "Currently Not Working";
    }

    public void RemoveFromUnemployedInventory(int inVillageIndex)
    {
        if (inVillageIndex < 0 || inVillageIndex >= modifyAvailableVillagerList.Count)
            return;

        GameObject unemployedObj = modifyAvailableVillagerList[inVillageIndex][1];
        if (unemployedObj != null)
        {
            unemployedItems.Remove(unemployedObj);
            Destroy(unemployedObj);
            modifyAvailableVillagerList[inVillageIndex][1] = null;

            // Reorganize remaining unemployed items
            ReorganizeItems(unemployedItems, unemployedRows, UnemployedAvailableDisplay.transform);
        }
    }

    private void ReorganizeItems(List<GameObject> items, List<GameObject> rows, Transform parentContainer)
    {
        // Clear all current parent-child relationships
        for (int i = 0; i < items.Count; i++)
        {
            items[i].transform.SetParent(null);
        }

        // Destroy all existing rows
        foreach (GameObject row in rows)
        {
            if (row != null)
                Destroy(row);
        }
        rows.Clear();

        // Recreate rows and redistribute items
        for (int i = 0; i < items.Count; i++)
        {
            // Create new row every 4 items
            if (i % 4 == 0)
            {
                GameObject newRow = Instantiate(villagerRow, parentContainer);
                rows.Add(newRow);
            }

            // Parent the item to the current row
            items[i].transform.SetParent(rows[rows.Count - 1].transform);
        }

        // Adjust container size
        AdjustContainerSize(parentContainer, rows.Count);
    }

    private void AdjustContainerSize(Transform container, int rowCount)
    {
        RectTransform containerRect = container.GetComponent<RectTransform>();
        if (containerRect != null)
        {
            // Base height + (additional rows beyond 2) * 95
            float newHeight = rowCount > 2 ? containerRect.sizeDelta.y - ((rowCount - 2) * 95) : containerRect.sizeDelta.y;
            containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, newHeight);
        }
    }

    public void ShowTierAll()
    {
        allDisplay.SetActive(true);
        tierOneDisplay.SetActive(false);
        tierTwoDisplay.SetActive(false);
        tierThreeDisplay.SetActive(false);

        allSelected.SetActive(true);
        oneSelected.SetActive(false);
        twoSelected.SetActive(false);
        threeSelected.SetActive(false);

        noVillagers.SetActive(allVillagerItems.Count == 0);
    }

    public void ShowTierOne()
    {
        allDisplay.SetActive(false);
        tierOneDisplay.SetActive(true);
        tierTwoDisplay.SetActive(false);
        tierThreeDisplay.SetActive(false);

        allSelected.SetActive(false);
        oneSelected.SetActive(true);
        twoSelected.SetActive(false);
        threeSelected.SetActive(false);

        noVillagers.SetActive(tierOneVillagerItems.Count == 0);
    }

    public void ShowTierTwo()
    {
        allDisplay.SetActive(false);
        tierOneDisplay.SetActive(false);
        tierTwoDisplay.SetActive(true);
        tierThreeDisplay.SetActive(false);

        allSelected.SetActive(false);
        oneSelected.SetActive(false);
        twoSelected.SetActive(true);
        threeSelected.SetActive(false);

        noVillagers.SetActive(tierTwoVillagerItems.Count == 0);
    }

    public void ShowTierThree()
    {
        allDisplay.SetActive(false);
        tierOneDisplay.SetActive(false);
        tierTwoDisplay.SetActive(false);
        tierThreeDisplay.SetActive(true);

        allSelected.SetActive(false);
        oneSelected.SetActive(false);
        twoSelected.SetActive(false);
        threeSelected.SetActive(true);

        noVillagers.SetActive(tierThreeVillagerItems.Count == 0);
    }

    public void openAllInventory()
    {
        allInventory.SetActive(true);
        noVillagers.SetActive(allVillagerItems.Count == 0);
    }
    public void exitAllInventory()
    {
        allInventory.SetActive(false);
        villagerManager.selectedBuildingIndex = -1;
        villagerManager.selectedVillagerSlot = -1;
    }

    public void openAvailableInventory()
    {
        availableInventory.SetActive(true);
        noVillagersTwo.SetActive(allAvailableVillagers.Count == 0);
    }
    public void exitAvailableInventory()
    {
        availableInventory.SetActive(false);
        villagerManager.selectedBuildingIndex = -1;
        villagerManager.selectedVillagerSlot = -1;
    }

    public void showAllAvailable()
    {
        allAvailableDisplay.SetActive(true);
        UnemployedAvailableDisplay.SetActive(false);

        allAvailableSelected.SetActive(true);
        unemployedSelected.SetActive(false);
        unemployedScroller.SetActive(false);

        noVillagersTwo.SetActive(allAvailableVillagers.Count == 0);
    }

    public void showUnemployedAvailable()
    {
        allAvailableDisplay.SetActive(false);
        UnemployedAvailableDisplay.SetActive(true);

        allAvailableSelected.SetActive(false);
        unemployedSelected.SetActive(true);
        unemployedScroller.SetActive(true);

        noVillagersTwo.SetActive(unemployedItems.Count == 0);
    }
}
