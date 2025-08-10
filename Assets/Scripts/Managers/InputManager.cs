using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Camera sceneCamera;

    public event Action OnMouseUp, OnMouseClick, OnMouseTapped, OnMouseHold;
    public event Action<float> OnMouseScroll;
    private float mouseDownTime;
    private Vector3 initialClickPosition;
    private const float positionTolerance = 0.1f;

    private void Update()
    {
        // Check if the mouse button is pressed down (start timing)
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            mouseDownTime = Time.time;
            initialClickPosition = GetSelectedMapPosition();
            OnMouseClick?.Invoke();
        }

        // Check for mouse scroll
        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            OnMouseScroll?.Invoke(Input.GetAxis("Mouse ScrollWheel"));
        }

        // Check if the mouse button is still held down
        if (Input.GetMouseButton(0))
        {
            float elapsedTime = Time.time - mouseDownTime;
            Vector3 currentMousePosition = GetSelectedMapPosition();

            // Check if the mouse is held for more than 2 seconds and position hasn't changed significantly
            if (elapsedTime > 1f && PositionsAreClose(initialClickPosition, currentMousePosition))
            {
                OnMouseHold?.Invoke();
            }
        }

        // Check for mouse up
        if (Input.GetMouseButtonUp(0))
        {
            OnMouseUp?.Invoke();
            float elapsedTime = Time.time - mouseDownTime;
            Vector3 mouseUpPosition = GetSelectedMapPosition();

            if (elapsedTime < 0.3f && PositionsAreClose(initialClickPosition, mouseUpPosition))
            {
                OnMouseTapped?.Invoke();
            }
        }
    }

    // Checks if clicked on UI element
    public bool IsPointerOverUI()
        => EventSystem.current.IsPointerOverGameObject();

    public Vector3 GetSelectedMapPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 0f;

        // Convert mouse position to world position using the camera
        Vector3 worldPos = sceneCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f;

        return worldPos;

    }

    public Vector3 GetMousePosition()
    {
        return Input.mousePosition;
    }

    private bool PositionsAreClose(Vector3 pos1, Vector3 pos2)
    {
        return Vector3.Distance(pos1, pos2) <= positionTolerance;
    }
}
