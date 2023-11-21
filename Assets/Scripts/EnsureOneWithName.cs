using System.Collections.Generic;
using UnityEngine;

// MonoBehavious that ensures this is the only object in the scene with the name it has.

public class EnsureOneWithName : MonoBehaviour {
    public static Dictionary<string, GameObject> uniqueObjects;

    static EnsureOneWithName() {
        uniqueObjects = new Dictionary<string, GameObject>();
    }

    private void Awake() {
        if (uniqueObjects.ContainsKey(gameObject.name)) {
            Destroy(gameObject);
        }

        uniqueObjects[gameObject.name] = gameObject;
    }
}