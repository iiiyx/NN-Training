using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(UnitDeath))]
[RequireComponent(typeof(UnitHealthUI))]
[RequireComponent(typeof(UnitControl))]
public class UnitManager : Destroyable, IControlable
{
    public UnitControl m_UnitControl;
    public float m_ExplosionEffectsTime = 2f;

    private Bounds m_TerrainBounds;
    private Rigidbody m_RigidBody;
    private NavMeshAgent m_NavAgent;
    private int m_ExplosionCounter;
    private WaitForSeconds m_ExplosionEffectsWait;
    private WaitForSeconds m_GravityEffectsWait;
    private int m_FrameCounter = 0;

    private void Start()
    {
        m_TerrainBounds = GameObject.FindGameObjectWithTag("Terrain").GetComponent<TerrainCollider>().bounds;
        m_RigidBody = GetComponent<Rigidbody>();
        m_NavAgent = GetComponent<NavMeshAgent>();
        m_ExplosionEffectsWait = new WaitForSeconds(m_ExplosionEffectsTime);
        m_GravityEffectsWait = new WaitForSeconds(m_ExplosionEffectsTime * 2);
    }

    public void MoveTo(Vector3 targetPos, float magnitude)
    {
        m_UnitControl.MoveTo(targetPos, magnitude);
    }

    public void Attack(Transform targetTransform)
    {
        m_UnitControl.Attack(targetTransform);
    }

    private void Update()
    {
        m_FrameCounter++;
        if (m_FrameCounter < 20)
        {
            return;
        }
        Vector3 pos = new Vector3(transform.position.x, m_TerrainBounds.center.y, transform.position.z);
        if (!m_IsDead && !m_TerrainBounds.Contains(pos))
        {
            Kill();
        }
        m_FrameCounter = 0;
    }

    private void LateUpdate()
    {
        if (transform.position.y < 0f || float.IsNaN(transform.position.y))
        {
            transform.position = new Vector3(transform.position.x, m_InitialPosHeight, transform.position.z);
        }
    }

    internal void ApplyExplosion(Vector3 explosionPosition, float explosionRadius, float explosionForce, float maxDamage)
    {
        if (!m_RigidBody)
        {
            return;
        }

        m_RigidBody.isKinematic = false;
        m_RigidBody.useGravity = true;

        if (m_NavAgent)
        {
            m_NavAgent.enabled = false;
        }


        float damage = CalculateExplosionDamage(m_RigidBody.position, explosionPosition, explosionRadius, maxDamage);
        Damage(damage);
        transform.position += Vector3.up;
        m_RigidBody.AddExplosionForce(explosionForce, explosionPosition, explosionRadius, 5f);

        StartCoroutine(RestoreNav());
    }

    private float CalculateExplosionDamage(Vector3 targetPosition, Vector3 explosionPosition, float explosionRadius, float maxDamge)
    {
        //Calculate the amount of damage a target should take based on it's position.
        Vector3 explosion2Target = targetPosition - explosionPosition;
        float explosionDistance = explosion2Target.magnitude;

        float explosionDamageFactor = (explosionRadius - explosionDistance) / explosionRadius;
        float damage = explosionDamageFactor * maxDamge;

        return Mathf.Max(0f, damage);
    }

    private IEnumerator RestoreNav()
    {
        m_ExplosionCounter++;
        yield return m_ExplosionEffectsWait;

        m_ExplosionCounter--;
        if (m_ExplosionCounter <= 0)
        {
            yield return m_GravityEffectsWait;
       
            m_RigidBody.useGravity = false;
            m_RigidBody.isKinematic = true;
            if (m_NavAgent)
            {
                m_NavAgent.enabled = true;
            }
        }
    }
}
