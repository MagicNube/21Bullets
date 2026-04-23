using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FasesManager : MonoBehaviour
{
    public GameObject contenedorCartas;
    public GameObject contenedorDisparo;

    public CartasManager cartasManager;
    public DisparoManager disparoManager;
    public AnimacionTambor animacionTambor;
    public ObjetosManager objetosManager;
    public IntroAnimacionManager introManager;

    public GameObject contenedorImagenDealerHUD;

    public GameObject panelHUDJugador;
    public GameObject panelHUDDealer;
    public MenuManager menuManager;

    [Header("Botones y Creditos")]
    public Button botonVersus1;
    public Button botonVersus2;
    public GameObject panelCreditosFinal;

    public Sprite spriteProtagonista;
    public EnemigoData[] listaEnemigos;
    public int indiceEnemigoActual = 0;


    private bool primerMensajeCartasMostrado = false;

    void Start()
    {
        contenedorCartas.SetActive(false);
        contenedorDisparo.SetActive(false);

        if (panelHUDJugador != null) panelHUDJugador.SetActive(false);
        if (panelHUDDealer != null) panelHUDDealer.SetActive(false);

        if (panelCreditosFinal != null) panelCreditosFinal.SetActive(false);

        if (listaEnemigos != null && listaEnemigos.Length > 0 && contenedorImagenDealerHUD != null)
        {
            Image img = contenedorImagenDealerHUD.GetComponent<Image>();
            if (img == null) img = contenedorImagenDealerHUD.GetComponentInChildren<Image>();

            if (img != null)
            {
                img.sprite = listaEnemigos[0].spriteRival;
            }
        }

        StartCoroutine(RutinaInicioJuego());
    }

    private IEnumerator RutinaInicioJuego()
    {
        yield return new WaitForSeconds(1.0f);
        PrepararSiguienteEnemigo();
    }

    public void PrepararSiguienteEnemigo()
    {
        if (indiceEnemigoActual < listaEnemigos.Length)
        {
            EnemigoData datos = listaEnemigos[indiceEnemigoActual];

            cartasManager.dealerThreshold = datos.umbralPlantadoBlackjack;

            if (objetosManager != null)
            {
                objetosManager.enemigoRecibeObjetos = datos.puedeUsarObjetos;
            }

            primerMensajeCartasMostrado = false;

            IniciarFlujoPartida(datos);
        }
        else
        {
            MostrarFinalJuego("¡HAS SOBREVIVIDO A TODOS!");
        }
    }

    public void JugadorHaMuerto()
    {
        MostrarFinalJuego("HAS CAÍDO...");
    }

    private void MostrarFinalJuego(string mensaje)
    {
        NotificacionManager.Instance.MostrarMensaje(mensaje, false);

        if (menuManager != null)
        {
            bool esVictoria = mensaje.Contains("SOBREVIVIDO");
            menuManager.AbrirCreditosResultado(esVictoria);
        }
        else if (panelCreditosFinal != null)
        {
            panelCreditosFinal.SetActive(true);
        }
    }

    private void IniciarFlujoPartida(EnemigoData enemigo)
    {
        NotificacionManager.Instance.OcultarMensaje();

        if (panelHUDJugador != null) panelHUDJugador.SetActive(false);
        if (panelHUDDealer != null) panelHUDDealer.SetActive(false);

        if (botonVersus1 != null) botonVersus1.gameObject.SetActive(false);
        if (botonVersus2 != null) botonVersus2.gameObject.SetActive(false);

        if (contenedorImagenDealerHUD != null)
        {
            Image img = contenedorImagenDealerHUD.GetComponent<Image>();
            if (img == null) img = contenedorImagenDealerHUD.GetComponentInChildren<Image>();

            if (img != null)
            {
                img.sprite = enemigo.spriteRival;
            }
        }

        disparoManager.ResetPartidaCompleta(enemigo.vidasMaximas);
        if (objetosManager != null)
        {
            objetosManager.LimpiarTodosLosObjetos();
        }

        if (introManager != null)
        {
            introManager.EjecutarAnimacion(spriteProtagonista, enemigo.spriteRival);
        }
        else
        {
            IniciarFaseRecarga();
        }
    }

    public void VictoriaSobreEnemigo()
    {
        indiceEnemigoActual++;
        Invoke("PrepararSiguienteEnemigo", 2f);
    }

    public void IniciarFaseRecarga()
    {
        if (panelHUDJugador != null) panelHUDJugador.SetActive(true);
        if (panelHUDDealer != null) panelHUDDealer.SetActive(true);

        if (botonVersus1 != null) botonVersus1.gameObject.SetActive(true);
        if (botonVersus2 != null) botonVersus2.gameObject.SetActive(true);

        contenedorCartas.SetActive(false);
        contenedorDisparo.SetActive(false);
        NotificacionManager.Instance.OcultarMensaje();

        if (cartasManager.textoPuntosJugador != null) cartasManager.textoPuntosJugador.text = "0";
        if (cartasManager.textoPuntosDealer != null) cartasManager.textoPuntosDealer.text = "0 + ?";

        disparoManager.PrepararNuevaRonda();
        int numReales = disparoManager.ObtenerTotalRealesCalculadas();
        int numFogueo = disparoManager.ObtenerTotalFogueosCalculadas();
        animacionTambor.IniciarSecuencia(numReales, numFogueo, AlTerminarRecarga);
    }

    private void AlTerminarRecarga()
    {
        if (objetosManager != null)
        {
            objetosManager.RepartirObjetosRonda(IniciarFaseCartas);
        }
        else
        {
            IniciarFaseCartas();
        }
    }

    public void IniciarFaseCartas()
    {
        contenedorDisparo.SetActive(false);
        contenedorCartas.SetActive(true);

        if (!primerMensajeCartasMostrado)
        {
            NotificacionManager.Instance.MostrarMensaje("Gana la mano para tener el primer turno.", true, 3f);
            primerMensajeCartasMostrado = true;
        }
        else
        {
            NotificacionManager.Instance.OcultarMensaje();
        }

        cartasManager.ResetSequence();
    }

    public void IniciarFaseDisparo(bool ganaJugador)
    {
        NotificacionManager.Instance.OcultarMensaje();
        contenedorCartas.SetActive(false);
        contenedorDisparo.SetActive(true);
        disparoManager.IniciarFase(ganaJugador);
    }

    public void ReiniciarRondaCompleta()
    {
        NotificacionManager.Instance.MostrarMensaje("Tambor vacío. Siguiente ronda...", true, 2f);
        Invoke("IniciarFaseRecarga", 2f);
    }
}
