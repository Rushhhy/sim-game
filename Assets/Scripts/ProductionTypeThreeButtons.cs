using UnityEngine;

public class ProductionTypeThreeButtons : MonoBehaviour
{
    public void ToggleProgressBar()
    {
        Transform progressBarOne = transform.Find("UpgradeTime");
        Transform progressBarTwo = transform.Find("UpgradeTimeTwo");

        Transform productionInfoOne = transform.Find("1");
        Transform productionInfoTwo = transform.Find("1 (Two)");

        bool isOneActive = progressBarOne.gameObject.activeSelf && productionInfoOne.gameObject.activeSelf;

        progressBarOne.gameObject.SetActive(!isOneActive);
        productionInfoOne.gameObject.SetActive(!isOneActive);

        progressBarTwo.gameObject.SetActive(isOneActive);
        productionInfoTwo.gameObject.SetActive(isOneActive);
    }
}
