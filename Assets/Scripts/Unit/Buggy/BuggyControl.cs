using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuggyControl : TankControl
{
    public Transform[] m_FireTransforms;
    public float m_RocketDelay = 0.1f;
    WaitForSeconds m_WaitForRocket;

    protected override void Awake()
    {
        base.Awake();
        m_WaitForRocket = new WaitForSeconds(m_RocketDelay);
    }

    protected override IEnumerator LaunchShellRoutine()
    {
        float randomLaunchPause = Random.Range(0.01f, 0.1f);
        yield return new WaitForSeconds(randomLaunchPause);

        for (int i = 0; i < m_FireTransforms.Length; i++)
        {
            if (IsNoTarget())
            {
                break;
            }

            //TODO: make a parabolic path
            m_FireTransforms[i].LookAt(m_TargetCollider.bounds.center);
            // Instantiate and launch the shell.
            Shell shell = Instantiate(m_Shell, m_FireTransforms[i].position, m_FireTransforms[i].rotation);
            shell.Fire(m_TargetTransform, m_TargetCollider, m_TargetManager, m_ShellSpeed0);
            //LaunchShell(shell.transform, m_FireTransforms[i]);

            m_ShootingAudio.Play();

            yield return m_WaitForRocket;
        }
        yield return ReloadFire();
    }
}
