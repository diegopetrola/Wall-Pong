using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject.Find("Plane").GetComponent<Modify>().enabled = false;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom() {
        GameObject.Find("Plane").GetComponent<Modify>().enabled = true;
        Debug.Log("Joined room");
    }

    public override void OnJoinRandomFailed(short returnCode, string message) {
        Debug.Log("failed to join room: " + message);
        PhotonNetwork.CreateRoom(null);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
