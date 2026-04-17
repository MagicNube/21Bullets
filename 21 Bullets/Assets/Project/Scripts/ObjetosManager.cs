using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TipoObjeto
{
    Botiquin, Esposas, Guante, Inversor, Lupa, 
    Muñeco, Polvora, RayosX, Telefono, Tijeras
}

public class ObjetosManager : MonoBehaviour
{
    public Sprite[] spritesObjetos; 
    public Transform[] slotsJugador; 
    public Transform[] slotsDealer; 

    public GameObject prefabObjetoAnimacion; 
    public GameObject objetoCajaVisual; 
    public SpriteRenderer spriteRendererCaja;
    
    public Sprite spriteCajaCerrada;
    public Sprite spriteCajaAbierta;
    
    public Transform posicionCajaJugador;
    public Transform posicionCajaDealer;

    public float duracionVuelo = 0.4f;
    public float tiempoEntreObjetos = 0.2f;
    
    public bool enemigoRecibeObjetos = false;

    private List<TipoObjeto> inventarioJugador = new List<TipoObjeto>();
    private List<TipoObjeto> inventarioDealer = new List<TipoObjeto>();
    private int capacidadMaxima = 8;
    private bool esperandoClickCaja = false;

    void Awake()
    {
        if (objetoCajaVisual != null)
        {
            objetoCajaVisual.SetActive(false);
        }
    }

    void Update()
    {
        if (esperandoClickCaja && Input.GetMouseButtonDown(0))
        {
            esperandoClickCaja = false;
        }
    }

    public void RepartirObjetosRonda(Action onTerminado)
    {
        StartCoroutine(RutinaRepartirObjetos(onTerminado));
    }

    private IEnumerator RutinaRepartirObjetos(Action onTerminado)
    {
        int cantidadARepartir = UnityEngine.Random.Range(2, 5);

        if (objetoCajaVisual != null && posicionCajaJugador != null)
        {
            objetoCajaVisual.transform.position = posicionCajaJugador.position;
            if (spriteRendererCaja != null) spriteRendererCaja.sprite = spriteCajaCerrada;
            objetoCajaVisual.SetActive(true);
        }

        esperandoClickCaja = true;
        yield return new WaitUntil(() => !esperandoClickCaja);

        if (spriteRendererCaja != null) spriteRendererCaja.sprite = spriteCajaAbierta;
        yield return new WaitForSeconds(0.4f);

        for (int i = 0; i < cantidadARepartir; i++)
        {
            if (inventarioJugador.Count < capacidadMaxima)
            {
                TipoObjeto nuevoObjeto = (TipoObjeto)UnityEngine.Random.Range(0, 10);
                inventarioJugador.Add(nuevoObjeto);
                int indexSlot = inventarioJugador.Count - 1;
                yield return StartCoroutine(AnimarObjetoVolando(nuevoObjeto, slotsJugador[indexSlot], posicionCajaJugador.position));
                yield return new WaitForSeconds(tiempoEntreObjetos);
            }
        }

        yield return new WaitForSeconds(0.6f);
        if (objetoCajaVisual != null) objetoCajaVisual.SetActive(false);

        if (enemigoRecibeObjetos)
        {
            yield return new WaitForSeconds(0.5f);

            if (objetoCajaVisual != null && posicionCajaDealer != null)
            {
                objetoCajaVisual.transform.position = posicionCajaDealer.position;
                if (spriteRendererCaja != null) spriteRendererCaja.sprite = spriteCajaCerrada;
                objetoCajaVisual.SetActive(true);
            }

            yield return new WaitForSeconds(0.8f);

            if (spriteRendererCaja != null) spriteRendererCaja.sprite = spriteCajaAbierta;
            yield return new WaitForSeconds(0.4f);

            for (int i = 0; i < cantidadARepartir; i++)
            {
                if (inventarioDealer.Count < capacidadMaxima)
                {
                    TipoObjeto nuevoObjeto = (TipoObjeto)UnityEngine.Random.Range(0, 10);
                    inventarioDealer.Add(nuevoObjeto);
                    int indexSlot = inventarioDealer.Count - 1;
                    yield return StartCoroutine(AnimarObjetoVolando(nuevoObjeto, slotsDealer[indexSlot], posicionCajaDealer.position));
                    yield return new WaitForSeconds(tiempoEntreObjetos);
                }
            }

            yield return new WaitForSeconds(0.6f);
            if (objetoCajaVisual != null) objetoCajaVisual.SetActive(false);
        }

        onTerminado?.Invoke();
    }

    private IEnumerator AnimarObjetoVolando(TipoObjeto tipo, Transform destino, Vector3 puntoOrigen)
    {
        GameObject objAnim = Instantiate(prefabObjetoAnimacion, puntoOrigen, Quaternion.identity);
        
        SpriteRenderer sr = objAnim.GetComponent<SpriteRenderer>();
        if (sr != null && spritesObjetos.Length > (int)tipo)
        {
            sr.sprite = spritesObjetos[(int)tipo];
        }

        Vector3 posInicial = puntoOrigen;
        Vector3 posFinal = destino.position;
        float tiempo = 0f;

        while (tiempo < duracionVuelo)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracionVuelo;
            t = t * t * (3f - 2f * t); 

            objAnim.transform.position = Vector3.Lerp(posInicial, posFinal, t);
            yield return null;
        }

        objAnim.transform.position = posFinal;
        objAnim.transform.SetParent(destino);
    }
}