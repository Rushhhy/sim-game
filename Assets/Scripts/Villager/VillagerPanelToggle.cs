using UnityEngine;

public class VillagerPanelToggle : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject villagerDescription;

    private bool showingMain = true;

    public void TogglePanels()
    {
        showingMain = !showingMain;
        mainPanel.SetActive(showingMain);
        villagerDescription.SetActive(!showingMain);
    }
}
