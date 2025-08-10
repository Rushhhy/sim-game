using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

public class ProductionBuilding : Building
{
    public int[] ProductionSpeedLevel { get; protected set; }
    public int ProductionType;

    public int[] NeededResourcesID { get; protected set; }
    public int[] NeededResourcesAmount { get; protected set; }
    public int[] ProducedResourcesID { get; protected set; }
    public int[] ProducedResourcesAmount { get; protected set; }

    public float[] ResourceProductionTime { get; protected set; }
    
    public float[] ProductionPerSec {  get; protected set; }
    public float[] ConsumptionPerSec { get; protected set; }

    public float[] individualTimers { get; protected set; }

    protected ResourceManager resourceManager;
    protected ProductionRegistryManager productionRegistryManager;

    protected int[] inputMethodBase, inputMethodOne, inputMethodTwo, inputMethodThree;
    protected int[] outputMethodBase, outputMethodOne, outputMethodTwo, outputMethodThree;

    private int numVillagersAssigned = 0;

    protected override void Awake()
    {
        base.Awake();

        ProductionSpeedLevel = new int[] { 1, 1 };
        individualTimers = new float[2];

        InitializeProduction();

        resourceManager = GameObject.Find("ResourceManager").GetComponent<ResourceManager>();
        productionRegistryManager = GameObject.Find("ProductionRegistryManager").GetComponent<ProductionRegistryManager>();
    }

    public void InitializeProduction()
    {
        if (NeededResourcesAmount == null)
        {
            NeededResourcesAmount = inputMethodBase;
        }
        if (ProducedResourcesAmount == null)
        {
            ProducedResourcesAmount = outputMethodBase;
        }
    }

    protected override void Update()
    {
        base.Update();
        ProduceResource();
    }

    protected void ProduceResource()
    {
        if (ProducedResourcesID == null || ResourceProductionTime == null || ProducedResourcesAmount == null)
            return;

        if (State != BuildingState.Active)
            return;

        if (numVillagersAssigned == 0)
            return;

        for (int i = 0; i < ProducedResourcesID.Length; i++)
        {
            individualTimers[i] += Time.deltaTime;

            if (individualTimers[i] < ResourceProductionTime[i])
                continue;

            // Reset per iteration
            bool canProduceNow = true;

            if (NeededResourcesID != null && NeededResourcesAmount != null && NeededResourcesID.Length > 0)
            {
                for (int j = 0; j < NeededResourcesID.Length; j++)
                {
                    if (NeededResourcesAmount[j] > resourceManager.resourceTotals[NeededResourcesID[j]])
                    {
                        canProduceNow = false;
                        break;
                    }
                }

                if (canProduceNow)
                {
                    for (int j = 0; j < NeededResourcesID.Length; j++)
                    {
                        resourceManager.resourceTotals[NeededResourcesID[j]] -= NeededResourcesAmount[j];
                        productionRegistryManager.UpdateTotalOfResourceWithID(NeededResourcesID[j]);
                    }
                    if (ProductionType == 3)
                    {
                        resourceManager.resourceTotals[NeededResourcesID[0]] += 1;
                        productionRegistryManager.UpdateTotalOfResourceWithID(NeededResourcesID[0]);
                    }
                    resourceManager.resourceTotals[ProducedResourcesID[i]] += ProducedResourcesAmount[i];
                    productionRegistryManager.UpdateTotalOfResourceWithID(ProducedResourcesAmount[i]);
                }
            }
            else
            {
                resourceManager.resourceTotals[ProducedResourcesID[i]] += ProducedResourcesAmount[i];
                productionRegistryManager.UpdateTotalOfResourceWithID(ProducedResourcesID[i]);
            }

            individualTimers[i] = 0f;
        }
    }

    public override void ClearVillagers()
    {
        base.ClearVillagers();
        numVillagersAssigned = 0;
        UpdateProductionMethod();
    }

