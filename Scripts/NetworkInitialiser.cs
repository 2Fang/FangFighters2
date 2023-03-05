using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkInitialiser : MonoBehaviour
{

    [SerializeField] NetworkSO neuralNetworkSO;
    // Start is called before the first frame update
    void Awake()
    {
        NeuralNetwork network = new NeuralNetwork(new int[] { 64, 100, 100, 5 });
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
        neuralNetworkSO.WeightTemplate = randomweights;
        //neuralNetworkSO.Weights = randomweights;

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void initialiseNeuralNetwork()
    {
        NeuralNetwork network = new NeuralNetwork(new int[] { 4, 8, 8, 2});
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
