using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour {
    private Rigidbody rb;

    public bool wheelFrontLeft;
    public bool wheelFrontRight;
    public bool wheelBackLeft;
    public bool wheelBackRight;

    [Header("Suspension")]
    public float restLength;
    public float springTravel;
    public float springStiffness;
    public float damperStiffness;

    private float minLength;
    private float maxLength;
    private float lastLength;
    private float springLength;
    private float springForce;
    private float damperForce;
    private float springVelocity;
    private Vector3 suspensionForce;

    [Header("Wheel")]
    public float wheelRadius;
    public float steerAngle;
    public float steerTime;
    public float accelerationForce;
    public Transform wheel;

    private float wheelAngle;
    private float fZ;
    private float fX;
    private Vector3 wheelVelocity; // local space

    void Start() {
        rb = transform.root.GetComponent<Rigidbody>();

        minLength = restLength - springTravel;
        maxLength = restLength + springTravel;
    }

    void Update() {
        wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, steerTime * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);

        Debug.DrawRay(transform.position, -transform.up * (springLength), Color.green);
    }

    void FixedUpdate() {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, maxLength + wheelRadius)) {
            lastLength = springLength;
            springLength = Mathf.Clamp(hit.distance - wheelRadius, minLength, maxLength);
            springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;

            damperForce = damperStiffness * springVelocity;
            springForce = springStiffness * (restLength - springLength);
            suspensionForce = (springForce + damperForce) * transform.up;

            wheelVelocity = transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));
            fZ = Input.GetAxis("Vertical") * accelerationForce;
            fX = wheelVelocity.x * accelerationForce;

            rb.AddForceAtPosition(suspensionForce + (fZ * transform.forward) + (fX * -transform.right), hit.point);

            wheel.position = hit.point + (wheelRadius * transform.up);
        }
    }
}
