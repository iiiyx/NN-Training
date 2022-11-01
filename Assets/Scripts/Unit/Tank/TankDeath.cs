using System.Collections;
using UnityEngine;

public class TankDeath : UnitDeath
{
    public Transform m_TurretTransform;
    public Transform m_ChassisTransform;
    public float m_ExplosionForce = 300f;

    private Rigidbody m_TurretRigidBody;
    private BoxCollider m_TurretCollider;
    private Rigidbody m_ChassisRigidBody;
    private BoxCollider m_ChassisCollider;
    private float m_TankSize;

    protected override void Awake()
    {
        base.Awake();
        m_TurretRigidBody = m_TurretTransform.GetComponent<Rigidbody>();
        m_TurretCollider = m_TurretTransform.GetComponent<BoxCollider>();
        
        m_ChassisRigidBody = m_ChassisTransform.GetComponent<Rigidbody>();
        m_ChassisCollider = m_ChassisTransform.GetComponent<BoxCollider>();

        m_TankSize = Mathf.Max(m_BoxCollider.size.x, m_BoxCollider.size.y, m_BoxCollider.size.z);
    }

    protected override void Ghostify()
    {
        base.Ghostify();

        m_TurretTransform.gameObject.layer = m_DeadUnitsLayer.LayerIndex;
        m_ChassisTransform.gameObject.layer = m_DeadUnitsLayer.LayerIndex;

        ExplodeTurret();
        ExplodeChassis();
    }

    private void ExplodeTurret()
    {
        m_TurretRigidBody.isKinematic = false;
        m_TurretRigidBody.useGravity = true;
        m_TurretCollider.enabled = true;

        float dx = Random.Range(-m_TankSize, m_TankSize);
        float dz = Random.Range(-m_TankSize, m_TankSize);
        Vector3 explosionPosition = transform.position + Vector3.forward * dz + Vector3.right * dx;
        float explosionRadius = m_TankSize * 2;

        m_TurretRigidBody.AddExplosionForce(m_ExplosionForce, explosionPosition, explosionRadius);
        m_TurretTransform.rotation = Random.rotation;
    }

    private void ExplodeChassis()
    {
        m_ChassisRigidBody.isKinematic = false;
        m_ChassisRigidBody.useGravity = true;
        m_ChassisCollider.enabled = true;

        m_ChassisTransform.rotation = Random.rotation;
    }
}
