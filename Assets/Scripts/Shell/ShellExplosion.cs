using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ShellExplosion : Shell
{
    public float m_ExplosionForce = 200f;
    public float m_ExplosionRadius = 8f;
    public LayerMask m_ExplosionMask;

    private readonly Vector3 SHIFTER = new Vector3(0f, 2f, 0f);

    internal override void CheckCollision(Collider other, Vector3 startingPosition)
    {
        if (!IsInExplosionLayers(other))
        {
            return;
        }

        Vector3 collisionPosition = other.gameObject.layer == LayerMask.NameToLayer("EnemyUnits")
            ? other.ClosestPoint(startingPosition)
            : transform.position;

        Collider[] colliders = Physics.OverlapSphere(collisionPosition, m_ExplosionRadius, m_ExplosionMask);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject.layer != LayerMask.NameToLayer("EnemyUnits"))
            {
                continue;
            }

            UnitManager targetManager = colliders[i].GetComponent<UnitManager>();
            if (targetManager == null)
            {
                continue;
            }

            targetManager.ApplyExplosion(collisionPosition, m_ExplosionRadius, m_ExplosionForce, m_Damage);
        }

        Explode(collisionPosition, SHIFTER);
    }

    private bool IsInExplosionLayers(Collider other)
    {
        return m_ExplosionMask == (m_ExplosionMask | (1 << other.gameObject.layer));
    }

    protected override void Aim(BoxCollider targetCollider, float shellSpeed0)
    {
        Quaternion rt = transform.rotation;
        float alfa = float.NaN;
        Vector3 targetCenter = targetCollider.bounds.center;
     
        if (Mathf.Abs(targetCenter.y - transform.position.y) < 4f)
        {
            float distance = Vector3.Distance(targetCenter, transform.position);
            alfa = 90 - Mathf.Asin(distance * 9.81f / Mathf.Pow(shellSpeed0, 2f)) * 90 / Mathf.PI;
        }
        else
        {
            float distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(targetCenter.x, targetCenter.z));
            float a = 2 * shellSpeed0 * shellSpeed0;
            float b = (transform.position.y - targetCenter.y) * 9.81f;
            float c = distance * 9.81f;
            float d = a + b;
            float x = Cubic.Solve(c, -d, c, a - d);
            if (!float.IsNaN(x))
            {
                alfa = Mathf.Atan(x) * 180 / Mathf.PI;
            }
        }
        alfa = float.IsNaN(alfa) ? -45f : -alfa;
        rt = Quaternion.Euler(new Vector3(alfa, rt.eulerAngles.y, rt.eulerAngles.z));
        transform.rotation = rt;
    }
}