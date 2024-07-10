using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatableData : ScriptableObject {
    public event System.Action OnValuesUpdated;
    public bool autoUpdate;

    void OnValidate() {
        if (autoUpdate) {
            NotifyOfUpdatedValues();
        }
    }

    public void NotifyOfUpdatedValues() {
        if (OnValuesUpdated != null) {
            OnValuesUpdated();
        }
    }
}
