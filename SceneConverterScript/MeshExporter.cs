/*
 * Some contents are copied from ros-sharp.
 * https://github.com/siemens/ros-sharp
 */

using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GazeboSceneConverter {
  public class MeshExporter {
    private GameObject _gameObject;
    private readonly bool _withCollision;
    private readonly bool _convertAxis;
    private List<Mesh> _meshes;

    public MeshExporter(bool withCollision = false, bool convertAxis = true) {
      // _gameObject = obj;
      _withCollision = withCollision;
      _convertAxis = convertAxis;
    }

    public void SetTarget(GameObject obj) {
      _gameObject = obj;
      _meshes = null;
    }

    public void ConvertMeshes() {
      if (!_gameObject) {
        Debug.LogError("MeshExporter: no GameObject set!");
        return;
      }

      // Create a clone with no scale, rotation or transform, so that mesh will be
      // at original size and position when exported.
      GameObject clone = Object.Instantiate(_gameObject, Vector3.zero, Quaternion.identity);
      clone.name = _gameObject.name;
      clone.transform.localScale = Vector3.one;

      GameObject root = new GameObject();
      clone.transform.SetParent(root.transform, true);

      List<Mesh> meshes = new List<Mesh>();
      if (_withCollision) {
        MeshCollider[] meshColliders = root.GetComponentsInChildren<MeshCollider>();

        foreach (MeshCollider meshCollider in meshColliders) {
          if (meshCollider.sharedMesh != null) {
            meshes.Add(TransformMeshToWorldSpace(meshCollider.transform, meshCollider.sharedMesh));
          }
        }
      }
      else {
        // var mf = root.transform.GetComponent<MeshFilter>();
        // meshes.Add(TransformMeshToWorldSpace(mf.transform, mf.sharedMesh));
        // return;

        MeshFilter[] meshFilters = root.transform.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter meshFilter in meshFilters) {
          if (meshFilter.sharedMesh != null) {
            meshes.Add(TransformMeshToWorldSpace(meshFilter.transform, meshFilter.sharedMesh));
          }
        }
      }

      Object.DestroyImmediate(root);

      _meshes = meshes;
    }

    private static Mesh TransformMeshToWorldSpace(Transform meshTransform, Mesh sharedMesh) {
      Vector3[] v = sharedMesh.vertices;
      Vector3[] n = sharedMesh.normals;

      for (int it = 0; it < v.Length; it++) {
        v[it] = meshTransform.TransformPoint(v[it]);
        n[it] = meshTransform.TransformDirection(n[it]);
      }

      return new Mesh {
        name = meshTransform.name,
        vertices = v,
        normals = n,
        triangles = sharedMesh.triangles
      };
    }

    public string ToStringWithName(string desiredName = "") {
      StringBuilder sb = new StringBuilder();

      string name = desiredName.Length != 0 ? desiredName : (_meshes.Count == 1 ? _meshes[0].name : "Composite Mesh");

      sb.AppendLine($"solid {name}");

      foreach (Mesh mesh in _meshes) {
        Vector3[] v = _convertAxis ? Left2Right(mesh.vertices) : mesh.vertices;
        Vector3[] n = _convertAxis ? Left2Right(mesh.normals) : mesh.normals;
        int[] t = mesh.triangles;

        if (_convertAxis) {
          System.Array.Reverse(t);
        }

        int triLen = t.Length;

        for (int i = 0; i < triLen; i += 3) {
          int a = t[i];
          int b = t[i + 1];
          int c = t[i + 2];

          Vector3 nrm = AverageNormal(n[a], n[b], n[c]);

          sb.AppendLine($"facet normal {nrm.x} {nrm.y} {nrm.z}");

          sb.AppendLine("outer loop");

          sb.AppendLine($"\tvertex {v[a].x} {v[a].y} {v[a].z}");
          sb.AppendLine($"\tvertex {v[b].x} {v[b].y} {v[b].z}");
          sb.AppendLine($"\tvertex {v[c].x} {v[c].y} {v[c].z}");

          sb.AppendLine("endloop");

          sb.AppendLine("endfacet");
        }
      }

      sb.AppendLine($"endsolid {name}");

      return sb.ToString();
    }

    private static Vector3[] Left2Right(Vector3[] v) {
      Vector3[] r = new Vector3[v.Length];
      for (int i = 0; i < v.Length; i++)
        r[i] = v[i].Unity2Ros();
      return r;
    }

    private static Vector3 AverageNormal(Vector3 a, Vector3 b, Vector3 c) {
      return new Vector3(
        (a.x + b.x + c.x) / 3f,
        (a.y + b.y + c.y) / 3f,
        (a.z + b.z + c.z) / 3f);
    }
  }
}