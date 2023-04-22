using System.Collections.Generic;
using UnityEngine;

public class ThetaStarSearch : MonoBehaviour
{

    Dictionary<string, float[][]> precomputed;
    float[][] map = new float[0][];
    float[][] bricks = new float[0][];

    private void Awake()
    {
        precomputed = new Dictionary<string, float[][]>();
    }

    bool CCW(float[] a, float[] b, float[] c)
    {
        return (c[1] - a[1]) * (b[0] - a[0]) > (b[1] - a[1]) * (c[0] - a[0]);
    }

    bool Intersects(float[] a, float[] b, float[] c, float[] d)
    {
        return CCW(a, c, d) != CCW(b, c, d) && CCW(a, b, c) != CCW(a, b, d);
    }

    bool PointInRect(float px, float py, float x, float y)
    {
        return x - 0.5 <= px && px <= x + 0.5 && y - 0.5 <= py && py <= y + 0.5;
    }

    bool LineOfSight(float[][] walls, float[]a, float[] b)
    {
        foreach (float[] w in walls)
        {
            if (PointInRect(a[0], a[1], w[0], w[1]) || PointInRect(b[0], b[1], w[0], w[1]))
                return false;
        }
        foreach (float[] w in walls)
        {
            float[][] wallCorners = new float[][]
            {
                new float[] { w[0] - 0.5f, w[1] - 0.5f},
                new float[] { w[0] - 0.5f, w[1] + 0.5f},
                new float[] { w[0] + 0.5f, w[1] - 0.5f},
                new float[] { w[0] + 0.5f, w[1] + 0.5f}
            };

            for (int i = 0; i < 3; i++)
            {
                if (Intersects(a, b, wallCorners[i], wallCorners[i + 1]))
                    return false;
            }
            if (Intersects(a, b, wallCorners[3], wallCorners[0]))
                return false;
            if (a[0] == w[0] && a[1] == w[1])
                return false;
        }
        return true;
    }

    float[][][] ExpandSuccessors(float[] node, float[] end, float[][] walls, Dictionary<string, float> nodeToOpen)
    {
        float nx; float ny;
        float g_cost; float h_cost;
        List<float[][]> successors = new List<float[][]>();
        for (int dx = -2; dx <= 2; dx++)
        {
            for (int dy = -2; dy <= 2; dy++)
            {
                if (!(dx == 0 && dy == 0))
                {
                    nx = node[0] + dx;
                    ny = node[1] + dy;
                    if (LineOfSight(walls, node, new float[] { nx, ny }))
                    {
                        g_cost = nodeToOpen.ContainsKey(node[0] + ", " + node[1]) ? nodeToOpen[node[0] + ", " + node[1]] : 0;
                        g_cost += Mathf.Sqrt(dx * dx + dy * dy);
                        h_cost = Mathf.Sqrt(Mathf.Pow(nx - end[0], 2) + Mathf.Pow(ny - end[1], 2));
                        successors.Add(new float[][] { new float[] { nx, ny }, new float[] { g_cost, h_cost } });
                    }
                }
            }
        }
        return successors.ToArray();
    }

    float[][] MinOfList(List<float[][]> openList)
    {
        float[][] minimum = openList[0];
        float minScore = 10000;
        for (int i = 0; i < openList.Count; i++)
        {
            if (openList[i][1][0] + openList[i][1][1] < minScore)
            {
                minScore = openList[i][1][0] + openList[i][1][1];
                minimum = openList[i];
            }
        }
        return minimum;
    }

    string ArrayToString(float[] theArray)
    {
        string theString = "";
        foreach (float elem in theArray)
        {
            theString += elem;
            theString += ", ";
        }
        return theString;
    }

    public string ArrayToString(float[][] theArray)
    {
        string theString = "";
        foreach (float[] subArray in theArray)
        {
            theString += ArrayToString(subArray);
            theString += "\n";
        }
        return theString;
    }

    float[][] ConvertBack(float[][] thePath)
    {
        float[][] converted = new float[thePath.Length][];
        for (int i = 0; i < thePath.Length; i++)
        {
            converted[i] = new float[] { (thePath[i][0] - 19.5f) / 2, (thePath[i][1] - 8.5f) / 2 };
        }
        return converted;
    }

