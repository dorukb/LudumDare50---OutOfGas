using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CarController : MonoBehaviour
{

    [Header("Station fields")]
    public GameObject StationTriggerPopup;
    public GameObject StationUI;
    public float SlowDownTimeScale = 0.2f;

    [Header("UI References")]
    public TextMeshProUGUI velocityText;
    public TextMeshProUGUI cruiseControlText;
    public TextMeshProUGUI fuelConsumptionText;
    public TextMeshProUGUI remainingFuelText;
    public TextMeshProUGUI distanceTravelledText;
    public TextMeshProUGUI engineWearText;

    [Header("Fuel Consumption")]
    public AnimationCurve FuelConsumptionCurve;
    public float MaxConsumption = 50f; // this will be scaled by the curve.
    public float EngineWear = 3f;

    public float handbrakeFuelCostPerct = 8f;
    public float crashFuelCostPerct = 20f;
    public int crashStatusDecrease = 20; // lose 20% engine status.
    public float MaxFuel = 250f;

    [Range(0f, 100f)]
    // efficiency modifier/factor
    public float MaxEngineDurability = 100f;
    public float CarSpecificEfficiencyModifier = 0f; // no efficiency for this one.
    public int ScoreMultiplier = 100;

    [Header("Movement")]
    public float VerticalSpeed = 100f;
    public float HorizontalSpeed = 50f;

    public float MinForwardVelocity = 10f;
    public float MinSidewaysVelocity = 5f;
    public float MaxSideWaysVelocity = 6f;

    public float MaxForwardVelocity = 50f;

    public float SidewaysFriction = 10f;

    public float HandbrakeForce = 500f;
    public float VelocityAfterCrash = 5f;


    private bool isCruiseControlActive = false;
    private float targetCruiseVelocity = 0f;
    private Vector2 targetVel;

    private bool crashed = false;
    private Rigidbody2D rb;
    private float maxVelocityMgn;
    private float handbrakeFuelCost;
    private float crashFuelCost;

    private bool gameOver = false;
    private float remainingFuel;
    private float distanceTravelled = 0f;
    private float engineDurability = 100f;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        maxVelocityMgn = new Vector2(MaxSideWaysVelocity, MaxForwardVelocity).magnitude;
        handbrakeFuelCost = handbrakeFuelCostPerct * MaxFuel / 100f;
        crashFuelCost = crashFuelCostPerct * MaxFuel / 100f;

        remainingFuel = MaxFuel;

        Application.targetFrameRate = 120;
    }

    public void OnAnotherCarCrash()
    {
        // lose gasoline here.
        // also slow down.
        crashed = true;
    }
    void Update()
    {
        if (gameOver) return;

        float vert = Input.GetAxisRaw("Vertical");
        float horz = Input.GetAxisRaw("Horizontal");

        bool handbrakeActive = false;
        bool payCrashPenalty = false;

        //Debug.LogFormat("Raw: ({0},{1})", horz, vert);
        Vector2 normalizedInputs = new Vector2(horz, vert).normalized;

        // to keep the code below unchanged, do this trick to use normalized values!.
        horz = normalizedInputs.x;
        vert = normalizedInputs.y;
        //Debug.LogFormat("normalized: ({0},{1})", horz, vert);

        if (Input.GetKeyDown(KeyCode.C))
        {   // toggle cruise control
            isCruiseControlActive = !isCruiseControlActive;

            if (isCruiseControlActive)
            {
                // Set current velocity as target velocity until cruise control is disabled.
                OnCruiseActive();
            }
            else
            {
                OnCruiseDisabled();
            }
        }
        if (Input.GetKey(KeyCode.Space) && rb.velocity.y > 0.1)
        {
            // Handbrake, very fast deceleration.
            rb.AddForce(new Vector2(0, -HandbrakeForce * Time.deltaTime), ForceMode2D.Force);
            handbrakeActive = true;

            // handbraking disabled cruise control automatically.
            isCruiseControlActive = false;
            OnCruiseDisabled();
        }
        if (vert > 0 && !handbrakeActive)
        {
            if (rb.velocity.y < 0.5f)
            {
                rb.velocity = new Vector2(rb.velocity.x, MinForwardVelocity);
            }
        }
        if (crashed)
        {
            crashed = false;
            rb.velocity = new Vector2(rb.velocity.x, VelocityAfterCrash);
            payCrashPenalty = true; // lose gas.
        }

        #region Sideways movement
        if (Mathf.Abs(horz) > 0)
        {
            if (Mathf.Abs(rb.velocity.x) < MinSidewaysVelocity)
            {
                float sign = 1f;
                if (horz < 0) sign = -1f;
                rb.velocity = new Vector2(MinSidewaysVelocity * sign, rb.velocity.y);
            }
            if ((horz > 0 && rb.velocity.x < 0)
                || horz < 0 && rb.velocity.x > 0)
            {
                // want to move to opposition direction as currently headed(in X)
                // reset it first to 0 for quicker "turning"
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }
        else //Manual sideways only friction
        {
            if (rb.velocity.x < 0)
            {
                float decreasedVel = Mathf.Clamp(rb.velocity.x + SidewaysFriction * Time.deltaTime, rb.velocity.x, 0);
                rb.velocity = new Vector2(decreasedVel, rb.velocity.y);
            }
            else if (rb.velocity.x > 0)
            {
                float decreasedVel = Mathf.Clamp(rb.velocity.x - SidewaysFriction * Time.deltaTime, 0, rb.velocity.x);
                rb.velocity = new Vector2(decreasedVel, rb.velocity.y);
            }
        }
        #endregion


        if (vert < 0)
        {
            // slow down request, disable cruise control automatically.
            isCruiseControlActive = false;
            OnCruiseDisabled();
        }

        if (Mathf.Abs(horz) > 0 || Mathf.Abs(vert) > 0 && !handbrakeActive)
        {
            if (isCruiseControlActive)
            {
                // do not Accelerate in Y.
                rb.AddForce(new Vector2(horz * HorizontalSpeed * Time.deltaTime, 0), ForceMode2D.Force);
            }
            else
            {
                rb.AddForce(new Vector2(horz * HorizontalSpeed * Time.deltaTime, vert * VerticalSpeed * Time.deltaTime), ForceMode2D.Force);
            }
        }
        if (isCruiseControlActive)
        {   // Just maintain current Y velocity.
            rb.AddForce(new Vector2(horz * HorizontalSpeed * Time.deltaTime, 0), ForceMode2D.Force);

            targetVel.x = rb.velocity.x; // maintain same X
            targetVel.y = targetCruiseVelocity; // set to cruise vevl
            rb.velocity = targetVel;
        }

        if (rb.velocity.y >= MaxForwardVelocity)
        {
            //Debug.Log("CAP to max forward");
            // cap velocity
            rb.velocity = new Vector2(rb.velocity.x, MaxForwardVelocity);
        }
        else if (rb.velocity.y <= 0.01)
        {
            // can not go backwards.
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
        if (rb.velocity.x < -MaxSideWaysVelocity)
        {
            rb.velocity = new Vector2(-MaxSideWaysVelocity, rb.velocity.y);
        }
        else if (rb.velocity.x > MaxSideWaysVelocity)
        {
            rb.velocity = new Vector2(MaxSideWaysVelocity, rb.velocity.y);
        }

        CalculateScore(rb.velocity);
        CalculateFuelConsumption(rb.velocity, handbrakeActive, payCrashPenalty);
        velocityText.text = string.Format("Speed:{0}", rb.velocity.y.ToString("F1"));
    }

    private void CalculateScore(Vector2 velocity)
    {
        distanceTravelled += velocity.magnitude * Time.deltaTime;
        UpdateScore(distanceTravelled * ScoreMultiplier);
    }

    private void CalculateFuelConsumption(Vector2 currVelocity, bool usedHandbrake, bool hasCrashed)
    {

        // currVelocity scaled to [0,1]
        float unscaledConsumption, scaledConsumption;
        if(currVelocity.sqrMagnitude > 0.01)
        {

            // calculate engine wear & tear
            float wearAmount = EngineWear * Time.deltaTime;
            if (hasCrashed)
            {
                wearAmount += crashStatusDecrease;
            }
            engineDurability -= wearAmount;
            if(engineDurability < 0f)
            {
                engineDurability = 0f;
            }
            
            // moving, calculate Fuel consumption
            float velocityScale = currVelocity.magnitude / maxVelocityMgn;
            unscaledConsumption = FuelConsumptionCurve.Evaluate(velocityScale) * MaxConsumption;

            // worst case, x2 fuel consumption.
            float extraConsumptionDueToEngineWear = (MaxEngineDurability - engineDurability) * unscaledConsumption / 100f;
            unscaledConsumption += extraConsumptionDueToEngineWear;


            UpdateEngineDurability(engineDurability, extraConsumptionDueToEngineWear);

            if (usedHandbrake)
            {
                unscaledConsumption += handbrakeFuelCost;
            }
            if (hasCrashed)
            {
                unscaledConsumption += crashFuelCost;
            }
            scaledConsumption = Time.deltaTime * unscaledConsumption;
            //Debug.LogFormat("Consumption for t:{0}, UnscaledVal: {1}, Scaled: {2}", velocityScale, unscaledConsumption, scaledConsumption);

        }
        else
        {
            unscaledConsumption = scaledConsumption = 0f;
        }

        // how to calculate consumption per second? (to show on UI)

        remainingFuel -= scaledConsumption;
        if(remainingFuel < 0)
        {
            remainingFuel = 0;
            gameOver = true;
            Debug.Log("Out of gas! Game over.");
        }
        UpdateFuelUI(unscaledConsumption, remainingFuel);
    }

    private void UpdateEngineDurability(float currDurability, float extraConsumption)
    {
        engineWearText.text = string.Format("Engine Durability: {0} \n Extra consumption due to wear:{1}", currDurability.ToString("F0"), extraConsumption.ToString("F1"));
    }
    private void UpdateScore(float score)
    {
        distanceTravelledText.text = "Score: " + ((int)score).ToString();
    }
    private void UpdateFuelUI(float currConsumption, float remainingFuel)
    {
        fuelConsumptionText.text = "Current Consumption: " + currConsumption.ToString("F2");
        remainingFuelText.text = "Remaining Fuel: " + remainingFuel.ToString("F1");
    }
    private void OnCruiseActive()
    {
        targetCruiseVelocity = rb.velocity.y;
        cruiseControlText.text = "Cruising at: " + targetCruiseVelocity.ToString("F1");
    }
    private void OnCruiseDisabled()
    {
        targetCruiseVelocity = -1f;
        cruiseControlText.text = "Press 'C' to activate Cruise control";
    }


    public void StationTriggered()
    {
        // slow down time?
        Time.timeScale = SlowDownTimeScale;

        // show "visit the station?" sth on UI
        StationTriggerPopup.SetActive(true);

        // enter OR esc to accept or refuse station interaction.
    }

    public void OnStationRequestAccepted()
    {
        Time.timeScale = 0f; // completely freeze
        StationTriggerPopup.SetActive(false); // hide popup

        // show actual station UI
        // where player can buy gas, or do repair.
        StationUI.SetActive(true);
    }
    public void OnStationRequestRejected()
    {
        Time.timeScale = 1f; //unfreeze
        StationTriggerPopup.SetActive(false); // hide popup
    }
}
