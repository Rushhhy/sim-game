using System;
using UnityEngine;
using System.Collections.Generic;

public class VillagerManager : MonoBehaviour
{
    public event Action<int> OnVillagerBought, OnVillagerHoused, OnVillagerRemovedWithoutReplacement;
    public event Action<int, int, bool> OnVillagerRemoved; // villagerIndex, inVillageIndex, isEmployed
    public event Action<int, int, Villager> OnVillagerAssigned; // this is for building registry manager, input is building index, inVillagerIndex, villager slot
    public event Action<int, int, int, string> OnVillagerEmployed; // inVillageIndex, previousVillagerInVillageIndex, prevVillagerIndex, buildingName
    public event Action<Villager, bool> OnVillagerTotallyRemoved; // adjust slot UI


    [SerializeField]
    private GameObject villagerPrefab;
    
    [SerializeField]
    private PlacementSystem placementManager;

    public List<Villager> villagersHeld = new List<Villager>();
    public List<int> villagersHeldID = new List<int>();
    public List<Villager> villagersInVillage = new List<Villager>();
    public List<int> villagerInVillageIndex = new List<int>(); // list of index of villager in villagers held for every villager in village

    public int selectedBuildingIndex;
    public int selectedVillagerSlot;
    public bool isHouse;

    [SerializeField]
    private VillagersDataSO villagerDatabase;
    [SerializeField]
    private VillagerInventoryManager inventoryManager;

    public void selectBuildingSlot(int buildingIndex, int villagerSlot, bool buildingIsHouse)
    {
        selectedBuildingIndex = buildingIndex;
        selectedVillagerSlot = villagerSlot;
        isHouse = buildingIsHouse;
        if (isHouse)
        {
            inventoryManager.openAllInventory();
        }
        else
        {
            inventoryManager.openAvailableInventory();
        }
    }


    public void buyVillager(int villagerID)
    {
        if (villagersHeldID.Contains(villagerID))
        {
            return;
        }
        villagersHeldID.Add(villagerID);
        VillagerData villagerData = villagerDatabase.GetVillagerDataByID(villagerID);
        // Instantiate villager prefab
        GameObject villagerGO = Instantiate(villagerPrefab, transform);
        Villager villager = villagerGO.GetComponent<Villager>();
        Animator villagerAnimator = villagerGO.GetComponent<Animator>();

        // Initialize villager
        villager.Initialize(villagerData);
        villagersHeld.Add(villager);
        villager.Index = villagersHeld.Count - 1;

        OnVillagerBought?.Invoke(villagerID);
    }

    public void AssignVillagertoBuilding(int villagerIndex)
    {
        Villager villager = villagersHeld[villagerIndex];

        if (isHouse)
        {
            villagersInVillage.Add(villager);
            villagerInVillageIndex.Add(villagerIndex);
            villager.Housed(selectedBuildingIndex, selectedVillagerSlot);
        }

        GameObject building = placementManager.placedGameObjects[selectedBuildingIndex];
        Building buildingComponent = building.GetComponent<Building>();
        Villager previousVillager = buildingComponent.GetVillagerInSlot(selectedVillagerSlot);

        int inVillageIndex = villagerInVillageIndex.IndexOf(villagerIndex);
        int prevInVillageIndex = -1;
        int prevVillagerIndex = -1;
        if (previousVillager != null)
        {
            prevVillagerIndex = previousVillager.Index;
            prevInVillageIndex = villagerInVillageIndex.IndexOf(prevVillagerIndex);
            if (isHouse)
            {
                Villager prevVillager = villagersHeld[prevVillagerIndex];
                RemoveVillagerFromVillage(prevVillagerIndex, prevVillager.isEmployed);
            }
            else
            {
                int prevVillagerSlot = previousVillager.assignedBuildingSlot;
                buildingComponent.RemoveVillagerFromSlot(prevVillagerSlot);
                previousVillager.Unemploy();

            }
        }

        // Assign the new villager to the selected slot
        buildingComponent.AssignVillagerToSlot(selectedVillagerSlot, villager);

        if (isHouse)
        {
            OnVillagerHoused?.Invoke(villagerIndex);
        }
        else
        {
            // If selected villager is already employed
            if (villager.isEmployed)
            {
                placementManager.placedGameObjects[villager.assignedBuildingIndex].GetComponent<Building>().RemoveVillagerFromSlot(villager.assignedBuildingSlot);
                OnVillagerTotallyRemoved?.Invoke(villager, false);
            }
            OnVillagerEmployed?.Invoke(inVillageIndex, prevInVillageIndex, prevVillagerIndex, buildingComponent.BuildingName);
            villager.Employed(selectedBuildingIndex, selectedVillagerSlot);
        }

        OnVillagerAssigned?.Invoke(selectedBuildingIndex, selectedVillagerSlot, villager);

        selectedBuildingIndex = -1;
        selectedVillagerSlot = -1;
        isHouse = false;
    }

    public void RemoveVillagerFromBuilding(int villagerIndex)
    {
        Villager villager = villagersHeld[villagerIndex];

        placementManager.placedGameObjects[villager.assignedBuildingIndex].GetComponent<Building>().RemoveVillagerFromSlot(villager.assignedBuildingSlot);
        OnVillagerRemovedWithoutReplacement?.Invoke(villagerIndex);
        OnVillagerTotallyRemoved?.Invoke(villager, false);
        villager.Unemploy();
    }

    public void RemoveVillagerFromVillage(int villagerIndex, bool isEmployed)
    {
        Villager villager = villagersHeld[villagerIndex];
        Building house = placementManager.placedGameObjects[villager.assignedHouseIndex].GetComponent<Building>();
        if (isEmployed)
        {
            Building building = placementManager.placedGameObjects[villager.assignedBuildingIndex].GetComponent<Building>();
            OnVillagerTotallyRemoved?.Invoke(villager, false);
            building.RemoveVillagerFromSlot(villager.assignedBuildingSlot);
            villager.Unemploy();
        }

        int availableVillagerIndex = villagerInVillageIndex.IndexOf(villagerIndex);
        villagerInVillageIndex.RemoveAt(availableVillagerIndex);
        villagersInVillage.RemoveAt(availableVillagerIndex);

        OnVillagerRemoved?.Invoke(villagerIndex, availableVillagerIndex, isEmployed);
        OnVillagerTotallyRemoved?.Invoke(villager, true);
        house.RemoveVillagerFromSlot(villager.assignedHouseSlot);
        villager.RemoveFromVillage();
    }
}
