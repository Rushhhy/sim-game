using System.Collections;
using UnityEngine;

public class LoggingBehavior : MonoBehaviour
{
    [SerializeField] private WorkBehaviorSettings settings;

    private IWorkBehaviorTarget target;
    private GridPathfinder pathfinder;
    private Coroutine loggingCoroutine;

    public bool IsLogging => loggingCoroutine != null;

    private void Awake()
    {
        target = GetComponent<IWorkBehaviorTarget>();
        if (target == null)
        {
            Debug.LogError($"LoggingBehavior requires a component that implements IWorkBehaviorTarget on {gameObject.name}");
        }
    }

    public void Initialize(WorkBehaviorSettings behaviorSettings, GridPathfinder gridPathfinder)
    {
        settings = behaviorSettings;
        pathfinder = gridPathfinder;
    }

    public void StartLogging()
    {
        if (loggingCoroutine != null)
        {
            StopLogging();
        }

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

    private IEnumerator LoggingCoroutine()
    {
        while (true)
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

            // Continue logging until tree is destroyed
            while (closestTree != null)
            {
                yield return new WaitForSeconds(settings.workInterval);

                // Check if tree still exists
                if (closestTree == null)
                {
                    break;
                }
            }

            // If we exit the inner loop, find next tree
            // The outer while(true) will handle finding the next tree
        }
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition, string animationName)
    {
        target.Animator.Play(animationName);

        // If no pathfinder, use simple movement
        if (pathfinder == null)
        {
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
            yield break;
        }

/*        // Use pathfinding
        while (Vector3.Distance(target.Transform.position, targetPosition) > 0.1f)
        {
            Vector3 currentPos = target.Transform.position;
            //Vector3 moveDirection = pathfinder.GetAvoidanceDirection(currentPos, targetPosition);

            //if (moveDirection == Vector3.zero)
            {
                RepositionToValidPosition();
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            //Vector3 newPosition = currentPos + moveDirection * settings.movementSpeed * Time.deltaTime;

            if (pathfinder.IsPositionWalkable(newPosition))
            {
                target.Transform.position = newPosition;
            }
            else
            {
                RepositionToValidPosition();
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            if (settings.enableSpriteFlipping)
            {
                HandleSpriteFlipping(targetPosition);
            }

            yield return null;
        }*/
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
            //positionValidator.ValidateAndRepositionImmediate();
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
        StopLogging();
    }
}