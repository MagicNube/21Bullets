using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OpcionesAudio : MonoBehaviour
{
    public AudioMixer mezclador;
    
    [Header("Sliders de Volumen")]
    public Slider sliderMaster;
    public Slider sliderMusica;
    public Slider sliderSFX;

    void Start()
    {
        ConfigurarSlider(sliderMaster, "VolumenMaster");
        ConfigurarSlider(sliderMusica, "VolumenMusica");
        ConfigurarSlider(sliderSFX, "VolumenSFX");
    }

    private void ConfigurarSlider(Slider slider, string parametroNombre)
    {
        if (slider != null)
        {
            float volumenGuardado = PlayerPrefs.GetFloat(parametroNombre, 0.75f);
            slider.value = volumenGuardado;
            
            slider.onValueChanged.AddListener((valor) => CambiarVolumen(parametroNombre, valor));
            
            StartCoroutine(AplicarVolumenSeguro(parametroNombre, volumenGuardado));
        }
    }

    public void CambiarVolumen(string parametroNombre, float valor)
    {
        AplicarVolumen(parametroNombre, valor);
        PlayerPrefs.SetFloat(parametroNombre, valor);
        PlayerPrefs.Save();
    }

    private void AplicarVolumen(string parametroNombre, float valor)
    {
        float volumenEnDecibelios = Mathf.Log10(Mathf.Clamp(valor, 0.0001f, 1f)) * 20f;
        mezclador.SetFloat(parametroNombre, volumenEnDecibelios);
    }

    private IEnumerator AplicarVolumenSeguro(string parametroNombre, float valor)
    {
        yield return null; 
        AplicarVolumen(parametroNombre, valor);
    }
}