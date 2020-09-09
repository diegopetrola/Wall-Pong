// Builds a Mesh containing a single triangle with uvs.
// Create arrays of vertices, uvs and triangles, and copy them into the mesh.

using UnityEngine;

public class meshTriangles : MonoBehaviour {
    Mesh mesh;
    Vector3[] vertices;
    Vector3[] normals;
    MeshCollider collider;

    void Start() {
        mesh = GetComponent<MeshFilter>().mesh;
        collider = GetComponent<MeshCollider>();
        vertices = mesh.vertices;
        normals = mesh.normals;
    }

    void Update() {

        if (Input.GetKey(KeyCode.Mouse0)) {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;

            for (var i = 0; i < vertices.Length; ++i) {
                if(Vector2.Distance(pos, vertices[i]) < 0.5f) {
                    if(vertices[i].z >= 0) {
                        Debug.Log("antes " + vertices[i]);
                        vertices[i] += 3*normals[i];
                        Debug.Log("depois " + vertices[i]);
                    }
                }
            }

            mesh.vertices = vertices;
            collider.sharedMesh.vertices = vertices;
            //collider.enabled = false;
            //collider.enabled = true;
        }
        if (Input.GetKey(KeyCode.Mouse1)) {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            for (var i = 0; i < vertices.Length; ++i) {
                if (Vector2.Distance(pos, vertices[i]) < 0.5f) {
                    if (vertices[i].z <= -3) {
                        Debug.Log(vertices[i]);
                        vertices[i] -= 3 * normals[i];
                    }
                }
            }

            mesh.vertices = vertices;
            collider.sharedMesh.vertices = vertices;
            //collider.enabled = false;
            //collider.enabled = true;
        }
    }
}

/*
using UnityEngine;

public class meshTriangles : MonoBehaviour {
    // Use this for initialization
    void Start() {
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        mesh.Clear();

        // make changes to the Mesh by creating arrays which contain the new values
        mesh.vertices = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0) };
        mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) };
        mesh.triangles = new int[] { 0, 1, 2 };
    }

    private void FixedUpdate() {
        if (Input.GetKey(KeyCode.Mouse0)) {
            RaycastHit hit;

            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (Physics.Raycast(pos, gameObject.transform.forward, out hit)) {
                Debug.Log("Hit " + hit.collider.name);

                Mesh mesh = hit.collider.GetComponent<MeshFilter>().mesh;
                MeshCollider collider = hit.collider.GetComponent<MeshCollider>();
                int[] triangles = mesh.triangles;

                Vector3[] vertices = mesh.vertices;

                for(var i = 0; i < vertices.Length; i++) {
                    if (Vector2.Distance(pos, vertices[i]) > 1)
                        vertices[i].z++;
                }

                mesh.vertices = vertices;
                mesh.triangles = triangles;
                //collider.sharedMesh.vertices = null;
                collider.sharedMesh.vertices = vertices;


                collider.enabled = false;
                collider.enabled = true;
            }
            else {
                Debug.Log("Didn't hit.");
            }

        }

    }
}
*/
