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
    public static ObjetosManager Instance;

    public FasesManager fasesManager;
    public CartasManager cartasManager;
    public DisparoManager disparoManager;

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
    
    [Header("Cursores Especiales")]
    public Texture2D cursorManoCerrada;
    public Texture2D cursorManoApuntar;

    public float distanciaInteraccion = 3.5f;

    public float duracionVuelo = 0.4f;
    public float tiempoEntreObjetos = 0.2f;
    
    [HideInInspector]
    public bool enemigoRecibeObjetos = false;

    private GameObject[] objetosJugador = new GameObject[8];
    private GameObject[] objetosDealer = new GameObject[8];
    private int capacidadMaxima = 8;
    private bool esperandoClickCaja = false;

    void Awake()
    {
        if (Instance == null) Instance = this;

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

    private int ObtenerSlotLibre(GameObject[] array)
    {
        for (int i = 0; i < capacidadMaxima; i++)
        {
            if (array[i] == null) return i;
        }
        return -1;
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
            int slotLibre = ObtenerSlotLibre(objetosJugador);
            if (slotLibre != -1)
            {
                TipoObjeto nuevoObjeto = (TipoObjeto)UnityEngine.Random.Range(0, 10);
                
                GameObject objAnim = Instantiate(prefabObjetoAnimacion, posicionCajaJugador.position, Quaternion.identity);
                objetosJugador[slotLibre] = objAnim;

                yield return StartCoroutine(AnimarObjetoVolando(objAnim, nuevoObjeto, slotsJugador[slotLibre], posicionCajaJugador.position, true));
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
                int slotLibre = ObtenerSlotLibre(objetosDealer);
                if (slotLibre != -1)
                {
                    TipoObjeto nuevoObjeto = (TipoObjeto)UnityEngine.Random.Range(0, 10);
                    
                    GameObject objAnim = Instantiate(prefabObjetoAnimacion, posicionCajaDealer.position, Quaternion.identity);
                    objetosDealer[slotLibre] = objAnim;

                    yield return StartCoroutine(AnimarObjetoVolando(objAnim, nuevoObjeto, slotsDealer[slotLibre], posicionCajaDealer.position, false));
                    yield return new WaitForSeconds(tiempoEntreObjetos);
                }
            }

            yield return new WaitForSeconds(0.6f);
            if (objetoCajaVisual != null) objetoCajaVisual.SetActive(false);
        }

        onTerminado?.Invoke();
    }

    private IEnumerator AnimarObjetoVolando(GameObject objAnim, TipoObjeto tipo, Transform destino, Vector3 puntoOrigen, bool esDelJugador)
    {
        SpriteRenderer sr = objAnim.GetComponent<SpriteRenderer>();
        if (sr != null && spritesObjetos.Length > (int)tipo)
        {
            sr.sprite = spritesObjetos[(int)tipo];
        }

        if (esDelJugador)
        {
            BoxCollider2D col = objAnim.GetComponent<BoxCollider2D>();
            if (col == null)
            {
                col = objAnim.AddComponent<BoxCollider2D>();
                if (sr != null && sr.sprite != null)
                {
                    col.size = sr.sprite.bounds.size;
                }
            }

            ObjetoInteractuable interactuable = objAnim.GetComponent<ObjetoInteractuable>();
            if (interactuable == null) interactuable = objAnim.AddComponent<ObjetoInteractuable>();
            interactuable.Configurar(tipo, this);
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

    public bool ComprobarZonaValida(TipoObjeto tipo, Vector3 posicionObjeto)
    {
        bool enFaseCartas = fasesManager.contenedorCartas.activeSelf;
        bool enFaseDisparo = fasesManager.contenedorDisparo.activeSelf;

        if (tipo == TipoObjeto.Lupa)
        {
            if (enFaseCartas && cartasManager.deckTransform != null)
            {
                if (Vector3.Distance(posicionObjeto, cartasManager.deckTransform.position) < distanciaInteraccion) return true;
            }
            if (enFaseDisparo && disparoManager.objetoPistola != null)
            {
                if (Vector3.Distance(posicionObjeto, disparoManager.objetoPistola.transform.position) < distanciaInteraccion) return true;
            }
        }
        else if (tipo == TipoObjeto.Tijeras)
        {
            if (enFaseCartas && cartasManager.playerHandContainer != null)
            {
                if (Vector3.Distance(posicionObjeto, cartasManager.playerHandContainer.position) < distanciaInteraccion) return true;
            }
        }
        else if (tipo == TipoObjeto.RayosX)
        {
            if (enFaseCartas && cartasManager.dealerHandContainer != null)
            {
                if (Vector3.Distance(posicionObjeto, cartasManager.dealerHandContainer.position) < distanciaInteraccion) return true;
            }
        }

        return false;
    }

    public bool IntentarUsarObjetoJugador(ObjetoInteractuable obj)
    {
        if (objetoCajaVisual != null && objetoCajaVisual.activeSelf) return false;

        if (!ComprobarZonaValida(obj.miTipo, obj.transform.position)) return false;

        bool enFaseCartas = fasesManager.contenedorCartas.activeSelf;
        bool enFaseDisparo = fasesManager.contenedorDisparo.activeSelf;
        bool usado = false;

        switch (obj.miTipo)
        {
            case TipoObjeto.Tijeras:
                if (enFaseCartas) usado = cartasManager.UsarTijeras();
                break;
            case TipoObjeto.RayosX:
                if (enFaseCartas) usado = cartasManager.UsarRayosX();
                break;
            case TipoObjeto.Lupa:
                if (enFaseCartas) usado = cartasManager.UsarLupa();
                else if (enFaseDisparo) usado = disparoManager.UsarLupa();
                break;
        }

        if (usado)
        {
            Destroy(obj.gameObject);
            return true;
        }

        return false;
    }
}