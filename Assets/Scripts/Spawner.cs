using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject RoadPrefab;
    public GameObject StationPrefab;

    public List<GameObject> VehiclePrefabs;

    public float CarSpawnProbability = 0.5f; //half of the tile will have cars.
    public int roadPoolSize = 5;
    public int vehiclePoolSize = 25;

    public int stationSpawnPeriod = 5;

    private GameObject[] roads;
    private GameObject[] vehicles;

    private int activeRoadIndex = 0;
    private int activeVehicleIndex = 0;

    private int roadSpawnCount = 0;
    private void Awake()
    {
        roads = new GameObject[roadPoolSize];
        vehicles = new GameObject[vehiclePoolSize];

        for (int i = 0; i < roadPoolSize; i++)
        {
            roads[i] = Instantiate(RoadPrefab, Vector3.zero, Quaternion.identity);
            roads[i].SetActive(false);
            roads[i].GetComponent<Road>().Setup(false);
        }
        activeRoadIndex = 0;

        for(int i = 0; i < vehiclePoolSize; i++)
        {
            var randVehiclePrefab = VehiclePrefabs[Random.Range(0, VehiclePrefabs.Count)];
            vehicles[i] = Instantiate(randVehiclePrefab, Vector3.zero, Quaternion.identity);
            vehicles[i].SetActive(false);
        }
        activeVehicleIndex = 0;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("RoadSpawn"))
        {
            Road roadScript = collision.GetComponent<Road>();
            if (roadScript != null)
            {
                
                GameObject newRoad = roads[activeRoadIndex];
                newRoad.transform.position = roadScript.NextRoadPos.position;
                newRoad.SetActive(true);
                newRoad.name = activeRoadIndex.ToString();
                activeRoadIndex = (activeRoadIndex + 1) % roadPoolSize;
                roadSpawnCount++;

                if (roadSpawnCount % stationSpawnPeriod == 0)
                {
                    // Setup as StationRoad
                    newRoad.GetComponent<Road>().Setup(true);
                }
                else
                {
                    // Setup as regular road tile
                    newRoad.GetComponent<Road>().Setup(false);
                }


                if (CarSpawnProbability > Random.Range(0f,1f))
                {
                    // prob check passed, spawn cars in this tile.

                    // Setup vehicles on that tile
                    CarSpawnOption randSpawnOption = roadScript.CarSpawnOptions[Random.Range(0, roadScript.CarSpawnOptions.Count)];
                    foreach (Transform spawnPos in randSpawnOption.SpawnPositions)
                    {
                        // setup a vehicle from the pool to appear at that position.
                        GameObject vehicle = vehicles[activeVehicleIndex];
                        vehicle.transform.position = spawnPos.position;

                        // also give initial velocity to this vehicle.
                        NpcVehicle vehicleScript = vehicle.GetComponent<NpcVehicle>();
                        vehicleScript.ResetCar();

                        vehicle.SetActive(true);
                        vehicle.name = activeVehicleIndex.ToString();
                        activeVehicleIndex = (activeVehicleIndex + 1) % vehiclePoolSize;
                    }
                }
            }
        }
    }
}
