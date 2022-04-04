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

    public AudioClip repairClip;
    public AudioClip refuelClip;
    public AudioClip outOfGasClip;

    public float engineSoundMaxVolume = 0.3f;

    [HideInInspector]
    public Vehicle selectedVehicle;

    public int Highscore = 0;

    public float engineSoundPlayerSetValue = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            SelectYellowCar(true);
            if(engineSource.volume > engineSoundMaxVolume)
            {
                engineSource.volume = engineSoundMaxVolume;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void SelectYellowCar(bool noSound = false)
    {
        if (!noSound)
        {
            Instance.PlayButtonClick();
        }
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

        engineSource.volume = engineSoundMaxVolume * engineSoundPlayerSetValue;
        SceneManager.LoadScene("Main");

        Instance.PlayBgmLoop();
    }

    public void SetBgmVol(float vol)
    {
        bgmSource.volume = vol;
    }

    public void SetSfxVol(float vol)
    {
        sfxSource.volume = vol;
    }
    public void SetEngineVol(float vol)
    {
        engineSource.volume = vol * engineSoundMaxVolume;

        engineSoundPlayerSetValue = vol * engineSoundMaxVolume;
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
        //Debug.Log("Play engine loop");
        float actualMax = engineSoundMaxVolume * engineSoundPlayerSetValue;

        float vol = actualMax * volumeFactor;
        if (vol > actualMax) vol = actualMax;

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

    public void PlayOutOfGas()
    {
        sfxSource.PlayOneShot(outOfGasClip);
    }
    public void PlayRepairSound()
    {
        sfxSource.PlayOneShot(repairClip);
    }

    public void PlayRefuelSfx()
    {
        sfxSource.PlayOneShot(refuelClip);
    }
    public void StopEngineLoop()
    {
        //Debug.Log("engine loop stopped");
        engineSource.volume = engineSoundMaxVolume* engineSoundPlayerSetValue;
        engineSource.loop = false;
        engineSource.Stop();
    }

    public void StopBgm()
    {
        bgmSource.Stop();
    }
    public void PlayBgmLoop()
    {
        if(bgmClip != null && !bgmSource.isPlaying)
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
