using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BuildingState
{
    UnderConstruction,
    Active
}

public class Building : MonoBehaviour
{
    public BuildingState State { get; private set; }

    public float[] constructionDurationsPerLevel = { 5f, 8f, 11f };
    private DateTime constructionEndTime;
    private float constructionDuration;

    public int Index = -1;
    public int ID;
    public int Level = 0;
    public int width;
    public string BuildingName;

    protected BuildingRegistryManager buildingRegistryManager;

    [Header("Sprites")]
    public Sprite BuildingLevelOne, BuildingLevelTwo, BuildingLevelThree;

    public Sprite constructionSprite;
    public SpriteRenderer SpriteRenderer;

    [Header("Villagers")]
    public Villager villagerOne, villagerTwo, villagerThree;

    [Header("UI")]
    [SerializeField] private GameObject progressBarPrefab;
    [SerializeField] private GameObject finishConstructionPrefab;
    [SerializeField] private GameObject smokeExplosionPrefab;
    [SerializeField] private GameObject hammerPrefab;
    protected GameObject hammerObj;
    protected GameObject finishConstructionObj;
    protected ConstructionProgressBar currentProgressBar;

    protected Animator animator;
    private bool isAnimated;

    private Transform worldCanvas;

    private VillagerManager villagerManager;

    public List<Vector3> workPositions;
    protected virtual void Awake()
    {
        buildingRegistryManager = GameObject.Find("BuildingRegistryManager").GetComponent<BuildingRegistryManager>();
        villagerManager = GameObject.Find("VillagerManager").GetComponent<VillagerManager>();
        animator = GetComponent<Animator>();
        isAnimated = animator != null;

        worldCanvas = GameObject.Find("WorldCanvasUI").transform;

        State = BuildingState.Active;
        SetBuildingSprite();
    }

    protected virtual void Update()
    {
        if (State == BuildingState.UnderConstruction)
        {
            float remainingSeconds = (float)(constructionEndTime - DateTime.UtcNow).TotalSeconds;
            remainingSeconds = Mathf.Max(remainingSeconds, 0f);

            // Progress bar fill update
            float progress = 1f - Mathf.Clamp01(remainingSeconds / constructionDuration);
            currentProgressBar.SetProgress(progress);

            // Timer Text update (HH:MM:SS)
            TimeSpan timeSpan = TimeSpan.FromSeconds(remainingSeconds);
            currentProgressBar.timerText.text = timeSpan.ToString(@"hh\:mm\:ss");

            // Time complete
            if (DateTime.UtcNow >= constructionEndTime)
            {
                finishConstructionObj.SetActive(true);
                currentProgressBar.gameObject.SetActive(false);
                Destroy(hammerObj);
                hammerObj = null;
            }              
        }
    }

    /// <summary>
    /// Starts construction or upgrade for the given level.
    /// </summary>
    protected virtual void StartBuildOrUpgrade(int level)
    {
        if (currentProgressBar == null)
        {
            Vector3 worldPos = GetProgressBarOffset();
            GameObject barGO = Instantiate(progressBarPrefab, worldCanvas);
            currentProgressBar = barGO.GetComponent<ConstructionProgressBar>();
            barGO.transform.position = worldPos;
        }

        if (finishConstructionObj == null)
        {
            Vector3 buttonPos = GetFinishButtonOffset();
            finishConstructionObj = Instantiate(finishConstructionPrefab, worldCanvas);
            finishConstructionObj.transform.position = buttonPos;

            Button finishConstructionButton = finishConstructionObj.GetComponent<Button>();
            finishConstructionButton.onClick.AddListener(FinishConstruction);
        }

        State = BuildingState.UnderConstruction;
        SmokeEffect();

        Vector3 hammerPos = GetHammerOffset();
        hammerObj = Instantiate(hammerPrefab, hammerPos, Quaternion.identity);

        constructionDuration = GetDurationForLevel(level - 1);
        constructionEndTime = DateTime.UtcNow.AddSeconds(constructionDuration);

        if (isAnimated && animator != null)
            animator.enabled = false;

        if (SpriteRenderer != null && constructionSprite != null)
            SpriteRenderer.sprite = constructionSprite;

        currentProgressBar.gameObject.SetActive(true);
        currentProgressBar.SetProgress(0f);
    }

    private Vector3 GetHammerOffset()
    {
        return width switch
        {
            3 => transform.position + new Vector3(1.4f, 1f, 0f),
            4 => transform.position + new Vector3(0f, 1f, 0f),
            _ => transform.position + new Vector3(1f, 1f, 0f)
        };
    }

    private Vector3 GetProgressBarOffset()
    {
        return width switch
        {
            3 => transform.position + new Vector3(0.5f, -0.5f, 0f),
            4 => transform.position + new Vector3(-0.55f, 0.5f, 0f),
            _ => transform.position + new Vector3(0f, -0.5f, 0f),
        };
    }

