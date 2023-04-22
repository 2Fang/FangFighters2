using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class LobbySO : ScriptableObject
{
    [SerializeField] private int[] selections;
    [SerializeField] private int[] team;
    [SerializeField] private int[] bots;
    [SerializeField] private int[] botModes;
    [SerializeField] private int map;
    [SerializeField] private int player;
    [SerializeField] private int trialNo;
    [SerializeField] private int batch;
    [SerializeField] private int batchSize;

    public int[] Selections
    {
        get { return selections; }
        set { selections = value; }
    }

    public int[] Team
    {
        get { return team; }
        set { team = value; }
    }

    public int[] Bots
    {
        get { return bots; }
        set { bots = value; }
    }

    public int[] BotModes
    {
        get { return botModes; }
        set { botModes = value; }
    }

    public int Map
    {
        get { return map; }
        set { map = value; }
    }

    public int Player
    {
        get { return player; }
        set { player = value; }
    }

    public int TrialNo
    {
        get { return trialNo; }
        set { trialNo = value; }
    }

    public int BatchNo
    {
        get { return batch; }
        set { batch = value; }
    }

    public int BatchSize
    {
        get { return batchSize; }
        set { batchSize = value; }
    }

}
