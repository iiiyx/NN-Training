using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    struct Tank
    {
        public TankNN nn;
        public float reward;
        public TankNNAgent agent;
    }

    private Terrain[] terrains;
    private List<Tank> tanks;
    private int rangeSize;
    private int size;
    private int i = 0;

    // Start is called before the first frame update
    void Start()
    {
        terrains = Terrain.activeTerrains;
        size = terrains.Length;
        tanks = new List<Tank>(size);
        rangeSize = size / 5;
    }

    internal void OnAgentEpisodeEnd(TankNNAgent agent, TankNN nn, float reward)
    {
        Tank t = tanks[i++];
        t.nn = nn;
        t.reward = reward;
        t.agent = agent;
        if (i == size)
        {
            List<Tank> ts = CreateNewPopulation();
            for (int i = 0; i < size; i++)
            {
                t.nn.UpdateNN(ts[i].nn);
                t.agent.OnEpisodeBegin();
            }
        }
    }

    private List<Tank> CreateNewPopulation()
    {
        List<Tank> selected = ConductSelection();
        List<Tank> newPop = ConductCrossing(selected);
        return newPop;
    }

    private List<Tank> ConductCrossing(List<Tank> selected)
    {
        HashSet<Tank> pair = new HashSet<Tank>(2);
        int lastIdx = 0;
        List<Tank> result = new List<Tank>(size);
        while (result.Count < size)
        {
            pair.Add(selected[lastIdx++]);
            if (lastIdx == size)
            {
                lastIdx = 0;
            }
            if (pair.Count == 2)
            {
                Tank firstT = pair.ToList()[0];
                Tank secondT = pair.ToList()[1];
                for (int i = 0; i < firstT.nn.GetLayersCount(); i++)
                {
                    for (int j = 0; j < firstT.nn.GetLayerNeuronsCount(i); j++)
                    {
                        for (int k = 0; k < firstT.nn.GetNeuronWeightsCount(i); k++)
                        {
                            float w1 = firstT.nn.GetWeight(i, j, k);
                            float w2 = secondT.nn.GetWeight(i, j, k);

                            if (UnityEngine.Random.Range(0, 1) == 1)
                            {
                                firstT.nn.SetWeightWithMutatePossibility(i, j, k, w2);
                                secondT.nn.SetWeightWithMutatePossibility(i, j, k, w1);
                            }
                            else
                            {
                                firstT.nn.SetWeightWithMutatePossibility(i, j, k, w1);
                                secondT.nn.SetWeightWithMutatePossibility(i, j, k, w2);
                            }
                            result.Add(firstT);
                            result.Add(secondT);
                        }
                    }
                }
                pair.Clear();
            }
        }
        return result;
    }

    private List<Tank> ConductSelection()
    {
        tanks.OrderBy(t => t.reward);
        List<Tank> selected = new List<Tank>(size);
        
        while (true)
        {
            for (int i = 0; i < size; i++)
            {
                float rnd = UnityEngine.Random.value;
                float val = GetRndRange(i);
                if (rnd <= val)
                {
                    selected.Add(tanks[i]);
                    if (selected.Count == size)
                    {
                        return selected;
                    }
                }

            }
        }
    }

    private float GetRndRange(int i)
    {
        return 0.8f / Mathf.Pow(2, i / rangeSize);
    }   
}
