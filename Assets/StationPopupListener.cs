using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationPopupListener : MonoBehaviour
{

    CarController player;
    private void OnEnable()
    {
        player = FindObjectOfType<CarController>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            player.OnStationRequestAccepted();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            player.OnStationRequestRejected();
        }
    }
}
