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
        Vector3 pos = new Vector3(position.x, Mathf.Max(0f, position.y), position.z) + shift;
        
        m_DustParticles.transform.parent = null;
        m_DustParticles.transform.position = pos;

        m_ExplosionParticles.transform.parent = null;
        m_ExplosionParticles.transform.position = pos;

        m_ExplosionParticles.Play();
        m_ExplosionAudio.Play();

        Destroy(m_DustParticles.gameObject, m_DustParticles.main.duration);
        Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.main.duration);
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
}