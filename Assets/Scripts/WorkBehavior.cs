using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum WorkState
{
    None,
    Bathing,
    Transporting,
    Training,
    Logging,
    Producing,
    Watering
}

[System.Serializable]
public class WorkBehaviorSettings
{
    [Header("Movement Settings")]
    public float movementSpeed = 1f;

    [Header("Work Settings")]
    public float workInterval = 10f;

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

    [Header("Market Positions")] // marketPositionOne will be available from the start, the rest will be made available as the markets are upgraded
    public Vector3 marketPositionOne = new Vector3(-2.73f, 29f, 0f);
    public Vector3 marketPositionTwo = new Vector3(-1.355f, 29f, 0f);
    public Vector3 marketPositionThree = new Vector3(4.2f, 29f, 0f);
    public Vector3 marketPositionFour = new Vector3(5.65f, 29f, 0f);

    [Header("Sprite Flipping")]
    public bool enableSpriteFlipping = true;
    public bool isLeftFacingDefault = true;

    [Header("Repositioning")]
    public float repositionCheckInterval = 1f;

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

    // add work positions dictionary here as parameter?
    public void StartWorking(int buildingID, Vector3 buildingPosition, List<Vector3> workPositions)
    {
        workPos = workPositions;
        switch (buildingID)
        {
            // Barracks
            case 19:
                workState = WorkState.Training;
                break;
            // Bath House
            case 20:
                workState = WorkState.Bathing;
                break;
            // Carpenter
            case 22:
                break;
            // Farm
            case 23:
                break;
            // Hospital
            case 24:
                workState = WorkState.None;
                break;
            // Logger
            case 26:
                workState = WorkState.Logging;
                break;
            // Lumber Mill
            case 27:
                break;
            // Mine 
            case 28:
                workState = WorkState.Logging;
                break;
            // Tailor
            case 34:
                break;
            // Workshop
            case 37:
                break;

            default:
                workState = WorkState.Transporting;
                break;
        }
    }
}
