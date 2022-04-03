using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarController : MonoBehaviour
{
    public enum VehicleType
    {
        YellowCar,
        Motor,
        // maybe a truck.
    }
    [Header("Station fields")]
    public GameObject StationTriggerPopup;
    public GameObject StationUI;
    public float SlowDownTimeScale = 0.2f;


    [Header("Player Vehicle")]
    public SpriteRenderer playerVehicleRenderer;
    public BoxCollider2D playerCollider;
    public Vector2 motorColliderSize;
    public Vector2 yellowCarColliderSize;

    [Header("Fuel Consumption")]
    public AnimationCurve FuelConsumptionCurve;
    public float MaxConsumption = 50f; // this will be scaled by the curve.
    public float EngineWear = 3f;

    public float handbrakeFuelCostPerct = 8f;
    public float crashFuelCostPerct = 20f;
    public int crashStatusDecrease = 20; // lose 20% engine status.
    public float MaxFuel = 250f;
    public float gasPedalUnpressedMaxDuration = 4f;

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

    private UiController uiController;
    private Vehicle _currVehicle;
    private float lastConsumption = 0f;


    private float gasPedalUnpressedFor = 0f;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        uiController = FindObjectOfType<UiController>();

        Application.targetFrameRate = 120;

        // use selecter vehicle parameters.
        SetPlayerVehicle(GameManager.Instance.selectedVehicle);
    }

    public void OnAnotherCarCrash()
    {
        // lose gasoline here.
        // also slow down.
        crashed = true;
        GameManager.Instance.PlayCrashSfx();
    }
    void Update()
    {
        if (gameOver) return;

        float vert = Input.GetAxisRaw("Vertical");
        float horz = Input.GetAxisRaw("Horizontal");


        bool handbrakeActive = false;
        bool payCrashPenalty = false; 
        
        if (vert <= 0.01f && !isCruiseControlActive)
        {
            gasPedalUnpressedFor += Time.deltaTime;
        }
        else
        {
            gasPedalUnpressedFor = 0f;
        }


        float t = gasPedalUnpressedFor / 10f;
        float velocityRatio = rb.velocity.y / MaxForwardVelocity;
        if (t > 1) t = 1f;
        GameManager.Instance.PlayEngineLoop(vert * velocityRatio, t);

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
            GameManager.Instance.PlayHandbrakeSfx();

            // handbraking disables cruise control automatically.
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
            // crashing disables cruise control automatically.
            isCruiseControlActive = false;
            OnCruiseDisabled();
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
            //float velocityScale = rb.velocity.y / MaxForwardVelocity;
            //Mathf.Clamp(velocityScale, 0f, 0.5f);

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

        if (crashed)
        {
            Debug.Log("crash speed penalty??");
            crashed = false;
            rb.velocity = new Vector2(rb.velocity.x, VelocityAfterCrash);
            payCrashPenalty = true; // lose gas.
        }

        if(rb.velocity.sqrMagnitude < 0.1f)
        {
            GameManager.Instance.StopEngineLoop();
        }
        CalculateScore(rb.velocity);
        CalculateFuelConsumption(vert, rb.velocity, handbrakeActive, payCrashPenalty);

        uiController.ShowSpeed(rb.velocity);
    }

    private void CalculateScore(Vector2 velocity)
    {
        distanceTravelled += velocity.magnitude * Time.deltaTime;
        uiController.UpdateScore((int)(distanceTravelled * ScoreMultiplier));
    }

    private void CalculateFuelConsumption(float vertMove, Vector2 currVelocity, bool usedHandbrake, bool hasCrashed)
    {
        float unscaledConsumption, scaledConsumption, extraConsumptionDueToEngineWear, speedConsumption;

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
            speedConsumption = unscaledConsumption;

            if (currVelocity.sqrMagnitude > 0.01 && !isCruiseControlActive && gasPedalUnpressedFor > 0.1f)
            {
                // then not using the gas pedal at all, shouldnt consumpt any more gas?

                // Slowly decrease consumption to 0.
                // decrease "unscaledConsumption". others will change depending on it.
                float unpressTime = gasPedalUnpressedFor / gasPedalUnpressedMaxDuration;
                unpressTime = Mathf.Clamp(unpressTime, 0f, 1f);
                unscaledConsumption = Mathf.Lerp(0, unscaledConsumption, 1 - unpressTime);
            }
            // worst case, x2 fuel consumption.
            extraConsumptionDueToEngineWear = (MaxEngineDurability - engineDurability) * unscaledConsumption / 100f;
            unscaledConsumption += extraConsumptionDueToEngineWear;
            uiController.UpdateEngineDurability(engineDurability);

            if (usedHandbrake)
            {
                unscaledConsumption += handbrakeFuelCost;
            }
            scaledConsumption = Time.deltaTime * unscaledConsumption;
            //Debug.LogFormat("Consumption for t:{0}, UnscaledVal: {1}, Scaled: {2}", velocityScale, unscaledConsumption, scaledConsumption);

        }
        else
        { // not moving at all!
            unscaledConsumption = scaledConsumption = extraConsumptionDueToEngineWear = speedConsumption = 0f;
        }


        if (hasCrashed)
        {
            scaledConsumption += crashFuelCost;
            uiController.DoCrashFuelDecreaseAnim();
        }

        float consumptionScale = unscaledConsumption / (MaxConsumption + extraConsumptionDueToEngineWear);
        consumptionScale = Mathf.Clamp(consumptionScale, 0f, 1f);
        uiController.UpdateFuelConsumptionIndicatorRotation(consumptionScale);

        remainingFuel -= scaledConsumption;
        uiController.UpdateFuelUI(extraConsumptionDueToEngineWear, speedConsumption, unscaledConsumption, remainingFuel, MaxFuel);
        if (remainingFuel < 0)
        {
            remainingFuel = 0;
            gameOver = true;
            Debug.Log("Out of gas! Game over.");
            uiController.ShowGameOver();
        }
    }

    private void OnCruiseActive()
    {
        gasPedalUnpressedFor = 0f;
        targetCruiseVelocity = rb.velocity.y;
        uiController.CruiseControlActivated(targetCruiseVelocity);
    }
    private void OnCruiseDisabled()
    {
        targetCruiseVelocity = -1f;
        uiController.CruiseControlDisabled();
    }

    public void StationTriggered()
    {
        uiController.OnStationEnter();

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

        uiController.OnStationExit();
    }

    public void GainFuel(float amount)
    {
        remainingFuel = Mathf.Clamp(remainingFuel + amount, remainingFuel, MaxFuel);
    }
    public void DoFuelGainAnim(Action callback)
    {
        uiController.DoFuelIncreaseAnimation(remainingFuel, MaxFuel, callback);
    }
    public void GainDurability(float amount)
    {
        engineDurability = Mathf.Clamp(engineDurability + amount, engineDurability, 100f);
        uiController.OnStationExit();

    }

    public void SetPlayerVehicle(Vehicle selected)
    {
        playerVehicleRenderer.sprite = selected.visual;

        playerCollider.size = selected.colliderSize;

        MaxFuel = selected.MaxFuel;
        MaxConsumption = selected.MaxConsumption;
        MaxEngineDurability = selected.MaxDurability;

        MaxSideWaysVelocity = selected.MaxSideWaysVelocity;
        MaxForwardVelocity = selected.MaxForwardVelocity;
        MinForwardVelocity = selected.MinForwardVelocity;
        MinSidewaysVelocity = selected.MinSidewaysVelocity;

        VerticalSpeed = selected.VerticalSpeed;
        HorizontalSpeed = selected.HorizontalSpeed;

        maxVelocityMgn = new Vector2(MaxSideWaysVelocity, MaxForwardVelocity).magnitude;
        handbrakeFuelCost = handbrakeFuelCostPerct * MaxFuel / 100f;
        crashFuelCost = crashFuelCostPerct * MaxFuel / 100f;

        remainingFuel = MaxFuel;
        engineDurability = MaxEngineDurability;

        _currVehicle = selected; // restart purposes

    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        GameManager.Instance.PlayButtonClick();
        SceneManager.LoadScene("Main");

        //isCruiseControlActive = false;
        //targetCruiseVelocity = 0f;
        //crashed = false;
        //gameOver = false;

        //SetPlayerVehicle(_currVehicle);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        GameManager.Instance.StopEngineLoop();
        GameManager.Instance.StopHandbrakeSfx();
        GameManager.Instance.PlayButtonClick();
        GameManager.Instance.StopBgm();
        SceneManager.LoadScene("Start");
    }

    public void Quit()
    {
        Application.Quit();
    }

}
