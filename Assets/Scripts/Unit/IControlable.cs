using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IControlable
{
    void Move();
    
    void TurnChasisRight();
    void TurnChasisLeft();

    void TurnTurretRight();
    void TurnTurretLeft();

    void Fire();


}
