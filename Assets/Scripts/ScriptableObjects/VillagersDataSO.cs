using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class VillagersDataSO : ScriptableObject
{
    public List<VillagerData> villagersData;
    public VillagerData GetVillagerDataByID(int id)
    {
        foreach (var data in villagersData)
        {
            if (data.ID == id)
                return data;
        }

        Debug.LogWarning($"VillagerData with ID {id} not found.");
        return null;
    }
}

[Serializable]
public class VillagerData
{
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public int ID { get; private set; }
    [field: SerializeField]
    public int tier;
    [field: SerializeField]
    public Sprite villagerIcon { get; private set; }
    [field: SerializeField]
    public Sprite horseIcon { get; private set; }
    [field: SerializeField]
    public RuntimeAnimatorController villagerAnimatorController { get; private set; }
    [field: SerializeField]
    public RuntimeAnimatorController battleAnimatorController { get; private set; }
    [field: SerializeField]
    public RuntimeAnimatorController battleHorseAnimatorController { get; private set; }
    [field: SerializeField]
    public int range { get; private set; }
}
