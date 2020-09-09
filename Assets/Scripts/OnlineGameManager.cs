using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using Photon.Realtime;

public class OnlineGameManager : Singleton<MonoBehaviourPunCallbacks> {
    string gameVersion = "1";
    // Start is called before the first frame update
    void Start() {
        if (!PhotonNetwork.IsConnected) {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
            //PhotonNetwork.CreateRoom(, new RoomOptions { MaxPlayers = 2 });
            //PhotonNetwork.off
        }
    }


}




