using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveController : MonoBehaviour
{
    public WheelCollider frontLeftW, rearLeftW, middleLeftW,
        frontRightW, rearRightW, middleRightW,
        frontLeftW2, rearLeftW2, middleLeftW2,
        frontRightW2, rearRightW2, middleRightW2;

    public Transform turretTransform;


    public float motorForce = 50f;
    public float maxSteerAngle = 30f;
    public float maxSpeed = 25f;
    public float turnSpeed = 100f;

    private List<WheelCollider> allWheels;
    private List<WheelCollider> rightWheels;
    private List<WheelCollider> leftWheels;
    private Rigidbody rigidBody;
    
    private void HandleInput()
    {
        var y = 0;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            y = 1;
        }

        else if (Input.GetKey(KeyCode.DownArrow))
        {
            y = -1;
        }

        var x = 0;
        if (Input.GetKey(KeyCode.RightArrow))
        {
            x = 1;
        }

        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            x = -1;
        }

        Accelerate(x, y);
    }

    internal void TurnTurret(int turretTurnH)
    {
        if (turretTurnH != 0)
        {
            turretTransform.Rotate(Vector3.up * Time.deltaTime * turretTurnH * turnSpeed, Space.Self);
        }
    }

    internal void Accelerate(int x, int y)
    {
        if (x == 0 && y == 1)
        {
            // fwd
            Drive(allWheels, 1);
        }
        else if (x == 0 && y == -1)
        {
            // back
            Drive(allWheels, -1);
        }
        else if (x == 1 && y == 0)
        {
            // right
            Drive(leftWheels, 1.5f);
            Drive(rightWheels, -1.5f);
        }
        else if (x == -1 && y == 0)
        {
            // left
            Drive(leftWheels, -1.5f);
            Drive(rightWheels, 1.5f);
        }
        else if (x == 1 && y == 1)
        {
            // fwd-right
            Drive(leftWheels, 1);
            Drive(rightWheels, .25f);
        }
        else if (x == -1 && y == 1)
        {
            // fwd-left
            Drive(leftWheels, .25f);
            Drive(rightWheels, 1);
        }
        else if (x == 1 && y == -1)
        {
            // back-right
            Drive(leftWheels, -1);
            Drive(rightWheels, .25f);
        }
        else if (x == -1 && y == -1)
        {
            // back-left
            Drive(leftWheels, .25f);
            Drive(rightWheels, -1);
        } else
        {
            // neutral
            Drive(allWheels, 0);
        }
    }

    private void Drive(List<WheelCollider> wheels, float dir)
    {
        wheels.ForEach(w =>
        {
            if (dir == 0)
            {
                w.motorTorque = 0;
                w.brakeTorque = motorForce * 1;
                return;
            }
            w.brakeTorque = 0;
            w.motorTorque = motorForce * dir;
        });
    }

    private void UpdateWheelPoses()
    {
        //allWheels.ForEach(w =>
        //{
        //    w.motorTorque = motorForce * dir;
        //});
    }

    private void UpdateWheelPose(WheelCollider c, Transform t)
    {

    }

    private void LimitSpeed()
    {
        if (rigidBody.velocity.magnitude > maxSpeed)
        {
            rigidBody.velocity = rigidBody.velocity.normalized * maxSpeed;
        }
    }

    void Start()
    {
        allWheels = new List<WheelCollider>
        {
            frontLeftW, rearLeftW, middleLeftW,
            frontRightW, rearRightW, middleRightW,
            frontLeftW2, rearLeftW2, middleLeftW2,
            frontRightW2, rearRightW2, middleRightW2
        };
        rightWheels = new List<WheelCollider>
        {
            frontRightW, rearRightW, middleRightW,
            frontRightW2, rearRightW2, middleRightW2
        };
        leftWheels = new List<WheelCollider>
        {
            frontLeftW, rearLeftW, middleLeftW,
            frontLeftW2, rearLeftW2, middleLeftW2
        };
        rigidBody = GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        //HandleInput();
        UpdateWheelPoses();
        LimitSpeed();
    }
}
