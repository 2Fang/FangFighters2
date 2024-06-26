using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DummySO : ScriptableObject
{
    [SerializeField] int[] layers;
    
    float[][] neurons;
    [SerializeField] float[] flattenedNeurons;

    float[][][][] weightTemplate;
    [SerializeField] float[] flattenedWeights;
    [SerializeField] int noWeights;
    [SerializeField] float[] flattenedFeedback;
    [SerializeField] float[] feedbackWeights;

    [SerializeField] int geneSample;
    [SerializeField] int[] scores;
    [SerializeField] int genNo;


    public int GenNo
    {
        get { return genNo; }
        set { genNo = value; }
    }

    public int GeneSample
    {
        get { return geneSample; }
    }

    public int NoWeights
    {
        get { return noWeights; }
        set { noWeights = value; }
    }

    public int[] Scores
    {
        get { return scores; }
        set { scores = value; }
    }

    public int[] Layers
    {
        get { return layers; }
        set
        {
            layers = value;
            int noNeurons = 0;
            for (int i = 0; i < layers.Length; i++)
            {
                noNeurons += layers[i];
            }
            flattenedNeurons = new float[noNeurons];
        }
    }

    public float[][] Neurons
    {
        get 
        {
            //create staggered array from flattened data
            float[][] result = new float[layers.Length][];
            int dataIndex = 0;
            for (int i = 0; i < layers.Length; i++)
            {
                result[i] = new float[layers[i]];
                for (int j = 0; j < layers[i]; j++)
                {
                    result[i][j] = flattenedNeurons[dataIndex];
                    dataIndex++;
                }
            }
            return result;
        }
        set
        {
            int dataIndex = 0;
            for (int i = 0; i < value.Length; i++)
            {
                for (int j = 0; j < value[i].Length; j++)
                {
                    flattenedNeurons[dataIndex] = value[i][j];
                    dataIndex++;
                }
            }
        }
    }

    public float[][][][] WeightTemplate
    {
        set
        {
            weightTemplate = new float[value.Length][][][];
            for (int i = 0; i < weightTemplate.Length; i++)
            {
                weightTemplate[i] = new float[value[i].Length][][];
                for (int j = 0; j < weightTemplate[i].Length; j++)
                {
                    weightTemplate[i][j] = new float[value[i][j].Length][];
                    for (int k = 0; k < weightTemplate[i][j].Length; k++)
                    {
                        weightTemplate[i][j][k] = new float[value[i][j][k].Length];
                    }
                }
            }
        }
    }

    public float[][][][] Weights
    {
        get
        {
            int dataIndex = 0;
            float[][][][] result = new float[3][][][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new float[weightTemplate[i].Length][][];
                for (int j = 0; j < result[i].Length; j++)
                {
                    result[i][j] = new float[weightTemplate[i][j].Length][];
                    for (int k = 0; k < result[i][j].Length; k++)
                    {
                        result[i][j][k] = new float[weightTemplate[i][j][k].Length];
                        for (int l = 0; l < result[i][j][k].Length; l++)
                        {
                            result[i][j][k][l] = flattenedWeights[dataIndex];
                            dataIndex++;
                        }
                    }
                }
            }
            return result;
        }
        set
        {
            int dataIndex = 0;
            flattenedWeights = new float[noWeights * geneSample];
            for (int i = 0; i < value.Length; i++)
            {
                for (int j = 0; j < value[i].Length; j++)
                {
                    for (int k = 0; k < value[i][j].Length; k++)
                    {
                        for (int l = 0; l < value[i][j][k].Length; l++)
                        {
                            flattenedWeights[dataIndex] = value[i][j][k][l];
                            dataIndex++;
                        }
                    }
                }
            }
        }
    }

    public float[][] Feedback
    {
        get
        {
            //create staggered array from flattened data
            float[][] result = new float[3][];
            int dataIndex = 0;
            for (int i = 0; i < geneSample; i++)
            {
                result[i] = new float[layers[1]];
                for (int j = 0; j < layers[1]; j++)
                {
                    result[i][j] = flattenedFeedback[dataIndex];
                    dataIndex++;
                }
            }
            return result;
        }
        set
        {
            flattenedFeedback = new float[value.Length * value[0].Length];
            int dataIndex = 0;
            for (int i = 0; i < value.Length; i++)
            {
                for (int j = 0; j < value[i].Length; j++)
                {
                    flattenedFeedback[dataIndex] = value[i][j];
                    dataIndex++;
                }
            }
        }
    }

    public float[][] FeedbackWeights
    {
        get
        {
            //create staggered array from flattened data
            float[][] result = new float[3][];
            int dataIndex = 0;
            for (int i = 0; i < geneSample; i++)
            {
                result[i] = new float[layers[1]];
                for (int j = 0; j < layers[1]; j++)
                {
                    result[i][j] = feedbackWeights[dataIndex];
                    dataIndex++;
                }
            }
            return result;
        }
        set
        {
            feedbackWeights = new float[value.Length * value[0].Length];
            int dataIndex = 0;
            for (int i = 0; i < value.Length; i++)
            {
                for (int j = 0; j < value[i].Length; j++)
                {
                    feedbackWeights[dataIndex] = value[i][j];
                    dataIndex++;
                }
            }
        }
    }
}
