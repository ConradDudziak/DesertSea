using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private Transform target;
    private Vector3 previousPosition;

    private void Start() {
        previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
    }

    private void Update() {
        Vector3 direction = previousPosition - cam.ScreenToViewportPoint(Input.mousePosition);

        cam.transform.position = target.position;

        cam.transform.Rotate(new Vector3(1, 0, 0), direction.y * 180);
        cam.transform.Rotate(new Vector3(0, 1, 0), -direction.x * 180, Space.World);
        cam.transform.Translate(new Vector3(0, 0, -10));

        previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
    }


    /*public GameObject car;
    public Vector3 followOffset;
    public float followSpeed;
    public float rotationSpeed;

    //values for internal use
    private Quaternion lookRotation;
    private Vector3 direction;

    void Start() {
        transform.position = car.transform.position - followOffset;
        transform.LookAt(car.transform);
    }

    void Update() {
        Vector3 newPos = car.transform.position - followOffset;

        transform.position = Vector3.Slerp(transform.position, newPos, followSpeed);

        transform.position = transform.position + Vector3.right * Time.deltaTime;
        //transform.Translate(Vector3.right * Time.deltaTime);

        //find the vector pointing from our position to the target
        direction = (car.transform.position - transform.position).normalized;

        //create the rotation we need to be in to look at the target
        lookRotation = Quaternion.LookRotation(direction);

        //rotate us over time according to speed until we are in the required rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }*/
}
