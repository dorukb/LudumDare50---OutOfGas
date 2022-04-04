using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionController : MonoBehaviour
{
    public Button GoButton;

    public Color unselectedColor;
    public Color selectedColor;

    public Image carImage;
    public Image motorImage;

    private void Start()
    {
        OnSelectCar(true);
    }
    public void OnSelectCar(bool noSound = false)
    {
        Debug.Log("on select car: noSound:" + noSound);
        GameManager.Instance.SelectYellowCar(noSound);
        GoButton.interactable = true;

        carImage.color = selectedColor;
        motorImage.color = unselectedColor;
    }
    public void OnSelectMotor()
    {
        GameManager.Instance.SelectMotor();
        GoButton.interactable = true;

        carImage.color = unselectedColor;
        motorImage.color = selectedColor;
    }

    public void OnConfirmButton()
    {
        GameManager.Instance.LoadMainScene();
    }
}
