using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    public static PathFinder Instance { get; private set; }

    private Dictionary<string, int[][]> paths = new Dictionary<string, int[][]>();
    float[][] bricks;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            string dataFolder = Path.Combine(Application.dataPath, "Data");
            string filePath = Path.Combine(dataFolder, "myData.txt");
            ReadDataFromFile(filePath);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void ReadDataFromFile(string filePath)
    {
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            string key = null;
            List<int[]> points = new List<int[]>();

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                if (line.StartsWith("key ="))
                {
                    if (key != null)
                    {
                        paths[key] = points.ToArray();
                        points = new List<int[]>();
                    }
                    key = line.Substring(line.IndexOf('=') + 1).Trim();
                }
                else if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                else
                {
                    string[] point = line.Split(',');

                    if (point.Length == 2)
                    {
                        int x = int.Parse(point[0].Trim());
                        int y = int.Parse(point[1].Trim());
                        points.Add(new int[] { x, y });
                    }
                }
            }

            if (key != null)
            {
                paths[key] = points.ToArray();
            }
        }
    }

    int[] ConvertToGrid(Vector2 point)
    {
        return new int[] { (int) (2 * point.x + 19.5f), (int) (2 * point.y + 8.5f) };
    }

    Vector2 ConvertToWorld(int[] point)
    {
        return new Vector2((point[0] - 19.5f) / 2, (point[1] - 8.5f) / 2);
    }

    string MakeKey(int[] start, int[] end)
    {
        return start[0] + ", " + start[1] + " -> " + end[0] + ", " + end[1];
    }

    public Vector2 BestPath(Vector2 a, Vector2 b)
    {
        int[] start = ConvertToGrid(a);
        int[] end = ConvertToGrid(b);
        if (start[0] == end[0] && start[1] == end[1])
            return b;
        string key = MakeKey(start, end);

        if (paths.ContainsKey(key))
        {
            int[][] path = paths[key];
            if (path.Length > 1)
            {
                return ConvertToWorld(path[1]);
            }
            else
            {
                return ConvertToWorld(path[0]);
            }
        } 
        else
        {
            return a;
        }
    }

    public void ResetMap(float[][] walls)
    {
        List<float[]> brickstacles = new List<float[]>();
        foreach (float[] obs in walls)
            if (!brickstacles.Contains(obs))
            {
                brickstacles.Add(obs);
            }
        bricks = brickstacles.ToArray();

    }

    public Vector2[] getWalls()
    {
        Vector2[] theBricks = new Vector2[bricks.Length];
        int i = 0;
        foreach (float[] brick in bricks)
        {
            theBricks[i] = new Vector2(brick[0], brick[1]);
            i++;
        }
        return theBricks;
    }


}