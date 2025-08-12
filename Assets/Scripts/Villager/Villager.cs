using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VillagerState
{
    Base,
    Idle,
    Working,
}

public class Villager : MonoBehaviour, IIdleBehaviorTarget, IWorkBehaviorTarget, IPositionValidationTarget
{
    public VillagerData villagerData;
    public int Index;
    public VillagerState currentState;

    private PlacementSystem placementSystem;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private IdleBehavior idleBehavior;
    [SerializeField] private WorkBehavior workBehavior;
    private PositionValidator positionValidator;

    #region IIdleBehaviorTarget Implementation
    public Transform Transform => transform;
    public Animator Animator => animator;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public bool ShouldContinueIdling => currentState == VillagerState.Idle;
    #endregion

    #region IWorkBehaviorTarget Implementation
    // Using the same interface properties as IIdleBehaviorTarget since they're identical
    // Transform, Animator, SpriteRenderer are already implemented above
    #endregion

    #region IPositionValidationTarget Implementation
    public bool IsValidationEnabled => currentState != VillagerState.Base; // Only validate when villager is in village
    #endregion

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
        positionValidator = GetComponent<PositionValidator>();
        workBehavior = GetComponent<WorkBehavior>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
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

        // Force position validation check after housing
        if (positionValidator != null)
        {
            positionValidator.RequestValidation();
        }
    }

    public void Employed(int buildingIndex, int selectedSlot)
    {
        // Stop idling when employed
        if (idleBehavior != null && idleBehavior.IsIdling)
        {
            idleBehavior.StopIdling();
        }

        // Set employment data
        assignedBuildingIndex = buildingIndex;
        assignedBuildingSlot = selectedSlot;
        isEmployed = true;
        currentState = VillagerState.Working;

        // Get building information
        Building building = placementSystem.placedGameObjects[assignedBuildingIndex].GetComponent<Building>();
        if (building == null)
        {
            Debug.LogError($"Building component not found on assigned building for villager {gameObject.name}");
            return;
        }

        assignedBuildingID = building.ID;
        assignedBuildingPosition = placementSystem.placedGameObjects[buildingIndex].transform.position;

        // Get work positions from the building
        List<Vector3> workPositions = GetWorkPositionsFromBuilding(building);

        // Move villager to building position BEFORE starting work behavior
        // This ensures the villager is at the correct location when work behavior starts
        transform.position = assignedBuildingPosition;

        // Set work behavior building position and start working
        if (workBehavior != null)
        {
            workBehavior.SetBuildingPos(assignedBuildingPosition);

            // Wait one frame to ensure position is set before starting work
            StartCoroutine(StartWorkingDelayed(assignedBuildingID, assignedBuildingPosition, workPositions));
        }
        else
        {
            Debug.LogError($"WorkBehavior component not found on villager {gameObject.name}");
        }

        // Validate position after employment
        if (positionValidator != null)
        {
            positionValidator.RequestValidation();
        }
    }

    /// <summary>
    /// Starts working after a one-frame delay to ensure position is properly set
    /// </summary>
    private IEnumerator StartWorkingDelayed(int buildingID, Vector3 buildingPosition, List<Vector3> workPositions)
    {
        yield return null; // Wait one frame

        Debug.Log($"Starting work behavior for villager {gameObject.name} - Building ID: {buildingID}");
        workBehavior.StartWorking(buildingID, buildingPosition, workPositions);
    }

    public void Unemploy()
    {
        // Stop work behavior first
        if (workBehavior != null && isEmployed)
        {
            // You might want to add a StopWorking() method to WorkBehavior for cleaner shutdown
            workBehavior.SetBuildingPos(Vector3.zero);
        }

        // Clear employment data
        assignedBuildingIndex = -1;
        assignedBuildingSlot = -1;
        assignedBuildingID = -1;
        assignedBuildingPosition = Vector3.zero;
        isEmployed = false;
        workBehavior.StopWorking();

        // Transition back to idle state if housed
        if (isHoused)
        {
            currentState = VillagerState.Idle;

            // Set idle position to current position and start idling
            if (idleBehavior != null)
            {
                idleBehavior.SetIdleStartPosition(transform.position);
                idleBehavior.StartIdling();
            }
        }
        else
        {
            // If not housed, go to base state
            currentState = VillagerState.Base;
        }

        // Validate position after state change
        if (positionValidator != null)
        {
            positionValidator.RequestValidation();
        }
    }

    public void RemoveFromVillage()
    {
        // Stop all behaviors
        if (idleBehavior != null)
        {
            idleBehavior.StopIdling();
        }

        if (workBehavior != null && isEmployed)
        {
            workBehavior.StopWorking();
        }

        // Reset all state
        currentState = VillagerState.Base;
        assignedBuildingIndex = -1;
        assignedHouseIndex = -1;
        assignedBuildingSlot = -1;
        assignedHouseSlot = -1;
        assignedBuildingID = -1;
        assignedBuildingPosition = Vector3.zero;
        isHoused = false;
        isEmployed = false;

        transform.position = Vector3.zero;

        if (idleBehavior != null)
        {
            idleBehavior.SetIdleStartPosition(transform.position);
        }

        if (workBehavior != null)
        {
            workBehavior.SetBuildingPos(transform.position);
        }
    }

    private List<Vector3> GetWorkPositionsFromBuilding(Building building)
    {
        // If Building has a workPositions field
        if (building.workPositions != null)
            return building.workPositions;
        else
            return new List<Vector3>();
    }
}