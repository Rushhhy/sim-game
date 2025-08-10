using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingRegistryManager : MonoBehaviour
{
    [SerializeField]
    private GridData gridData;
    [SerializeField]
    private Grid grid;

    public List<GameObject> buildingRegistryList = new List<GameObject>();
    private List<GameObject> buildingOverviewList = new List<GameObject>();
    private List<GameObject> buildingToggleList = new List<GameObject>();
    private List<GameObject> decorationToggleList = new List<GameObject>();

    [SerializeField]
    private PlacementSystem placementManager;
    [SerializeField]
    private VillagerManager villagerManager;
    [SerializeField]
    private InputManager inputManager;

    [SerializeField]
    private Transform uiRoot;

    [SerializeField]
    private GameObject buildingRegistryElement;
    [SerializeField]
    private RectTransform buildingRegistryColumn;

    [SerializeField]
    private Sprite alchemistOne, alchemistTwo, alchemistThree, armoryOne, armoryTwo, armoryThree, bakeryOne, bakeryTwo, bakeryThree, barnOne, barnTwo, barnThree, barracksOne,
        barracksTwo, barracksThree, bathHouseOne, bathHouseTwo, blacksmithOne, blacksmithTwo, blacksmithThree, carpenterOne, carpenterTwo, carpenterThree, farmOne, farmTwo,
        farmThree, hospitalOne, hospitalTwo, hospitalThree, houseOne, houseTwo, houseThree, loggerOne, loggerTwo, loggerThree, lumberMillOne, lumberMillTwo, lumberMillThree,
        mine, mill, schoolOne, schoolTwo, schoolThree, stablesOne, stablesTwo, refinery, tailorOne, tailorTwo, tailorThree, warehouseOne, warehouseTwo, warehouseThree, workshopOne,
        workshopTwo, workshopThree, orchardOne, orchardTwo, orchardThree, mineOne, marketOne, refineryOne;

    [SerializeField]
    private ResourceSO resources;
    [SerializeField]
    private StructuresDatabaseSO structures;

    [SerializeField]
    private GameObject buildingOverviewScreen, buildingOverviewScreenTwo, marketOverviewScreen;

    [SerializeField]
    private GameObject buildingToggleUI, fixedBuildingToggleUI;
    [SerializeField]
    private GameObject decorationToggleUI;
    [SerializeField]
    private Transform buildingToggleParent;
    private int openToggleIndex = -1;
    private int openToggleType = -1;
    private Vector3 scale = new Vector3(0.12f, 0.12f, 1f);

    public event Action<Vector3Int> buildingSelected;

    [SerializeField]
    private Sprite addVillagerIcon;

    // Subscribe to events
    private void Start()
    {
        placementManager.OnStructureBuilt += OnBuilt;
        placementManager.OnStructureRemoved += OnRemoved;
        placementManager.OnMoved += OnBuildingMoved;
        inputManager.OnMouseTapped += OnTapped;
        villagerManager.OnVillagerAssigned += ChangeOverviewVillagerSlot;
        villagerManager.OnVillagerTotallyRemoved += ClearBuildingSlot;

        Debug.Log("start");
    }

    private void ClearBuildingSlot(Villager villager, bool isHouse)
    {
        int buildingIndex;
        int buildingSlot;
        if (isHouse)
        {
            buildingIndex = villager.assignedHouseIndex;
            buildingSlot = villager.assignedHouseSlot;
        }
        else
        {
            buildingIndex = villager.assignedBuildingIndex;
            buildingSlot = villager.assignedBuildingSlot;
        }

        if(buildingIndex == -1) {
            return;
        }

        GameObject buildingOverview = buildingOverviewList[buildingIndex];
        GameObject buildingRegistry = buildingRegistryList[buildingIndex];
        GameObject overviewSlotObj = null;
        GameObject registrySlotObj = null;
        switch (buildingSlot)
        {
            case 1:
                overviewSlotObj = buildingOverview.transform.Find("Workers/WorkerOneFrame/Worker").gameObject;
                registrySlotObj = buildingRegistry.transform.Find("Image/Workers/WorkerOne/WorkerButton").gameObject;
                break;
            case 2:
                overviewSlotObj = buildingOverview.transform.Find("Workers/WorkerTwoFrame/Worker").gameObject;
                registrySlotObj = buildingRegistry.transform.Find("Image/Workers/WorkerTwo/WorkerButton").gameObject;
                break;
            case 3:
                overviewSlotObj = buildingOverview.transform.Find("Workers/WorkerThreeFrame/Worker").gameObject;
                registrySlotObj = buildingRegistry.transform.Find("Image/Workers/WorkerThree/WorkerButton").gameObject;
                break;
            default:
                Debug.LogWarning("Invalid villager slot: " + buildingSlot);
                return;
        }

        Image overviewSlotImage = overviewSlotObj.GetComponent<Image>();
        overviewSlotImage.sprite = addVillagerIcon;
        overviewSlotImage.SetNativeSize();

        Image registrySlotImage = registrySlotObj.GetComponent<Image>();
        registrySlotImage.sprite = addVillagerIcon;
        registrySlotImage.SetNativeSize();
    }

    public void ActivateBuildingOverview(int structureIndex)
    {
        buildingOverviewList[structureIndex].SetActive(true);
    }

    public void ActivateBuildingOverviewThroughRegistry(int structureIndex)
    {
        buildingOverviewList[structureIndex].SetActive(true);
    }

    public void CloseBuildingOverview(int buildingIndex)
    {
        buildingOverviewList[buildingIndex].SetActive(false);
    }

    private void OnBuilt(int structureID)
    {
        // Handle UI for decoration
        if ((structureID >= 0 && structureID < 16) || structureID > 38)
        {
            int decorationWidth = structures.objectsData[structureID].Size.x;
            Vector3 decorationPosition = placementManager.placedDecorations[placementManager.placedDecorations.Count - 1].transform.position;
            Vector3Int gridPosition = Vector3Int.RoundToInt(decorationPosition);

            Vector3 decorationToggleUIPosition;
            if (decorationWidth == 2)
            {
                decorationToggleUIPosition = new Vector3(gridPosition.x + 1, gridPosition.y, gridPosition.z);
            }
            else if (decorationWidth == 1)
            {
                decorationToggleUIPosition = new Vector3(gridPosition.x + 0.5f, gridPosition.y, gridPosition.z);
            }
            else
            {
                decorationToggleUIPosition = new Vector3(gridPosition.x + 1.5f, gridPosition.y, gridPosition.z);
            }

            GameObject decorationToggleButtons = Instantiate(decorationToggleUI);
            decorationToggleButtons.transform.position = decorationToggleUIPosition;
            decorationToggleButtons.transform.SetParent(buildingToggleParent, false);
            decorationToggleList.Add(decorationToggleButtons);

            Transform decorationEditButtonObject = decorationToggleButtons.transform.Find("EditButton");
            Button decorationEditButton = decorationEditButtonObject.GetComponent<Button>();
            if (decorationEditButton != null)
            {
                decorationEditButton.onClick.AddListener(() => placementManager.SelectStructure(gridPosition));
                decorationEditButton.onClick.AddListener(() => TurnOffBuildingToggleUI());
            }

            return;
        }


        // Handle UI for structure

        // Increase size of building registry to accomodate new entry
        RectTransform rectTransform = buildingRegistryColumn.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(
            rectTransform.sizeDelta.x,
            rectTransform.sizeDelta.y + 116
        );
        rectTransform.anchoredPosition = new Vector2(
            rectTransform.anchoredPosition.x,
            rectTransform.anchoredPosition.y - 58
        );

        // Create registry entry
        GameObject registryEntry = Instantiate(buildingRegistryElement, rectTransform);
        registryEntry.SetActive(false);
        buildingRegistryList.Add(registryEntry);
        AssignRegistryButton(registryEntry);

        Transform buildingImage = registryEntry.transform.Find("Image/BuildingImage");
        Transform buildingText = registryEntry.transform.Find("Image/BuildingName");

        TextMeshProUGUI text = buildingText.GetComponent<TextMeshProUGUI>();
        Image image = buildingImage.GetComponent<Image>();

        GameObject building = placementManager.placedGameObjects[placementManager.placedGameObjects.Count - 1];
        Building buildingData = building.GetComponent<Building>();

        buildingData.Index = placementManager.placedGameObjects.Count - 1;
        if (structureID != 28 && structureID != 31 && structureID != 38) // is not a fixed building
        {
            buildingData.Level = 1;
        }
        else // is a fixed building
        {
            buildingData.Level = 0;
        }

        int productionType = -1;
        if (buildingData is ProductionBuilding productionBuilding)
        {
            productionType = productionBuilding.ProductionType;
        }

        // Create Registry Entry for Respective Building ID
        switch (structureID)
        {
            case 16:
                EditRegistryEntry(text, image, alchemistOne, scale, "Alchemist LVL1");
                buildingData.BuildingName = "Alchemist LVL1";
                buildingData.ID = 16;
                break;
            case 17:
                EditRegistryEntry(text, image, armoryOne, scale, "Armory LVL1");
                buildingData.BuildingName = "Armory LVL1";
                buildingData.ID = 17;
                break;
            case 18:
                EditRegistryEntry(text, image, bakeryOne, scale, "Bakery LVL1");
                GameObject bakeryOverview = Instantiate(buildingOverviewScreen);
                SetUpBuildingOverview(bakeryOverview);
                ProductionBuilding bakeryData = building.GetComponent<Bakery>();
                InitializeProductionBuildingOverview(bakeryOverview, "Bakery LVL1", productionType, bakeryData);
                buildingData.BuildingName = "Bakery LVL1";

                AssignVillagerSlotButtons(bakeryOverview, false);

                buildingData.ID = 18;
                break;
            case 19:
                EditRegistryEntry(text, image, barracksOne, scale, "Barracks LVL1");
                registryEntry.transform.Find("Image/WorkerFrame/WorkersText").GetComponent<TextMeshProUGUI>().text = "Villagers"; // Change registry from workers to villagers
                GameObject barracksOverview = Instantiate(buildingOverviewScreenTwo);
                SetUpBuildingOverview(barracksOverview);
                InitializeUpgradeButtonForBuilding(barracksOverview, buildingData, placementManager.placedGameObjects.Count - 1, 19);
                ChangeBuildingOverviewName(barracksOverview, "Barracks LVL1");
                buildingData.BuildingName = "Barracks LVL1";

                AssignVillagerSlotButtons(barracksOverview, false);

                buildingData.ID = 19;
                break;
            case 20:
                EditRegistryEntry(text, image, bathHouseOne, scale, "Bath House LVL1");
                registryEntry.transform.Find("Image/WorkerFrame/WorkersText").GetComponent<TextMeshProUGUI>().text = "Villagers"; // Change registry from workers to villagers
                GameObject bathHouseOverview = Instantiate(buildingOverviewScreenTwo);
                SetUpBuildingOverview(bathHouseOverview);
                InitializeUpgradeButtonForBuilding(bathHouseOverview, buildingData, placementManager.placedGameObjects.Count - 1, 20);
                ChangeBuildingOverviewName(bathHouseOverview, "Bath House LVL1");
                buildingData.BuildingName = "Bath House LVL1";

                AssignVillagerSlotButtons(bathHouseOverview, false);

                buildingData.ID = 20;
                break;
            case 21:
                EditRegistryEntry(text, image, blacksmithOne, scale, "Blacksmith LVL1");
                buildingData.BuildingName = "Blacksmith LVL1";
                buildingData.ID = 21;
                break;
            case 22:
                EditRegistryEntry(text, image, carpenterOne, scale, "Carpenter LVL1");
                GameObject carpenterOverview = Instantiate(buildingOverviewScreen);
                SetUpBuildingOverview(carpenterOverview);
                ProductionBuilding carpenterData = building.GetComponent<Carpenter>();
                InitializeProductionBuildingOverview(carpenterOverview, "Carpenter LVL1", productionType, carpenterData);
                buildingData.BuildingName = "Carpenter LVL1";

                AssignVillagerSlotButtons(carpenterOverview, false);

                buildingData.ID = 22;
                break;
            case 23:
                EditRegistryEntry(text, image, farmOne, scale, "Farm LVL1");
                GameObject farmOverview = Instantiate(buildingOverviewScreen);
                SetUpBuildingOverview(farmOverview);
                ProductionBuilding farmData = building.GetComponent<Farm>();
                InitializeProductionBuildingOverview(farmOverview, "Farm LVL1", productionType, farmData);
                buildingData.BuildingName = "Farm LVL1";

                AssignVillagerSlotButtons(farmOverview, false);

                buildingData.ID = 23;
                break;
            case 24:
                EditRegistryEntry(text, image, hospitalOne, scale, "Hospital LVL1");
                registryEntry.transform.Find("Image/WorkerFrame/WorkersText").GetComponent<TextMeshProUGUI>().text = "Villagers"; // Change registry from workers to villagers
                GameObject hospitalOverview = Instantiate(buildingOverviewScreenTwo);
                SetUpBuildingOverview(hospitalOverview);
                InitializeUpgradeButtonForBuilding(hospitalOverview, buildingData, placementManager.placedGameObjects.Count - 1, 24);
                ChangeBuildingOverviewName(hospitalOverview, "Hospital LVL1");
                buildingData.BuildingName = "Hospital LVL1";

                AssignVillagerSlotButtons(hospitalOverview, false);

                buildingData.ID = 24;
                break;
            case 25:
                EditRegistryEntry(text, image, houseOne, scale, "House LVL1");
                registryEntry.transform.Find("Image/WorkerFrame/WorkersText").GetComponent<TextMeshProUGUI>().text = "Villagers"; // Change registry from workers to villagers
                GameObject houseOverview = Instantiate(buildingOverviewScreenTwo);
                SetUpBuildingOverview(houseOverview);

                houseOverview.transform.Find("Workers/WorkerOneFrame/ProgressBar").gameObject.SetActive(false);
                houseOverview.transform.Find("Workers/WorkerTwoFrame/ProgressBar").gameObject.SetActive(false);
                houseOverview.transform.Find("Workers/WorkerThreeFrame/ProgressBar").gameObject.SetActive(false);

                InitializeUpgradeButtonForBuilding(houseOverview, buildingData, placementManager.placedGameObjects.Count - 1, 25);
                ChangeBuildingOverviewName(houseOverview, "House LVL1");
                buildingData.BuildingName = "House LVL1";

                // Villager Assignment Buttons
                AssignVillagerSlotButtons(houseOverview, true);

                buildingData.ID = 25;
                break;
            case 26:
                EditRegistryEntry(text, image, loggerOne, scale, "Logger LVL1");
                GameObject loggerOverview = Instantiate(buildingOverviewScreen);
                SetUpBuildingOverview(loggerOverview);
                Logger loggerData = building.GetComponent<Logger>();
                InitializeProductionBuildingOverview(loggerOverview, "Logger LVL1", productionType, loggerData);
                buildingData.BuildingName = "Logger LVL1";

                AssignVillagerSlotButtons(loggerOverview, false);

                buildingData.ID = 26;
                break;
            case 27:
                EditRegistryEntry(text, image, lumberMillOne, scale, "Lumber Mill LVL1");
                GameObject lumberMilllOverview = Instantiate(buildingOverviewScreen);
                SetUpBuildingOverview(lumberMilllOverview);
                ProductionBuilding lumberMillData = building.GetComponent<LumberMill>();
                InitializeProductionBuildingOverview(lumberMilllOverview, "Lumber Mill LVL1", productionType, lumberMillData);
                buildingData.BuildingName = "Lumber Mill LVL1";

                AssignVillagerSlotButtons(lumberMilllOverview, false);

                buildingData.ID = 27;
                break;
            case 28:
                EditRegistryEntry(text, image, mineOne, scale, "Mine (Not Built)");
                registryEntry.SetActive(false);
                GameObject mineOverview = Instantiate(buildingOverviewScreen);
                SetUpBuildingOverview(mineOverview);
                ProductionBuilding mineData = building.GetComponent<Mine>();
                InitializeProductionBuildingOverview(mineOverview, "Mine (Not Built)", productionType, mineData);
                buildingData.BuildingName = "Mine (Not Built)";

                mineOverview.transform.Find("Workers/WorkerOneDisabled").gameObject.SetActive(true);
                mineOverview.transform.Find("Workers/WorkerOneFrame").gameObject.SetActive(false);

                AssignVillagerSlotButtons(mineOverview, false);

                buildingData.ID = 28;
                break;
            case 29:
                EditRegistryEntry(text, image, orchardOne, scale, "Orchard LVL1");
                buildingData.BuildingName = "Orchard LVL1";
                buildingData.ID = 29;
                break;
            case 30:
                EditRegistryEntry(text, image, barnOne, scale, "Ranch LVL1");
                GameObject barnOverview = Instantiate(buildingOverviewScreen);
                SetUpBuildingOverview(barnOverview);
                ProductionBuilding barnData = building.GetComponent<Barn>();
                InitializeProductionBuildingOverview(barnOverview, "Ranch LVL1", productionType, barnData);
                buildingData.BuildingName = "Ranch LVL1";

                AssignVillagerSlotButtons(barnOverview, false);

                buildingData.ID = 30;
                break;
            case 31:
                EditRegistryEntry(text, image, refineryOne, scale, "Refinery (Not Built)");
                registryEntry.SetActive(false);
                GameObject refineryOverview = Instantiate(buildingOverviewScreen);
                SetUpBuildingOverview(refineryOverview);
                ProductionBuilding refineryData = building.GetComponent<Refinery>();
                InitializeProductionBuildingOverview(refineryOverview, "Refinery (Not Built)", productionType, refineryData);
                buildingData.BuildingName = "Refinery (Not Built)";

                refineryOverview.transform.Find("Workers/WorkerOneDisabled").gameObject.SetActive(true);
                refineryOverview.transform.Find("Workers/WorkerOneFrame").gameObject.SetActive(false);

                AssignVillagerSlotButtons(refineryOverview, false);

                buildingData.ID = 31;
                break;
            case 32:
                EditRegistryEntry(text, image, schoolOne, scale, "School LVL1");
                registryEntry.transform.Find("Image/WorkerFrame/WorkersText").GetComponent<TextMeshProUGUI>().text = "Villagers"; // Change registry from workers to villagers
                GameObject schoolOverview = Instantiate(buildingOverviewScreenTwo);
                SetUpBuildingOverview(schoolOverview);
                InitializeUpgradeButtonForBuilding(schoolOverview, buildingData, placementManager.placedGameObjects.Count - 1, 32);
                ChangeBuildingOverviewName(schoolOverview, "School LVL1");
                buildingData.BuildingName = "School LVL1";

                AssignVillagerSlotButtons(schoolOverview, false);

                buildingData.ID = 32;
                break;
            case 33:
                EditRegistryEntry(text, image, stablesOne, scale, "Stables LVL1");
                registryEntry.transform.Find("Image/WorkerFrame/WorkersText").GetComponent<TextMeshProUGUI>().text = "Villagers"; // Change registry from workers to villagers
                buildingData.BuildingName = "Stables LVL1";

                // AssignVillagerButtonsInOverview(houseOverview, true);

                buildingData.ID = 33;
                break;
            case 34:
                EditRegistryEntry(text, image, tailorOne, scale, "Tailor LVL1");
                GameObject tailorOverview = Instantiate(buildingOverviewScreen);
                SetUpBuildingOverview(tailorOverview);
                ProductionBuilding tailorData = building.GetComponent<Tailor>();
                InitializeProductionBuildingOverview(tailorOverview, "Tailor LVL1", productionType, tailorData);
                buildingData.BuildingName = "Tailor LVL1";

                AssignVillagerSlotButtons(tailorOverview, false);

                buildingData.ID = 34;
                break;
            case 35:
                EditRegistryEntry(text, image, warehouseOne, scale, "Warehouse LVL1");
                buildingData.BuildingName = "Warehouse LVL1";
                break;
            case 36:
                EditRegistryEntry(text, image, mill, scale, "Windmill LVL1");
                GameObject millOverview = Instantiate(buildingOverviewScreen);
                SetUpBuildingOverview(millOverview);
                ProductionBuilding millData = building.GetComponent<Mill>();
                InitializeProductionBuildingOverview(millOverview, "Windmill LVL1", productionType, millData);
                buildingData.BuildingName = "Windmill LVL1";

                AssignVillagerSlotButtons(millOverview, false);

                buildingData.ID = 36;
                break;
            case 37:
                EditRegistryEntry(text, image, workshopOne, scale, "Workshop LVL1");
                GameObject workshopOverview = Instantiate(buildingOverviewScreen);
                SetUpBuildingOverview(workshopOverview);
                ProductionBuilding workshopData = building.GetComponent<Workshop>();
                InitializeProductionBuildingOverview(workshopOverview, "Workshop LVL1", productionType, workshopData);
                buildingData.BuildingName = "Workshop LVL1";

                AssignVillagerSlotButtons(workshopOverview, false);

                buildingData.ID = 37;
                break;
            case 38:
                EditRegistryEntry(text, image, marketOne, new Vector3(0.2f, 0.2f, 1f), "Market (Not Built)");

                // Adjust registry entry for market
                registryEntry.transform.Find("Image/Workers").gameObject.SetActive(false);
                registryEntry.transform.Find("Image/FrameTwo").gameObject.SetActive(true);

                // Disable registry
                registryEntry.SetActive(false);

                // Create Building Overview
                GameObject marketOverview = Instantiate(marketOverviewScreen);
                SetUpBuildingOverview(marketOverview);
                InitializeUpgradeButtonForBuilding(marketOverview, buildingData, placementManager.placedGameObjects.Count - 1, 38);
                // Initialize Market Overview

                buildingData.BuildingName = "Market (Not Built)";
                buildingData.ID = 38;
                break;
        }

        // Create Toggle Buttons
        int buildingWidth = structures.objectsData[structureID].Size.x;
        Vector3 buildingPosition = placementManager.placedGameObjects[placementManager.placedGameObjects.Count - 1].transform.position;
        Vector3Int cellPosition = Vector3Int.RoundToInt(buildingPosition);

        Vector3 buildingToggleUIPosition;
        if (buildingWidth == 2)
        {
            buildingToggleUIPosition = new Vector3(buildingPosition.x + 1, buildingPosition.y, buildingPosition.z);
        }
        else if (structureID == 38) // is a market
        {
            buildingToggleUIPosition = new Vector3(buildingPosition.x + 0.4f, buildingPosition.y, buildingPosition.z);
        }
        else if (structureID == 28) // is a mine
        {
            buildingToggleUIPosition = new Vector3(buildingPosition.x + 1.5f, buildingPosition.y + 0.7f, buildingPosition.z);
        }
        else
        {
            buildingToggleUIPosition = new Vector3(buildingPosition.x + 1.5f, buildingPosition.y, buildingPosition.z);
        }

        GameObject prefab = IsFixedBuilding(structureID) ? fixedBuildingToggleUI : buildingToggleUI;
        GameObject buildingToggleButtons = Instantiate(prefab, buildingToggleUIPosition, Quaternion.identity, buildingToggleParent);

        buildingToggleList.Add(buildingToggleButtons);

        // Set up InfoButton
        SetupButton(buildingToggleButtons.transform, "InfoButton", () =>
        {
            ActivateBuildingOverview(placementManager.selectedObjectIndex);
            TurnOffBuildingToggleUI();
        });

        // Set up EditButton only for non-fixed buildings
        if (!IsFixedBuilding(structureID))
        {
            SetupButton(buildingToggleButtons.transform, "EditButton", () =>
            {
                placementManager.SelectStructure(cellPosition);
                TurnOffBuildingToggleUI();
            });
        }
    }

    private void AssignVillagerSlotButtons(GameObject houseOverview, bool isHouse)
    {
        int buildingIndex = placementManager.placedGameObjects.Count - 1;

        // Building Overview
        GameObject firstVillagerOverview = houseOverview.transform.Find("Workers/WorkerOneFrame/Worker")?.gameObject;
        GameObject secondVillagerOverview = houseOverview.transform.Find("Workers/WorkerTwoFrame/Worker")?.gameObject;
        GameObject thirdVillagerOverview = houseOverview.transform.Find("Workers/WorkerThreeFrame/Worker")?.gameObject;

        Button firstVillagerOverviewButton = firstVillagerOverview.GetComponent<Button>();
        Button secondVillagerOverviewButton = secondVillagerOverview.GetComponent<Button>();
        Button thirdVillagerOverviewButton = thirdVillagerOverview.GetComponent<Button>();


        firstVillagerOverviewButton.onClick.AddListener(() =>
        {
            villagerManager.selectBuildingSlot(buildingIndex, 1, isHouse);
        });

        secondVillagerOverviewButton.onClick.AddListener(() =>
        {
            villagerManager.selectBuildingSlot(buildingIndex, 2, isHouse);
        });

        thirdVillagerOverviewButton.onClick.AddListener(() =>
        {
            villagerManager.selectBuildingSlot(buildingIndex, 3, isHouse);
        });

        // Building Registry
        GameObject buildingRegistryEntry = buildingRegistryList[buildingIndex];
        GameObject firstVillagerRegistry = buildingRegistryEntry.transform.Find("Image/Workers/WorkerOne/WorkerButton")?.gameObject;
        GameObject secondVillagerRegistry = buildingRegistryEntry.transform.Find("Image/Workers/WorkerTwo/WorkerButton")?.gameObject;
        GameObject thirdVillagerRegistry = buildingRegistryEntry.transform.Find("Image/Workers/WorkerThree/WorkerButton")?.gameObject;

        Button firstVillagerRegistryButton = firstVillagerRegistry.GetComponent<Button>();
        Button secondVillagerRegistryButton = secondVillagerRegistry.GetComponent<Button>();
        Button thirdVillagerRegistryButton = thirdVillagerRegistry.GetComponent<Button>();


        firstVillagerRegistryButton.onClick.AddListener(() =>
        {
            villagerManager.selectBuildingSlot(buildingIndex, 1, isHouse);
        });

        secondVillagerRegistryButton.onClick.AddListener(() =>
        {
            villagerManager.selectBuildingSlot(buildingIndex, 2, isHouse);
        });

        thirdVillagerRegistryButton.onClick.AddListener(() =>
        {
            villagerManager.selectBuildingSlot(buildingIndex, 3, isHouse);
        });
    }

    public void TurnOffBuildingToggleUI()
    {
        foreach (GameObject buildingToggleButtons in buildingToggleList)
        {
            buildingToggleButtons.SetActive(false);
        }
        foreach (GameObject decorationToggleButtons in decorationToggleList)
        {
            decorationToggleButtons.SetActive(false);
        }
        openToggleIndex = -1;
        openToggleType = -1;
    }

    public void ChangeBuildingToggleUIPlacement(int buildingIndex, Vector2Int buildingSize, Vector3Int gridPosition, int buildingType)
    {
        Vector3 buildingToggleUIPosition;
        GameObject buildingToggleButtons = (buildingType == 0) ? buildingToggleList[buildingIndex] : decorationToggleList[buildingIndex];
        if (buildingSize.x == 2)
        {
            buildingToggleUIPosition = new Vector3(gridPosition.x + 1, gridPosition.y, gridPosition.z);
        }
        else if (buildingSize.x == 1)
        {
            buildingToggleUIPosition = new Vector3(gridPosition.x + 0.5f, gridPosition.y, gridPosition.z);
        }
        else
        {
            buildingToggleUIPosition = new Vector3(gridPosition.x + 1.5f, gridPosition.y, gridPosition.z);
        }
        buildingToggleButtons.transform.position = buildingToggleUIPosition;
    }

    private void InitializeProductionBuildingOverview(GameObject buildingOverview, string buildingName, int productionType, ProductionBuilding buildingData)
    {
        //Set up
        ChangeBuildingOverviewName(buildingOverview, buildingName);
        buildingData.InitializeProduction();
        Transform productionTypeObject;

        if (productionType != 3)
        {
            productionTypeObject = buildingOverview.transform.Find(productionType.ToString());
        }
        else
        {
            productionTypeObject = buildingOverview.transform.Find("1");
        }

        productionTypeObject.gameObject.SetActive(true);

        // Goods interface
        if (productionType == 0)
        {
            int producedResourceID = buildingData.ProducedResourcesID[0];
            int producedResourceAmount = buildingData.ProducedResourcesAmount[0];

            Transform resourceIconObject = buildingOverview.transform.Find("0/OutputFrame/Image");
            Transform resourceTextObject = buildingOverview.transform.Find("0/OutputFrame/OutputAmount");

            TextMeshProUGUI resourceText = resourceTextObject.GetComponent<TextMeshProUGUI>();
            resourceText.text = producedResourceAmount.ToString() + "x";

            Image resourceIcon = resourceIconObject.GetComponent<Image>();
            resourceIcon.sprite = resources.resourcesData[producedResourceID].Icon;
            resourceIcon.SetNativeSize();
            SetResourceScale(producedResourceID, resourceIcon);
        }
        else if (productionType == 1)
        {
            int inputResourceID = buildingData.NeededResourcesID[0];
            int inputResourceAmount = buildingData.NeededResourcesAmount[0];

            int producedResourceID = buildingData.ProducedResourcesID[0];
            int producedResourceAmount = buildingData.ProducedResourcesAmount[0];

            Transform inputResourceIconObject = buildingOverview.transform.Find("1/InputFrame/Image");
            Transform inputResourceTextObject = buildingOverview.transform.Find("1/InputFrame/InputAmount");

            TextMeshProUGUI inputResourceText = inputResourceTextObject.GetComponent<TextMeshProUGUI>();
            inputResourceText.text = inputResourceAmount.ToString() + "x";

            Image inputResourceIcon = inputResourceIconObject.GetComponent<Image>();
            inputResourceIcon.sprite = resources.resourcesData[inputResourceID].Icon;
            inputResourceIcon.SetNativeSize();
            SetResourceScale(inputResourceID, inputResourceIcon);

            Transform producedResourceIconObject = buildingOverview.transform.Find("1/OutputFrame/Image");
            Transform producedResourceTextObject = buildingOverview.transform.Find("1/OutputFrame/OutputAmount");

            TextMeshProUGUI producedResourceText = producedResourceTextObject.GetComponent<TextMeshProUGUI>();
            producedResourceText.text = producedResourceAmount.ToString() + "x";

            Image producedResourceIcon = producedResourceIconObject.GetComponent<Image>();
            producedResourceIcon.sprite = resources.resourcesData[producedResourceID].Icon;
            producedResourceIcon.SetNativeSize();
            SetResourceScale(producedResourceID, producedResourceIcon);
        }
        else if (productionType == 2)
        {
            int inputResourceIDOne = buildingData.NeededResourcesID[0];
            int inputResourceAmountOne = buildingData.NeededResourcesAmount[0];

            int inputResourceIDTwo = buildingData.NeededResourcesID[1];
            int inputResourceAmountTwo = buildingData.NeededResourcesAmount[1];

            int producedResourceID = buildingData.ProducedResourcesID[0];
            int producedResourceAmount = buildingData.ProducedResourcesAmount[0];

            Transform inputResourceIconObjectOne = buildingOverview.transform.Find("2/InputFrameOne/Image");
            Transform inputResourceTextObjectOne = buildingOverview.transform.Find("2/InputFrameOne/InputAmount");

            Transform inputResourceIconObjectTwo = buildingOverview.transform.Find("2/InputFrameTwo/Image");
            Transform inputResourceTextObjectTwo = buildingOverview.transform.Find("2/InputFrameTwo/InputAmount");

            TextMeshProUGUI inputResourceTextOne = inputResourceTextObjectOne.GetComponent<TextMeshProUGUI>();
            inputResourceTextOne.text = inputResourceAmountOne.ToString() + "x";

            TextMeshProUGUI inputResourceTextTwo = inputResourceTextObjectTwo.GetComponent<TextMeshProUGUI>();
            inputResourceTextTwo.text = inputResourceAmountTwo.ToString() + "x";

            Image inputResourceIconOne = inputResourceIconObjectOne.GetComponent<Image>();
            inputResourceIconOne.sprite = resources.resourcesData[inputResourceIDOne].Icon;
            inputResourceIconOne.SetNativeSize();
            SetResourceScale(inputResourceIDOne, inputResourceIconOne);

            Image inputResourceIconTwo = inputResourceIconObjectTwo.GetComponent<Image>();
            inputResourceIconTwo.sprite = resources.resourcesData[inputResourceIDTwo].Icon;
            inputResourceIconTwo.SetNativeSize();
            SetResourceScale(inputResourceIDTwo, inputResourceIconTwo);

            Transform producedResourceIconObject = buildingOverview.transform.Find("2/OutputFrame/Image");
            Transform producedResourceTextObject = buildingOverview.transform.Find("2/OutputFrame/OutputAmount");

            TextMeshProUGUI producedResourceText = producedResourceTextObject.GetComponent<TextMeshProUGUI>();
            producedResourceText.text = producedResourceAmount.ToString() + "x";

            Image producedResourceIcon = producedResourceIconObject.GetComponent<Image>();
            producedResourceIcon.sprite = resources.resourcesData[producedResourceID].Icon;
            producedResourceIcon.SetNativeSize();
            SetResourceScale(producedResourceID, producedResourceIcon);
        }
        else if (productionType == 3)
        {
            // Input UI
            int inputResourceID = buildingData.NeededResourcesID[0];
            int inputResourceAmount = buildingData.NeededResourcesAmount[0];

            int producedResourceIDOne = buildingData.ProducedResourcesID[0];
            int producedResourceAmountOne = buildingData.ProducedResourcesAmount[0];

            int producedResourceIDTwo = buildingData.ProducedResourcesID[1];
            int producedResourceAmountTwo = buildingData.ProducedResourcesAmount[1];

            Transform inputResourceIconObjectOne = buildingOverview.transform.Find("1/InputFrame/Image");
            Transform inputResourceTextObjectOne = buildingOverview.transform.Find("1/InputFrame/InputAmount");

            Transform inputResourceIconObjectTwo = buildingOverview.transform.Find("1 (Two)/InputFrame/Image");
            Transform inputResourceTextObjectTwo = buildingOverview.transform.Find("1 (Two)/InputFrame/InputAmount");

            TextMeshProUGUI inputResourceTextOne = inputResourceTextObjectOne.GetComponent<TextMeshProUGUI>();
            inputResourceTextOne.text = inputResourceAmount.ToString() + "x";

            TextMeshProUGUI inputResourceTextTwo = inputResourceTextObjectTwo.GetComponent<TextMeshProUGUI>();
            inputResourceTextTwo.text = inputResourceAmount.ToString() + "x";

            Image inputResourceIconOne = inputResourceIconObjectOne.GetComponent<Image>();
            inputResourceIconOne.sprite = resources.resourcesData[inputResourceID].Icon;
            inputResourceIconOne.SetNativeSize();
            SetResourceScale(inputResourceID, inputResourceIconOne);

            Image inputResourceIconTwo = inputResourceIconObjectTwo.GetComponent<Image>();
            inputResourceIconTwo.sprite = resources.resourcesData[inputResourceID].Icon;
            inputResourceIconTwo.SetNativeSize();
            SetResourceScale(inputResourceID, inputResourceIconTwo);


            // Production UI
            Transform producedResourceIconObjectOne = buildingOverview.transform.Find("1/OutputFrame/Image");
            Transform producedResourceTextObjectOne = buildingOverview.transform.Find("1/OutputFrame/OutputAmount");

            Transform producedResourceIconObjectTwo = buildingOverview.transform.Find("1 (Two)/OutputFrame/Image");
            Transform producedResourceTextObjectTwo = buildingOverview.transform.Find("1 (Two)/OutputFrame/OutputAmount");

            TextMeshProUGUI producedResourceTextOne = producedResourceTextObjectOne.GetComponent<TextMeshProUGUI>();
            producedResourceTextOne.text = producedResourceAmountOne.ToString() + "x";

            TextMeshProUGUI producedResourceTextTwo = producedResourceTextObjectTwo.GetComponent<TextMeshProUGUI>();
            producedResourceTextTwo.text = producedResourceAmountTwo.ToString() + "x";

            Image producedResourceIconOne = producedResourceIconObjectOne.GetComponent<Image>();
            producedResourceIconOne.sprite = resources.resourcesData[producedResourceIDOne].Icon;
            producedResourceIconOne.SetNativeSize();
            SetResourceScale(producedResourceIDOne, producedResourceIconOne);

            Image producedResourceIconTwo = producedResourceIconObjectTwo.GetComponent<Image>();
            producedResourceIconTwo.sprite = resources.resourcesData[producedResourceIDTwo].Icon;
            producedResourceIconTwo.SetNativeSize();
            SetResourceScale(producedResourceIDTwo, producedResourceIconOne);

            Transform dualProductionToggleButtons = buildingOverview.transform.Find("ToggleProductionButtons");
            dualProductionToggleButtons.gameObject.SetActive(true);
        }

        // Set up Progress Bar
        int buildingIndex = placementManager.placedGameObjects.Count - 1;
        Transform progressBar = buildingOverview.transform.Find("UpgradeTime/ProgressBar");
        ProductionProgressBar productionProgressBar = progressBar.GetComponent<ProductionProgressBar>();

        // Initialize Progress Bar
        productionProgressBar.structureIndex = buildingIndex;
        productionProgressBar.Initialize(buildingData, 0);
        productionProgressBar.SetProductionTime(0);

        // If production type is 3, there are two progress bars
        if (productionType == 3)
        {
            Transform progressBarTwo = buildingOverview.transform.Find("UpgradeTimeTwo/ProgressBar");
            ProductionProgressBar productionProgressBarTwo = progressBarTwo.GetComponent<ProductionProgressBar>();
            productionProgressBarTwo.structureIndex = buildingIndex;
            productionProgressBarTwo.Initialize(buildingData, 1);
            productionProgressBarTwo.SetProductionTime(1);

            GameObject speedUpgradeButtonObjectTwo = buildingOverview.transform.Find("UpgradeTimeTwo/SpeedButtonCanBuy").gameObject;
            Button speedUpgradeButtonTwo = speedUpgradeButtonObjectTwo.GetComponent<Button>();
            speedUpgradeButtonTwo.onClick.AddListener(() => UpgradeProductionSpeedForBuildingWithIndex(buildingIndex, 1));
        }

        //Upgrade Buttons
        GameObject speedUpgradeButtonObject = buildingOverview.transform.Find("UpgradeTime/SpeedButtonCanBuy").gameObject;
        Button speedUpgradeButton = speedUpgradeButtonObject.GetComponent<Button>();
        speedUpgradeButton.onClick.AddListener(() => UpgradeProductionSpeedForBuildingWithIndex(buildingIndex, 0));
        InitializeUpgradeButtonForProductionBuilding(buildingOverview, buildingData, buildingIndex);
    }

    private static void SetResourceScale(int resourceID, Image resourceIcon)
    {
        if (resourceID == 11)
        {
            resourceIcon.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
        }
        else if (resourceID == 0 || resourceID == 5 || resourceID == 2)
        {
            resourceIcon.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else
        {
            resourceIcon.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        }
    }

    public void UpdateProductionMethodsUI(ProductionBuilding buildingData)
    {
        int buildingIndex = buildingData.Index;
        int productionType = buildingData.ProductionType;
        GameObject buildingOverview = buildingOverviewList[buildingIndex];
        // Goods interface
        if (productionType == 0)
        {
            int producedResourceAmount = buildingData.ProducedResourcesAmount[0];
            Transform resourceTextObject = buildingOverview.transform.Find("0/OutputFrame/OutputAmount");
            TextMeshProUGUI resourceText = resourceTextObject.GetComponent<TextMeshProUGUI>();
            resourceText.text = producedResourceAmount.ToString() + "x";
        }
        else if (productionType == 1)
        {
            int inputResourceAmount = buildingData.NeededResourcesAmount[0];
            int producedResourceAmount = buildingData.ProducedResourcesAmount[0];

            Transform inputResourceTextObject = buildingOverview.transform.Find("1/InputFrame/InputAmount");

            TextMeshProUGUI inputResourceText = inputResourceTextObject.GetComponent<TextMeshProUGUI>();
            inputResourceText.text = inputResourceAmount.ToString() + "x";

            Transform producedResourceIconObject = buildingOverview.transform.Find("1/OutputFrame/Image");
            Transform producedResourceTextObject = buildingOverview.transform.Find("1/OutputFrame/OutputAmount");

            TextMeshProUGUI producedResourceText = producedResourceTextObject.GetComponent<TextMeshProUGUI>();
            producedResourceText.text = producedResourceAmount.ToString() + "x";
        }
        else if (productionType == 2)
        {
            int inputResourceAmountOne = buildingData.NeededResourcesAmount[0];
            int inputResourceAmountTwo = buildingData.NeededResourcesAmount[1];
            int producedResourceAmount = buildingData.ProducedResourcesAmount[0];

            Transform inputResourceTextObjectOne = buildingOverview.transform.Find("2/InputFrameOne/InputAmount");
            Transform inputResourceTextObjectTwo = buildingOverview.transform.Find("2/InputFrameTwo/InputAmount");

            TextMeshProUGUI inputResourceTextOne = inputResourceTextObjectOne.GetComponent<TextMeshProUGUI>();
            inputResourceTextOne.text = inputResourceAmountOne.ToString() + "x";

            TextMeshProUGUI inputResourceTextTwo = inputResourceTextObjectTwo.GetComponent<TextMeshProUGUI>();
            inputResourceTextTwo.text = inputResourceAmountTwo.ToString() + "x";

            Transform producedResourceTextObject = buildingOverview.transform.Find("2/OutputFrame/OutputAmount");

            TextMeshProUGUI producedResourceText = producedResourceTextObject.GetComponent<TextMeshProUGUI>();
            producedResourceText.text = producedResourceAmount.ToString() + "x";
        }
        else if (productionType == 3)
        {
            // Input UI
            int inputResourceAmount = buildingData.NeededResourcesAmount[0];
            int producedResourceAmountOne = buildingData.ProducedResourcesAmount[0];
            int producedResourceAmountTwo = buildingData.ProducedResourcesAmount[1];

            Transform inputResourceTextObjectOne = buildingOverview.transform.Find("1/InputFrame/InputAmount");
            Transform inputResourceTextObjectTwo = buildingOverview.transform.Find("1 (Two)/InputFrame/InputAmount");

            TextMeshProUGUI inputResourceTextOne = inputResourceTextObjectOne.GetComponent<TextMeshProUGUI>();
            inputResourceTextOne.text = inputResourceAmount.ToString() + "x";

            TextMeshProUGUI inputResourceTextTwo = inputResourceTextObjectTwo.GetComponent<TextMeshProUGUI>();
            inputResourceTextTwo.text = inputResourceAmount.ToString() + "x";


            // Production UI
            Transform producedResourceTextObjectOne = buildingOverview.transform.Find("1/OutputFrame/OutputAmount");
            Transform producedResourceTextObjectTwo = buildingOverview.transform.Find("1 (Two)/OutputFrame/OutputAmount");

            TextMeshProUGUI producedResourceTextOne = producedResourceTextObjectOne.GetComponent<TextMeshProUGUI>();
            producedResourceTextOne.text = producedResourceAmountOne.ToString() + "x";

            TextMeshProUGUI producedResourceTextTwo = producedResourceTextObjectTwo.GetComponent<TextMeshProUGUI>();
            producedResourceTextTwo.text = producedResourceAmountTwo.ToString() + "x";
        }
    }

    private static void ChangeBuildingOverviewName(GameObject buildingOverview, string buildingName)
    {
        Transform buildingNameObject = buildingOverview.transform.Find("BuildingName");
        TextMeshProUGUI buildingNameText = buildingNameObject.GetComponent<TextMeshProUGUI>();
        buildingNameText.text = buildingName;
    }

    private void InitializeUpgradeButtonForProductionBuilding(GameObject buildingOverview, Building buildingData, int buildingIndex)
    {
        GameObject upgradeButtonObject = buildingOverview.transform.Find("Upgrade/ButtonCanBuy").gameObject;
        Button upgradeButton = upgradeButtonObject.GetComponent<Button>();
        upgradeButton.onClick.AddListener(() => UpgradeProductionBuildingWithIndex(buildingIndex));
        UpdateBuildingOverviewUpgradeButton(buildingData, buildingOverview);
    }

    private void InitializeUpgradeButtonForBuilding(GameObject buildingOverview, Building buildingData, int buildingIndex, int structureID)
    {
        GameObject upgradeButtonObject = buildingOverview.transform.Find("Upgrade/ButtonCanBuy").gameObject;
        Button upgradeButton = upgradeButtonObject.GetComponent<Button>();
        upgradeButton.onClick.AddListener(() => UpgradeBuildingWithIndex(buildingIndex, structureID));
        UpdateBuildingOverviewUpgradeButton(buildingData, buildingOverview);
    }

    public void UpgradeBuildingWithIndex(int buildingIndex, int structureID)
    {
        //Close overview
        buildingOverviewList[buildingIndex].SetActive(false);
        // Get Building
        GameObject building = placementManager.placedGameObjects[buildingIndex];
        Building buildingData = building.GetComponent<Building>();
        // Upgrade the first market instantly
        if (buildingIndex == 0)
        {
            buildingData.constructionDurationsPerLevel[0] = 0;
        }
        buildingData.UpgradeBuilding();

        GameObject buildingOverview = buildingOverviewList[buildingIndex];
        GameObject buildingRegistry = buildingRegistryList[buildingIndex];

        //Building Name
        Transform buildingOverviewNameObject = buildingOverview.transform.Find("BuildingName");
        TextMeshProUGUI buildingOverviewName = buildingOverviewNameObject.GetComponent<TextMeshProUGUI>();
        buildingOverviewName.text = buildingData.BuildingName;

        Transform buildingRegistryNameObject = buildingRegistry.transform.Find("Image/BuildingName");
        TextMeshProUGUI buildingRegistryName = buildingRegistryNameObject.GetComponent<TextMeshProUGUI>();
        buildingRegistryName.text = buildingData.BuildingName;

        //Building Sprite
        Transform buildingRegistryImageObject = buildingRegistry.transform.Find("Image/BuildingImage");
        Image buildingRegistryImage = buildingRegistryImageObject.GetComponent<Image>();

        int[] villagersBuilding = new int[] { 19, 20, 24, 25, 32 };

        switch (buildingData.Level)
        {
            case 1:
                buildingRegistryImage.sprite = buildingData.BuildingLevelOne;
                buildingRegistry.SetActive(true);
                break;
            case 2:
                buildingRegistryImage.sprite = buildingData.BuildingLevelTwo;
                if (Array.Exists(villagersBuilding, element => element == structureID))
                {
                    buildingOverview.transform.Find("Workers/WorkerTwoDisabled").gameObject.SetActive(false);
                    buildingOverview.transform.Find("Workers/WorkerTwoFrame").gameObject.SetActive(true);

                    buildingRegistry.transform.Find("Image/Workers/HideTwo").gameObject.SetActive(false);
                    buildingRegistry.transform.Find("Image/Workers/WorkerTwo").gameObject.SetActive(true);
                }
                break;
            case 3:
                buildingRegistryImage.sprite = buildingData.BuildingLevelThree;
                if (Array.Exists(villagersBuilding, element => element == structureID))
                {
                    buildingOverview.transform.Find("Workers/WorkerThreeDisabled").gameObject.SetActive(false);
                    buildingOverview.transform.Find("Workers/WorkerThreeFrame").gameObject.SetActive(true);

                    buildingRegistry.transform.Find("Image/Workers/HideThree").gameObject.SetActive(false);
                    buildingRegistry.transform.Find("Image/Workers/WorkerThree").gameObject.SetActive(true);
                }
                break;
        }

        buildingRegistryImage.SetNativeSize();
        
        // Market
        if ((buildingIndex == 0 || buildingIndex == 1) && buildingData.Level > 1)
        {
            buildingRegistryImage.rectTransform.localScale = new Vector3(0.1f, 0.1f, 1f);
        }

        //Edit Upgrade Button
        UpdateBuildingOverviewUpgradeButton(buildingData, buildingOverview);
    }

    private void SetUpBuildingOverview(GameObject buildingOverview)
    {
        buildingOverview.transform.SetParent(uiRoot, false);
        RectTransform buildingOverviewRectTransform = buildingOverview.GetComponent<RectTransform>();
        buildingOverviewRectTransform.anchoredPosition = Vector2.zero;
        buildingOverviewList.Add(buildingOverview);
        AssignExitButtonFunctionality(buildingOverview);
    }

    private void AssignRegistryButton(GameObject registryEntry)
    {
        Transform registryButtonObject = registryEntry.transform.Find("Image");
        Button registryButton = registryButtonObject.GetComponent<Button>();
        int structureIndex = buildingRegistryList.Count - 1;
        registryButton.onClick.AddListener(() => ActivateBuildingOverviewThroughRegistry(structureIndex));
        placementManager.selectedObjectIndex = structureIndex;
    }

    private void AssignExitButtonFunctionality(GameObject buildingOverview)
    {
        Transform exitButtonObject = buildingOverview.transform.Find("Exit");
        if (exitButtonObject != null)
        {
            Button exitButton = exitButtonObject.GetComponent<Button>();

            // Ensure the Button component exists
            if (exitButton != null)
            {
                int index = buildingOverviewList.Count - 1; // Save the index
                exitButton.onClick.AddListener(() => CloseBuildingOverview(index)); // Use a lambda to capture the index
            }
            else
            {
                Debug.LogError("No Button component found on the prefab!");
            }
        }
        else
        {
            Debug.LogError("Exit button object not found!");
        }
    }

    private static void EditRegistryEntry(TextMeshProUGUI text, Image image, Sprite buildingSprite, Vector3 imageScale, string buildingName)
    {
        image.sprite = buildingSprite;
        image.SetNativeSize();
        image.transform.localScale = imageScale;
        text.text = buildingName;
    }

    private void OnRemoved(int index, int type)
    {
        if (type == 1)
        {
            GameObject decorationToggleButtons = decorationToggleList[index];
            decorationToggleList.RemoveAt(index);
            Destroy(decorationToggleButtons);
            return;
        }

        // Get the registry entry at the specified index
        GameObject registryEntry = buildingRegistryList[index];
        GameObject buildingOverivew = buildingOverviewList[index];
        GameObject buildingToggleButtons = buildingToggleList[index];

        // Remove the entry from the list
        buildingRegistryList.RemoveAt(index);
        buildingOverviewList.RemoveAt(index);
        buildingToggleList.RemoveAt(index);

        // Destroy the registry entry GameObject
        Destroy(registryEntry);
        Destroy(buildingOverivew);
        Destroy(buildingToggleButtons);

        // Update the height and position of the UI element
        RectTransform rectTransform = buildingRegistryColumn.GetComponent<RectTransform>();

        rectTransform.sizeDelta = new Vector2(
            rectTransform.sizeDelta.x, // Keep the current width
            rectTransform.sizeDelta.y - 116 // Decrease the height
        );

        rectTransform.anchoredPosition = new Vector2(
            rectTransform.anchoredPosition.x, // Keep the current x position
            rectTransform.anchoredPosition.y + 58 // Raise the y position
        );

        openToggleIndex = -1;
        openToggleType = -1;
    }

    public void UpgradeProductionSpeedForBuildingWithIndex(int index, int resourceIndex)
    {
        // Get building script and upgrade it
        GameObject building = placementManager.placedGameObjects[index];
        ProductionBuilding buildingData = building.GetComponent<ProductionBuilding>();
        buildingData.UpgradeProductionSpeed(resourceIndex);

        GameObject buildingOverview = buildingOverviewList[index];
        if (resourceIndex == 0)
        {
            Transform progressBarObject = buildingOverview.transform.Find("UpgradeTime/ProgressBar");
            ProductionProgressBar progressbar = progressBarObject.GetComponent<ProductionProgressBar>();
            progressbar.productionTime = buildingData.ResourceProductionTime[resourceIndex];

            Transform productionSpeedLevelDisplay = buildingOverview.transform.Find("UpgradeTime/LevelFrame/ProductionSpeedText");
            TextMeshProUGUI productionSpeedLevelText = productionSpeedLevelDisplay.GetComponent<TextMeshProUGUI>();

            productionSpeedLevelText.text = "LVL" + buildingData.ProductionSpeedLevel[0];
        }
        else
        {
            Transform progressBarObject = buildingOverview.transform.Find("UpgradeTimeTwo/ProgressBar");
            ProductionProgressBar progressbar = progressBarObject.GetComponent<ProductionProgressBar>();
            progressbar.productionTime = buildingData.ResourceProductionTime[resourceIndex];

            Transform productionSpeedLevelDisplay = buildingOverview.transform.Find("UpgradeTimeTwo/LevelFrame/ProductionSpeedText");
            TextMeshProUGUI productionSpeedLevelText = productionSpeedLevelDisplay.GetComponent<TextMeshProUGUI>();

            productionSpeedLevelText.text = "LVL" + buildingData.ProductionSpeedLevel[1];
        }

        isProductionSpeedUpgradeAvailable();
    }

    public void UpgradeProductionBuildingWithIndex(int index)
    {
        // Close overview
        buildingOverviewList[index].SetActive(false);
        // Get Building
        GameObject building = placementManager.placedGameObjects[index];
        ProductionBuilding buildingData = building.GetComponent<ProductionBuilding>();
        buildingData.UpgradeBuilding();

        GameObject buildingOverview = buildingOverviewList[index];
        GameObject buildingRegistry = buildingRegistryList[index];

        //Building Name
        Transform buildingOverviewNameObject = buildingOverview.transform.Find("BuildingName");
        TextMeshProUGUI buildingOverviewName = buildingOverviewNameObject.GetComponent<TextMeshProUGUI>();
        buildingOverviewName.text = buildingData.BuildingName;

        Transform buildingRegistryNameObject = buildingRegistry.transform.Find("Image/BuildingName");
        TextMeshProUGUI buildingRegistryName = buildingRegistryNameObject.GetComponent<TextMeshProUGUI>();
        buildingRegistryName.text = buildingData.BuildingName;

        //Building Sprite
        Transform buildingRegistryImageObject = buildingRegistry.transform.Find("Image/BuildingImage");
        Image buildingRegistryImage = buildingRegistryImageObject.GetComponent<Image>();

        switch (buildingData.Level)
        {
            case 1:
                buildingRegistryImage.sprite = buildingData.BuildingLevelOne;
                buildingRegistry.SetActive(true);

                buildingOverview.transform.Find("Workers/WorkerOneDisabled").gameObject.SetActive(false);
                buildingOverview.transform.Find("Workers/WorkerOneFrame").gameObject.SetActive(true);

                break;
            case 2:
                buildingRegistryImage.sprite = buildingData.BuildingLevelTwo;

                buildingOverview.transform.Find("Workers/WorkerTwoDisabled").gameObject.SetActive(false);
                buildingOverview.transform.Find("Workers/WorkerTwoFrame").gameObject.SetActive(true);

                buildingRegistry.transform.Find("Image/Workers/HideTwo").gameObject.SetActive(false);
                buildingRegistry.transform.Find("Image/Workers/WorkerTwo").gameObject.SetActive(true);

                break;
            case 3:
                buildingRegistryImage.sprite = buildingData.BuildingLevelThree;

                buildingOverview.transform.Find("Workers/WorkerThreeDisabled").gameObject.SetActive(false);
                buildingOverview.transform.Find("Workers/WorkerThreeFrame").gameObject.SetActive(true);

                buildingRegistry.transform.Find("Image/Workers/HideThree").gameObject.SetActive(false);
                buildingRegistry.transform.Find("Image/Workers/WorkerThree").gameObject.SetActive(true);

                break;
        }

        buildingRegistryImage.SetNativeSize();

        if (buildingData.ProductionType == 0)
        {
            int producedResourceAmount = buildingData.ProducedResourcesAmount[0];
            Transform resourceTextObject = buildingOverview.transform.Find("0/OutputFrame/OutputAmount");

            TextMeshProUGUI resourceText = resourceTextObject.GetComponent<TextMeshProUGUI>();
            resourceText.text = producedResourceAmount.ToString() + "x";
        }
        else if (buildingData.ProductionType == 1)
        {
            int inputResourceAmount = buildingData.NeededResourcesAmount[0];
            int producedResourceAmount = buildingData.ProducedResourcesAmount[0];

            Transform inputResourceTextObject = buildingOverview.transform.Find("1/InputFrame/InputAmount");

            TextMeshProUGUI inputResourceText = inputResourceTextObject.GetComponent<TextMeshProUGUI>();
            inputResourceText.text = inputResourceAmount.ToString() + "x";

            Transform producedResourceTextObject = buildingOverview.transform.Find("1/OutputFrame/OutputAmount");

            TextMeshProUGUI producedResourceText = producedResourceTextObject.GetComponent<TextMeshProUGUI>();
            producedResourceText.text = producedResourceAmount.ToString() + "x";
        }
        else if (buildingData.ProductionType == 2)
        {
            int inputResourceAmountOne = buildingData.NeededResourcesAmount[0];
            int inputResourceAmountTwo = buildingData.NeededResourcesAmount[1];
            int producedResourceAmount = buildingData.ProducedResourcesAmount[0];

            Transform inputResourceTextObjectOne = buildingOverview.transform.Find("2/InputFrameOne/InputAmount");
            Transform inputResourceTextObjectTwo = buildingOverview.transform.Find("2/InputFrameTwo/InputAmount");

            TextMeshProUGUI inputResourceTextOne = inputResourceTextObjectOne.GetComponent<TextMeshProUGUI>();
            inputResourceTextOne.text = inputResourceAmountOne.ToString() + "x";

            TextMeshProUGUI inputResourceTextTwo = inputResourceTextObjectTwo.GetComponent<TextMeshProUGUI>();
            inputResourceTextTwo.text = inputResourceAmountTwo.ToString() + "x";

            Transform producedResourceTextObject = buildingOverview.transform.Find("2/OutputFrame/OutputAmount");

            TextMeshProUGUI producedResourceText = producedResourceTextObject.GetComponent<TextMeshProUGUI>();
            producedResourceText.text = producedResourceAmount.ToString() + "x";
        }
        else if (buildingData.ProductionType == 3)
        {
            int inputResourceAmount = buildingData.NeededResourcesAmount[0];
            int producedResourceAmountOne = buildingData.ProducedResourcesAmount[0];
            int producedResourceAmountTwo = buildingData.ProducedResourcesAmount[1];

            Transform inputResourceTextObjectOne = buildingOverview.transform.Find("1/InputFrame/InputAmount");
            Transform inputResourceTextObjectTwo = buildingOverview.transform.Find("1 (Two)/InputFrame/InputAmount");

            TextMeshProUGUI inputResourceTextOne = inputResourceTextObjectOne.GetComponent<TextMeshProUGUI>();
            inputResourceTextOne.text = inputResourceAmount.ToString() + "x";

            TextMeshProUGUI inputResourceTextTwo = inputResourceTextObjectTwo.GetComponent<TextMeshProUGUI>();
            inputResourceTextTwo.text = inputResourceAmount.ToString() + "x";

            Transform producedResourceTextObjectOne = buildingOverview.transform.Find("1/OutputFrame/OutputAmount");
            Transform producedResourceTextObjectTwo = buildingOverview.transform.Find("1 (Two)/OutputFrame/OutputAmount");

            TextMeshProUGUI producedResourceTextOne = producedResourceTextObjectOne.GetComponent<TextMeshProUGUI>();
            producedResourceTextOne.text = producedResourceAmountOne.ToString() + "x";

            TextMeshProUGUI producedResourceTextTwo = producedResourceTextObjectTwo.GetComponent<TextMeshProUGUI>();
            producedResourceTextTwo.text = producedResourceAmountTwo.ToString() + "x";
        }

        //Reset Progress Bar
        Transform progressBar = buildingOverview.transform.Find("UpgradeTime/ProgressBar");
        ProductionProgressBar productionProgressBar = progressBar.GetComponent<ProductionProgressBar>();
        productionProgressBar.timer = 0;

        if (buildingData.ProductionType == 3)
        {
            Transform progressBarTwo = buildingOverview.transform.Find("UpgradeTimeTwo/ProgressBar");
            ProductionProgressBar productionProgressBarTwo = progressBarTwo.GetComponent<ProductionProgressBar>();
            productionProgressBarTwo.timer = 0;
        }

        //Edit Upgrade Button
        UpdateBuildingOverviewUpgradeButton(buildingData, buildingOverview);
    }

    private void UpdateBuildingOverviewUpgradeButton(Building buildingData, GameObject buildingOverview)
    {
        Transform upgradeButtonObject = buildingOverview.transform.Find("Upgrade/Button");
        Transform upgradeButtonCanBuyObject = buildingOverview.transform.Find("Upgrade/ButtonCanBuy");

        Button upgradeButton = upgradeButtonObject.GetComponent<Button>();
        Button upgradeButtonCanBuy = upgradeButtonCanBuyObject.GetComponent<Button>();

        Transform upgradeButtonTextObject = buildingOverview.transform.Find("Upgrade/Button/ButtonText");
        Transform upgradeButtonCanBuyTextObject = buildingOverview.transform.Find("Upgrade/ButtonCanBuy/ButtonCanBuyText");

        TextMeshProUGUI upgradeButtonText = upgradeButtonTextObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI upgradeButtonCanBuyText = upgradeButtonCanBuyTextObject.GetComponent<TextMeshProUGUI>();


        if (buildingData.Level == 0)
        {
            upgradeButtonText.text = "Build";
            upgradeButtonCanBuyText.text = "Build";
            isUpgradeAvailable();
        }
        else if (buildingData.Level == 1)
        {
            upgradeButtonText.text = "Upgrade to Level 2";
            upgradeButtonCanBuyText.text = "Upgrade to Level 2";
            isUpgradeAvailable();
        }
        else if (buildingData.Level == 2)
        {
            upgradeButtonText.text = "Upgrade to Level 3";
            upgradeButtonCanBuyText.text = "Upgrade to Level 3";
            isUpgradeAvailable();
        }
        else if (buildingData.Level == 3)
        {
            upgradeButtonText.text = "MAXED OUT";
            upgradeButtonCanBuyText.text = "MAXED OUT";
            upgradeButtonCanBuyObject.gameObject.SetActive(false);
        }
    }

    private bool isUpgradeAvailable()
    {
        return true;
    }

    private bool isProductionSpeedUpgradeAvailable()
    {
        return true;
    }

    private void OnTapped()
    {
        if (placementManager.isBuilding == false)
        {
            // Get mouse click position --> Check building Index at position --> setActive toggle UI
            Vector3 mapPosition = inputManager.GetSelectedMapPosition();
            Vector3Int gridPosition = Vector3Int.FloorToInt(mapPosition);
            int buildingIndex = placementManager.gridData.GetPlacementDataAt(gridPosition)?.PlaceObjectIndex ?? -1;
            placementManager.selectedObjectIndex = buildingIndex;
            ObjectType? buildingType = placementManager.gridData.GetPlacementDataAt(gridPosition)?.Type;

            // If tapped on building or decoration
            if (buildingIndex != -1 && buildingType.HasValue)
            {
                // If tapped on same building or decoration
                if (buildingIndex == openToggleIndex && buildingType == (ObjectType)openToggleType)
                {
                    if (buildingType == ObjectType.Object)
                    {
                        buildingToggleList[openToggleIndex].SetActive(false);
                    }
                    else if (buildingType == ObjectType.Decoration)
                    {
                        decorationToggleList[openToggleIndex].SetActive(false);
                    }
                    openToggleIndex = -1;
                    openToggleType = -1;
                    return;
                }
                buildingSelected?.Invoke(gridPosition);

                // If there is already a toggle open, close it
                if (openToggleIndex != -1)
                {
                    if (openToggleType == (int)ObjectType.Object)
                    {
                        buildingToggleList[openToggleIndex].SetActive(false);
                    }
                    else if (openToggleType == (int)ObjectType.Decoration)
                    {
                        decorationToggleList[openToggleIndex].SetActive(false);
                    }
                }

                // Open tapped toggle
                if (buildingType == ObjectType.Object)
                {
                    BuildingState state = placementManager.placedGameObjects[buildingIndex].GetComponent<Building>().State;
                    if (state != BuildingState.UnderConstruction)
                    {
                        buildingToggleList[buildingIndex].SetActive(true);
                    }
                }
                else if (buildingType == ObjectType.Decoration)
                {
                    decorationToggleList[buildingIndex].SetActive(true);
                }

                openToggleIndex = buildingIndex;
                openToggleType = (int)buildingType.Value;
            }
            // If tapped on empty
            else if (openToggleIndex != -1)
            {
                if (openToggleType == (int)ObjectType.Object)
                {
                    buildingToggleList[openToggleIndex].SetActive(false);
                }
                else if (openToggleType == (int)ObjectType.Decoration)
                {
                    decorationToggleList[openToggleIndex].SetActive(false);
                }
                openToggleIndex = -1;
                openToggleType = -1;
            }
        }
    }

    private void OnBuildingMoved(Vector3Int newPosition, int buildingIndex, int buildingType)
    {
        if (buildingType == 1)
        {
            GameObject decorationToggleButtons = decorationToggleList[buildingIndex];
            Transform decorationEditButtonObject = decorationToggleButtons.transform.Find("EditButton");
            Button decorationEditButton = decorationEditButtonObject.GetComponent<Button>();
            if (decorationEditButton != null)
            {
                decorationEditButton.onClick.RemoveAllListeners();
                decorationEditButton.onClick.AddListener(() => placementManager.SelectStructure(newPosition));
                decorationEditButton.onClick.AddListener(() => TurnOffBuildingToggleUI());
            }
        }
        else if (buildingType == 0)
        {
            GameObject buildingToggleButtons = buildingToggleList[buildingIndex];
            Transform editButtonObject = buildingToggleButtons.transform.Find("EditButton");
            Button editButton = editButtonObject.GetComponent<Button>();
            if (editButton != null)
            {
                editButton.onClick.RemoveAllListeners();
                editButton.onClick.AddListener(() => placementManager.SelectStructure(newPosition));
                editButton.onClick.AddListener(() => TurnOffBuildingToggleUI());
            }
        }
        openToggleIndex = -1;
        openToggleType = -1;
    }

    private bool IsFixedBuilding(int structureID)
    {
        return structureID == 28 || structureID == 31 || structureID == 38;
    }

    private void SetupButton(Transform parent, string buttonName, UnityEngine.Events.UnityAction action)
    {
        Transform buttonTransform = parent.Find(buttonName);
        if (buttonTransform == null) return;

        Button button = buttonTransform.GetComponent<Button>();
        if (button == null) return;

        button.onClick.AddListener(action);
    }

    private void ChangeOverviewVillagerSlot(int buildingIndex, int villagerSlot, Villager villager)
    {
        GameObject buildingOverview = buildingOverviewList[buildingIndex];
        GameObject overviewSlotImage = null;

        GameObject buildingRegistry = buildingRegistryList[buildingIndex];
        GameObject registrySlotImage = null;

        switch (villagerSlot)
        {
            case 1:
                overviewSlotImage = buildingOverview.transform.Find("Workers/WorkerOneFrame/Worker").gameObject;
                registrySlotImage = buildingRegistry.transform.Find("Image/Workers/WorkerOne/WorkerButton").gameObject;
                break;
            case 2:
                overviewSlotImage = buildingOverview.transform.Find("Workers/WorkerTwoFrame/Worker").gameObject;
                registrySlotImage = buildingRegistry.transform.Find("Image/Workers/WorkerTwo/WorkerButton").gameObject;
                break;
            case 3:
                overviewSlotImage = buildingOverview.transform.Find("Workers/WorkerThreeFrame/Worker").gameObject;
                registrySlotImage = buildingRegistry.transform.Find("Image/Workers/WorkerThree/WorkerButton").gameObject;
                break;
            default:
                Debug.LogWarning("Invalid villager slot: " + villagerSlot);
                return;
        }

        Image overviewImageComponent = overviewSlotImage.GetComponent<Image>();

        overviewImageComponent.sprite = villager.villagerData.villagerIcon;
        overviewImageComponent.SetNativeSize();
        overviewSlotImage.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

        Vector3 pos = overviewSlotImage.transform.localPosition;
        overviewSlotImage.transform.localPosition = new Vector3(-1f, pos.y, pos.z);

        Image registryImageComponent = registrySlotImage.GetComponent<Image>();

        registryImageComponent.sprite = villager.villagerData.villagerIcon;
        registryImageComponent.SetNativeSize();
        registrySlotImage.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

        Vector3 registryPos = registrySlotImage.transform.localPosition;
        registrySlotImage.transform.localPosition = new Vector3(-1f, pos.y, pos.z);
    }

}
