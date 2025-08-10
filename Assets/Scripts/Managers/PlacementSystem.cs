using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementSystem : MonoBehaviour
{
    public event Action<int>  OnStructureBuilt;
    public event Action<int, int> OnStructureRemoved;
    public event Action<Vector3Int, int, int> OnMoved;

    [SerializeField]
    private InputManager inputManager;
    [SerializeField]
    private Grid grid;
    [SerializeField]
    private RoadManager roadManager;
    [SerializeField]
    private BuildingRegistryManager buildingUIManager;
    [SerializeField]
    private UIController userInterface;

    [SerializeField]
    private StructuresDatabaseSO database;

    public int selectedObjectIndex = -1;
    private int selectedObjectID = -1;

    public GridData gridData;

    public List<GameObject> placedGameObjects = new();
    public List<GameObject> placedDecorations = new();

    public bool isBuilding = false;
    public GameObject previewStructure;
    private GameObject buildingBlock;
    private GameObject occupiedBuildingBlock;

    [SerializeField]
    private GameObject occupiedBlockOne;
    [SerializeField]
    private GameObject unoccupiedBlockOne;

    [SerializeField]
    private GameObject occupiedBlockTwo;
    [SerializeField]
    private GameObject unoccupiedBlockTwo;

    [SerializeField]
    private GameObject occupiedBlockThree;
    [SerializeField]
    private GameObject unoccupiedBlockThree;

    int numberOfRoadsPlaced = 0;

    Color whiteColor = new Color(1f, 1f, 1f, 0.9215f);
    Color redColor = new Color(1f, 0.2688f, 0.2688f, 0.9215f);

    private void Start()
    {
        StopPlacement();
        gridData = new();

        gridData.AddPositionsInArea(new Vector3Int(-27, 33, 0), new Vector3Int(18, 52, 0));
        gridData.AddPositionsInArea(new Vector3Int(-30, 26, 0), new Vector3Int(-8, 31, 0));
        gridData.AddPositionsInArea(new Vector3Int(-28, 32, 0), new Vector3Int(11, 32, 0));
        gridData.AddPositionsInArea(new Vector3Int(-28, 25, 0), new Vector3Int(-9, 25, 0));
        gridData.AddPositionsInArea(new Vector3Int(-28, 36, 0), new Vector3Int(-28, 53, 0));
        gridData.AddPositionsInArea(new Vector3Int(19, 36, 0), new Vector3Int(20, 50, 0));
        gridData.AddPositionsInArea(new Vector3Int(-22, 53, 0), new Vector3Int(-9, 55, 0));
        gridData.AddPositionsInArea(new Vector3Int(-8, 53, 0), new Vector3Int(-2, 54, 0));
        gridData.AddPositionsInArea(new Vector3Int(-1, 53, 0), new Vector3Int(12, 55, 0));
        gridData.AddPositionsInArea(new Vector3Int(-7, 28, 0), new Vector3Int(-6, 31, 0));
        gridData.AddPositionsInArea(new Vector3Int(-4, 28, 0), new Vector3Int(7, 28, 0));
        gridData.AddPositionsInArea(new Vector3Int(7, 29, 0), new Vector3Int(11, 29, 0));
        gridData.AddPositionsInArea(new Vector3Int(21, 38, 0), new Vector3Int(21, 44, 0));

        gridData.RemovePositionsInArea(new Vector3Int(12, 36, 0), new Vector3Int(18, 39, 0));
        gridData.RemovePositionsInArea(new Vector3Int(-24, 33, 0), new Vector3Int(-18, 36, 0));

        gridData.mapBoundaries.ExceptWith(gridData.positionsToRemove);

        PlaceMinesMarketsRefinery();
    }

    public void StartPlacement(int ID)
    {
        selectedObjectID = ID;
        if (selectedObjectID < 0)
        {
            Debug.LogError($"No ID found {ID}");
            return;
        }

        if (selectedObjectID > 42)
        {
            userInterface.ActivateBuildRotatePanel();
        }
        else if (selectedObjectID == 0)
        {
            inputManager.OnMouseTapped += HandleRoadPlacement;
            userInterface.ActivateRoadBuildPanel();
        }
        else
        {
            userInterface.ActivateBuildPanel();
        }

        // Add preview of building with cells indicating where it can be placed
        isBuilding = true;

        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenCenter);
        Vector3Int cellPosition = grid.WorldToCell(worldPosition);
        cellPosition.z = 0;
        if (ID > 15)
        {
            previewStructure = Instantiate(database.objectsData[ID].LevelOnePrefab);
            previewStructure.GetComponent<SpriteRenderer>().sortingOrder = 1000;

            int structureLength = database.GetSizeByID(ID).x;

            if (structureLength > 2)
            {
                cellPosition = new Vector3Int(cellPosition.x - 1, cellPosition.y, 0);
                previewStructure.transform.position = cellPosition;
            }
            else
            {
                previewStructure.transform.position = cellPosition;
            }

            if (structureLength == 1)
            {
                buildingBlock = Instantiate(unoccupiedBlockOne);
                buildingBlock.SetActive(false);
                buildingBlock.transform.position = cellPosition;

                occupiedBuildingBlock = Instantiate(occupiedBlockOne);
                occupiedBuildingBlock.SetActive(false);
                occupiedBuildingBlock.transform.position = cellPosition;
            }

            else if (structureLength == 2)
            {
                buildingBlock = Instantiate(unoccupiedBlockTwo);
                buildingBlock.SetActive(false);
                buildingBlock.transform.position = cellPosition;

                occupiedBuildingBlock = Instantiate(occupiedBlockTwo);
                occupiedBuildingBlock.SetActive(false);
                occupiedBuildingBlock.transform.position = cellPosition;
            }

            else if (structureLength == 3)
            {
                buildingBlock = Instantiate(unoccupiedBlockThree);
                buildingBlock.SetActive(false);
                buildingBlock.transform.position = cellPosition;

                occupiedBuildingBlock = Instantiate(occupiedBlockThree);
                occupiedBuildingBlock.SetActive(false);
                occupiedBuildingBlock.transform.position = cellPosition;
            }

            inputManager.OnMouseTapped += MoveStructure;

            bool placementValidity = gridData.CanPlaceObjectAt(cellPosition, database.objectsData[ID].Size);
            if (placementValidity == false)
            {
                previewStructure.GetComponent<SpriteRenderer>().color = redColor;
                occupiedBuildingBlock.SetActive(true);
            }
            else
            {
                previewStructure.GetComponent<SpriteRenderer>().color = whiteColor;
                buildingBlock.SetActive(true);
            }

        }
    }

    private void HandleRoadPlacement()
    {
        Vector3 worldPosition = inputManager.GetSelectedMapPosition();
        Vector3Int cellPosition = grid.WorldToCell(worldPosition);

        bool placementValidity = gridData.CanPlaceObjectAt(cellPosition, database.objectsData[selectedObjectID].Size);
        if (placementValidity == true)
        {
            List<Vector3Int> neighbourRoadPositions = gridData.GetNeighbourRoadPositions(cellPosition);
            bool[] neighbourRoadCheck = gridData.GetNeighbouringRoads(cellPosition);
            roadManager.FixRoadAt(cellPosition, neighbourRoadCheck);
            roadManager.FixNeighbouringRoadsAt(neighbourRoadPositions);
            numberOfRoadsPlaced++;
            userInterface.UpdateRoadPlacedUI(numberOfRoadsPlaced);
        }
        else
        {
            if (gridData.GetIDAtPosition(cellPosition) < 16)
            {
                RemoveStructureAt(cellPosition);

                List<Vector3Int> neighbourRoadPositions = gridData.GetNeighbourRoadPositions(cellPosition);
                roadManager.FixNeighbouringRoadsAt(neighbourRoadPositions);

                numberOfRoadsPlaced--;
                userInterface.UpdateRoadPlacedUI(numberOfRoadsPlaced);
            }
        }
    }

    public void RemoveStructureAt(Vector3Int position)
    {
        GameObject objectToRemove;
        selectedObjectIndex = gridData.GetPlacementDataAt(position).PlaceObjectIndex; // This method should return the index of the object at the position, or -1 if there's no object.
        if (selectedObjectIndex == -1)
        {
            Debug.LogWarning("No structure found at this position.");
            return;
        }

        // Get the object prefab and remove it from the placed game objects list
        if (isStructure())
        {
            objectToRemove = placedGameObjects[selectedObjectIndex];
            placedGameObjects.RemoveAt(selectedObjectIndex);
            gridData.RemoveObjectAt(position);

            for (int i = selectedObjectIndex; i < placedGameObjects.Count; i++)
            {
                GameObject currentObject = placedGameObjects[i];
                Vector3 currentPosition = currentObject.transform.position;
                Vector3Int currentGridPosition = grid.WorldToCell(currentPosition);

                // Update PlacementData in gridData
                PlacementData currentData = gridData.GetPlacementDataAt(currentGridPosition);
                if (currentData != null)
                {
                    currentData.PlaceObjectIndex--;
                }
            }
        }
        else
        {
            objectToRemove = placedDecorations[selectedObjectIndex];
            placedDecorations.RemoveAt(selectedObjectIndex);
            gridData.RemoveObjectAt(position);
            OnStructureRemoved?.Invoke(selectedObjectIndex, 1);

            for (int i = selectedObjectIndex; i < placedDecorations.Count; i++)
            {
                GameObject currentObject = placedDecorations[i];
                Vector3 currentPosition = currentObject.transform.position;
                Vector3Int currentGridPosition = grid.WorldToCell(currentPosition);

                // Update PlacementData in gridData
                PlacementData currentData = gridData.GetPlacementDataAt(currentGridPosition);
                if (currentData != null)
                {
                    currentData.PlaceObjectIndex--;
                }
            }
        }

        Destroy(objectToRemove);
    }

    public void PlaceStructure()
    {
        Vector3 worldPosition = previewStructure.transform.position;
        Vector3Int gridPosition = grid.WorldToCell(worldPosition);

        bool placementValidity = gridData.CanPlaceObjectAt(gridPosition, database.objectsData[selectedObjectID].Size);
        if (placementValidity == false)
        {
            return;
        }
        
        GameObject newObject = Instantiate(database.objectsData[selectedObjectID].LevelOnePrefab);
        newObject.transform.position = grid.CellToWorld(gridPosition);
        newObject.GetComponent<SpriteRenderer>().sortingOrder = -gridPosition.y * 10;
        Destroy(previewStructure);

        if (isStructure())
        {
            placedGameObjects.Add(newObject);
            gridData.AddObjectAt(gridPosition,
                                 database.objectsData[selectedObjectID].Size,
                                 database.objectsData[selectedObjectID].ID,
                                 placedGameObjects.Count - 1, ObjectType.Object);
            newObject.GetComponent<Building>().Index = placedGameObjects.Count - 1;
            newObject.GetComponent<Building>().UpgradeBuilding();
        }
        else
        {
            placedDecorations.Add(newObject);
            gridData.AddObjectAt(gridPosition,
                                 database.objectsData[selectedObjectID].Size,
                                 database.objectsData[selectedObjectID].ID,
                                 placedDecorations.Count - 1, ObjectType.Decoration);
        }

        inputManager.OnMouseTapped -= MoveStructure;
        OnStructureBuilt?.Invoke(selectedObjectID);
        StopPlacement();
    }

    public void PlaceStructureAt(int ID, Vector3Int position)
    {
        // Find the object data by ID from the database
        selectedObjectID = database.objectsData.FindIndex(data => data.ID == ID);
        if (selectedObjectID < 0)
        {
            Debug.LogError($"No object found with ID {ID}");
            return;
        }

        // Check if the object can be placed at the given position
        bool placementValidity = gridData.CanPlaceObjectAt(position, database.objectsData[selectedObjectID].Size);
        if (placementValidity == false)
        {
            Debug.LogWarning("Placement is invalid.");
            return;
        }

        // Instantiate the object and add it to the placed objects
        GameObject newObject = Instantiate(database.objectsData[selectedObjectID].LevelOnePrefab);
        newObject.transform.position = grid.CellToWorld(position);
        newObject.GetComponent<SpriteRenderer>().sortingOrder = -position.y * 10;

        if (isStructure())
        {
            placedGameObjects.Add(newObject);
            gridData.AddObjectAt(position,
                                 database.objectsData[selectedObjectID].Size,
                                 database.objectsData[selectedObjectID].ID,
                                 placedGameObjects.Count - 1, ObjectType.Object);
        }
        else
        {
            placedDecorations.Add(newObject);
            OnStructureBuilt?.Invoke(selectedObjectID);
            gridData.AddObjectAt(position,
                                 database.objectsData[selectedObjectID].Size,
                                 database.objectsData[selectedObjectID].ID,
                                 placedDecorations.Count - 1, ObjectType.Decoration);
        }
    }

    private void MoveStructure()
    {
        if (previewStructure == null)
        {
            Debug.LogWarning("Preview structure has been destroyed.");
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Pointer is over a UI element. MoveStructure will not execute.");
            return;
        }

        int structureLength = database.GetSizeByID(selectedObjectID).x;
        Vector3 worldPosition = inputManager.GetSelectedMapPosition();
        if (structureLength == 2)
        {
            worldPosition = new Vector3(worldPosition.x - 0.5f, worldPosition.y, 0);
        }
        Vector3Int cellPosition = grid.WorldToCell(worldPosition);
        cellPosition.z = 0;

        if (structureLength > 2)
        {
            cellPosition.x--;
        }

        previewStructure.transform.position = cellPosition;
        buildingBlock.transform.position = cellPosition;
        occupiedBuildingBlock.transform.position = cellPosition;

        bool placementValidity = gridData.CanPlaceObjectAt(cellPosition, database.objectsData[selectedObjectID].Size);
        if (placementValidity == false)
        {
            previewStructure.GetComponent<SpriteRenderer>().color = redColor;
            buildingBlock.SetActive(false);
            occupiedBuildingBlock.SetActive(true);
        }
        else
        {
            previewStructure.GetComponent<SpriteRenderer>().color = whiteColor;
            buildingBlock.SetActive(true);
            occupiedBuildingBlock.SetActive(false);
        }
    }

    public void StopPlacement()
    {
        Destroy(previewStructure);
        Destroy(buildingBlock);
        Destroy(occupiedBuildingBlock);
        userInterface.DeactivateBuildPanel();
        userInterface.DeactivateEditPanel();
        userInterface.DeactivateBuildRotatePanel();
        userInterface.DeactivateEditRotatePanel();
        selectedObjectIndex = -1;
        inputManager.OnMouseTapped -= MoveStructure;
        isBuilding = false;
        return;
    }

    public void StopRoadPlacement()
    {
        inputManager.OnMouseTapped -= HandleRoadPlacement;
        userInterface.DeactivateRoadBuildPanel();
        isBuilding = false;
        numberOfRoadsPlaced = 0;
        userInterface.UpdateRoadPlacedUI(numberOfRoadsPlaced);
        return;
    }

    public void Rotate()
    {
        Vector3Int position = Vector3Int.RoundToInt(previewStructure.transform.position);

        if (selectedObjectID == -1 || previewStructure == null)
        {
            return;
        }

        // Flower Side
        if (selectedObjectID == 43)
        {
            selectedObjectID++;

            Destroy(previewStructure);

            previewStructure = Instantiate(database.objectsData[selectedObjectID].LevelOnePrefab);
            previewStructure.GetComponent<SpriteRenderer>().sortingOrder = 1000;
            previewStructure.transform.position = position;

            bool placementValidity = gridData.CanPlaceObjectAt(position, database.objectsData[selectedObjectID].Size);
            if (placementValidity == false)
            {
                previewStructure.GetComponent<SpriteRenderer>().color = redColor;
            }
            else
            {
                previewStructure.GetComponent<SpriteRenderer>().color = whiteColor;

            }
        }

        // Flower Straight
        else if (selectedObjectID == 44)
        {
            selectedObjectID--;

            Destroy(previewStructure);

            previewStructure = Instantiate(database.objectsData[selectedObjectID].LevelOnePrefab);
            previewStructure.GetComponent<SpriteRenderer>().sortingOrder = 1000;
            previewStructure.transform.position = position;

            bool placementValidity = gridData.CanPlaceObjectAt(position, database.objectsData[selectedObjectID].Size);
            if (placementValidity == false)
            {
                previewStructure.GetComponent<SpriteRenderer>().color = redColor;
            }
            else
            {
                previewStructure.GetComponent<SpriteRenderer>().color = whiteColor;
            }
        }

        // Bench
        else if (selectedObjectID > 44 && selectedObjectID < 48)
        {
            selectedObjectID++;

            Destroy(previewStructure);

            previewStructure = Instantiate(database.objectsData[selectedObjectID].LevelOnePrefab);
            previewStructure.GetComponent<SpriteRenderer>().sortingOrder = 1000;
            previewStructure.transform.position = position;

            bool placementValidity = gridData.CanPlaceObjectAt(position, database.objectsData[selectedObjectID].Size);
            if (placementValidity == false)
            {
                previewStructure.GetComponent<SpriteRenderer>().color = redColor;
            }
            else
            {
                previewStructure.GetComponent<SpriteRenderer>().color = whiteColor;
            }
        }

        // Reset Bench
        else if (selectedObjectID == 48)
        {
            selectedObjectID = 45;

            Destroy(previewStructure);

            previewStructure = Instantiate(database.objectsData[selectedObjectID].LevelOnePrefab);
            previewStructure.GetComponent<SpriteRenderer>().sortingOrder = 1000;
            previewStructure.transform.position = position;

            bool placementValidity = gridData.CanPlaceObjectAt(position, database.objectsData[selectedObjectID].Size);
            if (placementValidity == false)
            {
                previewStructure.GetComponent<SpriteRenderer>().color = redColor;
            }
            else
            {
                previewStructure.GetComponent<SpriteRenderer>().color = whiteColor;
            }
        }
    }

    public void SelectStructure(Vector3Int cellPosition)
    {
        if (isBuilding == false)
        {
            isBuilding = true;
            PlacementData placementData = gridData.GetPlacementDataAt(cellPosition);
            selectedObjectIndex = placementData.PlaceObjectIndex;
            selectedObjectID = placementData.ID;

            Vector3Int objectPosition = placementData.occupiedPositions[0];
                
            // if is a road
            if (selectedObjectID < 16)
            {
                RemoveStructureAt(objectPosition);
                inputManager.OnMouseTapped += HandleRoadPlacement;
                userInterface.ActivateRoadBuildPanel();

                List<Vector3Int> neighbourRoadPositions = gridData.GetNeighbourRoadPositions(cellPosition);
                roadManager.FixNeighbouringRoadsAt(neighbourRoadPositions);

                numberOfRoadsPlaced--;
                userInterface.UpdateRoadPlacedUI(numberOfRoadsPlaced);
            }
            //if not a road
            else {
                // if rotateable
                if (selectedObjectID > 42)
                {
                    userInterface.ActivateEditRotatePanel();
                }
                    
                else
                {
                    userInterface.ActivateEditPanel();
                }

                // if not decoration
                if (selectedObjectID < 39)
                {
                    placedGameObjects[selectedObjectIndex].SetActive(false);
                    int buildingLevel = placedGameObjects[selectedObjectIndex].GetComponent<Building>().Level;

                    if (buildingLevel == 1)
                    {
                        previewStructure = Instantiate(database.objectsData[selectedObjectID].LevelOnePrefab);
                    }
                    else if (buildingLevel == 2)
                    {
                        previewStructure = Instantiate(database.objectsData[selectedObjectID].LevelTwoPrefab);
                    }
                    else if (buildingLevel == 3)
                    {
                        previewStructure = Instantiate(database.objectsData[selectedObjectID].LevelThreePrefab);
                    }
                }

                // if is decoration
                else
                {
                    placedDecorations[selectedObjectIndex].SetActive(false);
                    previewStructure = Instantiate(database.objectsData[selectedObjectID].LevelOnePrefab);
                }

                // Initialize preview structure
                previewStructure.transform.position = objectPosition;
                previewStructure.GetComponent<SpriteRenderer>().sortingOrder = 1000;
                previewStructure.GetComponent<SpriteRenderer>().color = whiteColor;

                gridData.RemoveObjectAt(objectPosition);
                inputManager.OnMouseTapped += MoveStructure;
                    
                // create display blocks
                int structureLength = database.GetSizeByID(selectedObjectID).x;
                if (structureLength == 1)
                {
                    buildingBlock = Instantiate(unoccupiedBlockOne);
                    buildingBlock.SetActive(true);
                    buildingBlock.transform.position = objectPosition;

                    occupiedBuildingBlock = Instantiate(occupiedBlockOne);
                    occupiedBuildingBlock.SetActive(false);
                    occupiedBuildingBlock.transform.position = objectPosition;
                }

                else if (structureLength == 2)
                {
                    buildingBlock = Instantiate(unoccupiedBlockTwo);
                    buildingBlock.SetActive(true);
                    buildingBlock.transform.position = objectPosition;

                    occupiedBuildingBlock = Instantiate(occupiedBlockTwo);
                    occupiedBuildingBlock.SetActive(false);
                    occupiedBuildingBlock.transform.position = objectPosition;
                }

                else if (structureLength == 3)
                {
                    buildingBlock = Instantiate(unoccupiedBlockThree);
                    buildingBlock.SetActive(true);
                    buildingBlock.transform.position = objectPosition;

                    occupiedBuildingBlock = Instantiate(occupiedBlockThree);
                    occupiedBuildingBlock.SetActive(false);
                    occupiedBuildingBlock.transform.position = objectPosition;
                }
            }
        }
    }

    public void Demolish()
    {
        GameObject objectToRemove;
        if (isStructure())
        {
            objectToRemove = placedGameObjects[selectedObjectIndex];
            objectToRemove.GetComponent<Building>().ClearVillagers();
            placedGameObjects.RemoveAt(selectedObjectIndex);

            for (int i = selectedObjectIndex; i < placedGameObjects.Count; i++)
            {
                GameObject currentObject = placedGameObjects[i];
                Vector3 currentPosition = currentObject.transform.position;
                Vector3Int currentGridPosition = grid.WorldToCell(currentPosition);

                // Update PlacementData in gridData
                PlacementData currentData = gridData.GetPlacementDataAt(currentGridPosition);
                if (currentData != null)
                {
                    currentData.PlaceObjectIndex--;
                }
            }

            OnStructureRemoved?.Invoke(selectedObjectIndex, 0);
        }
        else
        {
            objectToRemove = placedDecorations[selectedObjectIndex];
            placedDecorations.RemoveAt(selectedObjectIndex);

            for (int i = selectedObjectIndex; i < placedDecorations.Count; i++)
            {
                GameObject currentObject = placedDecorations[i];
                Vector3 currentPosition = currentObject.transform.position;
                Vector3Int currentGridPosition = grid.WorldToCell(currentPosition);

                // Update PlacementData in gridData
                PlacementData currentData = gridData.GetPlacementDataAt(currentGridPosition);
                if (currentData != null)
                {
                    currentData.PlaceObjectIndex--;
                }
            }

            OnStructureRemoved?.Invoke(selectedObjectIndex, 1);
        }
        Destroy(objectToRemove);
        StopPlacement();
    }

    public void PlaceEditedStructure()
    {
        Vector3 worldPosition = previewStructure.transform.position;
        Vector3Int gridPosition = grid.WorldToCell(worldPosition);
        int buildingType;
        GameObject building;

        if (isStructure())
        {
            building = placedGameObjects[selectedObjectIndex];
        }
        else
        {
            building = placedDecorations[selectedObjectIndex];
        }

        bool placementValidity = gridData.CanPlaceObjectAt(gridPosition, database.objectsData[selectedObjectID].Size);
        if (placementValidity == false)
        {
            return;
        }


        if (selectedObjectID > 38)
        {
            Destroy(building);
            placedDecorations[selectedObjectIndex] = null;
            placedDecorations.RemoveAt(selectedObjectIndex);
            GameObject newObject = Instantiate(database.objectsData[selectedObjectID].LevelOnePrefab);
            newObject.transform.position = grid.CellToWorld(gridPosition);
            newObject.GetComponent<SpriteRenderer>().sortingOrder = -gridPosition.y * 10;
            placedDecorations.Insert(selectedObjectIndex, newObject);
            Destroy(previewStructure);
            buildingType = 1;

            gridData.AddObjectAt(gridPosition,
                     database.objectsData[selectedObjectID].Size,
                     database.objectsData[selectedObjectID].ID,
                     selectedObjectIndex, ObjectType.Decoration);
        }

        else
        {
            building.transform.position = gridPosition;
            buildingType = 0;

            gridData.AddObjectAt(gridPosition,
                     database.objectsData[selectedObjectID].Size,
                     database.objectsData[selectedObjectID].ID,
                     selectedObjectIndex, ObjectType.Object);
            building.SetActive(true);
            building.GetComponent<Building>().SetBuildingSprite();
        }

        building.GetComponent<SpriteRenderer>().sortingOrder = -gridPosition.y * 10;
        OnMoved?.Invoke(gridPosition, selectedObjectIndex, buildingType);
        buildingUIManager.ChangeBuildingToggleUIPlacement(selectedObjectIndex, database.objectsData[selectedObjectID].Size, gridPosition, buildingType);
        StopPlacement();
    }

    public void PlaceMinesMarketsRefinery()
    {
        // Fixed Buildings
        List<Vector3Int> refineryPositions = new List<Vector3Int> { new Vector3Int(10, 30, 0), new Vector3Int(11, 30, 0), new Vector3Int(12, 30, 0) };
        List<Vector3Int> mineOnePositions = new List<Vector3Int> { new Vector3Int(14, 37, 0), new Vector3Int(15, 37, 0), new Vector3Int(16, 37, 0), new Vector3Int(14, 36, 0), new Vector3Int(15, 36, 0) };
        List<Vector3Int> mineTwoPositions = new List<Vector3Int> { new Vector3Int(-22, 33, 0), new Vector3Int(-21, 33, 0), new Vector3Int(-20, 33, 0), new Vector3Int(-22, 34, 0), new Vector3Int(-21, 34, 0), new Vector3Int(-20, 34, 0) };
        List<Vector3Int> mineThreePositions = new List<Vector3Int> { new Vector3Int(-6, 55, 0), new Vector3Int(-5, 55, 0), new Vector3Int(-4, 55, 0), new Vector3Int(-6, 56, 0), new Vector3Int(-5, 56, 0), new Vector3Int(-4, 56, 0) };
        List<Vector3Int> mineFivePositions = new List<Vector3Int> { new Vector3Int(11, 54, 0), new Vector3Int(12, 54, 0), new Vector3Int(13, 54, 0), new Vector3Int(11, 55, 0), new Vector3Int(12, 55, 0), new Vector3Int(13, 55, 0) };
        List<Vector3Int> mineFourPositions = new List<Vector3Int> { new Vector3Int(-23, 54, 0), new Vector3Int(-22, 54, 0), new Vector3Int(-21, 54, 0), new Vector3Int(-23, 55, 0), new Vector3Int(-22, 55, 0), new Vector3Int(-21, 55, 0) };
        List<Vector3Int> marketOnePositions = new List<Vector3Int> { new Vector3Int(-4, 29, 0), new Vector3Int(-3, 29, 0), new Vector3Int(-2, 29, 0), new Vector3Int(-1, 29, 0), new Vector3Int(-4, 27, 0), new Vector3Int(-3, 27, 0), new Vector3Int(-2, 27, 0), new Vector3Int(-1, 27, 0) };
        List<Vector3Int> marketTwoPositions = new List<Vector3Int> { new Vector3Int(3, 29, 0), new Vector3Int(4, 29, 0), new Vector3Int(5, 29, 0), new Vector3Int(6, 29, 0), new Vector3Int(3, 27, 0), new Vector3Int(4, 27, 0), new Vector3Int(5, 27, 0), new Vector3Int(6, 27, 0) };

        // Place Markets
        // Market One
        GameObject marketOne = Instantiate(database.objectsData[38].Base);
        marketOne.transform.position = new Vector3(-2.49f, 27.3f, 0);
        placedGameObjects.Add(marketOne);
        gridData.AddFixedObjects(marketOnePositions, 38, 0);
        OnStructureBuilt?.Invoke(38);
        // Upgrade
        buildingUIManager.UpgradeBuildingWithIndex(0, 38);
        marketOne.GetComponent<Building>().FinishConstruction();

        // Market Two
        GameObject marketTwo = Instantiate(database.objectsData[38].Base);
        marketTwo.transform.position = new Vector3(4.53f, 27.3f, 0);
        placedGameObjects.Add(marketTwo);
        gridData.AddFixedObjects(marketTwoPositions, 38, 1);
        OnStructureBuilt?.Invoke(38);

        // Place Refinery
        GameObject refinery = Instantiate(database.objectsData[31].Base);
        refinery.transform.position = new Vector3Int(10, 30, 0);
        refinery.GetComponent<SpriteRenderer>().sortingOrder = 300;
        placedGameObjects.Add(refinery);
        gridData.AddFixedObjects(refineryPositions, 31, 2);
        OnStructureBuilt?.Invoke(31);

        // Place Mines
        // Mine One
        GameObject mineOne = Instantiate(database.objectsData[28].Base);
        mineOne.transform.position = new Vector3(14, 35.5f, 0);
        mineOne.GetComponent<SpriteRenderer>().sortingOrder = 360;
        placedGameObjects.Add(mineOne);
        gridData.AddFixedObjects(mineOnePositions, 28, 3);
        OnStructureBuilt?.Invoke(28);

        // Mine Two
        GameObject mineTwo = Instantiate(database.objectsData[28].Base);
        mineTwo.transform.position = new Vector3(-22, 32.5f, 0);
        mineTwo.GetComponent<SpriteRenderer>().sortingOrder = 330;
        placedGameObjects.Add(mineTwo);
        gridData.AddFixedObjects(mineTwoPositions, 28, 4);
        OnStructureBuilt?.Invoke(28);

        // Mine Three
        GameObject mineThree = Instantiate(database.objectsData[28].Base);
        mineThree.transform.position = new Vector3(-6, 54.5f, 0);
        mineThree.GetComponent<SpriteRenderer>().sortingOrder = 550;
        placedGameObjects.Add(mineThree);
        gridData.AddFixedObjects(mineThreePositions, 28, 5);
        OnStructureBuilt?.Invoke(28);

        // Mine Four
        GameObject mineFour = Instantiate(database.objectsData[28].Base);
        mineFour.transform.position = new Vector3(-23, 53.5f, 0);
        mineFour.GetComponent<SpriteRenderer>().sortingOrder = 540;
        placedGameObjects.Add(mineFour);
        gridData.AddFixedObjects(mineFourPositions, 28, 6);
        OnStructureBuilt?.Invoke(28);

        // Mine Five
        GameObject mineFive = Instantiate(database.objectsData[28].Base);
        mineFive.transform.position = new Vector3(11, 53.5f, 0);
        mineFive.GetComponent<SpriteRenderer>().sortingOrder = 540;
        placedGameObjects.Add(mineFive);
        gridData.AddFixedObjects(mineFivePositions, 28, 7);
        OnStructureBuilt?.Invoke(28);        
    }

    private bool isStructure()
    {
        return selectedObjectID > 16 && selectedObjectID < 39;
    }
}
