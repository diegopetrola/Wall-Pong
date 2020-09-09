using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public Player player;

    private void OnTriggerEnter(Collider other) {
        GameManager.Instance.Goal(player);
    }
}
