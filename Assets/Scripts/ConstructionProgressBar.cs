using TMPro;
using UnityEngine;

public class ConstructionProgressBar : MonoBehaviour
{
    [SerializeField]
    private GameObject progressMask;
    public TextMeshProUGUI timerText;

    public void SetProgress(float progress)
    {
        if (progressMask.TryGetComponent(out UnityEngine.UI.Image maskImage))
        {
            maskImage.fillAmount = progress;
        }
    }
}
