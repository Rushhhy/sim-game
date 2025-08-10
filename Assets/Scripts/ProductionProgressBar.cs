using TMPro;
using UnityEngine;

public class ProductionProgressBar : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI timerText;
    [SerializeField]
    private GameObject progressMask;

    public int structureIndex;
    public float productionTime;
    public float timer;
    public int resourceIndex;

    private ProductionBuilding buildingScript;

    // Optional: use this method instead of relying on Start for setup
    public void Initialize(ProductionBuilding building, int resourceIndex)
    {
        this.buildingScript = building;
        this.resourceIndex = resourceIndex;
    }

    private void Update()
    {
        if (buildingScript == null) return;

        timer = buildingScript.individualTimers[resourceIndex];
        productionTime = buildingScript.ResourceProductionTime[resourceIndex]; // Always get the latest

        float progress = Mathf.Clamp01(timer / productionTime);
        UpdateProgressMask(progress);

        float remainingTime = Mathf.Max(0f, productionTime - timer);
        UpdateTimerText(remainingTime);
    }

    private void UpdateProgressMask(float progress)
    {
        if (progressMask.TryGetComponent(out UnityEngine.UI.Image maskImage))
        {
            maskImage.fillAmount = progress;
        }
    }

    private void UpdateTimerText(float remainingTime)
    {
        timerText.text = $"{remainingTime:F2}";
    }

    public void SetProductionTime(int resourceIndex)
    {
        if (buildingScript == null)
        {
            Debug.LogError("SetProductionTime failed: buildingScript is null.");
            return;
        }

        if (buildingScript.ResourceProductionTime == null)
        {
            Debug.LogError("SetProductionTime failed: ResourceProductionTime array is null.");
            return;
        }

        if (resourceIndex < 0 || resourceIndex >= buildingScript.ResourceProductionTime.Length)
        {
            Debug.LogError($"SetProductionTime failed: resourceIndex {resourceIndex} is out of bounds. " +
                           $"Array length is {buildingScript.ResourceProductionTime.Length}.");
            return;
        }

        productionTime = buildingScript.ResourceProductionTime[resourceIndex];
    }
}
