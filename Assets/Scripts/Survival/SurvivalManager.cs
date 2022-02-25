using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BossCustomization;

public class SurvivalManager : MonoBehaviour
{
    public static SurvivalManager instance;
    public EnemyManager EManager;
    
    public float timeBeforeNextWave = 30f;
    private float timerStart = 0;
    public bool skipWait = false;

    public List<BossSettings> BossesInRotation;
    public List<GuardianSetup> GuardianSpawnlist;
    //public GameObject MiniBossReinforcements;
    public bool bossRound = false;

    float baseGrowth = 1;
    public float healthGrowthRate = 0.1f;

    public int currentWave = 0;
    public int baseAmountOfBosses = 1;
    public int bossIncrease = 1;
    public int increaseBossCountEveryXWave = 5;
    public int bossRoundEveryXWave = 3;

    public float spawnCapBase = 1;
    public int spawnCapGrowth = 10;

    public int totalSpawned = 0;
    public int baseEnemiesToSpawnEachRound = 50;
    public float enemyGrowth = 0.1f;

    public float surviveTimeLimit = 60f;
    private float survivalTimer = 0f;
    public bool roundOver = false;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        FirstRound();
    }

    private void Update()
    {

        if (roundOver == true)
        {
            if (timerStart <= 0 || skipWait == true)
            {
                currentWave++;
                skipWait = false;
                timerStart = 0;

                if (currentWave % bossRoundEveryXWave == 0)
                {
                    startLogicForNextWave();
                    startNextWave(true);
                }
                else
                {
                    startLogicForNextWave();
                    startNextWave(false);
                }
            }
            else
            {
                Debug.Log("GRACE PERIOD " + timerStart);
                timerStart -= Time.deltaTime;
            }
        }

        if (survivalTimer <= 0 && bossRound == false)
        {
            roundOver = true;
        }
        else if(bossRound == false)
        {
            Debug.Log("SURVIVAL TIMER " + survivalTimer);
            survivalTimer -= Time.deltaTime;
            
        }
    }

    public void FirstRound()
    {
        totalSpawned = 0;
        currentWave = 1;
        roundOver = false;
        survivalTimer = surviveTimeLimit;
        timerStart = timeBeforeNextWave;
    }

    public void updateGuardianList()
    {
        for (int i = 0; i < GuardianSpawnlist.Count; i++)
        {
            var go = GuardianSpawnlist[i];
            go.GuardianHP = (int)(baseGrowth * go.GuardianHP);
            /*
            Debug.Log("****Guardian HP = " + go.GuardianHP);
            Debug.Log("****Guardian Base Growth = " + baseGrowth);
            */
            GuardianSpawnlist[i] = go;
        }
    }


    // Will change this to update later once things are finalized
    public void startLogicForNextWave()
    {
        roundOver = false;
        timerStart = timeBeforeNextWave;
        survivalTimer = surviveTimeLimit;
    }

    int pickBoss = 0;
    private void startNextWave(bool isBossRound)
    {
        Debug.Log("WAVE " + currentWave + " STARTING");
        totalSpawned = 0;
        EManager.BossSettingList.Clear();
        baseGrowth += healthGrowthRate;
        if (currentWave % increaseBossCountEveryXWave == 0)
        {
            baseAmountOfBosses++;
        }
        if (isBossRound)
        {
            for (int i = 0; i < baseAmountOfBosses; i++)
            {
                pickBoss = Random.Range(0, BossesInRotation.Count);
                var go = BossesInRotation[pickBoss];
                go.BossHP = (int)(baseGrowth * go.BossHP);
                Debug.Log("NEW HP IS " + go.BossHP);

                EManager.BossSettingList.Add(go);

            }
        }
        bossRound = isBossRound;
        // spawnCapBase += spawnCapGrowth;
        baseEnemiesToSpawnEachRound += spawnCapGrowth; // (int)(baseEnemiesToSpawnEachRound * spawnCapBase);
        updateGuardianList();
        EManager.resetEnemyManager();
        
    }

    



}
