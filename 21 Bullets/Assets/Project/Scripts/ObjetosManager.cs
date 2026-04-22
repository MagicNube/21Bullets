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
    public TipoObjeto[] tiposDealer = new TipoObjeto[8]; 

    private int capacidadMaxima = 8;
    private bool esperandoClickCaja = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        if (objetoCajaVisual != null) objetoCajaVisual.SetActive(false);
    }

    void Update()
    {
        if (esperandoClickCaja && Input.GetMouseButtonDown(0))
        {
            esperandoClickCaja = false;
        }

        ActualizarEstadoTransparenciaObjetos();
    }

    private void ActualizarEstadoTransparenciaObjetos()
    {
        bool enFaseCartas = fasesManager.contenedorCartas.activeSelf;
        bool enFaseDisparo = fasesManager.contenedorDisparo.activeSelf;
        bool esMiTurno = (enFaseCartas && cartasManager.isPlayerTurn) || (enFaseDisparo && disparoManager.turnoDelJugadorActual);

        for (int i = 0; i < objetosJugador.Length; i++)
        {
            if (objetosJugador[i] != null)
            {
                ObjetoInteractuable oi = objetosJugador[i].GetComponent<ObjetoInteractuable>();
                if (oi != null)
                {
                    bool usable = esMiTurno;
                    if (usable)
                    {
                        switch (oi.miTipo)
                        {
                            case TipoObjeto.Tijeras:
                            case TipoObjeto.RayosX: 
                                usable = enFaseCartas; 
                                break;
                            case TipoObjeto.Polvora:
                            case TipoObjeto.Inversor:
                            case TipoObjeto.Muñeco:
                            case TipoObjeto.Esposas: 
                                usable = enFaseDisparo; 
                                break;
                        }
                    }
                    oi.SetActivo(usable);
                }
            }
        }
    }

    public void LimpiarTodosLosObjetos()
    {
        for (int i = 0; i < objetosJugador.Length; i++)
        {
            if (objetosJugador[i] != null) { Destroy(objetosJugador[i]); objetosJugador[i] = null; }
        }
        for (int i = 0; i < objetosDealer.Length; i++)
        {
            if (objetosDealer[i] != null) { Destroy(objetosDealer[i]); objetosDealer[i] = null; }
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
                    tiposDealer[slotLibre] = nuevoObjeto; 

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
                if (sr != null && sr.sprite != null) col.size = sr.sprite.bounds.size;
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

    public void ResaltarZonasValidas(TipoObjeto tipo, bool resaltar)
    {
        List<GameObject> objetivos = new List<GameObject>();
        bool enFaseCartas = fasesManager.contenedorCartas.activeSelf;
        bool enFaseDisparo = fasesManager.contenedorDisparo.activeSelf;

        switch (tipo)
        {
            case TipoObjeto.Lupa:
                if (enFaseCartas && cartasManager.deckTransform != null) objetivos.Add(cartasManager.deckTransform.gameObject);
                if (enFaseDisparo && disparoManager.objetoPistola != null) objetivos.Add(disparoManager.objetoPistola);
                break;
            case TipoObjeto.Polvora:
            case TipoObjeto.Inversor:
            case TipoObjeto.Muñeco:
                if (enFaseDisparo && disparoManager.objetoPistola != null) objetivos.Add(disparoManager.objetoPistola);
                break;
            case TipoObjeto.Botiquin:
            case TipoObjeto.Telefono:
                if (posicionCajaJugador != null) objetivos.Add(posicionCajaJugador.gameObject);
                break;
            case TipoObjeto.Esposas:
                if (enFaseDisparo && posicionCajaDealer != null) objetivos.Add(posicionCajaDealer.gameObject);
                break;
            case TipoObjeto.Tijeras:
                if (enFaseCartas) {
                    foreach (var card in cartasManager.playerCards) {
                        if (card != null) objetivos.Add(card);
                    }
                }
                break;
            case TipoObjeto.RayosX:
                if (enFaseCartas) {
                    GameObject oculta = cartasManager.ObtenerCartaOculta();
                    if (oculta != null) objetivos.Add(oculta);
                }
                break;
            case TipoObjeto.Guante:
                foreach (var obj in objetosDealer) {
                    if (obj != null) objetivos.Add(obj);
                }
                break;
        }

        foreach (GameObject obj in objetivos)
        {
            if (obj == null) continue;
            
            if (resaltar) {
                if (obj.GetComponent<EfectoLatido>() == null) obj.AddComponent<EfectoLatido>();
            } else {
                EfectoLatido latido = obj.GetComponent<EfectoLatido>();
                if (latido != null) Destroy(latido);
            }
        }
    }

    public bool ComprobarZonaValida(TipoObjeto tipo, Vector3 posicionObjeto)
    {
        bool enFaseCartas = fasesManager.contenedorCartas.activeSelf;
        bool enFaseDisparo = fasesManager.contenedorDisparo.activeSelf;

        switch (tipo)
        {
            case TipoObjeto.Lupa:
                if (enFaseCartas && cartasManager.deckTransform != null)
                    if (Vector3.Distance(posicionObjeto, cartasManager.deckTransform.position) < distanciaInteraccion) return true;
                if (enFaseDisparo && disparoManager.objetoPistola != null)
                    if (Vector3.Distance(posicionObjeto, disparoManager.objetoPistola.transform.position) < distanciaInteraccion) return true;
                break;
            case TipoObjeto.Polvora:
            case TipoObjeto.Inversor:
            case TipoObjeto.Muñeco:
                if (enFaseDisparo && disparoManager.objetoPistola != null)
                    if (Vector3.Distance(posicionObjeto, disparoManager.objetoPistola.transform.position) < distanciaInteraccion) return true;
                break;
            case TipoObjeto.Botiquin:
            case TipoObjeto.Telefono:
                if (posicionCajaJugador != null)
                    if (Vector3.Distance(posicionObjeto, posicionCajaJugador.position) < distanciaInteraccion * 1.5f) return true;
                break;
            case TipoObjeto.Esposas:
                if (enFaseDisparo && posicionCajaDealer != null)
                    if (Vector3.Distance(posicionObjeto, posicionCajaDealer.position) < distanciaInteraccion * 1.5f) return true;
                break;
            case TipoObjeto.Tijeras:
                if (enFaseCartas)
                {
                    foreach (GameObject card in cartasManager.playerCards)
                        if (card != null && Vector3.Distance(posicionObjeto, card.transform.position) < distanciaInteraccion) return true;
                }
                break;
            case TipoObjeto.RayosX:
                if (enFaseCartas && cartasManager.dealerHandContainer != null)
                    if (Vector3.Distance(posicionObjeto, cartasManager.dealerHandContainer.position) < distanciaInteraccion) return true;
                break;
            case TipoObjeto.Guante:
                for (int i = 0; i < objetosDealer.Length; i++)
                {
                    if (objetosDealer[i] != null && Vector3.Distance(posicionObjeto, objetosDealer[i].transform.position) < distanciaInteraccion) return true;
                }
                break;
        }
        return false;
    }

    public bool IntentarUsarObjetoJugador(ObjetoInteractuable obj)
    {
        if (objetoCajaVisual != null && objetoCajaVisual.activeSelf) return false;

        bool enFaseCartas = fasesManager.contenedorCartas.activeSelf;
        bool enFaseDisparo = fasesManager.contenedorDisparo.activeSelf;

        bool esMiTurno = false;
        if (enFaseCartas) esMiTurno = cartasManager.isPlayerTurn;
        if (enFaseDisparo) esMiTurno = disparoManager.turnoDelJugadorActual;

        if (!esMiTurno)
        {
            NotificacionManager.Instance.MostrarMensaje("No es tu turno.", true, 2f);
            return false;
        }

        if (!ComprobarZonaValida(obj.miTipo, obj.transform.position)) return false;

        int indexSlot = -1;
        for (int i = 0; i < objetosJugador.Length; i++)
        {
            if (objetosJugador[i] == obj.gameObject)
            {
                indexSlot = i;
                break;
            }
        }

        bool usado = false;

        switch (obj.miTipo)
        {
            case TipoObjeto.Tijeras:
                if (enFaseCartas) usado = cartasManager.UsarTijeras(obj.transform.position, distanciaInteraccion);
                break;
            case TipoObjeto.RayosX:
                if (enFaseCartas) usado = cartasManager.UsarRayosX();
                break;
            case TipoObjeto.Lupa:
                if (enFaseCartas) usado = cartasManager.UsarLupa();
                else if (enFaseDisparo) usado = disparoManager.UsarLupa();
                break;
            case TipoObjeto.Botiquin:
                usado = disparoManager.UsarBotiquin();
                break;
            case TipoObjeto.Esposas:
                if (enFaseDisparo) usado = disparoManager.UsarEsposas();
                break;
            case TipoObjeto.Polvora:
                if (enFaseDisparo) usado = disparoManager.UsarPolvora();
                break;
            case TipoObjeto.Inversor:
                if (enFaseDisparo) usado = disparoManager.UsarInversor();
                break;
            case TipoObjeto.Muñeco:
                if (enFaseDisparo) usado = disparoManager.UsarMuñeco();
                break;
            case TipoObjeto.Telefono:
                usado = disparoManager.UsarTelefono(); 
                break;
            case TipoObjeto.Guante:
                usado = UsarGuante(obj.transform.position, indexSlot);
                break;
        }

        if (usado)
        {
            if (obj.miTipo != TipoObjeto.Guante && indexSlot != -1)
            {
                objetosJugador[indexSlot] = null;
            }
            Destroy(obj.gameObject);
            return true;
        }

        return false;
    }

    private bool UsarGuante(Vector3 dropPos, int slotJugadorOrigen)
    {
        int closestIndex = -1;
        float minDistance = float.MaxValue;

        for (int i = 0; i < objetosDealer.Length; i++)
        {
            if (objetosDealer[i] != null)
            {
                float dist = Vector3.Distance(dropPos, objetosDealer[i].transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestIndex = i;
                }
            }
        }

        if (closestIndex != -1 && minDistance <= distanciaInteraccion)
        {
            if (slotJugadorOrigen == -1) return false;

            TipoObjeto tipoRobado = tiposDealer[closestIndex];

            Destroy(objetosDealer[closestIndex]);
            objetosDealer[closestIndex] = null;

            GameObject objAnim = Instantiate(prefabObjetoAnimacion, slotsDealer[closestIndex].position, Quaternion.identity);
            objetosJugador[slotJugadorOrigen] = objAnim; 

            StartCoroutine(AnimarObjetoVolando(objAnim, tipoRobado, slotsJugador[slotJugadorOrigen], slotsDealer[closestIndex].position, true));

            NotificacionManager.Instance.MostrarMensaje("¡Has robado un objeto!", true, 2f);
            return true;
        }
        return false;
    }
}