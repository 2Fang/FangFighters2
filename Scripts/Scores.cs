using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scores : MonoBehaviour
{
    [SerializeField] int side;
    [SerializeField] LobbySO selection;
    Character[] players;
    string text;
    int[] playerScores;
    int[] scores = new int[4];
    Text scorebox;
    int[] teams;

    void Start()
    {
        teams = selection.Team;
        scorebox = GetComponent<Text>();
        players = FindObjectsOfType<Character>();
        for (int i = 0; i < 4; i++)
        {
            updateScore(i + 1, 0);
        }
    }

    public void updateScore(int player, int scored)
    {
        if (teams[player - 1] != side)
            return;
        scores[teams[player - 1]] += scored;
        text = "  " + scores[teams[player - 1]] + "  ";
        scorebox.text = text;
        players[player -1].raiseBounty();
    }

    void Update()
    {

    }

}
