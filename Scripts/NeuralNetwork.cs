using System;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork
{
    private int[] layers;
    private float[][] neurons;
    private float[][][] weights;
    float glorot;

    public NeuralNetwork(int[] _layers)
    {
        layers = new int[_layers.Length];
        for (int i = 0; i < _layers.Length; i++)
            layers[i] = _layers[i];
        glorot = Mathf.Sqrt(6f / (layers[0] + layers[layers.Length - 1]));
        //glorot = 1f;
        InitNeurons();
        InitWeights();
    }

    private void InitNeurons()
    {
        List<float[]> neuronsList = new List<float[]>();

        for (int i = 0; i < layers.Length; i++)
            neuronsList.Add(new float[layers[i]]); //for every layer make a new list. the size of this list is the size of the layer denoted by layer[i]

        neurons = neuronsList.ToArray();

    }

    public int InitWeights()
    {
        List<float[][]> weightsList = new List<float[][]>();
        int noWeights = 0;

        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> layerWeightsList = new List<float[]>();

            int neuronsInPreviousLayer = layers[i - 1];

            for (int j = 0; j < neurons[i].Length; j++)
            {
                float[] neuronWeights = new float[neuronsInPreviousLayer];

                //set weights randomly from -He to He
                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    neuronWeights[k] = UnityEngine.Random.Range(-glorot, glorot);
                    noWeights++;
                }

                layerWeightsList.Add(neuronWeights);

            }

            weightsList.Add(layerWeightsList.ToArray());

        }

        weights = weightsList.ToArray();

        return noWeights;
    }


    public float[] FeedForward(float[] inputs)
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
                for (int k = 0; k < neurons[i-1].Length; k++)
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }
                neurons[i][j] = (float)Math.Tanh(value);
            }
        }

        return neurons[neurons.Length];
    }

    public int[] getLayers()
    {
        return layers;
    }

    public float[][] getNeurons()
    {
        return neurons;
    }

    public float[][][] getWeights()
    {
        return weights;
    }

}