    public float[][] ThetaSearch(Vector2 a, Vector2 b)
    {
        float[] start = new float[] { Mathf.Round(2 * a.x + 19.5f), Mathf.Round(2 * a.y + 8.5f) };
        float[] end = new float[] { Mathf.Round(2 * b.x + 19.5f), Mathf.Round(2 * b.y + 8.5f) };

        if (precomputed.ContainsKey(start[0] + ", " + start[1] + " -> " + end[0] + ", " + end[1]))
        {
            return ConvertBack(precomputed[start[0] + ", " + start[1] + " -> " + end[0] + ", " + end[1]]);
        }
        if (start[0] == end[0] && start[1] == end[1])
        {
            return new float[][] { start, end };
        }

        float[] temp;
        List<float[][]> openList = new List<float[][]>();
        openList.Add(new float[][] { start, new float[] { 0f, Vector2.Distance(a, b) } });
        List<float[]> closedList = new List<float[]>();
        Dictionary<string, float> nodeToOpen = new Dictionary<string, float>();
        nodeToOpen.Add(start[0] + ", " + start[1], 0);
        Dictionary<string, float[]> cameFrom = new Dictionary<string, float[]>();
        List<float[]> path = new List<float[]>();

        while (openList.Count > 0)
        {
            float[][] minimum = MinOfList(openList);
            float[] current = minimum[0];
            float g_cost = minimum[1][0];
            float h_cost = minimum[1][1];
            openList.Remove(minimum);

            if (current[0] == end[0] && current[1] == end[1])
            {
                path.Add(new float[] { current[0], current[1] });
                while (cameFrom.ContainsKey(current[0] + ", " + current[1]))
                {
                    temp = cameFrom[current[0] + ", " + current[1]];
                    current = new float[] { temp[0], temp[1] };
                    path.Insert(0, new float[] { current[0], current[1] });
                }
                for (int i = 0; i < path.Count - 1; i++)
                {
                    List<float[]> subPath = path.GetRange(i, path.Count - i);
                    precomputed[subPath[0][0] + ", " + subPath[0][1] + " -> " + end[0] + ", " + end[1]] = subPath.ToArray();
                    precomputed[end[0] + ", " + end[1] + " -> " + subPath[0][0] + ", " + subPath[0][1]] = subPath.ToArray();
                }
                return ConvertBack(path.ToArray());
            }
            closedList.Add(current);

            float[][][] successors = ExpandSuccessors(current, end, map, nodeToOpen);
            foreach (float[][] successor in successors)
            {
                if (!closedList.Contains(successor[0]))
                {
                    if (!nodeToOpen.ContainsKey(successor[0][0] + ", " + successor[0][1]) || successor[1][0] < nodeToOpen[successor[0][0] + ", " + successor[0][1]])
                    {
                        nodeToOpen[successor[0][0] + ", " + successor[0][1]] = successor[1][0];
                        temp = current;
                        if (cameFrom.ContainsKey(successor[0][0] + ", " + successor[0][1]))
                        {
                            cameFrom[successor[0][0] + ", " + successor[0][1]][0] = temp[0];
                            cameFrom[successor[0][0] + ", " + successor[0][1]][1] = temp[1];
                        }
                        else
                        {
                            cameFrom.Add(successor[0][0] + ", " + successor[0][1], new float[] { temp[0], temp[1] });
                        }
                        openList.Add(new float[][] { new float[] { successor[0][0], successor[0][1] }, new float[] { successor[1][0], successor[1][1] } });
                    }
                }
            }
        }
        return new float[][] { new float[] { a.x, a.y } };
    }

    public void ResetMap(float[][] walls, float[][] fencesV, float[][] fencesH)
    {
        List<float[]> obstacles = new List<float[]>();
        List<float[]> brickstacles = new List<float[]>();
        foreach (float[] obs in walls)
            if (!obstacles.Contains(obs))
            {
                obstacles.Add(obs);
                brickstacles.Add(obs);
            }
        foreach (float[] obs in fencesV)
            if (!obstacles.Contains(obs))
                obstacles.Add(obs);
        foreach (float[] obs in fencesH)
            if (!obstacles.Contains(obs))
                obstacles.Add(obs);
        map = obstacles.ToArray();
        bricks = brickstacles.ToArray();

    }

    public Vector2[] getWalls()
    {
        Vector2[] theBricks = new Vector2[bricks.Length];
        int i = 0;
        foreach (float[] brick in bricks)
        {
            theBricks[i] = new Vector2(brick[0], brick[1]);
        }
        return theBricks;
    }

}