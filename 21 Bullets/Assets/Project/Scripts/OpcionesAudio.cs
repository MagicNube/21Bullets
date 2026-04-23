using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OpcionesAudio : MonoBehaviour
{
    public AudioMixer mezclador;
    public Slider sliderVolumen;

    void Start()
    {
        float volumenGuardado = PlayerPrefs.GetFloat("VolumenMaster", 0.75f);
        
        if (sliderVolumen != null)
        {
            sliderVolumen.value = volumenGuardado;
            sliderVolumen.onValueChanged.AddListener(CambiarVolumen);
        }
        
        StartCoroutine(AplicarVolumenSeguro(volumenGuardado));
    }

    public void CambiarVolumen(float valor)
    {
        AplicarVolumen(valor);
        PlayerPrefs.SetFloat("VolumenMaster", valor);
        PlayerPrefs.Save();
    }

    private void AplicarVolumen(float valor)
    {
        float volumenEnDecibelios = Mathf.Log10(Mathf.Clamp(valor, 0.0001f, 1f)) * 20f;
        mezclador.SetFloat("VolumenMaster", volumenEnDecibelios);
    }

    private IEnumerator AplicarVolumenSeguro(float valor)
    {
        yield return null; 
        AplicarVolumen(valor);
    }
}