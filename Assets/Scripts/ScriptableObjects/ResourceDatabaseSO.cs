using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ResourceSO : ScriptableObject
{
    public List<ResourceData> resourcesData;
}

[Serializable]
public class ResourceData
{
    [field: SerializeField, Tooltip("Name of the resource")]
    public string Name { get; private set; }
    [field: SerializeField, Tooltip("ID of the resource")]
    public int ID { get; private set; }
    [field: SerializeField, Tooltip("Icon representing the resource")]
    public Sprite Icon { get; private set; }
    [field: SerializeField, Tooltip("Price of the resource in the game's currency")]
    public int Price { get; private set; }
}
