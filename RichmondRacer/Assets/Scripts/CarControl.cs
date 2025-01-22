using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControl : MonoBehaviour
{
    public float motorTorque = 2000;
    public float brakeTorque = 2000;
    public float maxSpeed = 20;
    public float steeringRange = 30;
    public float steeringRangeAtMaxSpeed = 10;
    public float centreOfGravityOffset = -1f;
    public bool car1;
    public bool car2;

    WheelControl[] wheels;
    Rigidbody rigidBody;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();

        //move center of gravity up
        rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;

        //find all child gameobjects with WheelControl script
        wheels = GetComponentsInChildren<WheelControl>();
    }

    // Update is called once per frame
    void Update()
    {
        float vInput;
        float hInput;
        if (car1)
        {
            vInput = Input.GetAxis("Vertical");
            hInput = Input.GetAxis("Horizontal");
        }
        else if (car2)
        {
            vInput = Input.GetAxis("Vertical2");
            hInput = Input.GetAxis("Horizontal2");
        }
        else
        {
            vInput = Input.GetAxis("Vertical");
            hInput = Input.GetAxis("Horizontal");
        }

        //calculate current speed in relation to forward direction of car
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.velocity);

        //calculate how close car is to top speed as num from 0-1
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);

        //use that to calc how much torque is available (0 at top speed)
        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);

        //and calculate how much to steer (less sharp when faster
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

        //check whether user input is in same direction as velocity
        bool isAccelerating = Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed);

        foreach(var wheel in wheels)
        {
            //apply steering to wheels w/ "steerable" enabled
            if (wheel.steerable)
            {
                wheel.WheelCollider.steerAngle = hInput * currentSteerRange;
            }

            if (isAccelerating)
            {
                //apply torque to "mmotorized" wheels
                if (wheel.motorized)
                {
                    wheel.WheelCollider.motorTorque = vInput * currentMotorTorque;
                }
                wheel.WheelCollider.brakeTorque = 0;
            }
            else
            {
                //if user is trying to go in opposite direction, apply brakes
                wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
                wheel.WheelCollider.motorTorque = 0;
            }
        }
    }
}
