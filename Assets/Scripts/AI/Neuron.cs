using System.Collections;
using UnityEngine;

public class Neuron
{
    public float[] weights;
    public float val = 0f;

    public Neuron(int size)
    {
        weights = new float[size];
        for (int i = 0; i < size; i++)
        {
            weights[i] = Random.Range(-1f, 1f);
        }
    }
}
