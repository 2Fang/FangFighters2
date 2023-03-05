using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour
{
    float timer = 0;
    float maxTime = 120;
    float startTimer = 1;
    float endTimer;
    Timer timeText;
    AIController[] playersA;
    Character[] playersB;
    [SerializeField] LobbySO lobbySelection;
    [SerializeField] Vector2[] spawnPoints1;
    [SerializeField] Vector2[] spawnPoints2;

    // Start is called before the first frame update
    void Start()
    {
        timeText = FindObjectOfType<Timer>();
        playersA = FindObjectsOfType<AIController>();
        playersB = FindObjectsOfType<Character>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (startTimer > 0)
        {
            startTimer -= Time.deltaTime;
            if (startTimer <= 0)
            {
                Activate();
                RemoveNonPlayers();
                timer = maxTime;
            }
            return;
        }
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            timeText.SetTimer((int)timer);
            if (timer <= 0)
            {
                Deactivate();
                endTimer = 10;
            }
            return;
        }
        if (endTimer > 0)
        {
            endTimer -= Time.deltaTime;
            if (endTimer <= 0)
            {
                SceneManager.LoadScene("Menu");
            }
        }
    }

    void Activate()
    {
        for (int i = 0; i < 4; i++)
        {
            playersA[i].SetGameActive(true);
            playersB[i].SetGameActive(true);
            playersB[i].RespawnPos(spawnPoints1, spawnPoints2);
        }
    }

    void Deactivate()
    {

        for (int i = 0; i < 4; i++)
        {
            playersA[i].SetGameActive(false);
            playersB[i].SetGameActive(false);
        }
    }

    void RemoveNonPlayers()
    {
        for (int i = 0; i < 4; i++)
        {
            if (lobbySelection.Team[i] == 0)
            {
                playersA[i].SetGameActive(false);
                playersB[i].SetGameActive(false);
                playersB[i].Die();
            }
        }
    }

}
