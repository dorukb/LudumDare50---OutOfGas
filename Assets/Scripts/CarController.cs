using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public AnimationCurve FuelConsumptionCurve;

    public TextMeshProUGUI velocityText;
    public TextMeshProUGUI cruiseControlText;

    public float VerticalSpeed = 100f;
    public float HorizontalSpeed = 50f;

    public float MinForwardVelocity = 10f;
    public float MinSidewaysVelocity = 5f;

    public float MaxForwardVelocity = 50f;

    public float SidewaysFriction = 10f;

    public float HandbrakeForce = 500f;

    private Rigidbody2D rb;

    private bool isCruiseControlActive = false;

    public float targetCruiseVelocity = 0f;
    Vector2 targetVel;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float vert = Input.GetAxisRaw("Vertical");
        float horz = Input.GetAxisRaw("Horizontal");

        bool handbrakeActive = false;
     
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
                Debug.Log("boost to Min forward");
                rb.velocity = new Vector2(rb.velocity.x, MinForwardVelocity);
            }

            // also handle cruise control here?
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
        }
        else //Manual sideways only friction
        {
            if(rb.velocity.x < 0)
            {
                float decreasedVel = Mathf.Clamp(rb.velocity.x + SidewaysFriction * Time.deltaTime, rb.velocity.x, 0);
                rb.velocity = new Vector2(decreasedVel, rb.velocity.y);
            }
            else if(rb.velocity.x > 0)
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
        
        if(Mathf.Abs(horz) > 0 || Mathf.Abs(vert) > 0 && !handbrakeActive)
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
            Debug.Log("CAP to max forward");
            // cap velocity
            rb.velocity = new Vector2(rb.velocity.x, MaxForwardVelocity);
        }
        else if(rb.velocity.y <= 0.01)
        {
            Debug.Log("reset to 0");
            // can not go backwards.
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }

        velocityText.text = string.Format("Forward:{0}, Side: {1}", rb.velocity.y.ToString(), rb.velocity.x.ToString());
    }

    private void OnCruiseActive()
    {
        targetCruiseVelocity = rb.velocity.y;
        cruiseControlText.text = "Limit set to: " + targetCruiseVelocity.ToString();
    }
    private void OnCruiseDisabled()
    {
        targetCruiseVelocity = -1f;
        cruiseControlText.text = "Press 'C' to activate Cruise control";
    }
}
