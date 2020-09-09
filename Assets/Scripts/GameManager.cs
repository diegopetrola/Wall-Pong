using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : Singleton<GameManager>
{

    //public GameObject obstacle;
    public Obstacle wall;
    public float radius = .5f;
    public TextMeshProUGUI startText;
    public TextMeshProUGUI gameOverText;
    public int scoresToWin = 3;
    public List<Player> players = new List<Player>();
    public float energyRegen = 0.004f; //Energy Regeneration per 0.1 sec.
    public int maxEnergy = 200;
    public UnityAction PrepareRound;  //this is called when the game needs to be reset, like after a score
    public UnityAction StartRound;    //this is called a little bit after prepare round, after some messages are displayed
    public UnityAction Score;         //this is called when a player scores
    public static bool startOffline = false;
    public Ball ball;
    public Slider enemyEnergySlider;

    private int textAnimatonId;
    public static string gameVersion = "1";
    private int _energy;
    public int Energy {
        get => _energy;
        set
        {
            if (value > maxEnergy)
                _energy = maxEnergy;
            else if (value < 0)
                _energy = 0;
            else
                _energy = value;
            energySlider.value = _energy;
        }
    }
    public Vector2 energySliderOffset = new Vector2(-0.1f, 0); //Those are percents
    public static string playerToConnect;   //If this is null we host a game, if not we connect to the players name

    private Slider energySlider;
    private bool gameStarted = false;
    public static class HashKey {
        public const string GameStarted = "GameStarted";
        public const string EnemyEnergy = "EnemyEnergy";
        public const string PlayerName = "PlayerName";
    };

    #region UNITY

    private void Awake() {
        PhotonNetwork.OfflineMode = startOffline;
    }

    private void Start() {
        //Initiate the slider and energy
        energySlider = GameObject.Find("EnergySlider").GetComponent<Slider>();
        energySlider.maxValue = maxEnergy;
        enemyEnergySlider.maxValue = maxEnergy;
        StartCoroutine(RegenerateEnergy());
        //Get components for texts used to display msgs
        startText.text = startOffline ? "Press Space to Start" : "Connecting...";
        //textAnimatonId = LeanTween.alphaText(startText.rectTransform, 0, .5f).setLoopPingPong(-1).id;
        Color startColor = startText.color;
        Color finalColor = startColor;
        finalColor.a = 0;
        textAnimatonId = LeanTween.value(startText.gameObject, ChangeTMPTween, startColor, finalColor, .5f).setLoopPingPong(-1).id;
        //ball = GameObject.Find("Sphere").GetComponent<Ball>();
        PrepareRound += _PrepareRound;
        //StartRound += _StartRound;
        PrepareRound();

        //TODO: fazer a conexão acontecer no menu
        if (!PhotonNetwork.OfflineMode && !PhotonNetwork.IsConnected) {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }

    private void Update() {
        bool condition = !gameStarted && PhotonNetwork.IsMasterClient && Input.GetKeyDown(KeyCode.Space);
        if (!PhotonNetwork.OfflineMode) {
            if (PhotonNetwork.CurrentRoom == null)
                //while the room is being created we can't start the game
                condition = false;
            else {
                condition = condition && PhotonNetwork.CurrentRoom.PlayerCount > 1;
            }
        }

        if (condition) {
            Hashtable h = new Hashtable { { HashKey.GameStarted, true } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(h);
        }

        Vector2 slideOffset = new Vector2(energySliderOffset.x * Camera.main.pixelWidth, energySliderOffset.y * Camera.main.pixelHeight);
        energySlider.transform.position = (Vector2)Input.mousePosition + slideOffset;

        if (Input.GetKey(KeyCode.Mouse0))
            wall.ModifyObstacle(true);
        else if (Input.GetKey(KeyCode.Mouse0))
            wall.ModifyObstacle(false);
    }
    #endregion

    public Action<Hashtable> roomUpdate;

    #region PHOTON

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        if (propertiesThatChanged.ContainsKey(HashKey.GameStarted) && (bool)propertiesThatChanged[HashKey.GameStarted]) {
            gameStarted = true;
            LeanTween.pause(textAnimatonId);
            CountDownAndStartRound();
        }
    }

    public override void OnJoinedRoom() {
        Debug.Log("on joined room - PlayerCount: " + PhotonNetwork.CountOfPlayers );
        if(PhotonNetwork.IsMasterClient) {
            players[0].Name = PhotonNetwork.LocalPlayer.NickName;
            ball = PhotonNetwork.Instantiate("Ball", Vector3.zero, Quaternion.identity).GetComponent<Ball>();
        }else {
            players[0].Name = PhotonNetwork.MasterClient.NickName;
            players[1].Name = PhotonNetwork.LocalPlayer.NickName;
            PrepareRound();
            SetTextAlpha(startText, 1f);
            startText.text = "Connection successfull \n";
            startText.text += "Waiting for Host to start";
            LeanTween.resume(textAnimatonId);
        }
        
        enemyEnergySlider.transform.parent.gameObject.SetActive(true);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("MainMenu");
    }

    override public void OnConnectedToMaster() {
        if (!(playerToConnect == null || playerToConnect == "")) {
            startText.text = "Joining " + playerToConnect + "'s room";
            PhotonNetwork.JoinRoom(playerToConnect.ToUpperInvariant());
            playerToConnect = null;
        }
        else {
            startText.text = "Creating Room";
            PhotonNetwork.CreateRoom(players[0].Name.ToUpperInvariant(), new RoomOptions { MaxPlayers = 2 });
        }
    }

    public override void OnCreatedRoom() {
        SetTextAlpha(startText, 1f);
        startText.text = "Waiting for Player \n";
        startText.text += "Give your name (" + players[0].Name + ") to someone so he can join";
        LeanTween.resume(textAnimatonId);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log(newPlayer.NickName + " entered the game!");
        players[1].Name = newPlayer.NickName;
        energySlider.gameObject.SetActive(true);
        energySlider.maxValue = maxEnergy;
        PrepareRound();
        SetTextAlpha(startText, 1f);
        startText.text = "Player " + players[1].Name + " joined the game! \n";
        startText.text += "Press Space to Start";
        LeanTween.resume(textAnimatonId);
    }


    #endregion

    void CountDownAndStartRound() {
        SetTextAlpha(startText, 1f);
        //Do a 3,2,1 countdown and starts the game
        startText.text = "3";
        LeanTween.resume(textAnimatonId);
        LeanTween.alphaText(startText.rectTransform, 0, 1f).setOnCompleteOnRepeat(true).setOnComplete(onCompleteText).setRepeat(3);
    }

    IEnumerator RegenerateEnergy() {
        int i = 0;
        while (true) {
            Energy += (int)(maxEnergy*energyRegen);
            //PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { HashKeys.EnemyEnergy, Energy } });
            yield return new WaitForSeconds(0.1f);
            if (i % 2 == 0 && PhotonNetwork.CurrentRoom != null) { //every 0.2s we update the energy to the enemy
                photonView.RPC("updateEnergyToEnemy", RpcTarget.Others, Energy);
            }
            i++;
        }
    }
    [PunRPC]
    void updateEnergyToEnemy(int enemyEnergy) {
        enemyEnergySlider.value = enemyEnergy;
    }

    void _PrepareRound() {
        Energy = maxEnergy;
        startText.transform.parent.gameObject.SetActive(true);
        //LeanTween.resume(textAnimatonId);
    }

    public void AddPlayer(Player newPlayer) {
        players.Add(newPlayer);
    }

    #region CAVAS_CHANGES
    void onCompleteText() {
        startText.text = (Int32.Parse(startText.text) - 1).ToString();
        if (startText.text == "0") {
            startText.gameObject.transform.parent.gameObject.SetActive(false);
            StartRound();
            return;
        }
    }

    void SetTextAlpha(TextMeshProUGUI text, float alpha) {
        Color c = text.color;
        c.a = alpha;
        text.color = c;
    }

    public void Goal(Player playerWhoScored) {

        playerWhoScored.score++;
        Score?.Invoke();

        if (playerWhoScored.score < scoresToWin) {
            SetTextAlpha(startText, 1);
            startText.text = playerWhoScored.Name + " scored!";
            startText.transform.parent.gameObject.SetActive(true);
            //if no player has won, go for another round
            PrepareRound();
            Invoke("CountDownAndStartRound", 2f);
        }
        else {
            //if someone won the game is over, display a text where the player can start a new round or go back to main menu
            gameOverText.text = playerWhoScored.Name + " won the game!";
            gameOverText.rectTransform.parent.gameObject.SetActive(true);
        }
    }

    void ChangeTMPTween(Color val) {
        startText.color = val;
    }
    #endregion

}
