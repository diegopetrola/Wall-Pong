using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Border : MonoBehaviour
{
    public Vector2 dir;
    private void OnTriggerEnter(Collider other) {
        other.attachedRigidbody.velocity *= dir;
    }
}
