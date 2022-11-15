using UnityEngine;

public class ShellHit : Shell
{
    internal override void CheckCollision(Collider other, Vector3 startingPosition)
    {
        if (exploded
            || other.gameObject.layer != LayerMask.NameToLayer("EnemyUnits")
            && other.gameObject.layer != LayerMask.NameToLayer("Ground")
            && other.gameObject.layer != LayerMask.NameToLayer("Borders")
            || other.gameObject == m_Parent)
        {
            return;
        }

        UnitManager targetManager = other.GetComponent<UnitManager>();

        if (targetManager != null)
        {
            SendReward(targetManager.Damage(m_Damage));
        }
        //else
        //{
        //    SendNegativeReward();
        //}

        Vector3 collisionPosition = transform.position;
        if (other.gameObject.layer == LayerMask.NameToLayer("EnemyUnits"))
        {
            collisionPosition = other.ClosestPoint(startingPosition);
            collisionPosition.y = transform.position.y;
        }
        Explode(collisionPosition);
    }
}