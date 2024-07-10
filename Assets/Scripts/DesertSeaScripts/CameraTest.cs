using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraTest : MonoBehaviour {

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject cinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float topClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float bottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float cameraAngleOverride = 0.0f;

    [Tooltip("Factor to multiply camera rotation speed by")]
    public float sensitivity = 1.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool lockCameraPosition = false;

    private MyStarterAssetsInputs input;
    private PlayerInput playerInput;
    private const float threshold = 0.0001f;
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;

    private bool isCurrentDeviceMouse {
        get {
            return playerInput.currentControlScheme == "KeyboardMouse";
        }
    }

    public void setSensitivity(float newSensitivity) {
        sensitivity = newSensitivity;
    }

    // Start is called before the first frame update
    void Start() {
        cinemachineTargetYaw = cinemachineCameraTarget.transform.rotation.eulerAngles.y;

        input = GetComponent<MyStarterAssetsInputs>();
        playerInput = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update() {
        
    }

    private void LateUpdate() {
        cameraRotation();
    }

    private void cameraRotation() {
        // if there is an input and camera position is not fixed
        if (input.look.sqrMagnitude >= threshold && !lockCameraPosition) {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = isCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            cinemachineTargetYaw += input.look.x * deltaTimeMultiplier * sensitivity;
            cinemachineTargetPitch += input.look.y * deltaTimeMultiplier * sensitivity;
        }

        // clamp our rotations so our values are limited 360 degrees
        cinemachineTargetYaw = clampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = clampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

        // Cinemachine will follow this target
        cinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + cameraAngleOverride,
            cinemachineTargetYaw, 0.0f);
    }

    private static float clampAngle(float lfAngle, float lfMin, float lfMax) {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}
