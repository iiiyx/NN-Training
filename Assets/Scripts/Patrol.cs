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
        new Vector3(150.2f, 0.00f, 133.3f),
        new Vector3(215.4f, 0.00f, 313.1f),
        new Vector3(233.3f, 0.00f, 458.1f),
        new Vector3(313.4f, 0.00f, 436.4f),
        new Vector3(384.8f, 0.00f, 282.2f),
        new Vector3(464.5f, 0.00f, 258.5f),
        new Vector3(333.5f, 0.00f, 194.5f)
    };
    private int counter = 0;
    private UnitControl m_Unit;
    private bool m_IsStopped;
    private bool m_UnitStopped;

    private void Start()
    {
        var terrain = Terrain.activeTerrain;
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
