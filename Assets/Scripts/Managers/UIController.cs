using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    [SerializeField]
    private GameObject mainMenu, confirmCancelPanel, buildMenu, buildingsList, decorationsList, selectedBuildings, selectedDecorations, overviewMenu, mainOverview,
        productionOverview, buildingRegistry, shopMenu, villagersShop, equipmentShop, mainShop, dailyRewards, selectedVillagersShop, selectedWeaponsShop,
        selectedGemsShop, selectedDailyRewards, missionsMenu, dailyMissions, normalMissions, campaignMissions, selectedDailyMissions, selectedNormalMissions,
        selectedCampaignMissions, selectBuildingsButton, selectDecorationsButton, selectVillagersShopButton, selectWeaponsShopButton, selectGemsShopButton, selectDailyRewardsButton,
        selectDailyMissionsButton, selectNormalMissionsButton, selectCampaignMissionsButton, editPanel, buildRotatePanel, editRotatePanel, roadPanel, shopRefreshText, villagerInventory;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    // Build Menu UI
    public void AccessBuildMenu()
    {
        buildMenu.SetActive(true);
    }

    public void ExitBuildMenu()
    {
        buildMenu.SetActive(false);
    }

    public void SwitchToBuildings()
    {
        buildingsList.SetActive(true);
        decorationsList.SetActive(false);

        selectedBuildings.SetActive(true);
        selectBuildingsButton.SetActive(false);

        selectedDecorations.SetActive(false);
        selectDecorationsButton.SetActive(true);
    }

    public void SwitchToDecorations()
    {
        buildingsList.SetActive(false);
        decorationsList.SetActive(true);

        selectedBuildings.SetActive(false);
        selectBuildingsButton.SetActive(true);

        selectedDecorations.SetActive(true);
        selectDecorationsButton.SetActive(false);
    }

    public void ActivateBuildPanel()
    {
        buildMenu.SetActive(false);
        confirmCancelPanel.SetActive(true);
    }

    public void DeactivateBuildPanel()
    {
        confirmCancelPanel.SetActive(false);
    }

    public void ActivateEditPanel()
    {
        editPanel.SetActive(true);
    }

    public void DeactivateEditPanel()
    {
        editPanel.SetActive(false);
    }

    public void ActivateEditRotatePanel()
    {
        editRotatePanel.SetActive(true);
    }

    public void DeactivateEditRotatePanel()
    {
        editRotatePanel.SetActive(false);
    }

    public void ActivateBuildRotatePanel()
    {
        buildMenu.SetActive(false);
        buildRotatePanel.SetActive(true);
    }

    public void DeactivateBuildRotatePanel()
    {
        buildRotatePanel.SetActive(false);
    }

    // Overview UI
    public void AccessOverview()
    {
        overviewMenu.SetActive(true);
    }

    public void ExitOverview()
    {
        overviewMenu.SetActive(false);
    }

    public void OverviewToProduction()
    {
        mainOverview.SetActive(false);
        productionOverview.SetActive(true);
    }

    public void OverviewToBuildingRegistry()
    {
        mainOverview.SetActive(false);
        buildingRegistry.SetActive(true);
    }

    public void ProductionToBuildingRegistry()
    {
        productionOverview.SetActive(false);
        buildingRegistry.SetActive(true);
    }

    public void ProductionToOverview()
    {
        productionOverview.SetActive(false);
        mainOverview.SetActive(true);
    }

    public void BuildingRegistryToOverview()
    {
        buildingRegistry.SetActive(false);
        mainOverview.SetActive(true);
    }

    public void BuildingRegistryToProduction()
    {
        buildingRegistry.SetActive(false);
        productionOverview.SetActive(true);
    }

    // Shop UI
    public void AccessShopMenu()
    {
        shopMenu.SetActive(true);
    }

    public void ExitShopMenu()
    {
        shopMenu.SetActive(false);
    }

    public void SwitchToVillagersShop()
    {
        villagersShop.SetActive(true);
        equipmentShop.SetActive(false);
        mainShop.SetActive(false);

        selectedVillagersShop.SetActive(true);
        selectedWeaponsShop.SetActive(false);
        selectedGemsShop.SetActive(false);

        selectVillagersShopButton.SetActive(false);
        selectWeaponsShopButton.SetActive(true);
        selectGemsShopButton.SetActive(true);

        shopRefreshText.SetActive(true);
    }

    public void SwitchToEquipmentShop()
    {
        villagersShop.SetActive(false);
        equipmentShop.SetActive(true);
        mainShop.SetActive(false);

        selectedVillagersShop.SetActive(false);
        selectedWeaponsShop.SetActive(true);
        selectedGemsShop.SetActive(false);

        selectVillagersShopButton.SetActive(true);
        selectWeaponsShopButton.SetActive(false);
        selectGemsShopButton.SetActive(true);

        shopRefreshText.SetActive(true);
    }

    public void SwitchToMainShop()
    {
        villagersShop.SetActive(false);
        equipmentShop.SetActive(false);
        mainShop.SetActive(true);

        selectedVillagersShop.SetActive(false);
        selectedWeaponsShop.SetActive(false);
        selectedGemsShop.SetActive(true);

        selectVillagersShopButton.SetActive(true);
        selectWeaponsShopButton.SetActive(true);
        selectGemsShopButton.SetActive(false);

        shopRefreshText.SetActive(false);
    }

    // Mission UI
    public void AccessMissionMenu()
    {
        missionsMenu.SetActive(true);
    }

    public void ExitMissionMenu()
    {
        missionsMenu.SetActive(false);
    }

    public void SwitchToDailyMissions()
    {
        dailyMissions.SetActive(true);
        normalMissions.SetActive(false);
        campaignMissions.SetActive(false);

        selectDailyMissionsButton.SetActive(false);
        selectNormalMissionsButton.SetActive(true);
        selectCampaignMissionsButton.SetActive(true);

        selectedDailyMissions.SetActive(true);
        selectedNormalMissions.SetActive(false);
        selectedCampaignMissions.SetActive(false);
    }

    public void SwitchToNormalMissions()
    {
        dailyMissions.SetActive(false);
        normalMissions.SetActive(true);
        campaignMissions.SetActive(false);

        selectDailyMissionsButton.SetActive(true);
        selectNormalMissionsButton.SetActive(false);
        selectCampaignMissionsButton.SetActive(true);

        selectedDailyMissions.SetActive(false);
        selectedNormalMissions.SetActive(true);
        selectedCampaignMissions.SetActive(false);
    }

    public void SwitchToCampaignMissions()
    {
        dailyMissions.SetActive(false);
        normalMissions.SetActive(false);
        campaignMissions.SetActive(true);

        selectDailyMissionsButton.SetActive(true);
        selectNormalMissionsButton.SetActive(true);
        selectCampaignMissionsButton.SetActive(false);

        selectedDailyMissions.SetActive(false);
        selectedNormalMissions.SetActive(false);
        selectedCampaignMissions.SetActive(true);
    }

    public void ActivateRoadBuildPanel()
    {
        buildMenu.SetActive(false);
        roadPanel.SetActive(true);
    }

    public void DeactivateRoadBuildPanel()
    {
        roadPanel.SetActive(false );
    }

    public void UpdateRoadPlacedUI(int value)
    {
        Transform roadPanelObject = roadPanel.transform.Find("RoadConfirmDemolish/CostPanel/CostContents/CostText");
        TextMeshProUGUI roadPanelText = roadPanelObject.GetComponent<TextMeshProUGUI>();
        roadPanelText.text = value.ToString();
    }

    // Villager
    public void DeactivateVillagerInventory()
    {
        villagerInventory.SetActive(false);
    }
}
