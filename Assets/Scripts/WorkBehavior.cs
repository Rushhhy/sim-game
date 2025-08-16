using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorkState
{
    None,
    Bathing,
    Transporting,
    Training,
    Logging,
    Producing,
    Watering,
    Mining
}

[System.Serializable]
public class WorkBehaviorSettings
{
    [Header("Movement Settings")]
    public float movementSpeed = 1f;

    [Header("Work Settings")]
    public float workInterval = 10f;
    public float workDuration = 30f; // Duration for Mining, Producing, Watering

    [Header("Animation Settings")]
    public string bathingAnimationName = "Swim";
    public string walkAnimationName = "Walk";
    public string boxWalkAnimationName = "Box";
    public string waterAnimationName = "Water";
    public string axeAnimationName = "Axe";
    public string axeWalkAnimationName = "Axe Walk";
    public string attackAnimationName = "Attack";
    public string workAnimationName = "Work";
    public string mineAnimationName = "Mine";

    [Header("Market Positions")]
    public Vector3 marketPositionOne = new Vector3(-2.73f, 29f, 0f);
    public Vector3 marketPositionTwo = new Vector3(-1.355f, 29f, 0f);
    public Vector3 marketPositionThree = new Vector3(4.2f, 29f, 0f);
    public Vector3 marketPositionFour = new Vector3(5.65f, 29f, 0f);

    [Header("Sprite Flipping")]
    public bool enableSpriteFlipping = true;
    public bool isLeftFacingDefault = true;

    [Header("Pathfinding")]
    public PathfindingSettings pathfindingSettings;
}

public interface IWorkBehaviorTarget
{
    Transform Transform { get; }
    Animator Animator { get; }
    SpriteRenderer SpriteRenderer { get; }
}

public class WorkBehavior : MonoBehaviour
{
    private WorkState workState;
    private List<Vector3> workPos;
    private Vector3 buildingPos;
    private int currentBuildingID;

    private float validationDisabledUntil = 0f;
    private const float VALIDATION_TRANSITION_DELAY = 3f;

    public bool IsValidationTemporarilyDisabled => Time.time < validationDisabledUntil;

    [SerializeField] private GridData gridData;
    [SerializeField] private WorkBehaviorSettings settings;

    private IWorkBehaviorTarget target;
    private GridPathfinder pathfinder;

    // Specialized behavior components
    private LoggingBehavior loggingBehavior;
    private TransportingBehavior transportingBehavior;

    // Remaining coroutines for non-separated behaviors
    private Coroutine producingCoroutine;
    private Coroutine bathingCoroutine;
    private Coroutine trainingCoroutine;
    private Coroutine wateringCoroutine;
    private Coroutine miningCoroutine;

    private void Awake()
    {
        target = GetComponent<IWorkBehaviorTarget>();
        if (target == null)
        {
            Debug.LogError($"WorkBehavior requires a component that implements IWorkBehaviorTarget on {gameObject.name}");
        }

        // Find GridData if not assigned
        if (gridData == null)
        {
            var placementSystem = GameObject.Find("PlacementSystem")?.GetComponent<PlacementSystem>();
            if (placementSystem != null)
            {
                gridData = placementSystem.gridData;
            }

            if (gridData == null)
            {
                Debug.LogError($"GridData not found for WorkBehavior on {gameObject.name}");
            }
        }

        // Initialize pathfinder
        if (gridData != null && settings != null && settings.pathfindingSettings != null)
        {
            pathfinder = new GridPathfinder(gridData, settings.pathfindingSettings, gameObject.name);
        }

        // Initialize specialized behaviors
        InitializeSpecializedBehaviors();
    }

    private void InitializeSpecializedBehaviors()
    {
        // Add or get LoggingBehavior component
        loggingBehavior = GetComponent<LoggingBehavior>();
        if (loggingBehavior == null)
        {
            loggingBehavior = gameObject.AddComponent<LoggingBehavior>();
        }
        loggingBehavior.Initialize(settings, pathfinder);

        // Add or get TransportingBehavior component
        transportingBehavior = GetComponent<TransportingBehavior>();
        if (transportingBehavior == null)
        {
            transportingBehavior = gameObject.AddComponent<TransportingBehavior>();
        }
        transportingBehavior.Initialize(settings, gridData);

        // Subscribe to transporting behavior events
        transportingBehavior.OnWorkPositionAvailable += OnWorkPositionBecameAvailable;
    }

    private void OnWorkPositionBecameAvailable()
    {
        SwitchToWorkState();
    }

    private void ResetSpriteFlip()
    {
        if (target?.SpriteRenderer != null)
        {
            target.SpriteRenderer.flipX = false; // Reset to default left-facing
        }
    }

