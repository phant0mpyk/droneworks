using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MissionPath : MonoBehaviour
{
    [Header("Setup")]
    public CargoScript cargoTarget;
    public float lineWidth = 0.1f;
    public Color lineColor = Color.cyan;

    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();

        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.positionCount = 2;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = lineColor;
        line.endColor = lineColor;

        line.enabled = false;
    }

    void Update()
    {
        if (cargoTarget == null) return;

        if (cargoTarget.isAttached)
        {
            line.enabled = true;
            line.SetPosition(0, cargoTarget.transform.position);
            line.SetPosition(1, transform.position);
            float offset = Time.time * -2f;
            line.material.mainTextureOffset = new Vector2(offset, 0);
        }
        else
        {
            line.enabled = false;
        }
    }
}