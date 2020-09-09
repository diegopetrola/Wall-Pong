using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private string _name;
    public string Name {
        get => _name;
        set {
            _name = value;
            score = score; //update the UI
        }
    }
    public TextMeshProUGUI playerScoreText;
    private int _score;
    
    public int score
    {
        get=>_score;
        set
        {
            _score = value;
            playerScoreText.text = Name + ": " + _score;
        }
    }

    private void Awake() {
        Name = PlayerPrefs.GetString(name + "Name");
        if (Name == null || Name == "")
            Name = name; //name of the game object

        PhotonNetwork.LocalPlayer.NickName = Name;
        score = 0; //update the score on the UI
    }
}
