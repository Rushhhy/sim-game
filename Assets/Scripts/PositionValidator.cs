using System.Collections;
using UnityEngine;

[System.Serializable]
public class PositionValidatorSettings
{
    [Header("Validation Settings")]
    public float validationCheckInterval = 1f;
    public bool enableAutoRepositioning = true;

    [Header("Repositioning")]
    public float maxRepositionDistance = 5f; // Maximum distance to search for valid position

    [Header("Debug")]
    public bool enableDebugLogs = true;

    [Header("Pathfinding")]
    public PathfindingSettings pathfindingSettings;
}

public interface IPositionValidationTarget
{
    Transform Transform { get; }
    bool IsValidationEnabled { get; } // Allow targets to temporarily disable validation
}

public class PositionValidator : MonoBehaviour
{
    [SerializeField] private PositionValidatorSettings settings;
    [SerializeField] private GridData gridData;

    private IPositionValidationTarget target;
    private GridPathfinder pathfinder;
    private Coroutine validationCoroutine;
    private bool isValidating = false;

    // References to check villager state
    private Villager villager;
    private WorkBehavior workBehavior;

    private void Awake()
    {
        target = GetComponent<IPositionValidationTarget>();
        if (target == null)
        {
            if (settings.enableDebugLogs)
                Debug.LogError($"PositionValidator requires a component that implements IPositionValidationTarget on {gameObject.name}");
            return;
        }

        // Get villager and work behavior references
        villager = GetComponent<Villager>();
        workBehavior = GetComponent<WorkBehavior>();

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
                if (settings.enableDebugLogs)
                    Debug.LogError($"GridData not found for PositionValidator on {gameObject.name}");
                return;
            }
        }

        // Initialize pathfinder
        if (gridData != null)
        {
            pathfinder = new GridPathfinder(gridData, settings.pathfindingSettings, gameObject.name);
        }
    }

    private void Start()
    {
        if (settings.enableAutoRepositioning && target != null && pathfinder != null)
        {
            StartValidation();
        }
    }

    public void StartValidation()
    {
        if (isValidating || target == null || pathfinder == null) return;

        isValidating = true;

        if (validationCoroutine != null)
        {
            StopCoroutine(validationCoroutine);
        }

        validationCoroutine = StartCoroutine(ValidationCoroutine());
    }

    public void StopValidation()
    {
        if (!isValidating) return;

        isValidating = false;

        if (validationCoroutine != null)
        {
            StopCoroutine(validationCoroutine);
            validationCoroutine = null;
        }
    }

    /// <summary>
    /// Checks if position validation should be active based on villager state
    /// Only validates during Idle state or Transporting work state
    /// </summary>
    private bool ShouldValidatePosition()
    {
        if (!target.IsValidationEnabled)
            return false;

        // If no villager component, use the general validation enabled flag
        if (villager == null)
            return target.IsValidationEnabled;

        // Only validate during specific states
        switch (villager.currentState)
        {
            case VillagerState.Idle:
                return true;

            case VillagerState.Working:
                // During working state, only validate if transporting
                return IsVillagerTransporting();

            case VillagerState.Base:
            default:
                return false;
        }
    }

    /// <summary>
    /// Checks if the villager is currently in transporting work state
    /// </summary>
    private bool IsVillagerTransporting()
    {
        if (workBehavior == null) return false;

        // We need to access the current work state from WorkBehavior
        // Since workState is private, we'll use reflection or add a public property
        // For now, we'll add a public property to WorkBehavior
        return workBehavior.GetCurrentWorkState() == WorkState.Transporting;
    }

    private IEnumerator ValidationCoroutine()
    {
        while (isValidating)
        {
            yield return new WaitForSeconds(settings.validationCheckInterval);

            if (!isValidating || !ShouldValidatePosition())
                continue;

            Vector3 currentPosition = target.Transform.position;

            if (!pathfinder.IsPositionWalkable(currentPosition))
            {
                Vector3? newPosition = FindValidRepositionPosition(currentPosition);

                if (newPosition.HasValue)
                {
                    Vector3 oldPos = currentPosition;
                    target.Transform.position = newPosition.Value;
                }
            }
        }
    }

    private Vector3? FindValidRepositionPosition(Vector3 fromPosition)
    {
        // First try to find nearest walkable position
        Vector3? nearestPos = pathfinder.FindNearestWalkablePosition(fromPosition);

        if (nearestPos.HasValue)
        {
            float distance = Vector3.Distance(fromPosition, nearestPos.Value);
            if (distance <= settings.maxRepositionDistance)
            {
                return nearestPos.Value;
            }
        }

        // If nearest position is too far, try to find a random walkable position within range
        return pathfinder.FindRandomWalkablePosition(fromPosition, settings.maxRepositionDistance);
    }

    // Public method to force immediate validation and repositioning
    public bool ValidateAndRepositionImmediate()
    {
        if (target == null || pathfinder == null || !ShouldValidatePosition())
            return false;

        Vector3 currentPosition = target.Transform.position;

        if (!pathfinder.IsPositionWalkable(currentPosition))
        {
            Vector3? newPosition = FindValidRepositionPosition(currentPosition);

            if (newPosition.HasValue)
            {
                Vector3 oldPos = currentPosition;
                target.Transform.position = newPosition.Value;

                return true;
            }
            else
            {
                return false;
            }
        }

        return true; // Position was already valid
    }

    // Public method to check if current position is valid without repositioning
    public bool IsCurrentPositionValid()
    {
        if (target == null || pathfinder == null)
            return false;

        return pathfinder.IsPositionWalkable(target.Transform.position);
    }

    // Public method for external systems to request validation
    public void RequestValidation()
    {
        if (isValidating)
        {
            // Trigger immediate check
            StartCoroutine(DelayedValidationCheck());
        }
    }

    private IEnumerator DelayedValidationCheck()
    {
        yield return null; // Wait one frame for grid updates

        if (target != null && ShouldValidatePosition())
        {
            ValidateAndRepositionImmediate();
        }
    }

    private void OnDestroy()
    {
        StopValidation();
    }

    // Debug visualization in scene view
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;

        Vector3 pos = target.Transform.position;

        // Draw validation range circle
        Gizmos.color = Color.yellow;
        DrawWireCircle(pos, settings.maxRepositionDistance);

        // Draw current position validation status
        if (pathfinder != null)
        {
            bool shouldValidate = ShouldValidatePosition();
            bool isPositionValid = pathfinder.IsPositionWalkable(pos);

            if (!shouldValidate)
            {
                Gizmos.color = Color.gray; // Validation disabled
            }
            else
            {
                Gizmos.color = isPositionValid ? Color.green : Color.red;
            }

            Gizmos.DrawWireCube(pos, Vector3.one * 0.5f);

            // Draw state info
            if (villager != null)
            {
                Vector3 textPos = pos + Vector3.up * 1f;
                // Note: Gizmos can't draw text, but this shows the concept
            }
        }
    }

    // Helper method to draw a wire circle using Gizmos
    private void DrawWireCircle(Vector3 center, float radius, int segments = 32)
    {
        if (segments < 3) segments = 3;

        float angleStep = 360f / segments;
        Vector3 prevPoint = center + Vector3.right * radius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}