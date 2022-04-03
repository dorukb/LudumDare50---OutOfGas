using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{

    [Header("Speed")]
    public TextMeshProUGUI velocityText;
    public TextMeshProUGUI cruiseControlText;
    public Animator cruiseAnimator;
    public GameObject cKey;

    [Header("Fuel")]
    public TextMeshProUGUI totalConsumptionText;
    public TextMeshProUGUI baseConsumptionText;
    public TextMeshProUGUI wearConsumptionText;

    public TextMeshProUGUI remainingFuelPercentageText;
    public Image fuelIndicator;

    public TextMeshProUGUI engineWearText;
    public Animator crashFuelAnimator;
    public RectTransform fuelConsumptionIndicatorAnchor;
    public float minZRot = 0f;
    public float maxZRot = -78f;

    [Header("Score")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highscoreText;
    public GameObject gameOverScreen;
    public TextMeshProUGUI goScoreText;
    public TextMeshProUGUI goHighscoreText;

    [Header("Animations")]
    public float fuelIncreaseAnimDuration = 0.5f;
    public float flashInterval = 0.05f;
    public Color flashColor;
    public Color darkColor;

    [Header("Pause Screen")]
    public GameObject PauseUI;
    public GameObject HowToPlayUI;


    private Color originalColor;
    private bool fuelIncreaseAnimActive = false;
    private bool canPause = true;

    private bool isPaused = false;
    private void Awake()
    {
        originalColor = fuelIndicator.color;
        gameOverScreen.SetActive(false);
    }

    private void Update()
    {
        if (canPause && Input.GetKeyDown(KeyCode.Escape))
        {
            if(!isPaused)
            {
                //pause
                Time.timeScale = 0f;
                PauseUI.SetActive(true);
                isPaused = true;
            }
            else
            {
                //resume
                Time.timeScale = 1f;
                PauseUI.SetActive(false);
                isPaused = false;
            }
        }
    }
    public void OnHowToPlayButton()
    {
        if (HowToPlayUI.activeSelf)
        {
            HowToPlayUI.SetActive(false);
        }
        else
        {
            HowToPlayUI.SetActive(true);
        }
    }
    public void OnResumeButton()
    {
        PauseUI.SetActive(false);
        Time.timeScale = 1f;
        PauseUI.SetActive(false);
        isPaused = false;
    }
    public void OnStationEnter()
    {
        canPause = false;
        isPaused = false;
    }
    public void OnStationExit()
    {
        canPause = true;
        isPaused = false;
    }
    public void UpdateEngineDurability(float currDurability)
    {
        engineWearText.text = "%" + currDurability.ToString("F1");//, extraConsumption.ToString("F1"));
    }
    public void UpdateScore(int score)
    {
        scoreText.text = "SCORE: " + score.ToString();
        if(score > GameManager.Instance.Highscore)
        {
            GameManager.Instance.Highscore = score;
        }
        highscoreText.text = "HIGHSCORE: " + GameManager.Instance.Highscore.ToString();
    }


    // [0,1] input 
    public void UpdateFuelConsumptionIndicatorRotation(float consumptionScale)
    {
        fuelConsumptionIndicatorAnchor.rotation = Quaternion.Euler(0f, 0f, consumptionScale * maxZRot);
    }
    public void UpdateFuelUI(float wearConsumption, float baseConsumption, float totalConsump, float remainingFuel, float maxFuel)
    {
        wearConsumptionText.text = "WEAR: +" + wearConsumption.ToString("F1");
        baseConsumptionText.text = "BASE: " + baseConsumption.ToString("F1");
        totalConsumptionText.text = totalConsump.ToString("F1");

        remainingFuelPercentageText.text = "%" + (remainingFuel/maxFuel *100f).ToString("F1");

        if (!fuelIncreaseAnimActive)
        {
            fuelIndicator.fillAmount = remainingFuel / maxFuel;
        }
    }

    public void DoCrashFuelDecreaseAnim()
    {
        crashFuelAnimator.SetTrigger("Crash");
      
    }
    public void DoFuelIncreaseAnimation(float currFuel, float maxFuel, Action callback)
    {
        fuelIncreaseAnimActive = true;
        StartCoroutine(FuelIncreaseAnim(currFuel / maxFuel, callback));
    }
    public void CruiseControlActivated(float velocity)
    {
        cruiseControlText.text = "LIMIT SET TO: " + velocity.ToString("F1") + "\n PRESS C TO DISABLE.";

        cKey.SetActive(false);
        cruiseAnimator.SetBool("cruiseActive", true);
    }
    public void CruiseControlDisabled()
    {
        cKey.SetActive(true);
        cruiseAnimator.SetBool("cruiseActive", false);
        cruiseControlText.text = "";
    }
    public void ShowGameOver()
    {
        gameOverScreen.SetActive(true);
        goScoreText.text = scoreText.text;
        goHighscoreText.text = highscoreText.text;
    }
    public void ShowSpeed(Vector2 vel)
    {
        velocityText.text = vel.y.ToString("F1");
    }

    private IEnumerator FuelIncreaseAnim(float targetFillAmount, Action callback)
    {
        float timePassed = 0f;

        float currFill = fuelIndicator.fillAmount;

        float lastFlickerTime = 0f;
        bool shouldFlash = true;

        while(timePassed <= fuelIncreaseAnimDuration)
        {
            timePassed += Time.unscaledDeltaTime;
            float t = timePassed / fuelIncreaseAnimDuration;
            // fill the bar gradually
            fuelIndicator.fillAmount = Mathf.Lerp(currFill, targetFillAmount, t);

            if(timePassed > lastFlickerTime + flashInterval)
            {
                if (shouldFlash)
                {
                    fuelIndicator.color = flashColor;
                }
                else
                {
                    fuelIndicator.color = darkColor;
                }
                shouldFlash = !shouldFlash;
                lastFlickerTime = timePassed;
            }

            yield return new WaitForEndOfFrame(); 
        }

        fuelIndicator.color = originalColor;
        fuelIncreaseAnimActive = false;
        callback?.Invoke();
    }
}