    private Vector3 GetFinishButtonOffset()
    {
        return width switch
        {
            3 => transform.position + new Vector3(1.5f, -0.4f, 0f),
            4 => transform.position + new Vector3(0.4f, 0.6f, 0f),
            _ => transform.position + new Vector3(1f, -0.4f, 0f)
        }; 
    }

    /// <summary>
    /// External call to upgrade this building. Automatically increases level.
    /// </summary>
    public virtual void UpgradeBuilding()
    {
        if (Level != 0)
        {
            ClearVillagers();
        }
        StartBuildOrUpgrade(Level);
        Level++;
    }

    public virtual void FinishConstruction()
    {
        State = BuildingState.Active;
        SmokeEffect();
        SetBuildingSprite();
        currentProgressBar.gameObject.SetActive(false);
        if (currentProgressBar != null)
        {
            Destroy(currentProgressBar.gameObject);
            currentProgressBar = null;
        }
        if (animator != null)
            animator.enabled = true;
        finishConstructionObj.SetActive(false);

        if(hammerObj != null)
        {
            Destroy(hammerObj);
            hammerObj = null;
        }

        if (finishConstructionObj != null)
        {
            Destroy(finishConstructionObj);
            finishConstructionObj = null;
        }
        buildingRegistryManager.buildingRegistryList[Index].SetActive(true);
    }

    public virtual void ClearVillagers()
    {
        if (BuildingName.StartsWith("House"))
        {
            if (villagerOne != null)
            {
                villagerManager.RemoveVillagerFromVillage(villagerOne.Index, villagerOne.isEmployed);
            }
            if (villagerTwo != null)
            {
                villagerManager.RemoveVillagerFromVillage(villagerTwo.Index, villagerTwo.isEmployed);
            }
            if (villagerThree != null)
            {
                villagerManager.RemoveVillagerFromVillage(villagerThree.Index, villagerThree.isEmployed);
            }
        }
        else
        {
            if (villagerOne != null)
            {
                villagerManager.RemoveVillagerFromBuilding(villagerOne.Index);
            }
            if (villagerTwo != null)
            {
                villagerManager.RemoveVillagerFromBuilding(villagerTwo.Index);
            }
            if (villagerThree != null)
            {
                villagerManager.RemoveVillagerFromBuilding(villagerThree.Index);
            }
        }
    }

    private void SmokeEffect()
    {
        Vector3 spawnPosition = GetSmokeEffectOffset();
        Destroy(Instantiate(smokeExplosionPrefab, spawnPosition, Quaternion.identity), 0.58f);
    }

    private Vector3 GetSmokeEffectOffset()
    {
        return width switch
        {
            3 => transform.position + new Vector3(-0.1f, -0.5f, 0f),
            4 => transform.position + new Vector3(-1.2f,0.25f,0f),
            _ => transform.position + new Vector3(-0.5f, -0.6f, 0f)
        };
    }

    private float GetDurationForLevel(int level)
    {
        return constructionDurationsPerLevel[Mathf.Clamp(level, 0, constructionDurationsPerLevel.Length - 1)];
    }

    public virtual void SetBuildingSprite()
    {
        if (!isAnimated && SpriteRenderer != null)
        {
            switch (Level)
            {
                case 0:
                case 1:
                    SpriteRenderer.sprite = BuildingLevelOne;
                    break;
                case 2:
                    SpriteRenderer.sprite = BuildingLevelTwo;
                    break;
                case 3:
                    SpriteRenderer.sprite = BuildingLevelThree;
                    break;
            }
            return;
        }

        if (animator == null)
            return;

        string[] animationNames = new string[]
        {
            "BuildingBaseAnimation",
            "BuildingLevelOneAnimation",
            "BuildingLevelTwoAnimation",
            "BuildingLevelThreeAnimation"
        };

        int layer = 0;
        string fallback = "BuildingLevelOneAnimation";
        string targetAnimation = Level == 0 ? animationNames[0] : animationNames[Mathf.Clamp(Level, 1, 3)];

        if (animator.HasState(layer, Animator.StringToHash(targetAnimation)))
            animator.Play(targetAnimation, layer, 1f);
        else if (animator.HasState(layer, Animator.StringToHash(fallback)))
            animator.Play(fallback, layer, 1f);
    }

    public virtual void AssignVillagerToSlot(int slot, Villager villager)
    {
        switch (slot)
        {
            case 1: villagerOne = villager; break;
            case 2: villagerTwo = villager; break;
            case 3: villagerThree = villager; break;
        }
    }

    public virtual void RemoveVillagerFromSlot(int slot)
    {
        switch (slot)
        {
            case 1: villagerOne = null; break;
            case 2: villagerTwo = null; break;
            case 3: villagerThree = null; break;
        }
    }

    public Villager GetVillagerInSlot(int slot)
    {
        return slot switch
        {
            1 => villagerOne,
            2 => villagerTwo,
            3 => villagerThree,
            _ => null,
        };
    }
}
