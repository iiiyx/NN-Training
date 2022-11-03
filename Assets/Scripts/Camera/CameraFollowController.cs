using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowController : MonoBehaviour
{
    public Transform followed;
    public Vector3 offset;
    public float followSpeed = 10f;
    public float lookSpeed = 10f;

    private void MoveToTarget()
    {
        Vector3 t = followed.position + followed.forward * offset.z + followed.right * offset.x + followed.up * offset.y;
        transform.position = Vector3.Lerp(transform.position, t, followSpeed * Time.deltaTime);
    }

    private void LookAtTarget()
    {
        Vector3 lookDir = followed.position - transform.position;
        Quaternion r = Quaternion.LookRotation(lookDir, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, r, lookSpeed * Time.deltaTime);
    }

    void FixedUpdate()
    {
        LookAtTarget();
        MoveToTarget();
    }
}
