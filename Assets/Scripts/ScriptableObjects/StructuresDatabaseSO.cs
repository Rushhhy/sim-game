using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class StructuresDatabaseSO : ScriptableObject
{
    public List<StructureData> objectsData;

    public Vector2Int GetSizeByID(int id)
    {
        var data = objectsData.FirstOrDefault(obj => obj.ID == id);
        return data != null ? data.Size : Vector2Int.zero;
    }
}

[Serializable]
public class StructureData
{
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public int ID { get; private set; }
    [field: SerializeField]
    public GameObject Base { get; private set; }
    [field: SerializeField]
    public GameObject LevelOnePrefab { get; private set; }
    [field: SerializeField]
    public GameObject LevelTwoPrefab { get; private set; }
    [field: SerializeField]
    public GameObject LevelThreePrefab { get; private set; }
    [field: SerializeField]
    public Vector2Int Size { get; private set; } = Vector2Int.one;
}