    public void StartWorking(int buildingID, Vector3 buildingPosition, List<Vector3> workPositions)
    {
        Debug.Log($"StartWorking called for {gameObject.name} - Building ID: {buildingID}");

        currentBuildingID = buildingID;
        workPos = workPositions;
        buildingPos = buildingPosition;

        // Update transporting behavior with building data
        transportingBehavior.SetBuildingData(buildingID, buildingPosition, workPositions);

        Debug.Log($"Work positions count: {workPositions?.Count ?? 0}");
        Debug.Log($"Building position: {buildingPosition}");

        // Stop any existing work
        StopAllWork();

        // Determine initial work state based on building ID
        switch (buildingID)
        {
            case 19: // Barracks
                Debug.Log($"Starting Training for {gameObject.name}");
                StartTraining();
                break;
            case 20: // Bath House
                Debug.Log($"Starting Bathing for {gameObject.name}");
                StartBathing();
                break;
            case 22: // Carpenter
            case 27: // Lumber Mill
            case 34: // Tailor
            case 37: // Workshop
                if (HasAvailableWorkPosition())
                {
                    Debug.Log($"Starting Producing for {gameObject.name}");
                    StartProducing();
                }
                else
                {
                    Debug.Log($"No work position available, starting Transporting for {gameObject.name}");
                    StartTransporting();
                }
                break;
            case 23: // Farm
                if (HasAvailableWorkPosition())
                {
                    Debug.Log($"Starting Watering for {gameObject.name}");
                    StartWatering();
                }
                else
                {
                    Debug.Log($"No work position available, starting Transporting for {gameObject.name}");
                    StartTransporting();
                }
                break;
            case 26: // Logger
                Debug.Log($"Starting Logging for {gameObject.name}");
                StartLogging();
                break;
            case 28: // Mine
                if (HasAvailableWorkPosition())
                {
                    Debug.Log($"Starting Mining for {gameObject.name}");
                    StartMining();
                }
                else
                {
                    Debug.Log($"No work position available, starting Transporting for {gameObject.name}");
                    StartTransporting();
                }
                break;
            case 24: // Hospital
                SetWorkStateToNone();
                break;
            default:
                StartTransporting();
                break;
        }
    }

    #region Start/Stop Methods

    public void StartTraining()
    {
        ResetSpriteFlip();
        workState = WorkState.Training;
        Vector3? availablePos = CheckWorkPosAvailability();
        if (availablePos.HasValue)
        {
            target.SpriteRenderer.sortingOrder = 0;
            trainingCoroutine = StartCoroutine(TrainingCoroutine(availablePos.Value));
        }
    }

    public void StopTraining()
    {
        if (trainingCoroutine != null)
        {
            StopCoroutine(trainingCoroutine);
            trainingCoroutine = null;
        }
    }

    public void StartBathing()
    {
        ResetSpriteFlip();
        workState = WorkState.Bathing;
        Vector3? availablePos = CheckWorkPosAvailability();
        if (availablePos.HasValue)
        {
            target.SpriteRenderer.sortingOrder = 0;
            bathingCoroutine = StartCoroutine(BathingCoroutine(availablePos.Value));
        }
    }

    public void StopBathing()
    {
        if (bathingCoroutine != null)
        {
            StopCoroutine(bathingCoroutine);
            bathingCoroutine = null;
        }
    }

    public void StartTransporting()
    {
        ResetSpriteFlip();
        // If coming from stationary work states, disable validation temporarily
        if (workState == WorkState.Producing || workState == WorkState.Mining ||
            workState == WorkState.Watering || workState == WorkState.Training ||
            workState == WorkState.Bathing)
        {
            DisableValidationTemporarily();
        }

        workState = WorkState.Transporting;
        transportingBehavior.StartTransporting();
    }

    private void DisableValidationTemporarily()
    {
        validationDisabledUntil = Time.time + VALIDATION_TRANSITION_DELAY;
    }

    public void StopTransporting()
    {
        transportingBehavior.StopTransporting();
    }

    public void StartProducing()
    {
        ResetSpriteFlip();
        workState = WorkState.Producing;
        Vector3? availablePos = CheckWorkPosAvailability();
        if (availablePos.HasValue)
        {
            target.SpriteRenderer.sortingOrder = 0;
            producingCoroutine = StartCoroutine(ProducingCoroutine(availablePos.Value));
        }
    }

    public void StopProducing()
    {
        if (producingCoroutine != null)
        {
            StopCoroutine(producingCoroutine);
            producingCoroutine = null;
        }
    }

    public void StartWatering()
    {
        ResetSpriteFlip();
        workState = WorkState.Watering;
        Vector3? availablePos = CheckWorkPosAvailability();
        if (availablePos.HasValue)
        {
            target.SpriteRenderer.sortingOrder = 0;
            wateringCoroutine = StartCoroutine(WateringCoroutine(availablePos.Value));
        }
    }

    public void StopWatering()
    {
        ResetSpriteFlip();
        if (wateringCoroutine != null)
        {
            StopCoroutine(wateringCoroutine);
            wateringCoroutine = null;
        }
    }

    public void StartMining()
    {
        ResetSpriteFlip();
        workState = WorkState.Mining;
        
        Vector3? availablePos = CheckWorkPosAvailability();
        if (availablePos.HasValue)
        {
            target.SpriteRenderer.sortingOrder = 0;
            miningCoroutine = StartCoroutine(MiningCoroutine(availablePos.Value));
        }
    }

