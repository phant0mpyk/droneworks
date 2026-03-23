using UnityEngine;

public class RainInstancer : MonoBehaviour
{
    public Mesh mesh;
    public Material material;
    public int count = 1000;

    Matrix4x4[] matrices;
    float[] offsets;
    float[] speeds;

    MaterialPropertyBlock props;

    void Start()
    {
        matrices = new Matrix4x4[count];
        offsets = new float[count];
        speeds = new float[count];

        props = new MaterialPropertyBlock();

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-25f, 25f),
                Random.Range(0f, 20f),
                Random.Range(-25f, 25f)
            );

            matrices[i] = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);

            offsets[i] = Random.value * 20f;
            speeds[i] = Random.Range(0.8f, 1.2f);
        }

        props.SetFloatArray("_DropOffset", offsets);
        props.SetFloatArray("_SpeedMul", speeds);
    }

    void Update()
    {
        Graphics.DrawMeshInstanced(mesh, 0, material, matrices, count, props);
    }
}
