using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(AudioSource))]
public class LuzParpadeante : MonoBehaviour
{
    public Light2D luzMesa;
    public AudioClip sonidoParpadeo;
    
    public float intensidadNormal = 1.5f;
    public float intensidadMinima = 0.2f;
    
    public float tiempoEsperaMinimo = 20f;
    public float tiempoEsperaMaximo = 30f;
    
    public float duracionMinimaParpadeo = 0.5f;
    public float duracionMaximaParpadeo = 1.5f;
    
    public float velocidadMinimaFallo = 0.05f;
    public float velocidadMaximaFallo = 0.15f;

    private AudioSource audioSource;

    void Start()
    {
        if (luzMesa == null)
        {
            luzMesa = GetComponent<Light2D>();
        }
        
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        
        luzMesa.intensity = intensidadNormal;
        StartCoroutine(RutinaParpadeo());
    }

    private IEnumerator RutinaParpadeo()
    {
        while (true)
        {
            float tiempoHastaFallo = Random.Range(tiempoEsperaMinimo, tiempoEsperaMaximo);
            yield return new WaitForSeconds(tiempoHastaFallo);

            float duracionFallo = Random.Range(duracionMinimaParpadeo, duracionMaximaParpadeo);
            float tiempoActual = 0f;

            if (sonidoParpadeo != null)
            {
                audioSource.clip = sonidoParpadeo;
                audioSource.loop = true;
                audioSource.Play();
            }

            while (tiempoActual < duracionFallo)
            {
                luzMesa.intensity = Random.Range(intensidadMinima, intensidadNormal);
                float microPausa = Random.Range(velocidadMinimaFallo, velocidadMaximaFallo);
                
                yield return new WaitForSeconds(microPausa);
                tiempoActual += microPausa;
            }

            if (sonidoParpadeo != null)
            {
                audioSource.Stop();
            }

            luzMesa.intensity = intensidadNormal;
        }
    }
}