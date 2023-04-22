using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour
{

    [SerializeField] int playerNum;
    [SerializeField] LobbySO lobbySelection;
    [SerializeField] NetworkSO neuralNetworkSO;
    [SerializeField] NetworkSO trainingNetworkSO;


    int[] layers;
    float[][] neurons;
    float[][][][] weights;

    [SerializeField] float[] outputs = new float[6];
    [SerializeField] string decision_made;
    string[] decision_choices = new string[] { "Full Chase", "Strafe Chase", "Free Reign", "Hide", "Run Away", "Back To Base" };

    bool bot;
    Character character;
    Character[] enemies = new Character[2];
    Character[] teammates;
    int noTeammates = -1;
    int noEnemies = 0;
    Vector2[] enemyPositions;
    Vector2 closest;

    Vector2 strafe;
    float strafeChance = 0.05f;
    bool strafing;

    float distance;
    float tempDistance;
    int change;
    bool active;
    int botMode;
    int sampleNo;

    float[] playerVariables;
    float[] networkInputs;
    float[] botInputs = new float[5];

    int dummyBrain;

    PathFinder pathFinder;

    List<System.Action> options;

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
        dummyBrain = Random.Range(0, 2);
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

        layers = neuralNetworkSO.Layers;
        neurons = neuralNetworkSO.Neurons;
        weights = neuralNetworkSO.Weights;
        playerVariables = new float[neuralNetworkSO.Layers[0] / 4];
        networkInputs = new float[neuralNetworkSO.Layers[0]];

        pathFinder = FindObjectOfType<PathFinder>();
        options = new List<System.Action> { FullChase, StrafeChase, FreeReign, Hide, RunAway, BackToBase };

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
            else if (botMode == 1)
                NeuralBot(dummyBrain, 1);
            else
                NeuralBot(sampleNo, 2);
        }


    }

    void ControlledBot()
    {
        distance = 10000f;
        foreach (Vector2 enemyPos in enemyPositions)
        {
            tempDistance = Vector2.Distance(character.getPos(), enemyPos);
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
            if ((int)(Random.value * 100) == 5)
            {
                if ((closest - character.getPos()).magnitude > 1)
                    character.Attack((closest - character.getPos()).normalized * 0.75f);
                else
                    character.Attack((closest - character.getPos()) * 0.75f);
            }
        }

        character.SetRotation();

    }



    void NeuralBot(int networkNum, int mode)
    {
        networkInputs = new float[networkInputs.Length];
        GetPlayerVariables(character, 0);
        if (noTeammates > 0)
            GetPlayerVariables(teammates[0], 1);
        GetPlayerVariables(enemies[0], 2);
        if (noEnemies > 1)
            GetPlayerVariables(enemies[1], 3);
        networkInputs[60] = CharacterInSight(enemies[0]);
        if (noEnemies > 1)
            networkInputs[61] = CharacterInSight(enemies[1]);
        LookForProjectiles(2);
        if (mode == 1)
            botInputs = DummyFeedForward(networkInputs, dummyBrain);
        if (mode == 2)
            botInputs = FeedForward(networkInputs, networkNum);
        int decision = 0;
        float bestChoice = -10;
        for (int i = 0; i < 6; i++)
        {
            if (botInputs[i] > bestChoice)
            {
                decision = i;
                bestChoice = botInputs[i];
            }
        }
        decision_made = decision_choices[decision];
        options[decision]();
        if (botInputs[6] > botInputs[7])
        {
            character.Attack(new Vector2(botInputs[8], botInputs[9]));
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

    bool ProjectileInSight(Vector2 projectile, float distance, float width, float angle)
    {
        RaycastHit2D vision = Physics2D.BoxCast(projectile, new Vector2(distance, width), angle, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
        if (vision.collider == gameObject.GetComponent<Collider2D>())
            return true;
        return false;

        
    }

    float CharacterInSight(Character enemy)
    {
        RaycastHit2D vision = Physics2D.Raycast(enemy.getPos(), character.getPos(), Mathf.Sqrt((character.getPos() - enemy.getPos()).sqrMagnitude), LayerMask.GetMask("Wall"));
        if (vision.collider != null)
            return 1;
        vision = Physics2D.Raycast(enemy.getPos(), character.getPos(), Mathf.Sqrt((character.getPos() - enemy.getPos()).sqrMagnitude), LayerMask.GetMask("Fence"));
        if (vision.collider != null)
            return 0.5f;
        return 0;

    }
    void LookForProjectiles(int n)
    {
        bool[] inSight = new bool[n];
        Bullet[] closestProjectiles = new Bullet[n];
        Bullet tempB;
        Bullet contB;
        float[][] directions = new float[n][];
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
                    speed = Mathf.Sqrt(vel.sqrMagnitude);
                    dis = Mathf.Sqrt(Mathf.Pow(projectile.transform.position.x - transform.position.x, 2) + Mathf.Pow(projectile.transform.position.y - transform.position.y, 2));
                    contender = new float[] { dir, dis / speed };
                    contB = projectile;
                    for (int i = 0; i < directions.Length; i++)
                    {
                        if (contender[1] < directions[i][1])
                        {
                            temp[0] = contender[0];
                            temp[1] = contender[1];
                            tempB = contB;
                            contender[0] = directions[i][0];
                            contender[1] = directions[i][1];
                            contB = tempB;
                            directions[i][0] = temp[0];
                            directions[i][1] = temp[1];
                            closestProjectiles[i] = tempB;
                        }
                    }
                }
            }
        }
        for (int i = 0; i < closestProjectiles.Length; i++)
        {
            if (closestProjectiles[i] == null)
            {
                networkInputs[62 + 3 * i] = 0;
                networkInputs[63 + 3 * i] = 0;
                networkInputs[64 + 3 * i] = 0;
            }
            else
            {
                inSight[i] = ProjectileInSight(closestProjectiles[i].transform.position, closestProjectiles[i].getDistance(), closestProjectiles[i].getWidth(), directions[i][0]);
                networkInputs[62 + 3 * i] = directions[i][0] / Mathf.PI;
                networkInputs[63 + 3 * i] = directions[i][1] / 10;
                networkInputs[64 + 3 * i] = inSight[i] ? 1 : 0;
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
                    neurons[i][j] = Mathf.Max(0, value);
                    //neurons[i][j] = (float)System.Math.Tanh(value / 10);
                if (i == 1)
                    feedback[networkNum][j] = neurons[i][j];
            }
        }
        neuralNetworkSO.Feedback = feedback;
        outputs = neurons[neurons.Length - 1];
        return neurons[neurons.Length - 1];
    }

    float[] DummyFeedForward(float[] inputs, int networkNum)
    {
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }
        float[][] feedback = trainingNetworkSO.Feedback;

        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0f;
                if (i == 1)
                    value += feedback[networkNum][j] * trainingNetworkSO.FeedbackWeights[networkNum][j];
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    value += 0.5f * weights[networkNum][i - 1][j][k] * neurons[i - 1][k];
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
        trainingNetworkSO.Feedback = feedback;
        outputs = neurons[neurons.Length - 1];
        return neurons[neurons.Length - 1];
    }

    void FullChase()
    {
        distance = 10000f;
        int enemy = (botInputs[12] > botInputs[13]) ? 0 : 1;
        Vector2 destination = pathFinder.BestPath(character.getPos(), enemyPositions[enemy]);
        character.SetMovement((destination - character.getPos()).normalized);
    }

    void StrafeChase()
    {
        distance = 10000f;
        int enemy = (botInputs[12] > botInputs[13]) ? 0 : 1;
        Vector2 destination = pathFinder.BestPath(character.getPos(), enemyPositions[enemy]);
        Vector2 chase = (destination - character.getPos()).normalized;
        if (strafing)
        {
            if (Random.value < 3 * strafeChance)
            {
                strafing = false;
            }
            else
                chase = strafe.normalized;
        }
        else
        {
            if (Random.value < strafeChance)
            {
                strafing = true;
                if (Random.Range(1, 2) == 1)
                {
                    strafe = new Vector2(-chase.y, chase.x);
                }
                else
                {
                    strafe = new Vector2(chase.y, -chase.x);
                }

                strafe = strafe.normalized + new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));
                chase = strafe.normalized;
            }
        }
        character.SetMovement(chase);
    }

    void FreeReign()
    {
        if (botInputs[16] > 0)
            character.SetMovement(new Vector2(botInputs[10], botInputs[11]));
        else
            character.SetMovement(new Vector2(botInputs[10], botInputs[11]).normalized);
    }

    void Hide()
    {
        Vector2 enemyPos = enemyPositions[(botInputs[14] > botInputs[15]) ? 0 : 1];
        Vector2[] walls = pathFinder.getWalls();
        float[] nearest = new float[] { 10000f, 10000f, 10000f };
        int[] nearestIndex = new int[3];
        Vector2 hidingSpot;
        for (int i = 0; i < walls.Length; i++)
        {
            hidingSpot = walls[i] - (walls[i] - enemyPos).normalized;
            float[] temp = new float[2];
            for (int j = 0; j < nearest.Length; j++)
            {
                float[] contender = new float[] { (character.getPos() - hidingSpot).magnitude, (float)i };
                if (contender[0] < nearest[j])
                {
                    temp[0] = nearest[j];
                    temp[1] = nearestIndex[j];
                    nearest[j] = contender[0];
                    nearestIndex[j] = (int)contender[1];
                    contender[0] = temp[0];
                    contender[1] = temp[1];
                }
            }
        }
        Vector2[] hidingSpots = new Vector2[3];
        for (int i = 0; i < hidingSpots.Length; i++)
        {
            hidingSpots[i] = walls[nearestIndex[i]] - (walls[nearestIndex[i]] - enemyPos).normalized;
        }
        Vector2 destination = pathFinder.BestPath(character.getPos(), hidingSpots[Random.Range(0, nearest.Length)]);
        character.SetMovement((destination - character.getPos()).normalized);

    }

    void RunAway()
    {
        distance = 10000f;
        foreach (Vector2 enemyPos in enemyPositions)
        {
            tempDistance = Vector2.Distance(character.getPos(), enemyPos);
            if (tempDistance < distance)
            {
                distance = tempDistance;
                closest = enemyPos;
            }
        }
        character.SetMovement((character.getPos() - closest).normalized);
    }

    void BackToBase()
    {
        Vector2 midSpawn = (character.getSpawns()[0] + character.getSpawns()[1]) / 2;
        Vector2[] retreatPoints = new Vector2[] { midSpawn + Vector2.up * 3, midSpawn, midSpawn - Vector2.up * 3 };
        Vector2 retreatPoint = (botInputs[17] > 0.4) ? retreatPoints[0] : (botInputs[17] < -0.4) ? retreatPoints[2] : retreatPoints[1];
        Vector2 destination = pathFinder.BestPath(character.getPos(), retreatPoint);

        character.SetMovement((destination - character.getPos()).normalized);
        /*
        foreach (Vector2 safe in retreatPoints)
        {
            RaycastHit2D vision = Physics2D.Raycast(character.getPos(), safe - character.getPos(), Vector2.Distance(safe, character.getPos()));
            if (vision.collider == null)
            {
                home = true;
                if (Vector2.Distance(safe, character.getPos()) < distance)
                {
                    distance = Vector2.Distance(safe, character.getPos());
                    closest = safe;
                }
            }
            character.SetMovement((closest - character.getPos()).normalized);
        }
        if (!home)
        {
            distance = 10000f;
            foreach (Vector2 enemyPos in enemyPositions)
            {
                tempDistance = Vector2.Distance(character.getPos(), enemyPos);
                if (tempDistance < distance)
                {
                    distance = tempDistance;
                    closest = enemyPos;
                }
            }
            RaycastHit2D vision = Physics2D.Raycast(character.getPos(), closest - character.getPos(), Vector2.Distance(safe, character.getPos()));
            if (vision.collider == null)
                if ((2 * lobbySelection.Team[playerNum] - 3) * character.getPos().x - closest.x < 0)
            {
                path = pathFinder.ThetaSearch(character.getPos(), midSpawn);
                if (path.Length > 1)
                    closest = new Vector2(path[1][0], path[1][1]);
                else
                    closest = new Vector2(path[0][0], path[0][1]);
            }
        } */
    }


    public void setSampleNo(int s)
    {
        sampleNo = s;
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