    public void UpdateProductionMethod()
    {
        ResetProductionRates();

        switch (numVillagersAssigned)
        {
            case 0:
                NeededResourcesAmount = inputMethodBase;
                ProducedResourcesAmount = outputMethodBase;
                break;
            case 1:
                NeededResourcesAmount = inputMethodOne;
                ProducedResourcesAmount = outputMethodOne;
                break;
            case 2:
                NeededResourcesAmount = inputMethodTwo;
                ProducedResourcesAmount = outputMethodTwo;
                break;
            case 3:
                NeededResourcesAmount = inputMethodThree;
                ProducedResourcesAmount = outputMethodThree;
                break;
        }

        UpdateProductionMethodRates();

        buildingRegistryManager.UpdateProductionMethodsUI(this);
    }

    private void ResetProductionRates()
    {
        // Mine/Ranch producton type
        if (ResourceProductionTime.Length > 1)
        {
            for (int i = 0; i < ResourceProductionTime.Length; i++)
            {
                resourceManager.resourceConsumptionTotals[NeededResourcesID[i]] -= ((float)NeededResourcesAmount[i] / ResourceProductionTime[i]);
                resourceManager.resourceProductionTotals[ProducedResourcesID[i]] -= ((float)ProducedResourcesAmount[i] / ResourceProductionTime[i]);
            }
        }
        // Default production type
        else
        {
            for (int i = 0; i < NeededResourcesID.Length; i++)
            {
                resourceManager.resourceConsumptionTotals[NeededResourcesID[i]] -= ((float)NeededResourcesAmount[i] / ResourceProductionTime[0]);
            }
            for (int i = 0; i < ProducedResourcesID.Length; i++)
            {
                resourceManager.resourceProductionTotals[ProducedResourcesID[i]] -= ((float)ProducedResourcesAmount[i] / ResourceProductionTime[0]);
            }
        }
    }

    private void UpdateProductionMethodRates()
    {
        // Mine/Ranch producton type
        if (ResourceProductionTime.Length > 1)
        {
            for (int i = 0; i < ResourceProductionTime.Length; i++)
            {
                resourceManager.resourceConsumptionTotals[NeededResourcesID[i]] += ((float)NeededResourcesAmount[i] / ResourceProductionTime[i]);
                productionRegistryManager.UpdateConsumptionRateOfResourceWithID(NeededResourcesID[i]);
                resourceManager.resourceProductionTotals[ProducedResourcesID[i]] += ((float)ProducedResourcesAmount[i] / ResourceProductionTime[i]);
                productionRegistryManager.UpdateProductionRateOfResourceWithID(ProducedResourcesID[i]);
            }
        }
        // Default production type
        else
        {
            for (int i = 0; i < NeededResourcesID.Length; i++)
            {
                resourceManager.resourceConsumptionTotals[NeededResourcesID[i]] += ((float)NeededResourcesAmount[i] / ResourceProductionTime[0]);
                productionRegistryManager.UpdateConsumptionRateOfResourceWithID(NeededResourcesID[i]);
            }
            for (int i = 0; i < ProducedResourcesID.Length; i++)
            {
                resourceManager.resourceProductionTotals[ProducedResourcesID[i]] += ((float)ProducedResourcesAmount[i] / ResourceProductionTime[0]);
                productionRegistryManager.UpdateProductionRateOfResourceWithID(ProducedResourcesID[i]);
            }
        }
    }

    public void UpgradeProductionSpeed(int productionIndex)
    {
        ResetProductionRates();
        ProductionSpeedLevel[productionIndex]++;
        ResourceProductionTime[productionIndex] *= 0.99f; // 1% faster
        UpdateProductionMethodRates();
    }

    public override void UpgradeBuilding()
    {
        base.UpgradeBuilding();     
    }

    public override void AssignVillagerToSlot(int slot, Villager villager)
    {
        base.AssignVillagerToSlot(slot, villager);
        numVillagersAssigned += 1;
        UpdateProductionMethod();
    }

    public override void RemoveVillagerFromSlot(int slot)
    {
        base.RemoveVillagerFromSlot(slot);
        numVillagersAssigned -= 1;
        UpdateProductionMethod();
    }
}
