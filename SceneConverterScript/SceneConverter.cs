using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GazeboSceneConverter {
  public class SceneConverter : MonoBehaviour {
    private static string EXPORT_DIR = "GazeboExport";

    private HashSet<string> _objectNames;
    private Dictionary<string, int> _duplicateNames;
    private MeshExporter _me;

    // Start is called before the first frame update
    void Start() {
      _duplicateNames = new Dictionary<string, int>();
      _me = new MeshExporter();

      // Application.targetFrameRate = 1;

      // load root GameObject
      var root = GameObject.Find("GazeboRoot");
      if (root == null) {
        Debug.LogError("GazeboRoot not found");
        return;
      }

      var sceneName = SceneManager.GetActiveScene().name;

      // create output folder
      Directory.CreateDirectory("GazeboExport");

      // prepare xml
      // export path: <UNITY_PROJECT_ROOT>/GazeboExport
      XmlWriter w = XmlWriter.Create(Path.Combine(EXPORT_DIR, $"gazebo-{sceneName}.world"));
      w.WriteStartDocument();

      // write root xml
      w.WriteStartElement("sdf");
      w.WriteAttributeString("version", "1.7");

      w.WriteStartElement("world");
      w.WriteAttributeString("name", sceneName);

      // write models
      foreach (Transform sub in root.transform) {
        // child is your child transform
        WriteSdfSubObject(w, sub.name, sub);
      }

      // </world>
      w.WriteEndElement();

      // </sdf>
      w.WriteEndElement();

      // don't forget to close
      w.WriteEndDocument();
      w.Close();
    }

    private void WriteSdfSubObject(XmlWriter xw, string parentName, Transform t) {
      var name = $"{parentName}_{t.name}";

      // check existence
      if (!_duplicateNames.ContainsKey(name)) {
        _duplicateNames.Add(name, 0);
      }

      var concatentedName = $"{name}_{_duplicateNames[name]}";
      _duplicateNames[name]++;

      if (!t.gameObject.activeSelf) {
        Debug.LogWarning(
          $"GameObject {parentName} is inactive and both itself together with its children will be ignored.");
        return;
      }

      var mf = t.gameObject.GetComponent<MeshFilter>();
      if (mf != null) {
        // <model>
        xw.WriteStartElement("model");
        xw.WriteAttributeString("name", concatentedName);

        // <static>1</static>
        xw.WriteStartElement("static");
        xw.WriteString("1");
        xw.WriteEndElement();

        // <link>
        xw.WriteStartElement("link");
        xw.WriteAttributeString("name", concatentedName);

        // <pose>
        xw.WriteStartElement("pose");

        // Quaternion q = t.rotation;
        // float roll = Mathf.Asin(2 * q.x * q.y + 2 * q.z * q.w);
        // float pitch = Mathf.Atan2(2 * q.x * q.w - 2 * q.y * q.z, 1 - 2 * q.x * q.x - 2 * q.z * q.z);
        // float yaw = Mathf.Atan2(2 * q.y * q.w - 2 * q.x * q.z, 1 - 2 * q.y * q.y - 2 * q.z * q.z);

        var pos = t.position.Unity2Ros();
        var rot = t.rotation.Unity2Ros().eulerAngles;

        // var eu = t.rotation.eulerAngles;
        float rx = Mathf.Deg2Rad * rot.x;
        float ry = Mathf.Deg2Rad * rot.y;
        float rz = Mathf.Deg2Rad * rot.z;

        // write <geometry>
        xw.WriteString($"{pos.x} {pos.y} {pos.z} {rx} {ry} {rz}");
        xw.WriteEndElement();

        // <visual>
        xw.WriteStartElement("visual");
        xw.WriteAttributeString("name", concatentedName);

        // <pose>
        xw.WriteStartElement("pose");
        xw.WriteString($"0 0 0 0 0 0");
        xw.WriteEndElement();

        // resolve exported filename
        var pathInfo = GetExportedFilePath(concatentedName, mf);

        // export mesh to obj file
        // ExportMeshToObj(concatentedName, mf, pathInfo);
        // export mesh to stl file
        ExportMeshToStl(concatentedName, t.gameObject, pathInfo);

        string scale =
          $"{t.localScale.x * t.parent.localScale.x} {t.localScale.y * t.parent.localScale.y} {t.localScale.z * t.parent.localScale.z}";

        // write geometry
        WriteSdfGeometry(xw, concatentedName, pathInfo[2], scale);

        // write meta
        WriteSdfMeta(xw);

        // </visual>
        xw.WriteEndElement();

        // write collision
        WriteSdfCollision(xw, concatentedName, pathInfo[2], scale);

        // </link>
        xw.WriteEndElement();

        // </model>
        xw.WriteEndElement();
      }
      else {
        Debug.LogWarning($"GameObject \"{concatentedName}\" has no Mesh Filter and will be ignored.");
      }

      // write children
      foreach (Transform sub in t) {
        WriteSdfSubObject(xw, concatentedName, sub);
      }
    }

    private static void WriteSdfMeta(XmlWriter w) {
      // <meta>
      w.WriteStartElement("meta");

      // <layer>
      w.WriteStartElement("layer");

      // <layer>
      w.WriteString("0");
      // </layer>
      w.WriteEndElement();

      // </meta>
      w.WriteEndElement();
    }

    private static void WriteSdfGeometry(XmlWriter w, string name, string path, string scaleString) {
      // <geometry>
      w.WriteStartElement("geometry");

      // mesh
      w.WriteStartElement("mesh");

      // mesh uri
      w.WriteStartElement("uri");
      w.WriteString(path);
      // </uri>
      w.WriteEndElement();

      // <scale>
      w.WriteStartElement("scale");
      w.WriteString(scaleString);
      // </scale>
      w.WriteEndElement();

      // </mesh>
      w.WriteEndElement();

      // </geometry>
      w.WriteEndElement();
    }

    private static void WriteSdfCollision(XmlWriter w, string name, string path, string scaleString) {
      // <collision>
      w.WriteStartElement("collision");
      w.WriteAttributeString("name", name);

      // <geometry>
      w.WriteStartElement("geometry");

      // mesh
      w.WriteStartElement("mesh");

      // mesh uri
      w.WriteStartElement("uri");
      w.WriteString(path);
      // </uri>
      w.WriteEndElement();

      // <scale>
      w.WriteStartElement("scale");
      w.WriteString(scaleString);
      // </scale>
      w.WriteEndElement();

      // </mesh>
      w.WriteEndElement();

      // </geometry>
      w.WriteEndElement();

      // </collision>
      w.WriteEndElement();
    }

    private void ExportMeshToStl(string name, GameObject obj, string[] outPath) {
      _me.SetTarget(obj);
      _me.ConvertMeshes();
      // var str = _me.ToString();

      // create output dir
      Directory.CreateDirectory(outPath[1]);

      // write to file
      File.WriteAllText(outPath[0], _me.ToStringWithName(name));
    }

    private static void ExportMeshToObj(string name, MeshFilter mf, string[] outPath) {
      StringBuilder sb = new StringBuilder();

      Debug.Log($"Exporting {name}");

      Mesh m = mf.sharedMesh;
      Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

      // write vertices
      sb.Append("g ").Append(name).Append("\n");
      foreach (Vector3 v in m.vertices) {
        sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
      }

      // write normals
      sb.Append("\n");
      foreach (Vector3 v in m.normals) {
        sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
      }

      // write UVs
      sb.Append("\n");
      foreach (Vector3 v in m.uv) {
        sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
      }

      // write materials
      for (int material = 0; material < m.subMeshCount; material++) {
        sb.Append("\n");
        // try {
        sb.Append("usemtl ").Append(mats[material].name).Append("\n");
        // }
        // catch (NullReferenceException e) {
        // Debug.Log(e);
        // }

        sb.Append("usemap ").Append(mats[material].name).Append("\n");

        int[] triangles = m.GetTriangles(material);
        for (int i = 0; i < triangles.Length; i += 3) {
          sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
            triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
        }
      }

      // create output dir
      Directory.CreateDirectory(outPath[1]);

      // write to file
      File.WriteAllText(outPath[0], sb.ToString());

      // Debug.Log($"Saved {outPath[0]}");
    }

    private string[] GetExportedFilePath(string objectName, MeshFilter mf) {
      // obtain asset file path
      string path = AssetDatabase.GetAssetPath(mf.sharedMesh);

      string origPath = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar) + 1);

      string exportedPath =
        Path.Combine(
          EXPORT_DIR,
          origPath);

      // get new filename
      var exportedFilePath = Path.Combine(
        EXPORT_DIR,
        origPath,
        objectName + ".stl");

      var pathInSdf = Path.Combine(
        origPath,
        objectName + ".stl");

      return new[] {exportedFilePath, exportedPath, pathInSdf};
    }
  }
}