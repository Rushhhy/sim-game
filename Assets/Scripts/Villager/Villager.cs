using System.Collections;
using UnityEngine;

public enum VillagerState
{
    Base,
    Idle,
    Working,
}

public class Villager : MonoBehaviour, IIdleBehaviorTarget
{
    public VillagerData villagerData;
    public int Index;
    public VillagerState currentState;

    private PlacementSystem placementSystem;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private IdleBehavior idleBehavior;

    // Interface implementations
    public Transform Transform => transform;
    public Animator Animator => animator;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public bool ShouldContinueIdling => currentState == VillagerState.Idle;

    // ... all your existing fields ...
    public int happiness;
    public int level = 1;
    public int upgradePoints = 0;
    public float HEALTH = 100f;
    public float attack;
    public float defense;

    public float evasion;
    public float critChance;

    public int mobility;
    public int range;
    public Animator animator;

    public bool isCavalry = false;
    public bool isRanged = false;
    public bool isHoused = false;
    public bool isEmployed = false;

    public int assignedHouseIndex = -1;
    public int assignedHouseSlot = -1;
    public int assignedBuildingIndex = -1;
    public int assignedBuildingSlot = -1;

    public int assignedBuildingID = -1;
    public Vector3 assignedBuildingPosition = Vector3.zero;

    private void Awake()
    {
        idleBehavior = GetComponent<IdleBehavior>();
        
    }

    public void Initialize(VillagerData vd)
    {
        villagerData = vd;
        placementSystem = GameObject.Find("PlacementSystem").GetComponent<PlacementSystem>();

        animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = villagerData.villagerAnimatorController;

        if (villagerData.horseIcon != null)
        {
            isCavalry = true;
        }
        if (villagerData.range > 0)
        {
            isRanged = true;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj is Villager other)
        {
            return this.Index == other.Index;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Index.GetHashCode();
    }

    // ... all your existing methods ...
    public void LevelUp() { level++; }
    public void UpgradeDefense() { defense = defense * 1.05f; }
    public void UpgradeAttack() { attack = attack * 1.05f; }
    public void UpgradeEvasion() { evasion = evasion * 1.05f; }
    public void UpgradeCritChance() { critChance = critChance * 1.05f; }

    public void Housed(int houseIndex, int selectedSlot)
    {
        assignedHouseIndex = houseIndex;
        assignedHouseSlot = selectedSlot;
        isHoused = true;

        // Set idle start position
        Vector3 housePosition = placementSystem.placedGameObjects[houseIndex].transform.position;
        idleBehavior.SetIdleStartPosition(housePosition);

        // Only start idling if not employed
        if (!isEmployed)
        {
            currentState = VillagerState.Idle;
            idleBehavior.StartIdling();
        }
    }

    public void Employed(int buildingIndex, int selectedSlot)
    {
        // Stop idling when employed
        idleBehavior.StopIdling();

        assignedBuildingIndex = buildingIndex;
        assignedBuildingSlot = selectedSlot;

        assignedBuildingID = placementSystem.placedGameObjects[assignedBuildingIndex].GetComponent<Building>().ID;
        assignedBuildingPosition = placementSystem.placedGameObjects[assignedBuildingIndex].transform.position;

        currentState = VillagerState.Working;
        isEmployed = true;
    }

    public void Unemploy()
    {
        assignedBuildingIndex = -1;
        assignedBuildingSlot = -1;
        assignedBuildingID = -1;
        assignedBuildingPosition = Vector3.zero;
        isEmployed = false;

        // Set idle position and start idling if housed
        idleBehavior.SetIdleStartPosition(transform.position);

        if (isHoused)
        {
            currentState = VillagerState.Idle;
            idleBehavior.StartIdling();
        }
    }

    public void RemoveFromVillage()
    {
        // Stop all behaviors
        idleBehavior.StopIdling();

        currentState = VillagerState.Base;
        assignedBuildingIndex = -1;
        assignedHouseIndex = -1;
        assignedBuildingSlot = -1;
        assignedHouseSlot = -1;
        isHoused = false;
        isEmployed = false;
        transform.position = new Vector3(0, 0, 0);
        idleBehavior.SetIdleStartPosition(transform.position);
    }
}
