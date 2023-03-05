using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{

    [SerializeField] int playerNum;
    [SerializeField] LobbySO lobbySelection;
    [SerializeField] NetworkSO neuralNetworkSO;


    int[] layers;
    float[][] neurons;
    float[][][][] weights;

    bool bot;
    Character character;
    Character[] enemies = new Character[2];
    Character[] teammates;
    int noTeammates = -1;
    int noEnemies = 0;
    Vector2[] enemyPositions;
    bool active;

    float[] playerVariables;
    float[] networkInputs;
    float[] botInputs = new float[5];

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
        layers = neuralNetworkSO.Layers;
        neurons = neuralNetworkSO.Neurons;
        weights = neuralNetworkSO.Weights;
        playerVariables = new float[neuralNetworkSO.Layers[0]/4];
        networkInputs = new float[neuralNetworkSO.Layers[0]];
    }

    // Update is called once per frame
    void FixedUpdate()
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
        networkInputs = new float[networkInputs.Length];
        GetPlayerVariables(character, 0);
        if (noTeammates > 0)
            GetPlayerVariables(teammates[0], 1);
        GetPlayerVariables(enemies[0], 2);
        if (noEnemies > 1)
            GetPlayerVariables(enemies[1], 3);
        botInputs = FeedForward(networkInputs, 0);
        character.SetMovement(new Vector2(botInputs[0], botInputs[1]));
        if (botInputs[4] > 0.5)
        {
            character.Attack(new Vector2(botInputs[2], botInputs[3]));
        }

    }

    void GetPlayerVariables(Character player, int playerVariablesOffset)
    {
        playerVariables = player.GatherInfo(character);
        print("playerVariables length: " + playerVariables.Length);
        for (int i = 0; i < playerVariables.Length; i++)
        {
            print(playerVariables.Length * playerVariablesOffset + i);
            networkInputs[playerVariables.Length * playerVariablesOffset + i] = playerVariables[i];
        }
    }

    float[] FeedForward(float[] inputs, int networkNum)
    {

        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }

        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0.25f;
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    value += weights[networkNum][i - 1][j][k] * neurons[i - 1][k];
                }
                neurons[i][j] = (float)System.Math.Tanh(value);
            }
        }

        return neurons[neurons.Length - 1];
    }


    public void SetGameActive(bool state)
    {
        active = state;
    }

}
