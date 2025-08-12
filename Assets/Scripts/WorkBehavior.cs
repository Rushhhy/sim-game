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
    Mine
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

    [SerializeField] private GridData gridData;
    [SerializeField] private WorkBehaviorSettings settings;

    private IWorkBehaviorTarget target;
    private GridPathfinder pathfinder;

    // Coroutines
    private Coroutine producingCoroutine;
    private Coroutine loggingCoroutine;
    private Coroutine bathingCoroutine;
    private Coroutine trainingCoroutine;
    private Coroutine wateringCoroutine;
    private Coroutine transportingCoroutine;
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
    }

    public void StartWorking(int buildingID, Vector3 buildingPosition, List<Vector3> workPositions)
    {
        Debug.Log($"StartWorking called for {gameObject.name} - Building ID: {buildingID}");

        currentBuildingID = buildingID;
        workPos = workPositions;
        buildingPos = buildingPosition;

        Debug.Log($"Work positions count: {workPositions?.Count ?? 0}");
        Debug.Log($"Building position: {buildingPosition}");

        // Stop any existing coroutines
        StopAllWorkCoroutines();

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
            default:
                Debug.Log($"Setting work state to None for {gameObject.name}");
                SetWorkStateToNone();
                break;
        }
    }

    #region Start/Stop Methods

    public void StartTraining()
    {
        workState = WorkState.Training;
        Vector3? availablePos = CheckWorkPosAvailability();
        if (availablePos.HasValue)
        {
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
        workState = WorkState.Bathing;
        Vector3? availablePos = CheckWorkPosAvailability();
        if (availablePos.HasValue)
        {
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
        Debug.Log($"StartTransporting called for {gameObject.name}");
        workState = WorkState.Transporting;
        Debug.Log($"Work state set to Transporting for {gameObject.name}");
        transportingCoroutine = StartCoroutine(TransportingCoroutine());
        Debug.Log($"TransportingCoroutine started for {gameObject.name}");
    }

    public void StopTransporting()
    {
        if (transportingCoroutine != null)
        {
            StopCoroutine(transportingCoroutine);
            transportingCoroutine = null;
        }
    }

    public void StartProducing()
    {
        workState = WorkState.Producing;
        Vector3? availablePos = CheckWorkPosAvailability();
        if (availablePos.HasValue)
        {
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
        workState = WorkState.Watering;
        Vector3? availablePos = CheckWorkPosAvailability();
        if (availablePos.HasValue)
        {
            wateringCoroutine = StartCoroutine(WateringCoroutine(availablePos.Value));
        }
    }

    public void StopWatering()
    {
        if (wateringCoroutine != null)
        {
            StopCoroutine(wateringCoroutine);
            wateringCoroutine = null;
        }
    }

    public void StartMining()
    {
        workState = WorkState.Mine;
        Vector3? availablePos = CheckWorkPosAvailability();
        if (availablePos.HasValue)
        {
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
        workState = WorkState.Logging;
        loggingCoroutine = StartCoroutine(LoggingCoroutine());
    }

    public void StopLogging()
    {
        if (loggingCoroutine != null)
        {
            StopCoroutine(loggingCoroutine);
            loggingCoroutine = null;
        }
    }

    #endregion

    #region Coroutines

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

    private IEnumerator TransportingCoroutine()
    {
        Debug.Log($"=== TransportingCoroutine ENTRY for {gameObject.name} ===");

        yield return null; // Simple yield first
        Debug.Log($"TransportingCoroutine first yield complete");

        Debug.Log($"Current workState: {workState}");
        Debug.Log($"WorkState.Transporting value: {(int)WorkState.Transporting}");
        Debug.Log($"Current workState value: {(int)workState}");
        Debug.Log($"Are they equal: {workState == WorkState.Transporting}");

        if (workState != WorkState.Transporting)
        {
            Debug.LogError($"WorkState is NOT Transporting! It's {workState}");
            yield break;
        }

        int loopCount = 0;
        while (workState == WorkState.Transporting && loopCount < 3) // Limit loops for testing
        {
            loopCount++;
            Debug.Log($"Transport loop iteration {loopCount} for {gameObject.name}");

            // Reposition villager to valid position (only on first iteration)
            if (loopCount == 1)
            {
                Debug.Log($"Repositioning villager for {gameObject.name}");
                RepositionToValidPosition();
            }

            // Move to random market position with box animation
            Vector3 marketPos = GetRandomAvailableMarketPosition();
            Debug.Log($"Moving to market position: {marketPos} for {gameObject.name}");

            yield return StartCoroutine(MoveToPosition(marketPos, settings.boxWalkAnimationName));
            Debug.Log($"Market movement complete for {gameObject.name}");

            // Switch to walk animation and return to building
            Debug.Log($"Returning to building position: {buildingPos} for {gameObject.name}");
            yield return StartCoroutine(MoveToPosition(buildingPos, settings.walkAnimationName));
            Debug.Log($"Building return complete for {gameObject.name}");

            // Check if work position is now available
            if (HasAvailableWorkPosition())
            {
                Debug.Log($"Work position available, switching work state for {gameObject.name}");
                SwitchToWorkState();
                yield break;
            }

            Debug.Log($"No work position available, continuing transport for {gameObject.name}");
            yield return new WaitForSeconds(1f);

            break; // Exit after first iteration for testing
        }

        Debug.Log($"=== TransportingCoroutine END for {gameObject.name} ===");
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

    private IEnumerator LoggingCoroutine()
    {
        // Find closest tree
        GameObject closestTree = FindClosestTree();
        if (closestTree == null)
        {
            Debug.LogWarning($"No trees found for logging on {gameObject.name}");
            yield break;
        }

        // Move to tree with axe walk animation
        yield return StartCoroutine(MoveToPosition(closestTree.transform.position, settings.axeWalkAnimationName));

        // Start axe animation
        target.Animator.Play(settings.axeAnimationName);

        // Continue logging until tree is destroyed or state changes
        while (workState == WorkState.Logging && closestTree != null)
        {
            yield return new WaitForSeconds(settings.workInterval);
        }

        // Find next tree if current one is gone
        if (workState == WorkState.Logging)
        {
            StartLogging(); // Restart to find new tree
        }
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition, string animationName)
    {
        Debug.Log($"=== MoveToPosition START for {gameObject.name} ===");
        Debug.Log($"Target: {targetPosition}, Pathfinder null: {pathfinder == null}");

        target.Animator.Play(animationName);

        // If no pathfinder, use simple movement
        if (pathfinder == null)
        {
            Debug.Log($"No pathfinder, using simple movement for {gameObject.name}");
            while (Vector3.Distance(target.Transform.position, targetPosition) > 0.1f)
            {
                target.Transform.position = Vector3.MoveTowards(
                    target.Transform.position,
                    targetPosition,
                    settings.movementSpeed * Time.deltaTime
                );

                if (settings.enableSpriteFlipping)
                {
                    HandleSpriteFlipping(targetPosition);
                }

                yield return null;
            }
            Debug.Log($"Simple movement complete for {gameObject.name}");
            yield break;
        }

        // Use pathfinding
        Debug.Log($"Using pathfinding for {gameObject.name}");
        while (Vector3.Distance(target.Transform.position, targetPosition) > 0.1f)
        {
            Vector3 currentPos = target.Transform.position;

            Vector3 moveDirection = pathfinder.GetAvoidanceDirection(currentPos, targetPosition);

            if (moveDirection == Vector3.zero)
            {
                Debug.Log($"No valid direction found for {gameObject.name}, repositioning");
                RepositionToValidPosition();
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            Vector3 newPosition = currentPos + moveDirection * settings.movementSpeed * Time.deltaTime;

            if (pathfinder.IsPositionWalkable(newPosition))
            {
                target.Transform.position = newPosition;
            }
            else
            {
                Debug.Log($"Calculated position not walkable for {gameObject.name}, repositioning");
                RepositionToValidPosition();
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            if (settings.enableSpriteFlipping)
            {
                HandleSpriteFlipping(targetPosition);
            }

            yield return null;
        }

        Debug.Log($"=== MoveToPosition END for {gameObject.name} ===");
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

    private Vector3 GetRandomAvailableMarketPosition()
    {
        List<Vector3> availableMarkets = new List<Vector3>
        {
            settings.marketPositionOne,
            settings.marketPositionTwo,
            settings.marketPositionThree,
            settings.marketPositionFour
        };

        return availableMarkets[Random.Range(0, availableMarkets.Count)];
    }

    private GameObject FindClosestTree()
    {
        GameObject[] trees = GameObject.FindGameObjectsWithTag("Tree");
        if (trees.Length == 0) return null;

        GameObject closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject tree in trees)
        {
            float distance = Vector3.Distance(target.Transform.position, tree.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = tree;
            }
        }

        return closest;
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

    private void HandleSpriteFlipping(Vector3 targetPosition)
    {
        if (target.SpriteRenderer == null) return;

        float direction = targetPosition.x - target.Transform.position.x;

        if (settings.isLeftFacingDefault)
        {
            target.SpriteRenderer.flipX = direction > 0;
        }
        else
        {
            target.SpriteRenderer.flipX = direction < 0;
        }
    }

    private void StopAllWorkCoroutines()
    {
        StopTraining();
        StopBathing();
        StopTransporting();
        StopProducing();
        StopWatering();
        StopMining();
        StopLogging();
    }

    private void RepositionToValidPosition()
    {
        PositionValidator positionValidator = GetComponent<PositionValidator>();
        if (positionValidator != null)
        {
            positionValidator.ValidateAndRepositionImmediate();
        }
        else
        {
            if (pathfinder != null)
            {
                Vector3 currentPos = target.Transform.position;
                if (!pathfinder.IsPositionWalkable(currentPos))
                {
                    Vector3? validPos = pathfinder.FindNearestWalkablePosition(currentPos);
                    if (validPos.HasValue)
                    {
                        target.Transform.position = validPos.Value;
                    }
                }
            }
        }
    }

    #endregion

    #region Public Methods

    public void SetBuildingPos(Vector3 newBuildingPos)
    {
        buildingPos = newBuildingPos;
    }

    public WorkState GetCurrentWorkState()
    {
        return workState;
    }

    public void SetWorkStateToNone()
    {
        StopAllWorkCoroutines();
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
        StopAllWorkCoroutines();
        SetWorkStateToNone();
    }

    #endregion

    private void OnDestroy()
    {
        StopAllWorkCoroutines();
    }
}