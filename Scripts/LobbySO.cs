using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class LobbySO : ScriptableObject
{
    [SerializeField] private int[] selections;
    [SerializeField] private int[] team;
    [SerializeField] private int[] bots;
    [SerializeField] private string map;
    [SerializeField] private int player;

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

    public string Map
    {
        get { return map; }
        set { map = value; }
    }

    public int Player
    {
        get { return player; }
        set { player = value; }
    }

}
