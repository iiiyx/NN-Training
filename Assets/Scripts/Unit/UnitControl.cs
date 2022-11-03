using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class UnitControl : MonoBehaviour
{
    protected abstract void SetLookRotation(Vector3 targetPosition);
    protected abstract void UpdateLookRotation();
    protected abstract void ResetLookRotation();

    public float m_AttackRange = 15f;
    public float m_MoveSpeed = 5f;
    public float m_TurnSpeed = 200f;
    public float m_ShellSpeed0 = 100f;
    public float m_FireReloadingTime = 1.5f;

    public Shell m_Shell;
    public Transform m_FireTransform;
    public AudioSource m_ShootingAudio;
    public AudioSource m_MovingAudio;


    protected Transform m_TargetTransform;
    protected BoxCollider m_TargetCollider;
    protected UnitManager m_TargetManager;

    private bool m_Fired;
    private WaitForSeconds m_WaitForReloading;
    private Vector3 m_LastKnownTargetPosition;
    private Vector3 m_LastPosition;
    protected Rigidbody m_Rigidbody;
    private Vector3 m_MovePosition;
    private bool m_LastCmdIsMove = true;
    private bool m_IsMoving;
    private bool m_SubscribedToTargetDeath;
    public float m_StoppingDistance = 4f;
    //private readonly List<GameObject> currentCollisions = new List<GameObject>();

    public float m_RemainingDistance {
        get {
            if (!m_TargetTransform && !m_IsMoving)
            {
                return 0f;
            }

            if (m_TargetTransform)
            {
                return Vector3.Distance(transform.position, m_TargetTransform.position);
            }

            return Vector3.Distance(transform.position, m_MovePosition);
        }
    }

    protected virtual void Awake()
    {
        m_WaitForReloading = new WaitForSeconds(m_FireReloadingTime);
    }

    private void Start()
    {
        m_LastPosition = transform.position;
        //Fetch the Rigidbody component you attach from your GameObject
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    public void MoveTo(Vector3 targetPos, float magnitude)
    {
        MoveToTarget(targetPos);
        m_LastCmdIsMove = true;
    }

    public void MoveToTarget(Vector3 targetPos)
    {
        m_MovePosition = targetPos;
        m_IsMoving = true;
    }

    private bool LookAt(Vector3 p)
    {
        Vector3 eulerAngles = Quaternion.LookRotation(p - transform.position).eulerAngles;
        eulerAngles.x = 0;
        eulerAngles.z = 0;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(eulerAngles), m_TurnSpeed * Time.deltaTime);

        return Mathf.Abs(eulerAngles.y - transform.rotation.eulerAngles.y) < 10f;
    }

    private void MoveStep()
    {
        if (LookAt(m_MovePosition))
        {
            var p = Vector3.MoveTowards(transform.position, m_MovePosition, m_MoveSpeed);
            transform.position = Vector3.Lerp(transform.position, p, Time.deltaTime);
        }
    }

    public void Attack(Transform newTarget)
    {
        m_LastCmdIsMove = false;

        if (newTarget == m_TargetTransform)
        {
            return;
        }

        if (m_TargetManager != null && m_SubscribedToTargetDeath)
        {
            m_TargetManager.OnDeath -= OnTargetDeath;
        }

        m_TargetTransform = newTarget;
        // TODO: null check
        m_TargetManager = newTarget.GetComponent<UnitManager>();
        m_TargetCollider = newTarget.GetComponent<BoxCollider>();
        m_LastKnownTargetPosition = m_TargetCollider.bounds.center;

        m_TargetManager.OnDeath += OnTargetDeath;
        m_SubscribedToTargetDeath = true;

        SetLookRotation(m_LastKnownTargetPosition);

        // TODO: raycast to target
        if (m_RemainingDistance > m_AttackRange)
        {
            MoveToTarget(m_LastKnownTargetPosition);
            m_StoppingDistance = m_AttackRange;
        }
        else
        {
            // Fire??
        }
    }

    private void OnDisable()
    {
        if (m_SubscribedToTargetDeath && m_TargetManager)
        {
            m_TargetManager.OnDeath -= OnTargetDeath;
        }
    }

    private void OnDestroy()
    {
        if (m_SubscribedToTargetDeath && m_TargetManager)
        {
            m_TargetManager.OnDeath -= OnTargetDeath;
        }
    }

    private void Fire()
    {
        if (m_Fired)
        {
            return;
        }

        if (IsNoTarget())
        {
            return;
        }

        m_Fired = true;

        StartCoroutine(LaunchShellRoutine());
    }

    protected virtual IEnumerator LaunchShellRoutine()
    {
        float randomLaunchPause = Random.Range(0.01f, 0.1f);
        yield return new WaitForSeconds(randomLaunchPause);
        if (!m_TargetCollider)
        {
            yield return ReloadFire();
            yield break;
        }
        m_FireTransform.LookAt(m_TargetCollider.bounds.center);
        // Instantiate and launch the shell.
        Shell shell = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation);
        shell.Fire(/*m_TargetTransform, m_TargetCollider, m_TargetManager,*/ m_ShellSpeed0, gameObject);
        //LaunchShell(shell.transform, m_FireTransform);

        m_ShootingAudio.Play();

        yield return ReloadFire();
    }

    protected bool IsNoTarget()
    {
        return m_TargetTransform == null;
    }

    protected IEnumerator ReloadFire()
    {
        yield return m_WaitForReloading;
        m_Fired = false;
    }

    public void StopMoving()
    {
        m_IsMoving = false;
        m_StoppingDistance = 4f;
    }

    private void Update1()
    {
        if (m_IsMoving)
        {
            if (m_RemainingDistance > m_StoppingDistance)
            {
                MoveStep();
            }
            else
            {
                StopMoving();
            }
        }
    }

    private void FixedUpdate1()
    {
        bool isMoved = (transform.position - m_LastPosition).magnitude > 0.1f;
        if (isMoved)
        {
            m_LastPosition = transform.position;
        }

        if (m_MovingAudio.isPlaying && !m_IsMoving)
        {
            m_MovingAudio.Stop();
        }
        if (!m_MovingAudio.isPlaying && m_IsMoving)
        {
            m_MovingAudio.Play();
        }

        if (m_LastCmdIsMove && m_TargetTransform != null)
        {
            ClearTarget();
        }

        UpdateLookRotation();

        if (m_TargetTransform == null)
        {
            return;
        }

        float targetMoveDistance = (m_TargetTransform.position - m_LastKnownTargetPosition).magnitude;
        if (targetMoveDistance > 0.2f || isMoved)
        {
            if (targetMoveDistance > 0.2f)
            {
                m_LastKnownTargetPosition = m_TargetCollider.bounds.center;
            }
            SetLookRotation(m_LastKnownTargetPosition);
        }

        float distance = Vector3.Distance(transform.position, m_LastKnownTargetPosition);
        if (distance <= m_AttackRange && IsGoodLookRotationForFire() && !m_Fired)
        {
            StopMoving();
            Fire();
        }
        if (distance > m_AttackRange)
        {
            MoveToTarget(m_LastKnownTargetPosition);
        }
    }

    protected virtual bool IsGoodLookRotationForFire()
    {
        return true;
    }

    private void ClearTarget()
    {
        if (m_SubscribedToTargetDeath && m_TargetTransform != null)
        {
            m_TargetManager.OnDeath -= OnTargetDeath;
            m_SubscribedToTargetDeath = false;
        }

        m_TargetManager = null;
        m_TargetTransform = null;
        m_TargetCollider = null;

        ResetLookRotation();

        m_StoppingDistance = 4f;
        if (!m_LastCmdIsMove)
        {
            StopMoving();
        }
    }

    private void OnTargetDeath(Transform tartgetTransform)
    {
        ClearTarget();
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.gameObject.layer != gameObject.layer)
        //{
        //    return;
        //}
        //currentCollisions.Add(other.gameObject);

        //if (m_NavAgent.enabled && m_NavAgent.isStopped)
        //{
        //    return;
        //}

        // avoiding collisions
        //if (m_RemainingDistance > m_StoppingDistance)
        //{
        //    NavMeshAgent otherAgent = other.gameObject.GetComponent<NavMeshAgent>();
        //    if (!otherAgent.enabled)
        //    {
        //        return;
        //    }
        //    if (m_RemainingDistance > otherAgent.remainingDistance)
        //    {
        //        m_NavAgent.isStopped = true;
        //        if (otherAgent.remainingDistance < 0.1f && !otherAgent.hasPath)
        //        {
        //            m_NavAgent.ResetPath();
        //        }
        //    }
        //}
    }

    private void OnTriggerExit(Collider other)
    {
        //if (other.gameObject.layer != gameObject.layer)
        //{
        //    return;
        //}
        //currentCollisions.Remove(other.gameObject);

        //if (m_NavAgent.enabled && !m_NavAgent.isStopped)
        //{
        //    return;
        //}

        // avoiding collisions
        //if (m_NavAgent.enabled && m_NavAgent.remainingDistance > m_NavAgent.stoppingDistance)
        //{
        //    if (currentCollisions.Count < 1)
        //    {
        //        m_NavAgent.isStopped = false;
        //        return;
        //    }

        //    foreach (GameObject item in currentCollisions)
        //    {
        //        NavMeshAgent otherAgent = item.GetComponent<NavMeshAgent>();
        //        if (otherAgent.enabled && otherAgent.remainingDistance > otherAgent.stoppingDistance)
        //        {
        //            if (m_NavAgent.remainingDistance > otherAgent.remainingDistance)
        //            {
        //                otherAgent.isStopped = false;
        //                return;
        //            }
        //        }
        //    }
        //    m_NavAgent.isStopped = false;
        //}
    }


    
}
