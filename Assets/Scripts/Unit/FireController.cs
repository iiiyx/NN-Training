using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FireController : MonoBehaviour
{
    public float m_AttackRange = 15f;
    public float m_ShellSpeed0 = 100f;
    public float m_FireReloadingTime = 1.5f;

    public Shell m_Shell;
    public Transform m_FireTransform;
    public AudioSource m_ShootingAudio;
    public AudioSource m_MovingAudio;


    private bool m_Fired;
    private WaitForSeconds m_WaitForReloading;

    protected virtual void Awake()
    {
        m_WaitForReloading = new WaitForSeconds(m_FireReloadingTime);
    }

    private void Fire()
    {
        if (m_Fired)
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
        Shell shell = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation);
        shell.Fire(m_ShellSpeed0, gameObject);

        m_ShootingAudio.Play();

        yield return ReloadFire();
    }

    protected IEnumerator ReloadFire()
    {
        yield return m_WaitForReloading;
        m_Fired = false;
    }

    private void HandleInput()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Fire();
        }

    }

    private void Update()
    {
        HandleInput();
    }
}
