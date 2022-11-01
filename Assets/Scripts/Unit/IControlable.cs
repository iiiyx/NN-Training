using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IControlable
{
    void MoveTo(Vector3 targetPos, float magnitude);
    void Attack(Transform targetTransform);
}
