using System;
using UnityEngine;

public class LeavesManager : MonoBehaviour
{
    [SerializeField] private GameObject drone;
    [SerializeField] private float rayDistance;
    [SerializeField] private GameObject leavesPrefab;
    [SerializeField] private GameObject dustPrefab;
    [SerializeField] private float offsetFromGround;
    [SerializeField] private float speed;
    private GameObject leaves;
    private GameObject dust;
    private ParticleSystem leafParticles;
    private ParticleSystem dustParticles;
    
    DroneInputManager inputManager;
    private float minSpeedMultiplier;
    
    void Start()
    {
        leaves = Instantiate(leavesPrefab);
        dust = Instantiate(dustPrefab);
        leafParticles = leaves.GetComponent<ParticleSystem>();
        dustParticles = dust.GetComponent<ParticleSystem>();
        leaves.SetActive(false);
        dust.SetActive(false);
        inputManager = drone.GetComponent<DroneInputManager>();
        FlightController controller = drone.GetComponent<FlightController>();
        minSpeedMultiplier = controller.minRPM / controller.maxRPM;
    }

    void UpdateParticles(GameObject particleGO, ParticleSystem particles, RaycastHit hit)
    {
        if(hit.collider != null && inputManager.IsArmed)
        {
            particleGO.transform.rotation = Quaternion.LookRotation(-hit.normal);
            particleGO.transform.position =  hit.point + hit.normal * offsetFromGround;
            var particlesShape = particles.shape;
            particlesShape.radiusThickness = 1 - Math.Clamp(hit.distance / rayDistance, 0.25f, 1);
            var particlesMain = particles.main;
            particlesMain.startSpeed = speed //only this needs to be adjusted in inspector so it feels correct
                                       * (0.25f + 0.75f * (1 - hit.distance / rayDistance)) //dependent on the distance from the ground
                                       * (minSpeedMultiplier + (inputManager.GetThrottleAxis()+1)/2f*(1-minSpeedMultiplier)); //dependent on current throttle
            particleGO.SetActive(true);
        }
        else
        {
            particleGO.SetActive(false);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        Physics.Raycast(drone.transform.position, -drone.transform.up,  out RaycastHit hit, rayDistance);
        UpdateParticles(leaves, leafParticles, hit);
        UpdateParticles(dust, dustParticles, hit);
    }
    
}
