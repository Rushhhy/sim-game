using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EquipmentsDataSO : ScriptableObject
{
    public List<EquipmentData> equipmentsData;
}

[Serializable]
public class EquipmentData
{
    [field: SerializeField, Tooltip("Name of the equipment")]
    public string Name { get; private set; }
    [field: SerializeField, Tooltip("ID of the equipiment")]
    public int ID { get; private set; }
    [field: SerializeField, Tooltip("Icon representing the equipment")]
    public Sprite Icon { get; private set; }
}

