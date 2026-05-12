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


    [Header("Sonidos")]
    public AudioClip sonidoAbrirCaja;
    public AudioClip sonidoRepartirObjeto;
    private AudioSource audioSource;

    [Header("Cursores Especiales")]
    public Texture2D cursorManoCerrada;
    public Texture2D cursorManoApuntar;

    public float distanciaInteraccion = 3.5f;
    public float duracionVuelo = 0.4f;
    public float tiempoEntreObjetos = 0.2f;

    [HideInInspector]
    public bool enemigoRecibeObjetos = false;

    [HideInInspector]
    public bool repartiendoObjetos = false;

    public GameObject[] objetosJugador = new GameObject[8];
    public GameObject[] objetosDealer = new GameObject[8];
    public TipoObjeto[] tiposDealer = new TipoObjeto[8];

    private int capacidadMaxima = 8;
    private bool esperandoClickCaja = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        if (objetoCajaVisual != null) objetoCajaVisual.SetActive(false);
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
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

        if (cartasManager.esperandoCorte) esMiTurno = false;

        bool sinLimites = false;
        if (DificultadManager.DificultadElegida == ModoDificultad.Normal) sinLimites = true;
        else if (DificultadManager.DificultadElegida == ModoDificultad.Personalizado) sinLimites = DificultadManager.CP_SinLimitesObjetos;

        for (int i = 0; i < objetosJugador.Length; i++)
        {
            if (objetosJugador[i] != null)
            {
                ObjetoInteractuable oi = objetosJugador[i].GetComponent<ObjetoInteractuable>();
                if (oi != null)
                {
                    bool usable = esMiTurno;

                    if (repartiendoObjetos)
                    {
                        usable = true;
                    }
                    else
                    {
                        if (!sinLimites)
                        {
                            if (usable && enFaseCartas && cartasManager.objetosUsadosEnCartas >= 2) usable = false;
                            if (usable && enFaseDisparo && disparoManager.objetosUsadosEsteDisparo >= 1) usable = false;
                        }

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
                                case TipoObjeto.Telefono:
                                    usable = enFaseDisparo;
                                    break;
                                case TipoObjeto.Lupa:
                                case TipoObjeto.Guante:
                                case TipoObjeto.Botiquin:
                                    usable = true;
                                    break;
                            }
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

    private TipoObjeto ObtenerObjetoAleatorioDealer()
    {
        TipoObjeto obj;
        do
        {
            obj = (TipoObjeto)UnityEngine.Random.Range(0, 10);
        } while (obj == TipoObjeto.RayosX || (fasesManager.indiceEnemigoActual == 0 && obj == TipoObjeto.Guante));
        return obj;
    }

    private IEnumerator RutinaRepartirObjetos(Action onTerminado)
    {
        repartiendoObjetos = true;

        int cantidadARepartir = UnityEngine.Random.Range(2, 5);

        int cantidadJugador = cantidadARepartir;
        if (DificultadManager.DificultadElegida == ModoDificultad.Dificil)
        {
            cantidadJugador = Mathf.Max(0, cantidadARepartir - 1);
        }
        else if (DificultadManager.DificultadElegida == ModoDificultad.Personalizado)
        {
            cantidadJugador = Mathf.Max(0, cantidadARepartir + DificultadManager.CP_DiferenciaObjetos);
        }

        if (objetoCajaVisual != null && posicionCajaJugador != null)
        {
            objetoCajaVisual.transform.position = posicionCajaJugador.position;
            if (spriteRendererCaja != null) spriteRendererCaja.sprite = spriteCajaCerrada;
            objetoCajaVisual.SetActive(true);
        }

        esperandoClickCaja = true;
        yield return new WaitUntil(() => !esperandoClickCaja);

        if (spriteRendererCaja != null) spriteRendererCaja.sprite = spriteCajaAbierta;
        if (audioSource != null && sonidoAbrirCaja != null) audioSource.PlayOneShot(sonidoAbrirCaja); // <-- PÓNLO AQUÍ
        yield return new WaitForSeconds(0.4f);

        for (int i = 0; i < cantidadJugador; i++)
        {
            int slotLibre = ObtenerSlotLibre(objetosJugador);
            if (slotLibre != -1)
            {
                TipoObjeto nuevoObjeto;
                do
                {
                    nuevoObjeto = (TipoObjeto)UnityEngine.Random.Range(0, 10);
                } while (fasesManager.indiceEnemigoActual == 0 && nuevoObjeto == TipoObjeto.Guante);

                GameObject objAnim = Instantiate(prefabObjetoAnimacion, posicionCajaJugador.position, Quaternion.identity);
                objetosJugador[slotLibre] = objAnim;

                if (audioSource != null && sonidoRepartirObjeto != null) audioSource.PlayOneShot(sonidoRepartirObjeto);
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
            if (audioSource != null && sonidoAbrirCaja != null) audioSource.PlayOneShot(sonidoAbrirCaja);
            yield return new WaitForSeconds(0.4f);

            for (int i = 0; i < cantidadARepartir; i++)
            {
                int slotLibre = ObtenerSlotLibre(objetosDealer);
                if (slotLibre != -1)
                {
                    TipoObjeto nuevoObjeto = ObtenerObjetoAleatorioDealer();

                    GameObject objAnim = Instantiate(prefabObjetoAnimacion, posicionCajaDealer.position, Quaternion.identity);
                    objetosDealer[slotLibre] = objAnim;
                    tiposDealer[slotLibre] = nuevoObjeto;

                    if (audioSource != null && sonidoRepartirObjeto != null) audioSource.PlayOneShot(sonidoRepartirObjeto);
                    yield return StartCoroutine(AnimarObjetoVolando(objAnim, nuevoObjeto, slotsDealer[slotLibre], posicionCajaDealer.position, false));
                    yield return new WaitForSeconds(tiempoEntreObjetos);
                }
            }

            yield return new WaitForSeconds(0.6f);
            if (objetoCajaVisual != null) objetoCajaVisual.SetActive(false);
        }

        repartiendoObjetos = false;
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

    public bool EsObjetoRobableYUsable(TipoObjeto tipo, bool esParaJugador)
    {
        if (tipo == TipoObjeto.Guante) return false;
        if (!esParaJugador && tipo == TipoObjeto.RayosX) return false;

        bool enFaseCartas = fasesManager.contenedorCartas.activeSelf;
        bool enFaseDisparo = fasesManager.contenedorDisparo.activeSelf;

        bool sinLimites = false;
        if (DificultadManager.DificultadElegida == ModoDificultad.Normal) sinLimites = true;
        else if (DificultadManager.DificultadElegida == ModoDificultad.Personalizado) sinLimites = DificultadManager.CP_SinLimitesObjetos;

        if (!sinLimites)
        {
            if (enFaseCartas && cartasManager.objetosUsadosEnCartas >= 2) return false;
            if (enFaseDisparo && disparoManager.objetosUsadosEsteDisparo >= 1) return false;
        }

        if (enFaseCartas)
        {
            if (esParaJugador) return tipo == TipoObjeto.Tijeras || tipo == TipoObjeto.RayosX || tipo == TipoObjeto.Lupa || tipo == TipoObjeto.Botiquin;
            else return tipo == TipoObjeto.Tijeras || tipo == TipoObjeto.Lupa || tipo == TipoObjeto.Botiquin;
        }
        else if (enFaseDisparo)
        {
            return tipo == TipoObjeto.Botiquin || tipo == TipoObjeto.Esposas || tipo == TipoObjeto.Inversor ||
                   tipo == TipoObjeto.Lupa || tipo == TipoObjeto.Muñeco || tipo == TipoObjeto.Polvora ||
                   tipo == TipoObjeto.Telefono;
        }
        return false;
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
                if (enFaseCartas)
                {
                    foreach (var card in cartasManager.playerCards)
                    {
                        if (card != null) objetivos.Add(card);
                    }
                }
                break;
            case TipoObjeto.RayosX:
                if (enFaseCartas)
                {
                    GameObject oculta = cartasManager.ObtenerCartaOculta();
                    if (oculta != null) objetivos.Add(oculta);
                }
                break;
            case TipoObjeto.Guante:
                for (int i = 0; i < objetosDealer.Length; i++)
                {
                    if (objetosDealer[i] != null && EsObjetoRobableYUsable(tiposDealer[i], true))
                    {
                        objetivos.Add(objetosDealer[i]);
                    }
                }
                break;
        }

        foreach (GameObject obj in objetivos)
        {
            if (obj == null) continue;

            if (resaltar)
            {
                if (obj.GetComponent<EfectoLatido>() == null) obj.AddComponent<EfectoLatido>();
            }
            else
            {
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
                    if (objetosDealer[i] != null && EsObjetoRobableYUsable(tiposDealer[i], true))
                        if (Vector3.Distance(posicionObjeto, objetosDealer[i].transform.position) < distanciaInteraccion) return true;
                }
                break;
        }
        return false;
    }

    public bool IntentarUsarObjetoJugador(ObjetoInteractuable obj)
    {
        if (objetoCajaVisual != null && objetoCajaVisual.activeSelf) return false;
        if (cartasManager.esperandoCorte) return false;

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

        bool sinLimites = false;
        if (DificultadManager.DificultadElegida == ModoDificultad.Normal) sinLimites = true;
        else if (DificultadManager.DificultadElegida == ModoDificultad.Personalizado) sinLimites = DificultadManager.CP_SinLimitesObjetos;

        if (!sinLimites)
        {
            if (enFaseCartas && cartasManager.objetosUsadosEnCartas >= 2)
            {
                NotificacionManager.Instance.MostrarMensaje("Límite de 2 objetos por mano.", true, 2f);
                return false;
            }
            if (enFaseDisparo && disparoManager.objetosUsadosEsteDisparo >= 1)
            {
                NotificacionManager.Instance.MostrarMensaje("Límite de 1 objeto por disparo.", true, 2f);
                return false;
            }
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
                if (enFaseCartas) usado = cartasManager.UsarLupa(true);
                else if (enFaseDisparo) usado = disparoManager.UsarLupa(true);
                break;
            case TipoObjeto.Botiquin:
                usado = disparoManager.UsarBotiquin(true);
                break;
            case TipoObjeto.Esposas:
                if (enFaseDisparo) usado = disparoManager.UsarEsposas(true);
                break;
            case TipoObjeto.Polvora:
                if (enFaseDisparo) usado = disparoManager.UsarPolvora(true);
                break;
            case TipoObjeto.Inversor:
                if (enFaseDisparo) usado = disparoManager.UsarInversor(true);
                break;
            case TipoObjeto.Muñeco:
                if (enFaseDisparo) usado = disparoManager.UsarMuñeco(true);
                break;
            case TipoObjeto.Telefono:
                if (enFaseDisparo) usado = disparoManager.UsarTelefono(true);
                break;
            case TipoObjeto.Guante:
                usado = UsarGuanteJugador(obj.transform.position);
                break;
        }

        if (usado)
        {
            if (enFaseCartas) cartasManager.objetosUsadosEnCartas++;
            if (enFaseDisparo) disparoManager.objetosUsadosEsteDisparo++;

            if (indexSlot != -1)
            {
                objetosJugador[indexSlot] = null;
            }
            Destroy(obj.gameObject);
            return true;
        }

        return false;
    }

    private bool EjecutarEfectoInmediato(TipoObjeto tipo, bool esJugador)
    {
        bool enFaseCartas = fasesManager.contenedorCartas.activeSelf;
        bool enFaseDisparo = fasesManager.contenedorDisparo.activeSelf;

        switch (tipo)
        {
            case TipoObjeto.Tijeras:
                if (enFaseCartas) return cartasManager.UsarTijerasAuto(esJugador);
                break;
            case TipoObjeto.RayosX:
                if (enFaseCartas) return cartasManager.UsarRayosX();
                break;
            case TipoObjeto.Lupa:
                if (enFaseCartas) return cartasManager.UsarLupa(esJugador);
                else if (enFaseDisparo) return disparoManager.UsarLupa(esJugador);
                break;
            case TipoObjeto.Botiquin:
                return disparoManager.UsarBotiquin(esJugador);
            case TipoObjeto.Esposas:
                return disparoManager.UsarEsposas(esJugador);
            case TipoObjeto.Polvora:
                return disparoManager.UsarPolvora(esJugador);
            case TipoObjeto.Inversor:
                return disparoManager.UsarInversor(esJugador);
            case TipoObjeto.Muñeco:
                return disparoManager.UsarMuñeco(esJugador);
            case TipoObjeto.Telefono:
                return disparoManager.UsarTelefono(esJugador);
        }
        return false;
    }

    private bool UsarGuanteJugador(Vector3 dropPos)
    {
        int closestIndex = -1;
        float minDistance = float.MaxValue;

        for (int i = 0; i < objetosDealer.Length; i++)
        {
            if (objetosDealer[i] != null && EsObjetoRobableYUsable(tiposDealer[i], true))
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
            TipoObjeto tipoRobado = tiposDealer[closestIndex];

            bool efectoAplicado = EjecutarEfectoInmediato(tipoRobado, true);

            if (efectoAplicado)
            {
                Destroy(objetosDealer[closestIndex]);
                objetosDealer[closestIndex] = null;
                //NotificacionManager.Instance.MostrarMensaje($"¡Has robado y usado: {tipoRobado}!", true, 3f);
                return true;
            }
            else
            {
                NotificacionManager.Instance.MostrarMensaje($"No puedes usar {tipoRobado} en este momento.", true, 2.5f);
                return false;
            }
        }
        return false;
    }

    public bool DealerTieneObjeto(TipoObjeto tipo)
    {
        for (int i = 0; i < objetosDealer.Length; i++)
        {
            if (objetosDealer[i] != null && tiposDealer[i] == tipo) return true;
        }
        return false;
    }

    public bool JugadorTieneObjetosFaseDisparo()
    {
        for (int i = 0; i < objetosJugador.Length; i++)
        {
            if (objetosJugador[i] != null)
            {
                ObjetoInteractuable oi = objetosJugador[i].GetComponent<ObjetoInteractuable>();
                if (oi != null && EsObjetoRobableYUsable(oi.miTipo, false)) return true;
            }
        }
        return false;
    }

    public void IntentarUsarObjetoDealer(TipoObjeto tipo)
    {
        bool enFaseCartas = fasesManager.contenedorCartas.activeSelf;
        bool enFaseDisparo = fasesManager.contenedorDisparo.activeSelf;

        if (enFaseCartas && cartasManager.objetosUsadosEnCartas >= 2) return;
        if (enFaseDisparo && disparoManager.objetosUsadosEsteDisparo >= 1) return;

        int index = -1;
        for (int i = 0; i < objetosDealer.Length; i++)
        {
            if (objetosDealer[i] != null && tiposDealer[i] == tipo)
            {
                index = i;
                break;
            }
        }
        if (index == -1) return;

        StartCoroutine(RutinaAnimarYUsarObjetoDealer(index, tipo));
    }

    private IEnumerator RutinaAnimarYUsarObjetoDealer(int index, TipoObjeto tipo)
    {
        GameObject obj = objetosDealer[index];
        objetosDealer[index] = null;

        Vector3 posInicial = obj.transform.position;
        Vector3 posCentro = Camera.main != null ? new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0f) : Vector3.zero;

        Vector3 escalaInicial = obj.transform.localScale;
        Vector3 escalaFinal = escalaInicial * 2.5f;

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 50;

        float duracion = 0.5f;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / duracion;
            float easedT = t * t * (3f - 2f * t);
            obj.transform.position = Vector3.Lerp(posInicial, posCentro, easedT);
            obj.transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, easedT);
            yield return null;
        }

        yield return new WaitForSeconds(0.6f);

        bool usado = false;
        bool enFaseCartas = fasesManager.contenedorCartas.activeSelf;
        bool enFaseDisparo = fasesManager.contenedorDisparo.activeSelf;

        switch (tipo)
        {
            case TipoObjeto.Botiquin: usado = disparoManager.UsarBotiquin(false); break;
            case TipoObjeto.Lupa:
                if (enFaseCartas) usado = cartasManager.UsarLupa(false);
                else usado = disparoManager.UsarLupa(false);
                break;
            case TipoObjeto.Polvora: usado = disparoManager.UsarPolvora(false); break;
            case TipoObjeto.Esposas: usado = disparoManager.UsarEsposas(false); break;
            case TipoObjeto.Inversor: usado = disparoManager.UsarInversor(false); break;
            case TipoObjeto.Telefono: usado = disparoManager.UsarTelefono(false); break;
            case TipoObjeto.Muñeco: usado = disparoManager.UsarMuñeco(false); break;
            case TipoObjeto.Tijeras: usado = cartasManager.UsarTijerasAuto(false); break;
            case TipoObjeto.Guante: usado = UsarGuanteDealer(); break;
        }

        if (usado)
        {
            if (enFaseCartas) cartasManager.objetosUsadosEnCartas++;
            if (enFaseDisparo) disparoManager.objetosUsadosEsteDisparo++;
        }

        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.2f;
            obj.transform.localScale = Vector3.Lerp(escalaFinal, Vector3.zero, t);
            yield return null;
        }

        Destroy(obj);
    }

    private bool UsarGuanteDealer()
    {
        List<int> validIndices = new List<int>();
        for (int i = 0; i < objetosJugador.Length; i++)
        {
            if (objetosJugador[i] != null)
            {
                ObjetoInteractuable oi = objetosJugador[i].GetComponent<ObjetoInteractuable>();
                if (oi != null && EsObjetoRobableYUsable(oi.miTipo, false))
                {
                    validIndices.Add(i);
                }
            }
        }

        if (validIndices.Count > 0)
        {
            int stolenIndex = validIndices[UnityEngine.Random.Range(0, validIndices.Count)];
            ObjetoInteractuable oi = objetosJugador[stolenIndex].GetComponent<ObjetoInteractuable>();
            TipoObjeto tipoRobado = oi.miTipo;

            bool usado = EjecutarEfectoInmediato(tipoRobado, false);

            if (usado)
            {
                Destroy(objetosJugador[stolenIndex]);
                objetosJugador[stolenIndex] = null;
                NotificacionManager.Instance.MostrarMensaje($"¡El Dealer te robó y usó: {tipoRobado}!", true, 3f);
                return true;
            }
        }
        return false;
    }
}
