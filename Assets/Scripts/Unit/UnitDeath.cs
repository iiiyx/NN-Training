using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class UnitDeath : MonoBehaviour, IKillable
{
    public GameObject m_ExplosionPrefab;
    public Transform m_UnitRenderers;
    public Rigidbody m_RigidBody;
    public BoxCollider m_BoxCollider;
    public Material m_DiedUnitMaterial;
    public float m_DeathTime = 2f;
    public SingleUnityLayer m_DeadUnitsLayer;

    private AudioSource m_ExplosionAudio;
    private ParticleSystem m_ExplosionParticles;
    private WaitForSeconds m_DeathWait;
    private MeshRenderer[] m_Renderers;

    protected virtual void Awake()
    {
        m_ExplosionParticles = Instantiate(m_ExplosionPrefab, transform).GetComponent<ParticleSystem>();
        m_ExplosionParticles.gameObject.SetActive(false);
        m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource>();
    }

    private void Start()
    {
        m_Renderers = m_UnitRenderers.GetComponentsInChildren<MeshRenderer>();
        m_DeathWait = new WaitForSeconds(m_DeathTime);
    }

    public void Kill()
    {
        // Play the effects for the death of the tank and deactivate it.
        m_ExplosionParticles.transform.parent = null;
        m_ExplosionParticles.transform.position = transform.position;

        m_ExplosionParticles.gameObject.SetActive(true);
        m_ExplosionParticles.Play();
        m_ExplosionAudio.Play();

        Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.main.duration);

        Ghostify();

        StartCoroutine(Deactivate());
    }

    protected virtual void Ghostify()
    {
        for (int i = 0; i < m_Renderers.Length; i++)
        {
            m_Renderers[i].material.color = m_DiedUnitMaterial.color;
        }

        transform.gameObject.layer = m_DeadUnitsLayer.LayerIndex;

        if (m_RigidBody)
        {
            m_RigidBody.isKinematic = true;
            m_RigidBody.useGravity = false;
        }

        if (m_BoxCollider)
        {
            m_BoxCollider.enabled = false;
        }

        var patrol = GetComponentInParent<Patrol>();
        if (patrol)
        {
            patrol.Stop();
        }
    }

    private IEnumerator Deactivate()
    {
        yield return m_DeathWait;
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        //Destroy(m_ExplosionParticles);
        //Debug.Log(transform);
    }
}
