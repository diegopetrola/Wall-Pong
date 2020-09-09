using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Random = Unity.Random;

public class Ball : MonoBehaviourPunCallbacks
{
    public Obstacle wall;
    public float startSpeed = 3f;
    private float _currentSpeed;
    private float currentSpeedSqr;
    private float lastCollisionTime;
    private float currentSpeed {
        get => _currentSpeed;
        set {
            _currentSpeed = value;
            currentSpeedSqr = currentSpeed * currentSpeed;
        }
    }

    private Rigidbody rb;
    public ParticleSystem explosion;

    #region UNITY
    private void Awake() {
        explosion = Instantiate(explosion);
        rb = GetComponent<Rigidbody>();
    }
    override public void OnEnable() {
        base.OnEnable();
        if (!PhotonNetwork.IsMasterClient) return; //Only the host manages the ball
        GameManager.Instance.PrepareRound += PrepareRound;
        GameManager.Instance.StartRound += StartRound;
        GameManager.Instance.Score += Score;
    }
    override public void OnDisable() {
        base.OnDisable();
        if (!PhotonNetwork.IsMasterClient) return;
        if (GameManager.Instance != null) {
            GameManager.Instance.StartRound -= StartRound;
            GameManager.Instance.PrepareRound -= PrepareRound;
            GameManager.Instance.Score -= Score;
        }
    }
    private void OnCollisionExit(Collision collision) {
        if (PhotonNetwork.IsMasterClient) {
            SyncPositionAndVelocity();
        }
        else {
            //if
            if(lastCollisionTime - Time.time < PhotonNetwork.GetPing()) {
                SyncPositionAndVelocity();
            }
            lastCollisionTime = Time.time;
        }
    }
    #endregion

    #region PHOTON
    [PunRPC]
    void SyncBall(int ping, Vector3 pos, Vector3 vel) {
        Debug.Log("ping: " + ping + " pos " + pos + " vel " + vel);

        int localPing = PhotonNetwork.GetPing();
        float lag = (float)(ping / 2 + localPing / 2) / 1000;

        rb.velocity = vel;
        rb.position = pos + vel * lag;
    }
    void SyncPositionAndVelocity() {
        int ping = PhotonNetwork.GetPing();
        photonView.RPC("SyncBall", RpcTarget.Others, ping, rb.position, rb.velocity);
    }
    #endregion

    private void PrepareRound() {
        rb.velocity = Vector2.zero;
        rb.position = Vector2.zero;
    }

    public void RandomizeVelocity(float speed) {
        rb.velocity = new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), 0f).normalized * speed;
        currentSpeed = speed;
        SyncPositionAndVelocity();
    }

    private void StartRound() {
        RandomizeVelocity(startSpeed);
        StartCoroutine(IncrementSpeed());
    }

    private void Score() {
        explosion.transform.position = transform.position;
        explosion.Play();
        StopAllCoroutines();

        if (!PhotonNetwork.IsMasterClient) return;
        rb.velocity = Vector2.zero;
        //hide the ball behind the board
        rb.position = new Vector3(0, 0, 100f);
    }

    IEnumerator IncrementSpeed() {
        while (true) {
            yield return new WaitForSeconds(5f);
            currentSpeed ++;
            SyncPositionAndVelocity();
        }
    }
}
