using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour
{
    public Transform NextRoadPos;
    public SpriteRenderer mainRenderer;
    public List<CarSpawnOption> CarSpawnOptions; 
    public List<CarSpawnOption> PropSpawnOptions;

    public GameObject regularColliders;
    public GameObject stationColliders;

    public Sprite stationRoad;
    public Sprite regularRoad;
    public void Setup(bool isStation)
    {
        if (isStation)
        {
            mainRenderer.sprite = stationRoad;
            stationColliders.SetActive(true);
            regularColliders.SetActive(false);
        }
        else
        {
            mainRenderer.sprite = regularRoad;
            stationColliders.SetActive(false);
            regularColliders.SetActive(true);
        }
    }
}
