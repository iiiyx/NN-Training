using UnityEngine;

public class ShellHit : Shell
{
    internal override void CheckCollision(Collider other, Vector3 startingPosition)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("EnemyUnits")
            && other.gameObject.layer != LayerMask.NameToLayer("Ground"))
        {
            return;
        }

        UnitManager targetManager = other.GetComponent<UnitManager>();

        if (targetManager != null)
        {
            targetManager.Damage(m_Damage);
        }

        Vector3 collisionPosition = other.gameObject.layer == LayerMask.NameToLayer("EnemyUnits")
            ? other.ClosestPoint(startingPosition)
            : transform.position;
        Explode(collisionPosition);
    }

    private void FixedUpdate()
    {
        
    }
}