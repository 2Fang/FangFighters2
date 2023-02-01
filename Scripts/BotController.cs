using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour
{

    [SerializeField] int playerNum;
    [SerializeField] LobbySO lobbySelection;

    bool bot;
    Character character;
    Character[] enemies = new Character[2];
    Character[] teammates;
    int noTeammates = -1;
    int noEnemies = 0;
    Vector2[] enemyPositions;
    Vector2 closest;
    float distance;
    float tempDistance;
    int change;
    bool active;
    // Start is called before the first frame update
    void Start()
    {
        if (lobbySelection.Bots[playerNum - 1] == 0)
            bot = false;
        else
            bot = true;
        if (!bot) return;
        int i = 0;
        int j = 0;
        character = GetComponentInChildren<Character>();
        foreach (Character player in FindObjectsOfType<Character>())
        {
            if (player.tag == character.tag)
                noTeammates++;
            else
                noEnemies++;
        }
        teammates = new Character[noTeammates];
        enemies = new Character[noEnemies];
        enemyPositions = new Vector2[noEnemies];
        foreach (Character player in FindObjectsOfType<Character>())
        {
            if (player.tag == character.tag)
            {
                if (player != character)
                {
                    teammates[i] = player;
                    i++;
                }
            }
            else
            {
                enemies[j] = player;
                j++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!bot) return;
        for (int i = 0; i < noEnemies; i++)
        {
            enemyPositions[i] = enemies[i].getPos();
        }
        if (active)
            Move();


    }

    void Move()
    {
        distance = 10000f;
        foreach (Vector2 enemyPos in enemyPositions)
        {
            tempDistance = Mathf.Sqrt(Mathf.Pow((character.getPos().x - enemyPos.x), 2) + Mathf.Pow((character.getPos().y - enemyPos.y), 2));
            if (tempDistance < distance)
            {
                distance = tempDistance;
                closest = enemyPos;
            }
        }
        if (distance > character.getRange() * 0.9)
            character.SetMovement((closest - character.getPos()).normalized);
        else if (distance < character.getRange() * 0.5)
            character.SetMovement((character.getPos() - closest).normalized);
        else
        {
            if (change > 0)
                change -= 1;
            else
            {
                character.SetMovement((new Vector2(-1f + 2 * Random.value, -1f + 2 * Random.value)).normalized);
                change = 100*(int)(Random.value * 5);
            }
            if ((int)(Random.value * 1000) == 5)
            {
                character.Attack((closest - character.getPos()).normalized);
            }
        }

        character.SetRotation();

    }

    public void SetGameActive(bool state)
    {
        active = state;
    }

}
