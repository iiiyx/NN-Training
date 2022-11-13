using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class Patrol : MonoBehaviour
{

    List<Vector3> points = new List<Vector3>()
    {
        new Vector3(11.7f, 0.00f, 12.2f),
        new Vector3(33.5f, 0.00f, 40.8f),
        new Vector3(10.4f, 0.00f, 87.9f),
        new Vector3(18.9f, 0.00f, 126.2f),
        new Vector3(58.7f, 0.00f, 105.9f),
        new Vector3(112.8f, 0.00f, 107.7f),
        new Vector3(119.7f, 0.00f, 69.6f),
        new Vector3(62.6f, 0.00f, 43.1f),
        new Vector3(43.5f, 0.00f, 36.9f),
        new Vector3(123f, 0.00f, 11.4f),
    };
    private int counter = 0;
    private UnitControl m_Unit;
    private bool m_IsStopped;
    private bool m_UnitStopped;

    private void Awake()
    {
        var terrain = GetComponentInParent<Terrain>();
        var bounds = terrain.GetComponent<TerrainCollider>().bounds;

        points = points.Select(p => new Vector3(p.x, terrain.SampleHeight(p), p.z)).ToList();

        m_Unit = GetComponentInParent<UnitControl>();
        MoveNext();
    }

    // Start is called before the first frame update
    private void MoveNext()
    {
        m_Unit.MoveTo(points[counter++ % points.Count], 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_UnitStopped)
        {
            return;
        }

        if (m_IsStopped)
        {
            ((UnitControl)m_Unit).StopMoving();
            m_UnitStopped = true;
            return;
        }

        if (m_Unit.m_RemainingDistance == 0)
        {
            MoveNext();
        }
    }

    internal void Stop()
    {
        m_IsStopped = true;
    }
}
