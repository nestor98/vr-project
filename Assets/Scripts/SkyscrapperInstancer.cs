using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SkyscrapperInstancer : MonoBehaviour
{
    public int Instances;
    public Mesh mesh;
    public Material mat;
    private List<Matrix4x4> batch = new List<Matrix4x4>();
    public float MaxHeight = 10000.0f;
    public float MaxRange = 10000.0f;
    public int Seed = 0;

    void Start()
    {
        Random.InitState(Seed);
        float MaxRangeDist = Mathf.Sqrt(MaxRange*MaxRange + MaxRange*MaxRange);
        for (int i = 0; i < Instances; ++i) {
            Vector3 pos = new Vector3(0.0f, 0.0f, 0.0f);
            bool valid_pos = false;
            while (!valid_pos) {
                pos = new Vector3(Random.Range(-MaxRange, MaxRange), 0.0f, Random.Range(-MaxRange, MaxRange));
                bool is_in_playable_area = Vector3.Distance(pos, new Vector3(0.0f, 0.0f, 0.0f)) < 150.0f;
                if (!is_in_playable_area) {
                    valid_pos = true;
                }
            }
            float dist_scale = 1.0f - Vector3.Distance(pos, new Vector3(0.0f, 0.0f, 0.0f)) / MaxRangeDist;
            float sun_dot = Vector3.Dot(pos.normalized, new Vector3(0.0f, 0.0f, 1.0f));
            float sun_scale = Mathf.Clamp(1.0f - ((Mathf.Clamp(sun_dot, 0.8f, 1.0f) - 0.8f) * 5.0f), 0.05f, 1.0f);
            float height = MaxHeight * dist_scale * sun_scale;
            pos.y = height * 0.5f;
            batch.Add(Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(100.0f, height, 100.0f)));
        }
    }

    void Update()
    {
        for (int i = 0; i < mesh.subMeshCount; ++i) {
            Graphics.DrawMeshInstanced(mesh, i, mat, batch);
        }
    }
}
