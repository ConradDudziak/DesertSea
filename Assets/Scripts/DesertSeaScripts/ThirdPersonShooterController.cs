using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class ThirdPersonShooterController : MonoBehaviour {

    [SerializeField]
    private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField]
    private float lookSensitivity;
    [SerializeField]
    private float aimSensitivity;
    [SerializeField]
    private LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField]
    private Transform[] bulletSpawnPointPositions;
    [SerializeField]
    private bool addBulletSpread = true;
    [SerializeField]
    private Vector3 bulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField]
    [Tooltip("The lower the number, the more accurate the weapon")]
    private float normalAccuracyModifier = 1f;
    [SerializeField]
    [Tooltip("The lower the number, the more accurate the weapon")]
    private float aimAccuracyModifier = 0.5f;
    [SerializeField]
    private RectTransform[] crossHairs;
    [SerializeField]
    private float crossHairsNormalDelta = 16f;
    [SerializeField]
    private float crossHairsNormalDeltaMax = 18f;
    [SerializeField]
    private float crossHairsAimDelta = 14f;
    [SerializeField]
    private float crossHairsAimDeltaMax = 16f;
    [SerializeField]
    [Tooltip("The lower the number, the longer it takes to reach max recoil")]
    private float normalRecoilRate = 1f;
    [SerializeField]
    [Tooltip("The lower the number, the longer it takes to reach max recoil")]
    private float aimRecoilRate = 2f;
    [SerializeField]
    private Transform muzzleFlashVfx;
    [SerializeField]
    private Transform hitVfx;
    [SerializeField]
    private TrailRenderer bulletTrail;
    [SerializeField]
    private float fireRate;

    private int bulletSpawnPointIndex = 0;
    private float lastShootTime;
    private CameraTest cameraTest;
    private MyStarterAssetsInputs starterAssetsInputs;
    private float currAccuracyModifier = 1f;
    private float currMaxRecoil = 1f;
    private float recoilSpread = 0;

    private void Awake() {
        cameraTest = GetComponent<CameraTest>();
        starterAssetsInputs = GetComponent<MyStarterAssetsInputs>();
    }

    void Start() {
        
    }

    void Update() {
        Vector3 mouseWorldPosition = Vector3.zero;

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, aimColliderLayerMask)) {
            mouseWorldPosition = raycastHit.point;
        }

        if (starterAssetsInputs.aim) {
            aimVirtualCamera.gameObject.SetActive(true);
            cameraTest.setSensitivity(aimSensitivity);
            currAccuracyModifier = aimAccuracyModifier;
            currMaxRecoil = crossHairsAimDeltaMax - crossHairsAimDelta;
            adjustCrosshairs(crossHairsAimDelta);
            applyRecoil(aimRecoilRate);
        } else {
            aimVirtualCamera.gameObject.SetActive(false);
            cameraTest.setSensitivity(lookSensitivity);
            currAccuracyModifier = normalAccuracyModifier;
            currMaxRecoil = crossHairsNormalDeltaMax - crossHairsAimDelta;
            adjustCrosshairs(crossHairsNormalDelta);
            applyRecoil(normalRecoilRate);
        }

        // Point the weapon at the location we are aiming at
        Vector3 worldAimTarget = mouseWorldPosition;
        Vector3 weaponAimDirection = (worldAimTarget - transform.position).normalized;
        transform.forward = weaponAimDirection;

        // Shoot the weapon at the location we are aiming at
        Vector3 bulletAimDirection = (worldAimTarget - bulletSpawnPointPositions[bulletSpawnPointIndex].position).normalized;

        if (starterAssetsInputs.shoot) {
            shootAt(bulletAimDirection);
        } else {
            recoilSpread = 0f;
        }
    }

    private void shootAt(Vector3 aimDirection) {
        if (lastShootTime + fireRate < Time.time) {
            Vector3 direction = getDirectionWithBulletSpread(aimDirection);

            if (Physics.Raycast(bulletSpawnPointPositions[bulletSpawnPointIndex].position, direction, out RaycastHit raycastHit, float.MaxValue, aimColliderLayerMask)) {
                TrailRenderer trail = Instantiate(bulletTrail, bulletSpawnPointPositions[bulletSpawnPointIndex].position, Quaternion.identity);

                Instantiate(muzzleFlashVfx, bulletSpawnPointPositions[bulletSpawnPointIndex].position, transform.rotation).SetParent(transform);

                // Increment next spawn point index before the coroutine to prevent buggy behavior
                incrementBulletSpawnPointIndex();

                StartCoroutine(spawnTrail(trail, raycastHit.point));

                Instantiate(hitVfx, raycastHit.point, Quaternion.identity);

                lastShootTime = Time.time;
            }
        }
    }

    private Vector3 getDirectionWithBulletSpread(Vector3 aimDirection) {
        Vector3 direction = aimDirection;

        if (addBulletSpread) {
            direction += new Vector3(
                Random.Range(-bulletSpreadVariance.x, bulletSpreadVariance.x),
                Random.Range(-bulletSpreadVariance.y, bulletSpreadVariance.y),
                Random.Range(-bulletSpreadVariance.z, bulletSpreadVariance.z)) * currAccuracyModifier * (recoilSpread / currMaxRecoil) * 2f;

            direction.Normalize();
        }

        return direction;
    }

    private IEnumerator spawnTrail(TrailRenderer trail, Vector3 hitPoint) {
        float time = 0;
        Vector3 startPosition = trail.transform.position;

        while (time < 1) {
            trail.transform.position = Vector3.Lerp(startPosition, hitPoint, time);
            time += Time.deltaTime / trail.time;

            yield return null;
        }

        trail.transform.position = hitPoint;
        Destroy(trail.gameObject, trail.time);
    }

    private void applyRecoil(float recoilRate) {
        recoilSpread = Mathf.Lerp(recoilSpread, currMaxRecoil, recoilRate * Time.deltaTime);
    }

    private void adjustCrosshairs(float delta) {
        Vector3 direction = Vector3.zero;

        for(int i = 0; i < crossHairs.Length; i++) {
            if (i == 0 || i == 2) {
                if (i == 0) {
                    direction = Vector3.up;
                } else {
                    direction = Vector3.down;
                }
            } else if (i == 1 || i == 3) {
                if (i == 1) {
                    direction = Vector3.right;
                } else {
                    direction = Vector3.left;
                }
            }
            crossHairs[i].anchoredPosition = direction * (delta + recoilSpread);
        }
    }

    private void incrementBulletSpawnPointIndex() {
        if (bulletSpawnPointIndex >= bulletSpawnPointPositions.Length - 1) {
            bulletSpawnPointIndex = 0;
        } else {
            bulletSpawnPointIndex++;
        }
    }
}
