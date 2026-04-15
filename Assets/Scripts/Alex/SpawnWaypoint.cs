using UnityEngine;

public class SpawnWaypoint : MonoBehaviour
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private float scale;

    public void Spawn()
    {
        GameObject go = new GameObject();
        GameObject newWaypoint = Instantiate( go, transform.position, Quaternion.identity);
        Destroy(go);
        Waypoint script = newWaypoint.AddComponent<Waypoint>();
        script.sprite = sprite;
        script.scale = scale;
    }
}
