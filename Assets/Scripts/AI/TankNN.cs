using System.Collections;
using UnityEngine;

public class TankNN
{
    private Neuron[][] layers;

    public TankNN(params int[] sizes)
    {
        layers = new Neuron[sizes.Length][];
        for (int i = 0; i < sizes.Length; i++)
        {
            int nextSize = i < sizes.Length - 1 ? sizes[i+1] : 0;
            int layerSize = i < sizes.Length - 1 ? sizes[i] + 1 : sizes[i];
            layers[i] = new Neuron[layerSize];
            for (int j = 0; j < layerSize; j++)
            {
                layers[i][j] = new Neuron(nextSize);
                if (i < sizes.Length - 1 && j == layerSize - 1)
                {
                    layers[i][j].val = Random.Range(-1f, 1f);
                }
            }
        }
    }

    public int[] FeedInputs(float[] inputs)
    {
        int[] res = new int[layers[^1].Length];
        for (int i = 0; i < inputs.Length; i++)
        {
            layers[0][i].val = inputs[i];
        }
        for (int k = 1; k < layers.Length - 1; k++)
        {
            Neuron[] prevLayer = layers[k - 1];
            Neuron[] currLayer = layers[k];
            for (int i = 0; i < prevLayer.Length; i++)
            {
                for (int j = 0; j < currLayer.Length; j++)
                {
                    if (k < layers.Length - 1 && j < currLayer.Length - 1 || k == layers.Length - 1)
                    {
                        currLayer[j].val = 0;
                    }
                    currLayer[j].val += prevLayer[i].weights[j] * prevLayer[i].val;
                    if (k == layers.Length - 1)
                    {
                        res[j] = BinaryStepWithDeltaActivation(currLayer[j].val, 1e-3f);
                    }
                }
            }
        }
        return res;
    }

    internal void UpdateNN(TankNN nn)
    {
        layers = nn.layers;
    }

    public void Mutate(float lim)
    {
        int layer = Random.Range(0, layers.Length);
        for (int i = 0; i < layers[layer].Length; i++)
        {
            for (int j = 0; j < layers[layer][i].weights.Length; j++)
            {
                if (Random.value < 0.1)
                {
                    layers[layer][i].weights[j] += Random.Range(-lim, lim);
                }
            }
        }
    }

    internal float GetWeight(int i, int j, int k)
    {
        return layers[i][j].weights[k];
    }

    internal void SetWeightWithMutatePossibility(int i, int j, int k, float weight)
    {
        if (Random.value <= 0.01)
        {
            layers[i][j].weights[k] += Random.Range(-0.5f, 0.5f);
        }
        else
        {
            layers[i][j].weights[k] = weight;
        }
    }

    private int BinaryStepWithDeltaActivation(float v, float delta)
    {
        if (Mathf.Abs(v) <= delta)
        {
            return 0;
        }
        if (v > 0)
        {
            return 1;
        }
        if (v < 0)
        {
            return -1;
        }
        return 0;
    }

    public int GetLayersCount()
    {
        return layers.Length;
    }

    public int GetLayerNeuronsCount(int idx)
    {
        if (idx < 0 || idx >= layers.Length)
        {
            throw new System.Exception(string.Format("wrong idx {}", idx));
        }
        return layers[idx].Length;
    }

    public int GetNeuronWeightsCount(int idx)
    {
        if (idx < 0 || idx >= layers.Length)
        {
            throw new System.Exception(string.Format("wrong idx {}", idx));
        }
        return layers[idx][0].weights.Length;
    }
}
