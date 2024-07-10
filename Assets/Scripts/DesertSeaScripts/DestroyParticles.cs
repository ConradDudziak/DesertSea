using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyParticles : MonoBehaviour {

    [SerializeField]
    private float defaultDuration = 1.0f;

    // Start is called before the first frame update
    void Start() {
        ParticleSystem particleSystem = GetComponent<ParticleSystem>();

        if (particleSystem != null) {
            Destroy(this.gameObject, particleSystem.main.duration);
        } else {
            Destroy(this.gameObject, defaultDuration);
        }
    }
}
