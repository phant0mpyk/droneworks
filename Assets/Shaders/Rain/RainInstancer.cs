using System;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class RainInstancer : MonoBehaviour
{
    public Mesh mesh;
    public Material material;
    public int count = 100;
    private Camera cam;

    [SerializeField] private Wind windScript;
    [CanBeNull] Matrix4x4[] matrices;
    Vector3[] positions;
    float[] offsets;
    float[] speeds;
    [SerializeField] private Vector4 direction;
    private float windStrength;

    MaterialPropertyBlock props;

    void Start()
    {
        cam = Camera.main;
        
        matrices = new Matrix4x4[count];
        positions = new Vector3[count];
        offsets = new float[count];
        speeds = new float[count];

        props = new MaterialPropertyBlock();

        direction = new Vector4(-windScript.windDirection.x, -windScript.windDirection.z, 0, 0);
        


        for (int i = 0; i < count; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-25f, 25f),
                Random.Range(0f, 20f),
                Random.Range(-25f, 25f)
            );
            positions[i] = pos;
            offsets[i] = Random.value * 20f;
            speeds[i] = Random.Range(0.8f, 1.2f);
        }

        props.SetFloatArray("_DropOffset", offsets);
        props.SetFloatArray("_SpeedMul", speeds);
        
    }

    void Update()
    {
        windStrength = windScript.windForce.magnitude*5;
        for (int i = 0; i < count; ++i)
        {
            

            matrices[i] = Matrix4x4.TRS(positions[i] + cam.transform.position, Quaternion.identity, Vector3.one);

        }
        material.SetVector("_WindDir", direction);
        material.SetFloat("_WindStrength", windStrength);
        Graphics.DrawMeshInstanced(mesh, 0, material, matrices, count, props);
        
    }

    private void OnDestroy()
    {
        material.SetFloat("_WindStrength", 0);
        material.SetVector("_WindDir", Vector4.zero);
    }
}
