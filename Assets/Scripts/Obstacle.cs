//#define DEBUG_OnRoomPropertiesUpdate
//#define DEBUG_ModifyWall
//#define DEBUG_UpdateMesh

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class Obstacle : MonoBehaviourPunCallbacks {
    Mesh mesh;
    MeshCollider collider;
    Vector3[] normals;
    Vector3[] vertices;
    //float constructedWall = 3f;
    //float destructedWall = 0f;
    bool canModifyWall = true;
    //this variable is completely unecessary but Photon's funcion "OnRoomPropertiesUpdate" expects a key...
    private string hashKey = "board";

    // Start is called before the first frame update
    void Awake() {
        mesh = GetComponent<MeshFilter>().mesh;
        collider = GetComponent<MeshCollider>();
    }

    private void Update() {
        if (!canModifyWall) return;
        if (Input.GetKey(KeyCode.Mouse0))
            ModifyObstacle(true);
        else if (Input.GetKey(KeyCode.Mouse1))
            ModifyObstacle(false);
    }

    /// <summary>
    /// Create or destroy the wall
    /// </summary>
    /// <param name="create"> If true, create wall, else destroy </param>
    public void ModifyObstacle(bool create) {

#if (DEBUG_ModifyObstacle)
        Debug.Log("Modify Obstacle was called");
#endif

        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        //transform the mouse position (global) into local position for computation of distances
        pos = transform.worldToLocalMatrix * pos;
        vertices = mesh.vertices;
        normals = mesh.normals;

        Dictionary<int, float> indexToBeMoved = new Dictionary<int, float>();
        for (var i = 0; i < vertices.Length; ++i) {
            bool condition = create ? vertices[i].z >= 0 : vertices[i].z <= -2.5f;
            if (Vector2.Distance(pos, vertices[i]) < .5f && condition) {
                indexToBeMoved.Add(i, create ? 3f : 0f);
            }
        }

        if (GameManager.Instance.Energy < indexToBeMoved.Count)
            //if we dont have enough energy we dont do anything
            return;
        else {
            GameManager.Instance.Energy -= indexToBeMoved.Count;
        }
        //We first update locally...
        UpdateMesh(indexToBeMoved);

#if (DEBUG_ModifyObstacle)
        Debug.Log("indexToBeMoved.Count : " + indexToBeMoved.Count);
#endif

#if (DEBUG_OnRoomPropertiesUpdate)
        Debug.Log("Calling SetCustomProperties - " + indexToBeMoved.Count);
#endif
        if (indexToBeMoved.Count > 0) {

            //...them we update remotely
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { hashKey, indexToBeMoved } });
        }
    }

    void UpdateMesh(Dictionary<int, float> indexToBeMoved) {
        vertices = mesh.vertices;
        normals = mesh.normals;
#if (DEBUG_UpdateMesh)
        Debug.Log("vertex count: " + vertices.Length);
#endif
        foreach (KeyValuePair<int, float> v in indexToBeMoved) {
            vertices[v.Key].z = normals[v.Key].z * v.Value;
        }
        mesh.vertices = vertices;
        //update the colliders
        collider.sharedMesh = null;
        collider.sharedMesh = mesh;
    }

    override public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        if (propertiesThatChanged.ContainsKey(hashKey)) {
            Dictionary<int, float> indexToBeMoved = (Dictionary<int, float>)propertiesThatChanged[hashKey];
#if (DEBUG_OnRoomPropertiesUpdate)
            Debug.Log("Updading " + indexToBeMoved.Count + " vertex on " + PhotonNetwork.LocalPlayer.NickName);
#endif
            UpdateMesh(indexToBeMoved);
        }
    }

    void PrepareRound() {
        Debug.Log("Reseted Board");
        canModifyWall = false;
        vertices = mesh.vertices;
        //Reset all vertices
        for(int i = 0; i < vertices.Length; i++)
            vertices[i].z = 0f;

        mesh.vertices = vertices;
        collider.sharedMesh = null;
        collider.sharedMesh = mesh;
    }

    void StartRound() {
        canModifyWall = true;
    }

    override public  void OnEnable() {
        base.OnEnable();
        GameManager.Instance.PrepareRound += PrepareRound;
        GameManager.Instance.StartRound += StartRound;
    }
    override public void OnDisable() {
        base.OnDisable();
        if(GameManager.Instance != null) {
            GameManager.Instance.PrepareRound -= PrepareRound;
            GameManager.Instance.StartRound -= StartRound;
        }
    }
}
