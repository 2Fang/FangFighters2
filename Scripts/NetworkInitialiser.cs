using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkInitialiser : MonoBehaviour
{

    [SerializeField] NetworkSO neuralNetworkSO;
    [SerializeField] bool restartWeights;
    float glorot;
    // Start is called before the first frame update
    void Awake()
    {
        NeuralNetwork network = new NeuralNetwork(new int[] { 68, 50, 5 });
        neuralNetworkSO.Layers = network.getLayers();
        neuralNetworkSO.Neurons = network.getNeurons();
        glorot = Mathf.Sqrt(6f / (neuralNetworkSO.Layers[0] + neuralNetworkSO.Layers[neuralNetworkSO.Layers.Length - 1]));
        //glorot = 1;
        print(glorot);
        float[][][][] randomweights = new float[neuralNetworkSO.GeneSample][][][];
        int noWeights = network.InitWeights();
        randomweights[0] = network.getWeights();
        neuralNetworkSO.NoWeights = noWeights;
        for (int i = 1; i < neuralNetworkSO.GeneSample; i++)
        {
            network.InitWeights();
            randomweights[i] = network.getWeights();
        }
        neuralNetworkSO.WeightTemplate = randomweights;
        if (restartWeights)
        {
            neuralNetworkSO.Weights = randomweights;
            float[][] feedback = new float[neuralNetworkSO.GeneSample][];
            for (int i = 0; i < neuralNetworkSO.GeneSample; i++)
            {
                feedback[i] = new float[neuralNetworkSO.Layers[1]];
            }
            neuralNetworkSO.Feedback = feedback;
            for (int i = 0; i < neuralNetworkSO.GeneSample; i++)
            {
                for (int j = 0; j < neuralNetworkSO.Layers[1]; j++)
                {
                    feedback[i][j] = glorot * Random.value;
                }
            }
            neuralNetworkSO.FeedbackWeights = feedback;
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void initialiseNeuralNetwork()
    {
        NeuralNetwork network = new NeuralNetwork(new int[] { 68, 50, 5 });
        neuralNetworkSO.Layers = network.getLayers();
        neuralNetworkSO.Neurons = network.getNeurons();
        float[][][][] randomweights = new float[neuralNetworkSO.GeneSample][][][];
        int noWeights = network.InitWeights();
        randomweights[0] = network.getWeights();
        neuralNetworkSO.NoWeights = noWeights;
        for (int i = 1; i < neuralNetworkSO.GeneSample; i++)
        {
            network.InitWeights();
            randomweights[i] = network.getWeights();
        }
        neuralNetworkSO.Weights = randomweights;
    }

}
