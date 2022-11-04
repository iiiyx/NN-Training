using UnityEngine;

public class UnitManager : Destroyable
{
    public float m_ExplosionEffectsTime = 2f;

    private Bounds m_TerrainBounds;
    private Rigidbody m_RigidBody;
    private int m_FrameCounter = 0;

    private void Start()
    {
        m_TerrainBounds = GameObject.FindGameObjectWithTag("Terrain").GetComponent<TerrainCollider>().bounds;
        m_RigidBody = GetComponent<Rigidbody>();
        m_RigidBody.centerOfMass = new Vector3(0, 0f, 0);
    }

    private void Update()
    {
        m_FrameCounter++;
        if (m_FrameCounter < 20)
        {
            return;
        }
        Vector3 pos = new Vector3(transform.position.x, (m_TerrainBounds.max.y - m_TerrainBounds.min.y)/2, transform.position.z);
        if (!m_IsDead &&
            (!m_TerrainBounds.Contains(pos)
                || transform.rotation.eulerAngles.x >= 60 && transform.rotation.eulerAngles.x <= 300
                || transform.rotation.eulerAngles.z >= 60 && transform.rotation.eulerAngles.z <= 300
            )
        )
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

        float damage = CalculateExplosionDamage(m_RigidBody.position, explosionPosition, explosionRadius, maxDamage);
        Damage(damage);
        transform.position += Vector3.up;
        m_RigidBody.AddExplosionForce(explosionForce, explosionPosition, explosionRadius, 5f);

        //StartCoroutine(RestoreNav());
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
}
