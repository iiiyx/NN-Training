using System;
using UnityEngine;

public abstract class Shell : MonoBehaviour
{
    internal abstract void CheckCollision(Collider other, Vector3 position);
    
    public ParticleSystem m_ExplosionParticles;
    public ParticleSystem m_DustParticles;

    public AudioSource m_ExplosionAudio;
    public float m_MaxLifeTime = 2f;
    public float m_Damage = 10f;

    protected Vector3 m_StartingPosition;
    protected Rigidbody m_RigidBody;
    protected GameObject m_Parent;

    protected bool exploded;

    private void Awake()
    {
        m_RigidBody = GetComponent<Rigidbody>();
        exploded = false;
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
        exploded = true;

        Vector3 pos = new Vector3(position.x, Mathf.Max(0f, position.y), position.z) + shift;

        if (m_DustParticles.gameObject.activeSelf)
        {
            m_DustParticles.transform.parent = null;
            m_DustParticles.transform.position = pos;
            Destroy(m_DustParticles.gameObject, m_DustParticles.main.duration);
        }

        if (m_ExplosionParticles.gameObject.activeSelf)
        {
            m_ExplosionParticles.transform.parent = null;
            m_ExplosionParticles.transform.position = pos;

            m_ExplosionParticles.Play();
            Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.main.duration);
        }

        if (m_ExplosionAudio.isActiveAndEnabled)
        {
            m_ExplosionAudio.Play();
        }

        Destroy(gameObject);
    }

    protected virtual void Launch(float shellSpeed0)
    {
        m_StartingPosition = transform.position;
        m_RigidBody.velocity = shellSpeed0 * transform.forward;
    }

    public void Fire(float shellSpeed0, GameObject parent)
    {
        m_Parent = parent;
        Launch(shellSpeed0);
    }

    protected void SendReward(bool killed)
    {
        TankAIAgent agent;
        if (m_Parent.TryGetComponent<TankAIAgent>(out agent))
        {
            agent.AddReward(10f);
            if (killed)
            {
                agent.AddReward(20f);
                agent.EndEpisode();
            }
        }
    }
}