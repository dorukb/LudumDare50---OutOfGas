using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StationUIController : MonoBehaviour
{
    enum StationAction
    {
        None,
        BuyFuel,
        Repair
    }

    [Header("UI References")]
    public GameObject allVisuals;

    public Button confirmButton;
    public Image confirmButtonImage;

    public Image buyFuelButtonImage;
    public Image repairButtonImage;
    public TextMeshProUGUI fuelAmountText;
    public TextMeshProUGUI repairAmountText;

    public Color greyedOutColor;

    [Header("Rewards")]
    public float fuelIncrease = 100f;
    public float durabilityIncrease = 30f;

    private CarController player;

    private StationAction selected;
    private void Awake()
    {
        player = FindObjectOfType<CarController>();

        selected = StationAction.BuyFuel;
        UpdateStationUI(selected);

        fuelAmountText.text = "+" + fuelIncrease.ToString();
        repairAmountText.text = "+" + durabilityIncrease.ToString();
    }

    private void Update()
    {
        if (!allVisuals.activeInHierarchy) return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            // left navigate
            BuyFuelSelected();
        }
        else if (Input.GetKeyDown(KeyCode.D)) 
        {
            // right navigate
            RepairSelected();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            // submit
            OnConfirm();
        }
    }
    private void UpdateStationUI(StationAction selected)
    {
        switch (selected)
        {
            case StationAction.None:
                // grey out both, disable confirmation!
                buyFuelButtonImage.color = greyedOutColor;
                repairButtonImage.color = greyedOutColor;
                confirmButton.image.color = greyedOutColor;

                repairAmountText.color = greyedOutColor;
                fuelAmountText.color = greyedOutColor;

                confirmButton.enabled = false;
                break;

            case StationAction.BuyFuel:
                // highlight/color the buy fuel image.
                buyFuelButtonImage.color = Color.white;
                fuelAmountText.color = Color.white;

                // grey-out the other option.
                repairButtonImage.color = greyedOutColor;
                repairAmountText.color = greyedOutColor;

                confirmButton.image.color = Color.white;
                confirmButton.enabled = true;
                break;

            case StationAction.Repair:
                // highlight/color the repair image.
                repairButtonImage.color = Color.white;
                repairAmountText.color = Color.white;

                // grey-out the other option.
                buyFuelButtonImage.color = greyedOutColor;
                fuelAmountText.color = greyedOutColor;

                confirmButton.image.color = Color.white;
                confirmButton.enabled = true;
                break;

            default:
                confirmButton.enabled = false;
                break;
        }
    }

    // Button Callbacks
    public void BuyFuelSelected()
    {
        GameManager.Instance.PlayButtonClick();
        selected = StationAction.BuyFuel;
        UpdateStationUI(selected);
    }

    public void RepairSelected()
    {
        GameManager.Instance.PlayButtonClick();
        selected = StationAction.Repair;
        UpdateStationUI(selected);
    }

    public void OnConfirm()
    {
        GameManager.Instance.PlayButtonClick();
        if (selected == StationAction.BuyFuel)
        {
            Debug.Log("Fuel option chosen. load up gas.");
            player.GainFuel(fuelIncrease);
            player.DoFuelGainAnim(OnFuelIncreaseAnimationEnd);
            allVisuals.SetActive(false);

        }
        else if(selected == StationAction.Repair)
        {
            Debug.Log("Repair option chosen. gain durability.");
            player.GainDurability(durabilityIncrease);
            OnFuelIncreaseAnimationEnd();
        }

    }

    public void OnFuelIncreaseAnimationEnd()
    {
        // Unfreeze, hide this UI.
        Time.timeScale = 1f;

        //re enable childs.
        allVisuals.SetActive(true);


        gameObject.SetActive(false);

        FindObjectOfType<UiController>().OnStationExit();
    }
}