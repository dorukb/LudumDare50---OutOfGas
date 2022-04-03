using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newVehicle", menuName ="Vehicle")]
public class Vehicle : ScriptableObject
{
    public Sprite visual;

    // default values are for the Yellow car.
    public float MaxFuel = 900f;
    public float MaxDurability = 100f;
    public float MaxConsumption = 20f;

    public float VerticalSpeed = 150f;
    public float HorizontalSpeed = 75f;

    public float MinForwardVelocity = 10f;
    public float MinSidewaysVelocity = 3f;
    public float MaxSideWaysVelocity = 6f;
    public float MaxForwardVelocity = 35f;
    //public float CarSpecificEfficiencyModifier = 0f; // no efficiency for this one.

    public Vector2 colliderSize; // car: (3.7, 7.6); motor: (3, 6.2);

}
