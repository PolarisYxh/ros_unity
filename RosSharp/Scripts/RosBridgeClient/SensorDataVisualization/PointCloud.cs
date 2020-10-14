using UnityEngine;
using System.Collections;
using System.IO;
public class PointCloud1 : MonoBehaviour
{
    ParticleSystem particleSystem;                  // 整个粒子系统
    ParticleSystem.Particle[] allParticles;         // 所有粒子的集合
    int pointCount;                                 // 粒子数目

    // Use this for initialization
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();

        // 1. 读取数据
        // 提前将点云存成txt文件放在Assert/StreamingAssets文件夹下，文本的每行代表一个点，由点的x，y，z，r，g，b六个float组成
        string fileAddress = (Application.streamingAssetsPath + "/" + "jiao.txt");
        FileInfo fInfo0 = new FileInfo(fileAddress);
        //Debug.Log(fileAddress);

        string s = "";
        StreamReader r;
        ArrayList arrayListXYZ = new ArrayList();
        ArrayList arrayListRGB = new ArrayList();

        if (fInfo0.Exists)
        {
            r = new StreamReader(fileAddress);
        }
        else
        {
            Debug.Log("NO THIS FILE!");
            return;
        }
        // 将文本中的点云数据读入分别存到xyz数组和rgb数组中
        while ((s = r.ReadLine()) != null)
        {
            string[] words = s.Split(" "[0]);

            Vector3 xyz = new Vector3(float.Parse(words[0]), -float.Parse(words[1]), float.Parse(words[2]));
            arrayListXYZ.Add(xyz);
            Color colorRGB = new Color(float.Parse(words[3]) / 255.0f, float.Parse(words[4]) / 255.0f, float.Parse(words[5]) / 255.0f);
            arrayListRGB.Add(colorRGB);
            //Debug.Log(xyz.ToString() + "," + colorRGB.ToString());
        }

        // 2. 设置粒子系统
        particleSystem.startSpeed = 0.0f;                           // 设置粒子的初始速度为0
        particleSystem.startLifetime = 1000.0f;                     // 粒子的生命周期尽量长

        // 3. 渲染出来（动态加载点云的时候再改这部分代码）
        pointCount = arrayListRGB.Count;
        allParticles = new ParticleSystem.Particle[pointCount];
        particleSystem.maxParticles = pointCount;                   // 设置粒子系统粒子数的最大值
        particleSystem.Emit(pointCount);                            // 发射pointCount个粒子
        particleSystem.GetParticles(allParticles);                  // 获取当前还存活的粒子数，不能少的一步，但是不是很清楚为什么
        for (int i = 0; i < pointCount; i++)
        {
            allParticles[i].position = (Vector3)arrayListXYZ[i];    // 设置每个点的位置
            allParticles[i].startColor = (Color)arrayListRGB[i];    // 设置每个点的rgb
            allParticles[i].startSize = 0.01f;                      // 设置点的大小，注意还要在unity界面上将粒子系统下面的Render模块中的Min/Max Particle Size改小--0.01差不多了
        }

        particleSystem.SetParticles(allParticles, pointCount);      // 将点云载入粒子系统
    }
}