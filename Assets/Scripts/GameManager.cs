using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static CarController;

public class GameManager : MonoBehaviour
{

    // im also an audio manager now.
    public VehicleType selectedType;
    public static GameManager Instance;

    public Vehicle motor;
    public Vehicle yellowCar;


    public AudioSource sfxSource;
    public AudioSource bgmSource;
    public AudioSource engineSource;

    public AudioClip handbrakeClip;
    public AudioClip engineClip;
    public AudioClip bgmClip;
    public AudioClip buttonClickClip;
    public AudioClip crashClip;

    public float engineSoundMaxVolume = 0.3f;

    [HideInInspector]
    public Vehicle selectedVehicle;

    public int Highscore = 0;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            SelectYellowCar(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void SelectYellowCar(bool noSound = false)
    {
        if(!noSound) Instance.PlayButtonClick();
        selectedType = VehicleType.YellowCar;
        selectedVehicle = yellowCar;
    }
    public void SelectMotor()
    {
        Instance.PlayButtonClick();
        selectedType = VehicleType.Motor;
        selectedVehicle = motor;
    }

    public void LoadMainScene()
    {
        GameManager.Instance.PlayButtonClick();
        SceneManager.LoadScene("Main");

        Instance.PlayBgmLoop();
    }

    public void PlayCrashSfx()
    {
        sfxSource.PlayOneShot(crashClip, 0.5f);
    }
    public void PlayButtonClick()
    {
        sfxSource.PlayOneShot(buttonClickClip);
    }
    public void PlayHandbrakeSfx()
    {
        if (sfxSource.isPlaying) return;

        sfxSource.loop = false;
        sfxSource.PlayOneShot(handbrakeClip);
    }
    public void StopHandbrakeSfx()
    {
        sfxSource.Stop();
    }
    public void PlayEngineLoop(float volumeFactor, float releasePerct)
    {
        float vol = engineSoundMaxVolume * volumeFactor;
        if (vol > engineSoundMaxVolume) vol = engineSoundMaxVolume;

        if (engineSource.isPlaying)
        {
            if(volumeFactor < 0.05f)
            {
                //start lowering the volume
                engineSource.volume = Mathf.Lerp(engineSource.volume, 0f, releasePerct);
                if(engineSource.volume < 0.01)
                {
                    engineSource.Stop();
                }
            }
            else
            {
                engineSource.volume = vol;
            }
        }
        else
        {
            engineSource.volume = vol;
            engineSource.clip = engineClip;
            engineSource.loop = true;
            engineSource.Play();
        }
    }
    public void StopEngineLoop()
    {
        engineSource.Stop();
    }

    public void StopBgm()
    {
        bgmSource.Stop();
    }
    public void PlayBgmLoop()
    {
        if(bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
        else
        {
            Debug.LogError("Background music clip is null on GameManager!");
        }
    }
}
