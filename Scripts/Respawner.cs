using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawner : MonoBehaviour
{

    Character[] players;
    int size;
    float[] timers;
    float respawnTimer = 3f;
    //set respawn locations here and set it based off team


    // Start is called before the first frame update
    void Start()
    {
        players = FindObjectsOfType<Character>();
        size = players.Length;
        timers = new float[size];
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < size; i++)
        {
            if (!players[i].isActiveAndEnabled)
            {
                if (players[i].PlayerInGame())
                {
                    if (timers[i] <= 0)
                        timers[i] = respawnTimer;
                    timers[i] -= Time.deltaTime;
                    if (timers[i] <= 0)
                    {
                        timers[i] = 0;
                        players[i].gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    public Character[] GetCharacters()
    {
        return players;
    }

}
