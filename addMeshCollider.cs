/*error: if model static,r1.sharedMesh=combined mesh(root:mesh)*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class addMeshCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        foreach (var render in transform.GetComponentsInChildren<MeshRenderer>())
        {
            var gameObject = render.gameObject;
            var r1 = gameObject.GetComponent<MeshFilter>();
            if (!gameObject.GetComponent<MeshCollider>())
            {
                var meshCollider = gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = r1.sharedMesh;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
