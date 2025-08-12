using System.Collections;
using UnityEngine;

[System.Serializable]
public class IdleBehaviorSettings
{
    [Header("Movement Settings")]
    public float movementRadius = 2f;
    public float movementSpeed = 1f;

    [Header("Timing Settings")]
    public float waitTimeMin = 2f;
    public float waitTimeMax = 5f;
    public float moveTimeMin = 3f;
    public float moveTimeMax = 5f;

    [Header("Animation Settings")]
    public string idleAnimationName = "Idle";
    public string walkAnimationName = "Walk";

    [Header("Sprite Flipping")]
    public bool enableSpriteFlipping = true;
    public bool isLeftFacingDefault = true;

    [Header("Pathfinding")]
    public PathfindingSettings pathfindingSettings;
}

public interface IIdleBehaviorTarget
{
    Transform Transform { get; }
    Animator Animator { get; }
    SpriteRenderer SpriteRenderer { get; }
    bool ShouldContinueIdling { get; }
}

public class IdleBehavior : MonoBehaviour
{
    [SerializeField] private IdleBehaviorSettings settings;
    [SerializeField] private GridData gridData; // Reference to your grid system

    private IIdleBehaviorTarget target;
    private GridPathfinder pathfinder;
    private Vector3 idleStartPosition;
    private Vector3 idleTargetPosition;
    private Coroutine idleCoroutine;
    private bool isIdling = false;

    public bool IsIdling => isIdling;
    public Vector3 IdleStartPosition
    {
        get => idleStartPosition;
        set => idleStartPosition = value;
    }

    private void Awake()
    {
        target = GetComponent<IIdleBehaviorTarget>();
        if (target == null)
        {
            Debug.LogError($"IdleBehavior requires a component that implements IIdleBehaviorTarget on {gameObject.name}");
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
                Debug.LogError($"GridData not found for IdleBehavior on {gameObject.name}");
            }
        }

        // Initialize pathfinder
        if (gridData != null && settings?.pathfindingSettings != null)
        {
            pathfinder = new GridPathfinder(gridData, settings.pathfindingSettings, gameObject.name);
        }
    }

    public void StartIdling()
    {
        if (isIdling || target == null || pathfinder == null) return;

        // Validate starting position and reposition if necessary
        if (!pathfinder.IsPositionWalkable(idleStartPosition))
        {
            Vector3? newPos = pathfinder.FindNearestWalkablePosition(idleStartPosition);
            if (newPos.HasValue)
            {
                idleStartPosition = newPos.Value;
            }
            else
            {
                Debug.LogWarning($"Cannot start idling for {gameObject.name} - no walkable position found");
                return;
            }
        }

        isIdling = true;
        target.Transform.position = idleStartPosition;

        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
        }

        idleCoroutine = StartCoroutine(IdleBehaviorCoroutine());
    }

    public void StopIdling()
    {
        if (!isIdling) return;

        isIdling = false;

        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
            idleCoroutine = null;
        }

        // Play idle animation when stopping
        if (target?.Animator != null)
        {
            target.Animator.Play(settings.idleAnimationName);
        }
    }

    public void SetIdleStartPosition(Vector3 position)
    {
        idleStartPosition = position;
    }

    private IEnumerator IdleBehaviorCoroutine()
    {
        // Start with idle animation
        if (target.Animator != null)
        {
            target.Animator.Play(settings.idleAnimationName);
        }

        while (isIdling && target.ShouldContinueIdling)
        {
            // Wait for random time
            float waitTime = Random.Range(settings.waitTimeMin, settings.waitTimeMax);
            yield return new WaitForSeconds(waitTime);

            if (!isIdling || !target.ShouldContinueIdling) break;

            // Try to find a valid target position
            Vector3? targetPosition = pathfinder.FindRandomWalkablePosition(idleStartPosition, settings.movementRadius);
            if (targetPosition.HasValue)
            {
                idleTargetPosition = targetPosition.Value;

                // Move to target using improved pathfinding
                yield return StartCoroutine(MoveToPositionWithAvoidance(idleTargetPosition));

                if (!isIdling || !target.ShouldContinueIdling) break;
            }
            else
            {
                // If no valid position found, wait a bit longer before trying again
                yield return new WaitForSeconds(1f);
            }
        }

        isIdling = false;
    }

    private IEnumerator MoveToPositionWithAvoidance(Vector3 targetPos)
    {
        // Play walk animation
        if (target.Animator != null)
        {
            target.Animator.Play(settings.walkAnimationName);
        }

        float maxMoveTime = Random.Range(settings.moveTimeMin, settings.moveTimeMax);
        float elapsed = 0f;

        while (elapsed < maxMoveTime && isIdling && target.ShouldContinueIdling)
        {
            Vector3 currentPos = target.Transform.position;

            // Check if we're close enough to the target
            if (Vector3.Distance(currentPos, targetPos) < 0.1f)
            {
                target.Transform.position = targetPos;
                break;
            }

            // Get avoidance direction from pathfinder
            Vector3 moveDirection = pathfinder.GetAvoidanceDirection(currentPos, targetPos);

            if (moveDirection == Vector3.zero)
            {
                // No valid direction found, try repositioning
                RepositionToValidPosition();
                yield return new WaitForSeconds(0.1f); // Small delay before trying again
                elapsed += 0.1f;
                continue;
            }

            // Move in the calculated direction
            Vector3 newPosition = currentPos + moveDirection * settings.movementSpeed * Time.deltaTime;
            target.Transform.position = newPosition;

            // Handle sprite flipping
            if (settings.enableSpriteFlipping)
            {
                HandleSpriteFlipping(moveDirection);
            }

            // Update sorting order
            if (target.SpriteRenderer != null)
            {
                target.SpriteRenderer.sortingOrder = (int)(-target.Transform.position.y * 10);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Return to idle animation
        if (target.Animator != null)
        {
            target.Animator.Play(settings.idleAnimationName);
        }
    }

    /// <summary>
    /// Repositions the villager to a valid walkable position
    /// </summary>
    private void RepositionToValidPosition()
    {
        // Try to use the PositionValidator component if available
        PositionValidator positionValidator = GetComponent<PositionValidator>();
        if (positionValidator != null)
        {
            positionValidator.ValidateAndRepositionImmediate();
        }
        else
        {
            // Fallback: use pathfinder directly if PositionValidator is not available
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

    private void HandleSpriteFlipping(Vector3 direction)
    {
        if (Mathf.Abs(direction.x) < 0.1f) return; // No significant horizontal movement

        bool shouldFlip = settings.isLeftFacingDefault ? direction.x > 0 : direction.x < 0;

        float scaleX = shouldFlip ? -Mathf.Abs(target.Transform.localScale.x) : Mathf.Abs(target.Transform.localScale.x);
        target.Transform.localScale = new Vector3(scaleX, target.Transform.localScale.y, target.Transform.localScale.z);
    }
}