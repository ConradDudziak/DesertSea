using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour {
    public Wheel[] wheels;

    [Header("Car Specs")]
    public float wheelBase; // in meters
    public float rearTrack; // in meters
    public float turnRadius; // in meters

    [Header("Inputs")]
    public float steerInput;

    private float ackermannAngleLeft;
    private float ackermannAngleRight;

    void Update() {
        steerInput = Input.GetAxis("Horizontal");
        
        if (steerInput > 0) { // is turning right
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
        } else if (steerInput < 0) { // is turning left
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
        } else {
            ackermannAngleLeft = 0;
            ackermannAngleRight = 0;
        }

        foreach (Wheel wheel in wheels) {
            if (wheel.wheelFrontLeft) {
                wheel.steerAngle = ackermannAngleLeft;
            }
            if (wheel.wheelFrontRight) {
                wheel.steerAngle = ackermannAngleRight;
            }
        }
    }
}
