using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Modify : MonoBehaviourPunCallbacks
{
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
        if (!canModifyWall) return;

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

        //We first update locally...
        UpdateMesh(indexToBeMoved);
        
        if(indexToBeMoved.Count > 0) {
            //...them we update remotely remotely
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { hashKey, indexToBeMoved } });
        }
        
    }

    void UpdateMesh( Dictionary<int, float> indexToBeMoved) {
        vertices = mesh.vertices;
        foreach(KeyValuePair<int, float> v in indexToBeMoved) {
            vertices[v.Key].z = normals[v.Key].z * v.Value;
        }
        mesh.vertices = vertices;
        collider.sharedMesh.vertices = vertices;
        //this is the only way I know how to update the collider after changing the mesh, Unity's doc is not clear on this
        collider.enabled = false;
        collider.enabled = true;
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {

        Dictionary<int, float> indexToBeMoved = (Dictionary<int, float>)propertiesThatChanged[hashKey];
        UpdateMesh(indexToBeMoved);
    }
}
