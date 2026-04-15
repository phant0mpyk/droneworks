using UnityEngine;

public class SpawnWaypoint : MonoBehaviour
{
    [SerializeField] private Sprite sprite;

    public void Spawn()
    {
        GameObject go = new GameObject();
        GameObject newWaypoint = Instantiate( go, transform.position, Quaternion.identity);
        Destroy(go);
        newWaypoint.AddComponent<Waypoint>().sprite = sprite;
    }
}
