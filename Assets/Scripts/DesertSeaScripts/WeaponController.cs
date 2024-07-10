using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour {
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private Transform target;
    private Vector3 previousPosition;

    // Start is called before the first frame update
    void Start() {
        previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
    }

    // Update is called once per frame
    void Update() {
        Vector3 direction = previousPosition - cam.ScreenToViewportPoint(Input.mousePosition);

        this.transform.Rotate(new Vector3(1, 0, 0), direction.y * 180);
        this.transform.Rotate(new Vector3(0, 1, 0), -direction.x * 180, Space.World);

        previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
    }
}
