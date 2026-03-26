using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimacionTambor : MonoBehaviour
{
    public RectTransform[] huecosTambor;
    public RectTransform puntoAparicionBalas;
    public GameObject prefabBalaReal;
    public GameObject prefabBalaFogueo;

    public AudioSource audioSource;
    public AudioClip sonidoClickBala;
    public AudioClip sonidoGiroTambor;

    public float duracionCaida = 0.15f;
    public float tiempoEntreBalas = 0.3f;
    public float velocidadGiro = 1500f;
    public float duracionGiro = 1.0f;

    public void IniciarSecuencia(int cantidadReales, int cantidadFogueo, Action onAnimacionTerminada)
    {
        gameObject.SetActive(true);
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        LimpiarBalasViejas();
        StartCoroutine(RutinaAnimacionCarga(cantidadReales, cantidadFogueo, onAnimacionTerminada));
    }

    private void LimpiarBalasViejas()
    {
        foreach (RectTransform hueco in huecosTambor)
        {
            foreach (Transform hijo in hueco)
            {
                Destroy(hijo.gameObject);
            }
        }
    }

    private IEnumerator RutinaAnimacionCarga(int reales, int fogueo, Action onTerminado)
    {
        int huecoActual = 0;

        for (int i = 0; i < reales; i++)
        {
            if (huecoActual < huecosTambor.Length)
            {
                yield return StartCoroutine(MoverBalaHaciaHueco(prefabBalaReal, huecosTambor[huecoActual]));
                huecoActual++;
                yield return new WaitForSeconds(tiempoEntreBalas);
            }
        }

        for (int i = 0; i < fogueo; i++)
        {
            if (huecoActual < huecosTambor.Length)
            {
                yield return StartCoroutine(MoverBalaHaciaHueco(prefabBalaFogueo, huecosTambor[huecoActual]));
                huecoActual++;
                yield return new WaitForSeconds(tiempoEntreBalas);
            }
        }

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(RutinaGirarTambor());

        gameObject.SetActive(false);

        onTerminado?.Invoke();
    }

    private IEnumerator MoverBalaHaciaHueco(GameObject prefab, RectTransform destino)
    {
        GameObject nuevaBala = Instantiate(prefab, puntoAparicionBalas.position, Quaternion.identity, destino);
        RectTransform rectBala = nuevaBala.GetComponent<RectTransform>();

        Vector3 startPos = puntoAparicionBalas.position;
        Vector3 endPos = destino.position;

        rectBala.position = startPos;
        float tiempo = 0f;

        while (tiempo < duracionCaida)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracionCaida;
            t = t * t;

            rectBala.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        rectBala.position = endPos;

        if (audioSource != null && sonidoClickBala != null)
        {
            audioSource.PlayOneShot(sonidoClickBala);
        }
    }

    private IEnumerator RutinaGirarTambor()
    {
        if (audioSource != null && sonidoGiroTambor != null)
        {
            audioSource.PlayOneShot(sonidoGiroTambor);
        }

        float tiempoPasado = 0f;
        Transform miRect = transform;

        while (tiempoPasado < duracionGiro)
        {
            miRect.Rotate(0, 0, velocidadGiro * Time.deltaTime);
            tiempoPasado += Time.deltaTime;
            yield return null;
        }
    }
}
