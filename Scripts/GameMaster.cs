using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour
{
    float glorot;
    float timer = 0;
    float maxTime = 30;
    float startTimer = 1;
    float endTimer;
    Timer timeText;
    GameObject[] players = new GameObject[4];
    Aim[] aimbars = new Aim[4];
    BotController[][] playersA;
    Character[][] playersB;
    [SerializeField] LobbySO lobbySelection;
    [SerializeField] NetworkSO neuralNetworkSO;
    [SerializeField] Vector2[] spawnPoints1;
    [SerializeField] Vector2[] spawnPoints2;
    [SerializeField] string mode;
    [SerializeField] int trials;
    [SerializeField] int[] winners = new int[100];
    public GameObject playerPrefab;

    
    void Awake()
    {
        playersA = new BotController[neuralNetworkSO.GeneSample][];
        playersB = new Character[neuralNetworkSO.GeneSample][];
        glorot = (float)Mathf.Sqrt(6 / (neuralNetworkSO.Layers[0] + neuralNetworkSO.Layers[neuralNetworkSO.Layers.Length - 1]));
        for (int j = 0; j < neuralNetworkSO.GeneSample; j++)
        {
            playersA[j] = new BotController[4];
            playersB[j] = new Character[4];
            for (int i = 0; i < 4; i++)
            {
                players[i] = Instantiate(playerPrefab, new Vector3(-20, -15, j), Quaternion.identity);
                players[i].layer = 6;
                players[i].tag = "team" + lobbySelection.Team[i];
                playersA[j][i] = players[i].GetComponent<BotController>();
                playersB[j][i] = players[i].GetComponent<Character>();
                playersA[j][i].setPlayerNum(i + 1);
                playersB[j][i].setPlayerNum(i + 1);
                aimbars[i] = players[i].GetComponentInChildren<Aim>();
                aimbars[i].setPlayerNum(i + 1);
                foreach (Bullet bullet in players[i].GetComponentsInChildren<Bullet>())
                {
                    bullet.transform.position = new Vector3(bullet.transform.position.x, bullet.transform.position.y, j);
                    bullet.setZ(j);
                }
            }
        }
    }
    

    // Start is called before the first frame update
    void Start()
    {
        timeText = FindObjectOfType<Timer>();
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
                endTimer = 1;
            }
            return;
        }
        if (endTimer > 0)
        {
            endTimer -= Time.deltaTime;
            if (endTimer <= 0)
            {
                EndGame();
            }
        }
    }

    void Activate()
    {
        for (int j = 0; j < playersA.Length; j++)
        {
            for (int i = 0; i < 4; i++)
            {
                playersA[j][i].SetGameActive(true);
                playersB[j][i].SetGameActive(true);
                playersB[j][i].RespawnPos(spawnPoints1, spawnPoints2);
            }
        }
    }

    void Deactivate()
    {

        for (int j = 0; j < playersA.Length; j++)
        {
            for (int i = 0; i < 4; i++)
            {
                playersA[j][i].SetGameActive(false);
                playersB[j][i].SetGameActive(false);
            }
        }
    }

    void RemoveNonPlayers()
    {
        for (int j = 0; j < playersA.Length; j++)
        {
            for (int i = 0; i < 4; i++)
            {
                if (lobbySelection.Team[i] == 0)
                {
                    playersA[j][i].SetGameActive(false);
                    playersB[j][i].SetGameActive(false);
                    playersB[j][i].Die();
                }
            }
        }
    }

    void EndGame()
    {
        if (mode == "Normal")
            SceneManager.LoadScene("Menu");
        if (mode == "Training")
        {
            AddScores();
            int temp = lobbySelection.BotModes[0];
            for (int i = 0; i < 3; i++)
            {
                lobbySelection.BotModes[i] = lobbySelection.BotModes[i + 1];
                lobbySelection.Selections[i] = (int)(Random.value * 4);
            }
            lobbySelection.BotModes[3] = temp;
            lobbySelection.TrialNo++;
            if (lobbySelection.TrialNo >= trials)
            {
                neuralNetworkSO.GenNo++;
                TrainNetwork();
                lobbySelection.TrialNo = 0;
            }
            SceneManager.LoadScene("Battle - Copy");
        }
    }


    void AddScores()
    {
        int[][] scores;
        for (int j = 0; j < neuralNetworkSO.GeneSample; j++)
        {
            int[][] scoreSum = new int[][] { new int[4], new int[4], new int[4], new int[4] };
            for (int i = 0; i < 4; i++)
            {
                scores = playersB[j][i].getScores();
                for (int k = 0; k < 4; k++)
                {
                    scoreSum[0][k] += scores[0][k];
                }
                for (int k = 0; k < 4; k++)
                {
                    scoreSum[1][k] += scores[1][k];
                }
                scoreSum[2][i] = scores[2][0];
                scoreSum[3][i] = scores[3][0];
            }
            for (int i = 0; i < 4; i++)
            {
                if (lobbySelection.BotModes[i] != 0)
                {
                    neuralNetworkSO.Scores[j] += 0 * scoreSum[0][i] + 0 * scoreSum[1][i] - 0 * scoreSum[2][i] - 10 * scoreSum[3][i];
                    //print("player " + (i + 1) + "'s stats: " + scoreSum[0][i] + " KILLS, " + scoreSum[1][i] + " DAMAGE DEALT, " + scoreSum[2][i] + " DEATHS, " + scoreSum[3][i] + " AREA COVERAGE");
                    print("player " + (i + 1) + "'s AREA COVERAGE: " + scoreSum[3][i]);
                }
            }
        }
    }

    void TrainNetwork()
    {
        float mean1 = (int)(neuralNetworkSO.Layers[1] / 2);
        float sd1 = (int)(neuralNetworkSO.Layers[1] / 10);
        float mean2 = (int)(neuralNetworkSO.NoWeights / 2);
        float sd2 = (int)(neuralNetworkSO.NoWeights / 10);
        int[] scores = neuralNetworkSO.Scores;
        int[] networkIndexes = new int[scores.Length];
        for (int i = 0; i < networkIndexes.Length; i++)
        {
            networkIndexes[i] = i;
        }

        //THIS IS A BUBBLE SORT MAKE IT A MERGE SORT ASAP
        for (int i = 0; i < scores.Length - 1; i++)
        {
            for (int j = 0; j < scores.Length - i - 1; j++)
            {
                if (scores[j] < scores[j + 1])
                {
                    int temp = scores[j];
                    scores[j] = scores[j + 1];
                    scores[j + 1] = temp;
                    int temp2 = networkIndexes[j];
                    networkIndexes[j] = networkIndexes[j + 1];
                    networkIndexes[j + 1] = temp2;
                }
            }
        }



        float[][][][] oldWeights = neuralNetworkSO.Weights;
        float[][][][] newWeights = new float[neuralNetworkSO.GeneSample][][][];
        float[][] oldFeedback = neuralNetworkSO.FeedbackWeights;
        float[][] newFeedback = new float[neuralNetworkSO.GeneSample][];
        for (int i = 0; i < newWeights.Length; i++)
        {
            newWeights[i] = new float[oldWeights[i].Length][][];
            newFeedback[i] = new float[neuralNetworkSO.Layers[1]];
            for (int j = 0; j < newWeights[i].Length; j++)
            {
                newWeights[i][j] = new float[oldWeights[i][j].Length][];
                for (int k = 0; k < newWeights[i][j].Length; k++)
                {
                    newWeights[i][j][k] = new float[oldWeights[i][j][k].Length];
                }
            }
        }

        int top10percent = (int)(neuralNetworkSO.GeneSample * 0.1);
        // original 10%
        for (int i = 0; i < top10percent; i++)
        {
            for (int x = 0; x < newFeedback[networkIndexes[i]].Length; x++)
            {
                newFeedback[i][x] = oldFeedback[i][x];
            }
            for (int j = 0; j < newWeights[i].Length; j++)
            {
                for (int k = 0; k < newWeights[i][j].Length; k++)
                {
                    for (int l = 0; l < newWeights[i][j][k].Length; l++)
                    {
                        newWeights[i][j][k][l] = oldWeights[networkIndexes[i]][j][k][l];
                    }
                }
            }
        }
        // original 10% guassian mutation
        for (int i = 0; i < top10percent; i++)
        {
            for (int x = 0; x < newFeedback[i].Length; x++)
            {
                newFeedback[i + top10percent][x] = oldFeedback[networkIndexes[i]][x] * Random.Range(0.9f, 1.1f);
            }
            for (int j = 0; j < newWeights[i].Length; j++)
            {
                for (int k = 0; k < newWeights[i][j].Length; k++)
                {
                    for (int l = 0; l < newWeights[i][j][k].Length; l++)
                    {
                        newWeights[i + top10percent][j][k][l] = oldWeights[networkIndexes[i]][j][k][l] * Random.Range(0.9f, 1.1f);
                    }
                }
            }
        }
        // original 10% standard mutation
        for (int i = 0; i < top10percent; i++)
        {
            for (int x = 0; x < newFeedback[i].Length; x++)
            {
                if (Random.value > 0.02)
                    newFeedback[i + 2 * top10percent][x] = oldFeedback[networkIndexes[i]][x];
                else
                    newFeedback[i + 2 * top10percent][x] = glorot * Random.value;
            }
            for (int j = 0; j < newWeights[i].Length; j++)
            {
                for (int k = 0; k < newWeights[i][j].Length; k++)
                {
                    for (int l = 0; l < newWeights[i][j][k].Length; l++)
                    {
                        if (Random.value > 0.02)
                            newWeights[i + 2 * top10percent][j][k][l] = oldWeights[networkIndexes[i]][j][k][l];
                        else
                            newWeights[i + 2 * top10percent][j][k][l] = Random.Range(-glorot, glorot);
                    }
                }
            }
        }
        // crossover of 2 parents in top 10%
        for (int i = 3 * top10percent; i < 5 * top10percent; i++)
        {
            int[] parents = new int[] {(int)(Mathf.Pow(Random.value, 2) * top10percent), (int)(Mathf.Pow(Random.value, 2) * top10percent) };
            int parent = 0;
            float variance = (Mathf.Sqrt(-2 * Mathf.Log(Random.value)) * Mathf.Sin(2 * Mathf.PI * Random.value));
            int crossover1 = (int)(mean1 + sd1 * variance);
            int crossover2 = (int)(mean2 + sd2 * variance);
            int count = 0;
            for (int x = 0; x < newFeedback[networkIndexes[i]].Length; x++)
            {
                newFeedback[i][x] = oldFeedback[networkIndexes[parents[parent]]][x];
                if (count == crossover1)
                    parent = 1;
                else
                    count += 1;
            }
            count = 0;
            parent = 0;
            for (int j = 0; j < newWeights[i].Length; j++)
            {
                for (int k = 0; k < newWeights[i][j].Length; k++)
                {
                    for (int l = 0; l < newWeights[i][j][k].Length; l++)
                    {
                        newWeights[i][j][k][l] = oldWeights[networkIndexes[parents[parent]]][j][k][l];
                        if (count == crossover2)
                            parent = 1;
                        else
                            count += 1;
                    }
                }
            }
        }
        // crossover of 2 parents in top 10% gaussian mutation
        for (int i = 5 * top10percent; i < 6 * top10percent; i++)
        {
            int[] parents = new int[] { (int)(Mathf.Pow(Random.value, 2) * top10percent), (int)(Mathf.Pow(Random.value, 2) * top10percent) };
            int parent = 0;
            float variance = (Mathf.Sqrt(-2 * Mathf.Log(Random.value)) * Mathf.Sin(2 * Mathf.PI * Random.value));
            int crossover1 = (int)(mean1 + sd1 * variance);
            int crossover2 = (int)(mean2 + sd2 * variance);
            int count = 0;
            for (int x = 0; x < newFeedback[networkIndexes[i]].Length; x++)
            {
                newFeedback[i][x] = oldFeedback[networkIndexes[parents[parent]]][x] * Random.Range(0.9f, 1.1f);
                if (count == crossover1)
                    parent = 1;
                else
                    count += 1;
            }
            count = 0;
            parent = 0;
            for (int j = 0; j < newWeights[i].Length; j++)
            {
                for (int k = 0; k < newWeights[i][j].Length; k++)
                {
                    for (int l = 0; l < newWeights[i][j][k].Length; l++)
                    {
                        newWeights[i][j][k][l] = oldWeights[networkIndexes[parents[parent]]][j][k][l] * Random.Range(0.9f, 1.1f);
                        if (count == crossover2)
                            parent = 1;
                        else
                            count += 1;
                    }
                }
            }
        }
        // crossover of 2 parents in top 10% standard mutation
        for (int i = 6 * top10percent; i < 7 * top10percent; i++)
        {
            int[] parents = new int[] { (int)(Mathf.Pow(Random.value, 2) * top10percent), (int)(Mathf.Pow(Random.value, 2) * top10percent) };
            int parent = 0;
            float variance = (Mathf.Sqrt(-2 * Mathf.Log(Random.value)) * Mathf.Sin(2 * Mathf.PI * Random.value));
            int crossover1 = (int)(mean1 + sd1 * variance);
            int crossover2 = (int)(mean2 + sd2 * variance);
            int count = 0;
            for (int x = 0; x < newFeedback[networkIndexes[i]].Length; x++)
            {
                if (Random.value > 0.02)
                    newFeedback[i][x] = oldFeedback[networkIndexes[parents[parent]]][x];
                else
                    newFeedback[i][x] = glorot * Random.value;
                if (count == crossover1)
                    parent = 1;
                else
                    count += 1;
            }
            count = 0;
            parent = 0;
            for (int j = 0; j < newWeights[i].Length; j++)
            {
                for (int k = 0; k < newWeights[i][j].Length; k++)
                {
                    for (int l = 0; l < newWeights[i][j][k].Length; l++)
                    {
                        if (Random.value > 0.02)
                            newWeights[i][j][k][l] = oldWeights[networkIndexes[parents[parent]]][j][k][l];
                        else
                            newWeights[i][j][k][l] = Random.Range(-glorot, glorot);
                        if (count == crossover2)
                            parent = 1;
                        else
                            count += 1;
                    }
                }
            }
        }
        // new random samples
        for (int i = 7 * top10percent; i < newWeights.Length; i++)
        {
            for (int x = 0; x < newFeedback[i].Length; x++)
            {
                newFeedback[i][x] = glorot * Random.value;
            }
            for (int j = 0; j < newWeights[i].Length; j++)
            {
                for (int k = 0; k < newWeights[i][j].Length; k++)
                {
                    for (int l = 0; l < newWeights[i][j][k].Length; l++)
                    {
                        newWeights[i][j][k][l] = Random.Range(-glorot, glorot);
                    }
                }
            }
        }

        neuralNetworkSO.Weights = newWeights;
        neuralNetworkSO.FeedbackWeights = newFeedback;
        neuralNetworkSO.Scores = new int[neuralNetworkSO.Scores.Length];
    }

}