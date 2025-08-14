using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransportingBehavior : MonoBehaviour
{
    [SerializeField] private WorkBehaviorSettings settings;

    private IWorkBehaviorTarget target;
    private GridPathfinder pathfinder;
    private Coroutine transportingCoroutine;

    // Dependencies
    private Vector3 buildingPos;
    private List<Vector3> workPos;
    private int currentBuildingID;

    public bool IsTransporting => transportingCoroutine != null;

    // Events
    public System.Action OnWorkPositionAvailable;

    private void Awake()
    {
        target = GetComponent<IWorkBehaviorTarget>();
        if (target == null)
        {
            Debug.LogError($"TransportingBehavior requires a component that implements IWorkBehaviorTarget on {gameObject.name}");
        }
    }

    public void Initialize(WorkBehaviorSettings behaviorSettings, GridPathfinder gridPathfinder)
    {
        settings = behaviorSettings;
        pathfinder = gridPathfinder;
    }

    public void SetBuildingData(int buildingID, Vector3 buildingPosition, List<Vector3> workPositions)
    {
        currentBuildingID = buildingID;
        buildingPos = buildingPosition;
        workPos = workPositions;
    }

    public void StartTransporting()
    {
        Debug.Log($"StartTransporting called for {gameObject.name}");

        if (transportingCoroutine != null)
        {
            StopTransporting();
        }

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

    private IEnumerator TransportingCoroutine()
    {
        Debug.Log($"=== TransportingCoroutine ENTRY for {gameObject.name} ===");

        yield return null; // Simple yield first
        Debug.Log($"TransportingCoroutine first yield complete");

        // Reposition villager to valid position
        Debug.Log($"Repositioning villager for {gameObject.name}");
        RepositionToValidPosition();

        while (IsTransporting)
        {
            Debug.Log($"Transport loop iteration for {gameObject.name}");

            // Move to random market position with box animation
            Vector3 marketPos = GetRandomAvailableMarketPosition();
            Debug.Log($"Moving to market position: {marketPos} for {gameObject.name}");

            yield return StartCoroutine(MoveToPosition(marketPos, settings.boxWalkAnimationName));
            Debug.Log($"Market movement complete for {gameObject.name}");

            if (!IsTransporting) yield break;

            // Switch to walk animation and return to building
            Debug.Log($"Returning to building position: {buildingPos} for {gameObject.name}");
            yield return StartCoroutine(MoveToPosition(buildingPos, settings.walkAnimationName));
            Debug.Log($"Building return complete for {gameObject.name}");

            if (!IsTransporting) yield break;

            // Check if work position is now available
            if (HasAvailableWorkPosition())
            {
                Debug.Log($"Work position available, notifying for {gameObject.name}");
                OnWorkPositionAvailable?.Invoke();
                yield break;
            }

            Debug.Log($"No work position available, continuing transport for {gameObject.name}");
            yield return new WaitForSeconds(1f);
        }

        Debug.Log($"=== TransportingCoroutine END for {gameObject.name} ===");
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

    private bool HasAvailableWorkPosition()
    {
        return CheckWorkPosAvailability().HasValue;
    }

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

    private void OnDestroy()
    {
        StopTransporting();
    }
}