using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMasters : MonoBehaviour
{

    float[][][] walls = new float[][][]
    {
        new float[][] {
            new float[] { 1.25f, 1.25f },
            new float[] { 1.25f, 1.75f },
            new float[] { 1.25f, 2.25f },
            new float[] { 1.25f, 2.75f },
            new float[] { 1.75f, 2.75f },
            new float[] { 2.25f, 2.75f },
            new float[] { 2.75f, 2.75f },
            new float[] { 3.25f, 2.75f }
        },
        new float[][] {
            new float[] { 3f, 1f },
            new float[] { 2f, 2f }
        }
    };

    float[][][] fencesV = new float[][][]
    {
        new float[][] {
            new float[] { 2.25f, -2f }
        },
        new float[][] {
            new float[] { 3f, 1f },
            new float[] { 2f, 2f }
        }
    };

    float[][][] fencesH = new float[][][]
    {
        new float[][] {
            new float[] { 2.5f, -1.75f },
            new float[] { 3f, -1.75f }
        },
        new float[][] {
            new float[] { 3f, 1f },
            new float[] { 2f, 2f }
        }
    };

    string[][] gridMap = new string[40][];


    float glorot;
    float timer = 0;
    float maxTime = 30;
    float startTimer = 1;
    float endTimer;
    Timer timeText;
    GameObject[] players = new GameObject[4];
    Aim[][] aimbars = new Aim[4][];
    BotController[][] playersA;
    Character[][] playersB;
    [SerializeField] LobbySO lobbySelection;
    [SerializeField] NetworkSO neuralNetworkSO;
    [SerializeField] NetworkSO trainingNetworkSO;
    [SerializeField] Vector2[] spawnPoints1;
    [SerializeField] Vector2[] spawnPoints2;
    [SerializeField] string mode;
    [SerializeField] int trials;
    [SerializeField] int[] winners = new int[50];
    [SerializeField] int trainingLevel;
    [SerializeField] bool changeTrainingLevel;
    public GameObject playerPrefab;
    public GameObject wall;
    public GameObject fenceV;
    public GameObject fenceH;

    PathFinder pathFinder;

    string path;


    void Awake()
    {
        path = Path.Combine(Application.dataPath, "Brains");
        if (changeTrainingLevel)
        {
            trainingNetworkSO.Weights = ReadWeightsFromFile("/weights " + trainingLevel);
            trainingNetworkSO.FeedbackWeights = ReadFeedbackFromFile("/feedback " + trainingLevel);
        }
        playersA = new BotController[neuralNetworkSO.GeneSample][];
        playersB = new Character[neuralNetworkSO.GeneSample][];
        glorot = 2 * Mathf.Sqrt(6 / (neuralNetworkSO.Layers[0] + neuralNetworkSO.Layers[neuralNetworkSO.Layers.Length - 1]));
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
                aimbars[i] = new Aim[2];
                foreach (Aim aimbar in players[i].GetComponentsInChildren<Aim>())
                {
                    aimbars[i][aimbar.GetPiece()] = aimbar;
                }
                aimbars[i][0].setPlayerNum(i + 1);
                aimbars[i][1].setPlayerNum(i + 1);
                foreach (Bullet bullet in players[i].GetComponentsInChildren<Bullet>(true))
                {
                    bullet.transform.position = new Vector3(bullet.transform.position.x, bullet.transform.position.y, j);
                    bullet.setZ(j);
                    bullet.gameObject.layer = 8;
                    bullet.setShooter(playersB[j][i]);
                }
            }
        }
        CreateMap(true);
    }
    

    // Start is called before the first frame update
    void Start()
    {
        timeText = FindObjectOfType<Timer>();
        pathFinder = FindObjectOfType<PathFinder>();
        int mapNum = lobbySelection.Map;
        pathFinder.ResetMap(walls[mapNum]);
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

    void CreateMap(bool mirror)
    {
        for (int i = 0; i < gridMap.Length; i++)
        {
            gridMap[i] = new string[18];
            for (int j = 0; j < gridMap[i].Length; j++)
            {
                gridMap[i][j] = "O";
            }
        }
        foreach (float[] point in walls[lobbySelection.Map])
        {
            Instantiate(wall, new Vector3(point[0], point[1], 0), Quaternion.identity);
            gridMap[(int)(2 * point[0] + 19.5f)][(int)(2 * point[1] + 8.5f)] = "W";
            if (mirror)
            {
                Instantiate(wall, new Vector3(-point[0], -point[1], 0), Quaternion.identity);
                gridMap[(int)(-2 * point[0] + 19.5f)][(int)(-2 * point[1] + 8.5f)] = "W";
            }
        }
        foreach (float[] point in fencesV[lobbySelection.Map])
        {
            Instantiate(fenceV, new Vector3(point[0], point[1], 0), Quaternion.identity);
            gridMap[(int)(2 * point[0] + 19.5f)][(int)(2 * point[1] + 8f)] = "F";
            gridMap[(int)(2 * point[0] + 19.5f)][(int)(2 * point[1] + 9f)] = "F";
            if (mirror)
            {
                Instantiate(fenceV, new Vector3(-point[0], -point[1], 0), Quaternion.identity);
                gridMap[(int)(-2 * point[0] + 19.5f)][(int)(-2 * point[1] + 8f)] = "F";
                gridMap[(int)(-2 * point[0] + 19.5f)][(int)(-2 * point[1] + 9f)] = "F";
            }
        }
        foreach (float[] point in fencesH[lobbySelection.Map])
        {
            Instantiate(fenceH, new Vector3(point[0], point[1], 0), Quaternion.identity);
            gridMap[(int)(2 * point[0] + 19f)][(int)(2 * point[1] + 8.5f)] = "F";
            gridMap[(int)(2 * point[0] + 20f)][(int)(2 * point[1] + 8.5f)] = "F";
            if (mirror)
            {
                Instantiate(fenceH, new Vector3(-point[0], -point[1], 0), Quaternion.identity);
                gridMap[(int)(-2 * point[0] + 19f)][(int)(-2 * point[1] + 8.5f)] = "F";
                gridMap[(int)(-2 * point[0] + 20f)][(int)(-2 * point[1] + 8.5f)] = "F";
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
                if (neuralNetworkSO.GenNo % 100 == 0)
                {
                    WriteWeightsToFile(neuralNetworkSO.Weights, "\\weights " + neuralNetworkSO.GenNo / 100);
                    WriteFeedbackToFile(neuralNetworkSO.FeedbackWeights, "\\feedback " + neuralNetworkSO.GenNo / 100);

                    trainingNetworkSO.Weights = ReadWeightsFromFile("weights " + neuralNetworkSO.GenNo / 100);
                    trainingNetworkSO.FeedbackWeights = ReadFeedbackFromFile("feedback " + neuralNetworkSO.GenNo / 100);
                }
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
                    neuralNetworkSO.Scores[j] += 10000 * scoreSum[0][i] + 1 * scoreSum[1][i] - 100 * scoreSum[2][i] - 100 * scoreSum[3][i];
                    print("player " + (i + 1) + "'s stats: " + scoreSum[0][i] + " KILLS, " + scoreSum[1][i] + " DAMAGE DEALT, " + scoreSum[2][i] + " DEATHS, " + scoreSum[3][i] + " AREA COVERAGE");
                    //print("player " + (i + 1) + "'s AREA COVERAGE: " + scoreSum[3][i]);
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

    public void WriteWeightsToFile(float[][][][] weights, string fileName)
    {
        string filePath = path + fileName + ".txt";
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        string line = string.Join(",", System.Array.ConvertAll(weights[i][j][k], x => x.ToString()));
                        writer.WriteLine(line);
                    }
                    writer.WriteLine("---");
                }
                writer.WriteLine("===");
            }
        }
    }

    public float[][][][] ReadWeightsFromFile(string fileName)
    {
        string filePath = path + fileName + ".txt";
        List<float[][][]> weightsList = new List<float[][][]>();
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null && line != "===")
            {
                List<float[][]> temp1 = new List<float[][]>();
                while (line != "---" && line != "===")
                {
                    List<float[]> temp2 = new List<float[]>();
                    while (!string.IsNullOrEmpty(line) && line != "---" && line != "===")
                    {
                        float[] floatArray = System.Array.ConvertAll(line.Split(','), float.Parse);
                        temp2.Add(floatArray);
                        line = reader.ReadLine();
                    }
                    temp1.Add(temp2.ToArray());
                    if (line != "===") line = reader.ReadLine();
                }
                weightsList.Add(temp1.ToArray());
            }
        }

        return weightsList.ToArray();
    }

    public void WriteFeedbackToFile(float[][] feedback, string fileName)
    {
        string filePath = path + fileName + ".txt";
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            for (int i = 0; i < 3; i++)
            {
                string line = string.Join(",", System.Array.ConvertAll(feedback[i], x => x.ToString()));
                writer.WriteLine(line);
            }
        }
    }

    public float[][] ReadFeedbackFromFile(string fileName)
    {
        string filePath = path + fileName + ".txt";
        List<float[]> feedbackList = new List<float[]>();
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line = reader.ReadLine();
            while (line != null)
            {
                float[] floatArray = System.Array.ConvertAll(line.Split(','), float.Parse);
                feedbackList.Add(floatArray);
                line = reader.ReadLine();
            }
        }

        return feedbackList.ToArray();
    }

}