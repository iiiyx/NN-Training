using UnityEngine;

public class ShellRocket : ShellHit
{
    public float m_TurnSpeed = 200f;
    public float focusDistance = 45f;
    
    private int m_FrameCounter = 0;
    private Vector3 m_LastKnownTargetPosition;
    private bool m_IsLookingAtTarget = true;

    protected override void Aim(BoxCollider targetCollider, float shellSpeed0)
    {
        base.Aim(targetCollider, shellSpeed0);
        m_LastKnownTargetPosition = targetCollider.bounds.center;
    }


    private void FixedUpdate()
    {
        if (!m_TargetManager)
        {
            if (m_FrameCounter != 0)
            {
                m_FrameCounter = 0;
            }
            return;
        }

        m_FrameCounter++;

        if (m_FrameCounter % 2 == 0)
        {
            // TODO: check viewsight
            // https://answers.unity.com/questions/1452516/dodgeable-homing-missiles-in-unity3d.html

            m_FrameCounter = 0;

            m_LastKnownTargetPosition = m_TargetCollider.bounds.center;

            Vector3 targetDirection = m_LastKnownTargetPosition - transform.position;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, m_TurnSpeed * Time.deltaTime, 0.0F);

            m_RigidBody.velocity = m_ShellSpeed0 * transform.forward;

            if (Vector3.Distance(transform.position, m_LastKnownTargetPosition) > focusDistance)
            {
                m_IsLookingAtTarget = false;
            }

            if (m_IsLookingAtTarget)
            {
                transform.rotation = Quaternion.LookRotation(newDirection);
            }
        }
    }
}
