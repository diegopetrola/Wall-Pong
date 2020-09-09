using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonsOnClick : MonoBehaviour
{
    public TMP_InputField input;
    public GameObject mainMenu;
    public GameObject joinGameMenu;
    public GameObject optionsMenu;

    private string player1Name = "Player1Name";
    //public string playerPrefString;
    
    public void MainMenuOnClick() {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void PlayAgainOnClick() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    public void TutorialOnClick() {
        SceneManager.LoadScene("Tutorial");
    }

    public void Quit() {
        Application.Quit();
    }
    
    public void JoinGameButton() {
        mainMenu.SetActive(false);
        joinGameMenu.SetActive(true);
    }

    public void BackJoinGameMenu() {
        mainMenu.SetActive(true);
        joinGameMenu.SetActive(false);
    }

    public void BackToMainMenu() {
        transform.parent.gameObject.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void OptionsButton() {
        transform.parent.gameObject.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void JoinButton() {
        TMP_InputField playerNameInput = GameObject.Find("PlayerNameInput").GetComponent<TMP_InputField>();
        TMP_InputField opponentNameInput = GameObject.Find("OpponentNameInput").GetComponent<TMP_InputField>();
        bool emptyField = false;
        if (playerNameInput.text == "") {
            

            emptyField = true;
        }

        if(opponentNameInput.text == "") {
            Color startColor = opponentNameInput.placeholder.color;
            Color finalColor = opponentNameInput.placeholder.color;
            finalColor.r = 1;
            LeanTween.value(opponentNameInput.gameObject, (Color c) => { opponentNameInput.placeholder.color = c; },
                            startColor, finalColor, .5f).setLoopPingPong(1);
            emptyField = true;
        }

        if (emptyField) return;

        GameManager.playerToConnect = opponentNameInput.text;
        SceneManager.LoadScene("PvP");
    }

    public void SavePlayerName() {
        if (input.text == "") return;
        PlayerPrefs.SetString(player1Name, input.text);
        PhotonNetwork.LocalPlayer.NickName = input.text;
    }

    public void HostGameButton() {
        GameManager.playerToConnect = ""; 
        SceneManager.LoadScene("PvP");
    }

    private void Awake() {
        if(input != null) {
            input.text = PlayerPrefs.GetString(player1Name);
        }
    }
}
