using System;
using UnityEngine;

public abstract class Shell : MonoBehaviour
{
    internal abstract void CheckCollision(Collider other, Vector3 position);
    
    public ParticleSystem m_ExplosionParticles;
    public AudioSource m_ExplosionAudio;
    public float m_MaxLifeTime = 2f;
    public float m_Damage = 10f;

    protected readonly Vector3 ZERO_VECTOR = Vector3.zero;
    protected Vector3 m_StartingPosition;
    protected Transform m_TargetTransform;
    protected BoxCollider m_TargetCollider;
    protected Rigidbody m_RigidBody;
    protected UnitManager m_TargetManager;
    protected float m_ShellSpeed0;
    private bool m_SubscribedToDeath;

    private void Awake()
    {
        m_RigidBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Destroy(gameObject, m_MaxLifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckCollision(other, m_StartingPosition);
    }

    protected void Explode(Vector3 position)
    {
        Explode(position, Vector3.zero);
    }

    protected void Explode(Vector3 position, Vector3 shift)
    {
        m_ExplosionParticles.transform.parent = null;
        m_ExplosionParticles.transform.position = new Vector3(position.x, Mathf.Max(0f, position.y), position.z) + shift;

        m_ExplosionParticles.Play();
        m_ExplosionAudio.Play();

        Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.main.duration);
        Destroy(gameObject);
    }

    protected virtual void Aim(BoxCollider targetCollider, float shellSpeed0)
    {
        transform.LookAt(targetCollider.bounds.center);
    }

    protected virtual void Launch(float shellSpeed0)
    {
        m_StartingPosition = transform.position;
        m_RigidBody.velocity = shellSpeed0 * transform.forward;
    }

    public void Fire(Transform targetTransform, BoxCollider targetCollider, UnitManager targetManager, float shellSpeed0)
    {
        m_TargetTransform = targetTransform;
        m_TargetCollider = targetCollider;
        m_ShellSpeed0 = shellSpeed0;
        m_TargetManager = targetManager;

        m_TargetManager.OnDeath += ClearTarget;

        Aim(targetCollider, shellSpeed0);
        Launch(shellSpeed0);
    }

    private void ClearTarget(Transform transform)
    {
        TryUnsubscribeFromTargetDeath();

        m_TargetTransform = null;
        m_TargetCollider = null;
        m_TargetManager = null;
    }

    private void TryUnsubscribeFromTargetDeath()
    {
        if (m_SubscribedToDeath && m_TargetManager)
        {
            m_TargetManager.OnDeath -= ClearTarget;
            m_SubscribedToDeath = false;
        }
    }

    private void OnDisable()
    {
        TryUnsubscribeFromTargetDeath();
    }

    private void OnDestroy()
    {
        TryUnsubscribeFromTargetDeath();
    }
}