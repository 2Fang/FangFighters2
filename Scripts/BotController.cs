using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour
{

    [SerializeField] int playerNum;
    [SerializeField] LobbySO lobbySelection;
    [SerializeField] NetworkSO neuralNetworkSO;


    int[] layers;
    float[][] neurons;
    float[][][][] weights;

    [SerializeField] float[] outputs = new float[5];

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
    int botMode;
    int sampleNo;

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
            if (player.transform.position.z == character.transform.position.z)
            {
                if (player.tag == character.tag)
                    noTeammates++;
                else
                    noEnemies++;
            }
        }
        teammates = new Character[noTeammates];
        enemies = new Character[noEnemies];
        enemyPositions = new Vector2[noEnemies];
        foreach (Character player in FindObjectsOfType<Character>())
        {
            if (player.transform.position.z == character.transform.position.z)
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
        botMode = lobbySelection.BotModes[playerNum - 1];
        sampleNo = (int)transform.position.z;

        layers = neuralNetworkSO.Layers;
        neurons = neuralNetworkSO.Neurons;
        weights = neuralNetworkSO.Weights;
        playerVariables = new float[neuralNetworkSO.Layers[0] / 4];
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
        {
            if (botMode == 0)
                ControlledBot();
            else
                NeuralBot(sampleNo);
        }


    }

    void ControlledBot()
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



    void NeuralBot(int networkNum)
    {
        networkInputs = new float[networkInputs.Length];
        GetPlayerVariables(character, 0);
        if (noTeammates > 0)
            GetPlayerVariables(teammates[0], 1);
        GetPlayerVariables(enemies[0], 2);
        if (noEnemies > 1)
            GetPlayerVariables(enemies[1], 3);

        botInputs = FeedForward(networkInputs, networkNum);
        character.SetMovement(new Vector2(botInputs[0], botInputs[1]));
        if (botInputs[4] > 0.5)
        {
            character.Attack(new Vector2(botInputs[2], botInputs[3]));
        }

    }

    void GetPlayerVariables(Character player, int playerVariablesOffset)
    {
        playerVariables = player.GatherInfo(character);
        for (int i = 0; i < playerVariables.Length; i++)
        {
            networkInputs[playerVariables.Length * playerVariablesOffset + i] = playerVariables[i];
        }
    }

    void LookForProjectiles()
    {
        float[][] directions = new float[2][];
        float[] contender;
        float[] temp = new float[2];
        float dir; float speed; float dis;
        Vector2 vel;
        for (int i = 0; i < directions.Length; i++)
        {
            directions[i] = new float[] { 1, 1000};
        }
        foreach (Bullet projectile in FindObjectsOfType<Bullet>())
        {
            if (projectile.isActiveAndEnabled)
            {
                if (projectile.getZ() == transform.position.z)
                {
                    vel = projectile.getVel();
                    dir = Mathf.Atan2(vel.y, vel.x);
                    speed = Mathf.Sqrt(Mathf.Pow(vel.x, 2) + Mathf.Pow(vel.y, 2));
                    dis = Mathf.Sqrt(Mathf.Pow(projectile.transform.position.x, 2) + Mathf.Pow(projectile.transform.position.y, 2));
                    contender = new float[] { dir, speed * dis };
                    for (int i = 0; i < directions.Length; i++)
                    {
                        if (contender[1] < directions[i][1])
                        {
                            temp[0] = contender[0];
                            temp[1] = contender[1];
                            contender[0] = directions[i][0];
                            contender[1] = directions[i][1];
                            directions[i][0] = temp[0];
                            directions[i][1] = temp[1];
                        }
                    }
                }
            }
        }
    }

    float[] FeedForward(float[] inputs, int networkNum)
    {
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }
        float[][] feedback = neuralNetworkSO.Feedback;

        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0f;
                if (i == 1)
                    value += feedback[networkNum][j] * neuralNetworkSO.FeedbackWeights[networkNum][j];
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    value += weights[networkNum][i - 1][j][k] * neurons[i - 1][k];
                }
                if (i == layers.Length - 1)
                    neurons[i][j] = (float)System.Math.Tanh(value);
                else
                    neurons[i][j] = Mathf.Max(0.1f * value, value);
                    //neurons[i][j] = (float)System.Math.Tanh(value / 10);
                if (i == 1)
                    feedback[networkNum][j] = neurons[i][j];
            }
        }
        neuralNetworkSO.Feedback = feedback;
        outputs = neurons[neurons.Length - 1];
        return neurons[neurons.Length - 1];
    }



    public void SetGameActive(bool state)
    {
        active = state;
    }

    public void setPlayerNum(int num)
    {
        playerNum = num;
    }

}
