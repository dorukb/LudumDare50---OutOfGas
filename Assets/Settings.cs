using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public float musicVol = -1f;
    public float sfxVol = -1f;
    public float engineVol = -1f;

    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider engineSlider;

    public GameObject SettingsPage;

    private void Start()
    {
        GameManager.Instance.StopEngineLoop();
    }
    public void ShowSettingsPage()
    {
        SettingsPage.SetActive(true);

        musicSlider.value = GameManager.Instance.bgmSource.volume;
        sfxSlider.value = GameManager.Instance.sfxSource.volume;
        float engineUnscaled = GameManager.Instance.engineSource.volume;
        float engine01 = engineUnscaled / GameManager.Instance.engineSoundMaxVolume;

        engineSlider.value = engine01;

    }
    public void MusicVolumeSlider(float val)
    {
        //Debug.Log("Music volume: " + val);
        GameManager.Instance.SetBgmVol(val);
    }

    public void SfxVolumeSlider(float val)
    {
        //Debug.Log("Sfx volume: " + val);
        GameManager.Instance.SetSfxVol(val);
    }

    public void EngineVolumeSlider(float val)
    {
        //Debug.Log("Engine volume: " + val);
        GameManager.Instance.SetEngineVol(val);
    }

    public void PlayButtonClick()
    {
        GameManager.Instance.PlayButtonClick();
    }
}
