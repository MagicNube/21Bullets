using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class DisparoManager : MonoBehaviour
{
    public FasesManager fasesManager;
    public DealerIA dealerIA;
    public EstacionBalasControl estacionBalas;

    [Header("Visuales Pistola")]
    public GameObject objetoPistola;
    public Animator animadorPistola;

    public Sprite spriteBalaReal;
    public Sprite spriteBalaFogueo;
    public Sprite spriteBalaDesconocida;
    public Sprite corazonLleno;
    public Sprite corazonVacio;

    public List<SpriteRenderer> slotsBalas;
    public Image[] corazonesJugador;
    public Image[] corazonesDealer;

    public TMP_Text textoReales;
    public TMP_Text textoFogueo;
    public Button botonYo;
    public Button botonDealer;

    public AudioClip sonidoDisparoReal;
    public AudioClip sonidoDisparoFogueo;

    private AudioSource audioSource;
    private List<bool> tambor = new List<bool>();
    private int indiceActual = 0;

    private int totalRealesCalculadas = 0;
    private int totalFogueosCalculadas = 0;

    private int vidasJugador = 3;
    public int vidasDealer = 3;
    public int vidasMaximasDealer = 3;

    private bool volverAFaseCartas = false;

    public bool turnoDelJugadorActual = false;
    private bool polvoraActivaJugador = false;
    private bool polvoraActivaDealer = false;
    private bool esposasActivasJugador = false;
    private bool esposasActivasDealer = false;

    public bool jugadorInmuneAEsposas = false;
    public bool dealerInmuneAEsposas = false;

    public int objetosUsadosEsteDisparo = 0;

    [Header("Efectos Adicionales")]
    public AudioMixerGroup sfxMixerGroup;
    public AudioSource heartbeatSource;
    public AudioClip sonidoHeartbeat;
    public Image pantallaRojaDaño;
    public GameObject muzzleFlashPistola;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        vidasJugador = corazonesJugador.Length;
        ActualizarCorazonesUI();

        if (sfxMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
            if (heartbeatSource != null) heartbeatSource.outputAudioMixerGroup = sfxMixerGroup;
        }

        if (pantallaRojaDaño != null)
        {
            Color c = pantallaRojaDaño.color;
            c.a = 0f;
            pantallaRojaDaño.color = c;
            pantallaRojaDaño.gameObject.SetActive(false);
        }

        if (muzzleFlashPistola != null) muzzleFlashPistola.SetActive(false);

        if (heartbeatSource != null)
        {
            heartbeatSource.loop = true;
            heartbeatSource.clip = sonidoHeartbeat;
        }

        if (objetoPistola != null) objetoPistola.SetActive(false);

        if (botonYo != null)
        {
            botonYo.onClick.RemoveAllListeners();
            botonYo.onClick.AddListener(DispararAYo);
        }

        if (botonDealer != null)
        {
            botonDealer.onClick.RemoveAllListeners();
            botonDealer.onClick.AddListener(DispararADealer);
        }
    }

    public void ResetPartidaCompleta(int vidasMaximasEnemigo)
    {
        bool esPrimerEnemigo = (fasesManager != null && fasesManager.indiceEnemigoActual == 0);

        if (esPrimerEnemigo)
        {
            vidasJugador = corazonesJugador.Length;
        }
        else
        {
            int curacion = 0;
            if (DificultadManager.DificultadElegida == ModoDificultad.Normal) curacion = corazonesJugador.Length;
            else if (DificultadManager.DificultadElegida == ModoDificultad.Dificil) curacion = 2;
            else curacion = DificultadManager.CP_VidasIniciales;

            vidasJugador = Mathf.Clamp(vidasJugador + curacion, 1, corazonesJugador.Length);
        }

        vidasDealer = vidasMaximasEnemigo;
        vidasMaximasDealer = vidasMaximasEnemigo;
        ActualizarCorazonesUI();

        tambor.Clear();
        indiceActual = 0;
        polvoraActivaJugador = false;
        polvoraActivaDealer = false;
        esposasActivasJugador = false;
        esposasActivasDealer = false;
        jugadorInmuneAEsposas = false;
        dealerInmuneAEsposas = false;
        volverAFaseCartas = false;
        objetosUsadosEsteDisparo = 0;

        if (estacionBalas != null) estacionBalas.gameObject.SetActive(false);

        ActualizarContadoresTexto(0, 0);
    }

    public void PrepararNuevaRonda()
    {
        indiceActual = 0;
        tambor = new List<bool>();
        polvoraActivaJugador = false;
        polvoraActivaDealer = false;
        esposasActivasJugador = false;
        esposasActivasDealer = false;
        jugadorInmuneAEsposas = false;
        dealerInmuneAEsposas = false;
        objetosUsadosEsteDisparo = 0;

        totalRealesCalculadas = UnityEngine.Random.Range(2, 5);
        totalFogueosCalculadas = 6 - totalRealesCalculadas;

        for (int i = 0; i < totalRealesCalculadas; i++) tambor.Add(true);
        for (int i = 0; i < totalFogueosCalculadas; i++) tambor.Add(false);

        for (int i = 0; i < tambor.Count; i++)
        {
            bool temp = tambor[i];
            int randomIndex = UnityEngine.Random.Range(i, tambor.Count);
            tambor[i] = tambor[randomIndex];
            tambor[randomIndex] = temp;
        }

        foreach (SpriteRenderer slot in slotsBalas)
        {
            if (slot != null)
            {
                slot.sprite = spriteBalaDesconocida;
                slot.color = Color.white;
            }
        }

        ActualizarContadoresTexto(ObtenerRealesRestantes(), ObtenerFogueosRestantes());
    }

    public int ObtenerTotalRealesCalculadas() { return totalRealesCalculadas; }
    public int ObtenerTotalFogueosCalculadas() { return totalFogueosCalculadas; }
    public bool BalaActualEsReal() { if (indiceActual < tambor.Count) return tambor[indiceActual]; return false; }

    public void IniciarFase(bool turnoJugador)
    {
        volverAFaseCartas = false;
        turnoDelJugadorActual = turnoJugador;
        objetosUsadosEsteDisparo = 0;
        jugadorInmuneAEsposas = false;
        dealerInmuneAEsposas = false;

        ActualizarContadoresTexto(ObtenerRealesRestantes(), ObtenerFogueosRestantes());

        if (objetoPistola != null)
        {
            objetoPistola.SetActive(true);
            if (animadorPistola != null) animadorPistola.Play("Pistola_Reposo");
        }

        bool verBalas = true;
        if (DificultadManager.DificultadElegida == ModoDificultad.Dificil) verBalas = false;
        else if (DificultadManager.DificultadElegida == ModoDificultad.Personalizado) verBalas = DificultadManager.CP_VerBalas;

        if (estacionBalas != null)
        {
            if (verBalas)
            {
                estacionBalas.gameObject.SetActive(true);
                estacionBalas.Desplegar();
            }
            else
            {
                estacionBalas.gameObject.SetActive(false);
            }
        }

        SetInteractuable(turnoDelJugadorActual);
        NotificacionManager.Instance.MostrarMensaje(turnoDelJugadorActual ? "TU TURNO" : "TURNO DEL DEALER", true, 2.5f);

        if (!turnoDelJugadorActual && dealerIA != null) dealerIA.IniciarTurno();
    }

    public int ObtenerRealesRestantes()
    {
        int count = 0;
        for (int i = indiceActual; i < tambor.Count; i++) if (tambor[i]) count++;
        return count;
    }

    public int ObtenerFogueosRestantes()
    {
        int count = 0;
        for (int i = indiceActual; i < tambor.Count; i++) if (!tambor[i]) count++;
        return count;
    }

    public void RecibirDecisionDealer(bool dispararAJugador)
    {
        StartCoroutine(EjecutarDisparoDealerRoutine(dispararAJugador));
    }

    private IEnumerator EjecutarDisparoDealerRoutine(bool dispararAJugador)
    {
        if (animadorPistola != null) animadorPistola.Play(dispararAJugador ? "Pistola_ApuntarJugador" : "Pistola_ApuntarDealer");
        yield return new WaitForSeconds(1.5f);
        if (!ComprobarSeguridad()) yield break;

        bool esReal = EjecutarDisparoLogico();
        bool tamborVacio = (indiceActual >= tambor.Count);

        int danoMultiplicador = polvoraActivaDealer ? 2 : 1;
        polvoraActivaDealer = false;

        objetosUsadosEsteDisparo = 0;

        if (dispararAJugador)
        {
            if (esReal)
            {
                vidasJugador -= danoMultiplicador;
                ActualizarCorazonesUI();

                if (esposasActivasDealer)
                {
                    esposasActivasDealer = false;
                    turnoDelJugadorActual = false;
                    jugadorInmuneAEsposas = true;
                    FinalizarTurno(tamborVacio ? "¡BALA REAL! (Jugador esposado)" : "¡BALA REAL! Estás esposado, el Dealer mantiene el turno.", false);
                }
                else
                {
                    turnoDelJugadorActual = true;
                    FinalizarTurno(tamborVacio ? "¡BALA REAL!" : "¡BALA REAL!", true);
                }
            }
            else
            {
                if (esposasActivasDealer)
                {
                    esposasActivasDealer = false;
                    turnoDelJugadorActual = false;
                    jugadorInmuneAEsposas = true;
                    FinalizarTurno(tamborVacio ? "¡FOGUEO! (Jugador esposado)" : "¡FOGUEO! Estás esposado, el Dealer mantiene el turno.", false);
                }
                else
                {
                    turnoDelJugadorActual = true;
                    FinalizarTurno(tamborVacio ? "¡FOGUEO!" : "¡FOGUEO!", true);
                }
            }
        }
        else
        {
            if (esReal)
            {
                vidasDealer -= danoMultiplicador;
                ActualizarCorazonesUI();

                if (esposasActivasDealer)
                {
                    esposasActivasDealer = false;
                    turnoDelJugadorActual = false;
                    jugadorInmuneAEsposas = true;
                    FinalizarTurno(tamborVacio ? "¡BALA REAL! (Jugador esposado)" : "¡BALA REAL al Dealer! Dealer mantiene el turno por esposas.", false);
                }
                else
                {
                    turnoDelJugadorActual = true;
                    FinalizarTurno(tamborVacio ? "¡BALA REAL!" : "¡BALA REAL!", true);
                }
            }
            else
            {
                turnoDelJugadorActual = false;
                FinalizarTurno(tamborVacio ? "¡FOGUEO!" : "¡FOGUEO!", false);
            }
        }
    }

    public void DispararAYo()
    {
        if (!ComprobarSeguridad()) return;
        SetInteractuable(false);
        StartCoroutine(RutinaDisparoJugador(true));
    }

    public void DispararADealer()
    {
        if (!ComprobarSeguridad()) return;
        SetInteractuable(false);
        StartCoroutine(RutinaDisparoJugador(false));
    }

    private IEnumerator RutinaDisparoJugador(bool dispararAJugador)
    {
        if (animadorPistola != null) animadorPistola.Play(dispararAJugador ? "Pistola_ApuntarJugador" : "Pistola_ApuntarDealer");

        yield return new WaitForSeconds(0.6f);

        bool esReal = EjecutarDisparoLogico();
        bool tamborVacio = (indiceActual >= tambor.Count);

        int danoMultiplicador = polvoraActivaJugador ? 2 : 1;
        polvoraActivaJugador = false;

        objetosUsadosEsteDisparo = 0;

        if (dispararAJugador)
        {
            if (esReal)
            {
                vidasJugador -= danoMultiplicador;
                ActualizarCorazonesUI();

                if (esposasActivasJugador)
                {
                    esposasActivasJugador = false;
                    turnoDelJugadorActual = true;
                    dealerInmuneAEsposas = true;
                    FinalizarTurno(tamborVacio ? "¡BALA REAL! (Dealer esposado)" : "¡BALA REAL! Dealer esposado, mantienes el turno.", false);
                }
                else
                {
                    turnoDelJugadorActual = false;
                    FinalizarTurno(tamborVacio ? "¡BALA REAL!" : "¡BALA REAL!", true);
                }
            }
            else
            {
                turnoDelJugadorActual = true;
                FinalizarTurno(tamborVacio ? "¡FOGUEO!" : "¡FOGUEO!", false);
            }
        }
        else
        {
            if (esReal)
            {
                vidasDealer -= danoMultiplicador;
                ActualizarCorazonesUI();

                if (esposasActivasJugador)
                {
                    esposasActivasJugador = false;
                    turnoDelJugadorActual = true;
                    dealerInmuneAEsposas = true;
                    FinalizarTurno(tamborVacio ? "¡BALA REAL! (Dealer esposado)" : "¡BALA REAL! Dealer esposado, mantienes el turno.", false);
                }
                else
                {
                    turnoDelJugadorActual = false;
                    FinalizarTurno(tamborVacio ? "¡BALA REAL!" : "¡BALA REAL!", true);
                }
            }
            else
            {
                if (esposasActivasJugador)
                {
                    esposasActivasJugador = false;
                    turnoDelJugadorActual = true;
                    dealerInmuneAEsposas = true;
                    FinalizarTurno(tamborVacio ? "¡FOGUEO! (Dealer esposado)" : "¡FOGUEO! Dealer esposado, mantienes el turno.", false);
                }
                else
                {
                    turnoDelJugadorActual = false;
                    FinalizarTurno(tamborVacio ? "¡FOGUEO!" : "¡FOGUEO!", true);
                }
            }
        }
    }

    private bool ComprobarSeguridad()
    {
        if (tambor == null || tambor.Count == 0 || indiceActual > tambor.Count) return false;
        return true;
    }

    private bool EjecutarDisparoLogico()
    {
        bool esReal = tambor[indiceActual];

        if (slotsBalas[indiceActual] != null)
        {
            slotsBalas[indiceActual].sprite = esReal ? spriteBalaReal : spriteBalaFogueo;
            slotsBalas[indiceActual].color = Color.white;
        }

        if (audioSource != null && (esReal ? sonidoDisparoReal : sonidoDisparoFogueo) != null)
        {
            audioSource.PlayOneShot(esReal ? sonidoDisparoReal : sonidoDisparoFogueo);
        }

        indiceActual++;
        ActualizarContadoresTexto(ObtenerRealesRestantes(), ObtenerFogueosRestantes());
        return esReal;
    }

    private void FinalizarTurno(string mensaje, bool volverACartas)
    {
        volverAFaseCartas = volverACartas;
        NotificacionManager.Instance.MostrarMensaje(mensaje, true, 2.5f);
        SetInteractuable(false);
        StartCoroutine(RutinaTransicionTurno());
    }

    private IEnumerator RutinaTransicionTurno()
    {
        yield return new WaitForSeconds(2.5f);

        if (vidasJugador <= 0)
        {
            fasesManager.JugadorHaMuerto();
        }
        else if (vidasDealer <= 0)
        {
            NotificacionManager.Instance.MostrarMensaje("¡ENEMIGO DERROTADO!", true, 2.5f);
            if (objetoPistola != null) objetoPistola.SetActive(false);
            if (estacionBalas != null && estacionBalas.gameObject.activeSelf) estacionBalas.Ocultar();
            fasesManager.VictoriaSobreEnemigo();
        }
        else if (indiceActual >= tambor.Count)
        {
            if (objetoPistola != null) objetoPistola.SetActive(false);
            if (estacionBalas != null && estacionBalas.gameObject.activeSelf) estacionBalas.Ocultar();
            fasesManager.ReiniciarRondaCompleta();
        }
        else if (volverAFaseCartas)
        {
            if (objetoPistola != null) objetoPistola.SetActive(false);
            if (estacionBalas != null && estacionBalas.gameObject.activeSelf) estacionBalas.Ocultar();
            fasesManager.IniciarFaseCartas();
        }
        else
        {
            if (animadorPistola != null) animadorPistola.Play("Pistola_Reposo");

            if (turnoDelJugadorActual)
            {
                jugadorInmuneAEsposas = false;
                SetInteractuable(true);
                NotificacionManager.Instance.MostrarMensaje("TU TURNO", true, 2.5f);
            }
            else
            {
                dealerInmuneAEsposas = false;
                SetInteractuable(false);
                NotificacionManager.Instance.MostrarMensaje("TURNO DEL DEALER", true, 2.5f);
                if (dealerIA != null) dealerIA.IniciarTurno();
            }
        }
    }

    private void ActualizarCorazonesUI()
    {
        for (int i = 0; i < corazonesJugador.Length; i++)
        {
            if (corazonesJugador[i] != null)
                corazonesJugador[i].sprite = (i < vidasJugador) ? corazonLleno : corazonVacio;
        }

        for (int i = 0; i < corazonesDealer.Length; i++)
        {
            if (corazonesDealer[i] != null)
                corazonesDealer[i].sprite = (i < vidasDealer) ? corazonLleno : corazonVacio;
        }

        ControlarHeartbeat();
    }

    private void ControlarHeartbeat()
    {
        if (heartbeatSource != null && sonidoHeartbeat != null)
        {
            if (vidasJugador == 1)
            {
                if (!heartbeatSource.isPlaying) heartbeatSource.Play();
            }
            else
            {
                if (heartbeatSource.isPlaying) heartbeatSource.Stop();
            }
        }
    }

    private IEnumerator EfectoPantallaRoja()
    {
        if (pantallaRojaDaño != null)
        {
            pantallaRojaDaño.gameObject.SetActive(true);
            Color c = pantallaRojaDaño.color;
            c.a = 0.5f;
            pantallaRojaDaño.color = c;

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / 0.5f;
                c.a = Mathf.Lerp(0.5f, 0f, t);
                pantallaRojaDaño.color = c;
                yield return null;
            }
            pantallaRojaDaño.gameObject.SetActive(false);
        }
    }

    private IEnumerator EfectoMuzzleFlash()
    {
        if (muzzleFlashPistola != null)
        {
            muzzleFlashPistola.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            muzzleFlashPistola.SetActive(false);
        }
    }

    private void ActualizarContadoresTexto(int r, int f)
    {
        bool verBalas = true;
        if (DificultadManager.DificultadElegida == ModoDificultad.Dificil) verBalas = false;
        else if (DificultadManager.DificultadElegida == ModoDificultad.Personalizado) verBalas = DificultadManager.CP_VerBalas;

        if (textoReales != null) textoReales.text = verBalas ? r + "x Reales" : "??x Reales";
        if (textoFogueo != null) textoFogueo.text = verBalas ? f + "x Fogueo" : "??x Fogueo";
    }

    private void SetInteractuable(bool estado)
    {
        if (botonYo != null) botonYo.interactable = estado;
        if (botonDealer != null) botonDealer.interactable = estado;
    }

    public bool UsarLupa(bool esJugador)
    {
        if (indiceActual >= tambor.Count) return false;
        bool esReal = tambor[indiceActual];
        if (esJugador)
        {
            NotificacionManager.Instance.MostrarMensaje(esReal ? "Lupa: La siguiente bala es REAL" : "Lupa: La siguiente bala es FOGUEO", true, 3.5f);
        }
        else
        {
            NotificacionManager.Instance.MostrarMensaje("El Dealer usa una Lupa...", true, 2.5f);
            if (dealerIA != null) dealerIA.RecibirInfoLupa(esReal);
        }
        return true;
    }

    public bool UsarBotiquin(bool esJugador)
    {
        if (esJugador)
        {
            if (vidasJugador >= corazonesJugador.Length)
            {
                NotificacionManager.Instance.MostrarMensaje("Salud al máximo. No se puede usar.", true, 2f);
                return false;
            }
            vidasJugador++;
            ActualizarCorazonesUI();
            NotificacionManager.Instance.MostrarMensaje("Te has curado 1 corazón.", true, 2f);
        }
        else
        {
            if (vidasDealer >= vidasMaximasDealer) return false;
            vidasDealer++;
            ActualizarCorazonesUI();
            NotificacionManager.Instance.MostrarMensaje("El Dealer se cura 1 corazón.", true, 2.5f);
        }
        return true;
    }

    public bool UsarPolvora(bool esJugador)
    {
        if (esJugador)
        {
            if (polvoraActivaJugador) return false;
            polvoraActivaJugador = true;
            NotificacionManager.Instance.MostrarMensaje("Pólvora: Próximo disparo hace daño DOBLE.", true, 2.5f);
        }
        else
        {
            if (polvoraActivaDealer) return false;
            polvoraActivaDealer = true;
            NotificacionManager.Instance.MostrarMensaje("El Dealer mete pólvora en la pistola...", true, 2.5f);
        }
        return true;
    }

    public bool UsarEsposas(bool esJugador)
    {
        if (esJugador)
        {
            if (esposasActivasJugador)
            {
                NotificacionManager.Instance.MostrarMensaje("El Dealer ya está inmovilizado.", true, 2f);
                return false;
            }
            if (dealerInmuneAEsposas)
            {
                NotificacionManager.Instance.MostrarMensaje("Debes esperar a que el Dealer tenga un turno para volver a esposarlo.", true, 3f);
                return false;
            }
            esposasActivasJugador = true;
            NotificacionManager.Instance.MostrarMensaje("Esposas: Mantendrás tu turno en el próximo disparo al enemigo.", true, 3f);
        }
        else
        {
            if (esposasActivasDealer || jugadorInmuneAEsposas) return false;
            esposasActivasDealer = true;
            NotificacionManager.Instance.MostrarMensaje("El Dealer te ha esposado a la mesa...", true, 3f);
        }
        return true;
    }

    public bool UsarInversor(bool esJugador)
    {
        if (indiceActual >= tambor.Count) return false;
        tambor[indiceActual] = !tambor[indiceActual];
        if (esJugador)
        {
            if (dealerIA != null) dealerIA.ForzarOlvidoBala();
            NotificacionManager.Instance.MostrarMensaje("Inversor: La polaridad de la bala actual se ha invertido.", true, 3f);
        }
        else
        {
            NotificacionManager.Instance.MostrarMensaje("El Dealer invierte la polaridad de la bala...", true, 2.5f);
            if (dealerIA != null) dealerIA.InvertirConocimientoBala();
        }
        return true;
    }

    public bool UsarTelefono(bool esJugador)
    {
        if (indiceActual >= tambor.Count) return false;

        int targetIndex = UnityEngine.Random.Range(indiceActual, tambor.Count);
        bool esReal = tambor[targetIndex];

        if (esJugador)
        {
            int balaNumero = targetIndex - indiceActual + 1;
            string mensaje = $"Teléfono: La bala número {balaNumero} es {(esReal ? "REAL" : "FOGUEO")}.";
            NotificacionManager.Instance.MostrarMensaje(mensaje, true, 4f);
        }
        else
        {
            NotificacionManager.Instance.MostrarMensaje("El Dealer escucha su Teléfono...", true, 2.5f);
            if (targetIndex == indiceActual && dealerIA != null) dealerIA.RecibirInfoLupa(esReal);
        }
        return true;
    }

    public bool UsarMuñeco(bool esJugador)
    {
        if (indiceActual >= tambor.Count) return false;
        StartCoroutine(RutinaMuñeco(esJugador));
        return true;
    }

    private IEnumerator RutinaMuñeco(bool esJugador)
    {
        SetInteractuable(false);
        if (animadorPistola != null) animadorPistola.Play("Pistola_Reposo");
        yield return new WaitForSeconds(0.5f);

        if (dealerIA != null) dealerIA.ForzarOlvidoBala();

        bool esReal = EjecutarDisparoLogico();
        objetosUsadosEsteDisparo = 0;

        if (esJugador)
        {
            string msg = esReal ? "¡BALA REAL al muñeco!" : "¡FOGUEO al muñeco!";
            NotificacionManager.Instance.MostrarMensaje($"{msg} Mantienes el turno.", true, 3f);
            polvoraActivaJugador = false;
        }
        else
        {
            NotificacionManager.Instance.MostrarMensaje("El Dealer dispara al muñeco... y mantiene el turno.", true, 3f);
            polvoraActivaDealer = false;
        }

        yield return new WaitForSeconds(2f);

        if (indiceActual >= tambor.Count)
        {
            FinalizarTurno("Tambor vacío.", false);
        }
        else
        {
            if (esJugador)
            {
                SetInteractuable(true);
                NotificacionManager.Instance.MostrarMensaje("TU TURNO", true, 2.5f);
            }
            else
            {
                dealerIA.IniciarTurno();
            }
        }
    }
}