    public void StopMining()
    {
        if (miningCoroutine != null)
        {
            StopCoroutine(miningCoroutine);
            miningCoroutine = null;
        }
    }

    public void StartLogging()
    {
        ResetSpriteFlip();
        workState = WorkState.Logging;
        loggingBehavior.StartLogging();
    }

    public void StopLogging()
    {
        loggingBehavior.StopLogging();
    }

    #endregion

    #region Remaining Coroutines

    private IEnumerator TrainingCoroutine(Vector3 workPosition)
    {
        Vector3 targetPos = buildingPos + workPosition;

        // Teleport to work position
        target.Transform.position = targetPos;

        // Start training animation
        target.Animator.Play(settings.attackAnimationName);

        // Training continues indefinitely until stopped
        while (workState == WorkState.Training)
        {
            yield return new WaitForSeconds(settings.workInterval);
        }
    }

    private IEnumerator BathingCoroutine(Vector3 workPosition)
    {
        Vector3 targetPos = buildingPos + workPosition;

        // Teleport to work position
        target.Transform.position = targetPos;

        // Start bathing animation
        target.Animator.Play(settings.bathingAnimationName);

        // Bathing continues indefinitely until stopped
        while (workState == WorkState.Bathing)
        {
            yield return new WaitForSeconds(settings.workInterval);
        }
    }

    private IEnumerator ProducingCoroutine(Vector3 workPosition)
    {
        Vector3 targetPos = buildingPos + workPosition;

        // Teleport to work position
        target.Transform.position = targetPos;

        // Start work animation
        target.Animator.Play(settings.workAnimationName);

        // Work for specified duration
        yield return new WaitForSeconds(settings.workDuration);

        // Return to transporting
        StartTransporting();
    }

    private IEnumerator WateringCoroutine(Vector3 workPosition)
    {
        Vector3 targetPos = buildingPos + workPosition;

        // Teleport to work position
        target.Transform.position = targetPos;

        // Start watering animation
        target.Animator.Play(settings.waterAnimationName);

        // Work for specified duration
        yield return new WaitForSeconds(settings.workDuration);

        // Return to transporting
        StartTransporting();
    }

    private IEnumerator MiningCoroutine(Vector3 workPosition)
    {
        Vector3 targetPos = buildingPos + workPosition;

        // Teleport to work position
        target.Transform.position = targetPos;

        // Start mining animation
        target.Animator.Play(settings.mineAnimationName);

        // Work for specified duration
        yield return new WaitForSeconds(settings.workDuration);

        // Return to transporting
        StartTransporting();
    }

    #endregion

    #region Helper Methods

    private Vector3? CheckWorkPosAvailability()
    {
        if (workPos == null || workPos.Count == 0) return null;

        bool loopForward = UnityEngine.Random.Range(0, 2) == 0;

        if (loopForward)
        {
            for (int i = 0; i < workPos.Count; i++)
            {
                Vector3 checkPosition = buildingPos + workPos[i];
                Collider2D hit = Physics2D.OverlapPoint(checkPosition);

                if (hit == null || hit.GetComponent<Villager>() == null)
                {
                    return workPos[i];
                }
            }
        }
        else
        {
            for (int i = workPos.Count - 1; i >= 0; i--)
            {
                Vector3 checkPosition = buildingPos + workPos[i];
                Collider2D hit = Physics2D.OverlapPoint(checkPosition);

                if (hit == null || hit.GetComponent<Villager>() == null)
                {
                    return workPos[i];
                }
            }
        }

        return null;
    }

    private bool HasAvailableWorkPosition()
    {
        return CheckWorkPosAvailability().HasValue;
    }

    private void SwitchToWorkState()
    {
        StopTransporting();

        switch (currentBuildingID)
        {
            case 22: // Carpenter
            case 27: // Lumber Mill
            case 34: // Tailor
            case 37: // Workshop
                StartProducing();
                break;
            case 23: // Farm
                StartWatering();
                break;
            case 28: // Mine
                StartMining();
                break;
        }
    }

    private void StopAllWork()
    {
        StopTraining();
        StopBathing();
        StopTransporting();
        StopProducing();
        StopWatering();
        StopMining();
        StopLogging();
    }

    #endregion

    #region Public Methods

    public void SetBuildingPos(Vector3 newBuildingPos)
    {
        buildingPos = newBuildingPos;

        // Update transporting behavior with new building position
        if (transportingBehavior != null)
        {
            transportingBehavior.SetBuildingData(currentBuildingID, newBuildingPos, workPos);
        }
    }

    public WorkState GetCurrentWorkState()
    {
        return workState;
    }

    public void SetWorkStateToNone()
    {
        StopAllWork();
        workState = WorkState.None;

        if (target?.Transform != null)
        {
            target.Transform.position = Vector3.zero;
        }

        if (target?.Animator != null)
        {
            target.Animator.Play("Idle");
        }
    }

    public void StopWorking()
    {
        StopAllWork();
        SetWorkStateToNone();
    }


    #endregion

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (transportingBehavior != null)
        {
            transportingBehavior.OnWorkPositionAvailable -= OnWorkPositionBecameAvailable;
        }

        StopAllWork();
    }
}