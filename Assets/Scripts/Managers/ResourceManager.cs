using System;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public int[] resourceTotals = new int[13];
    public float[] resourceProductionTotals = new float[13];
    public float[] resourceConsumptionTotals = new float[13];

    private string[] resourceNames = new string[]
    {
        "Wheat", "Wood", "Tools", "Stone", "Iron", "Hardwood",
        "Leather", "Meat", "Flour", "Bread", "Clothing",
        "Furniture", "Gems"
    };


    public void PrintTotals()
    {
        string totals = "Resource Totals:\n";
        for (int i = 0; i < Mathf.Min(resourceTotals.Length, resourceNames.Length); i++)
        {
            totals += $"{resourceNames[i]}: {resourceTotals[i]}\n";
        }
        Debug.Log(totals);
    }
}

