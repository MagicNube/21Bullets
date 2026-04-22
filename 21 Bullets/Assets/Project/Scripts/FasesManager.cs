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

    public Sprite spriteProtagonista;
    public EnemigoData[] listaEnemigos;
    private int indiceEnemigoActual = 0;

    private bool juegoIniciado = false;
    private bool primerMensajeCartasMostrado = false;

    void Start()
    {
        contenedorCartas.SetActive(false);
        contenedorDisparo.SetActive(false);
        
        NotificacionManager.Instance.MostrarMensaje("(Click para iniciar)", false);

        if (panelHUDJugador != null) panelHUDJugador.SetActive(false);
        if (panelHUDDealer != null) panelHUDDealer.SetActive(false);

        if (listaEnemigos != null && listaEnemigos.Length > 0 && contenedorImagenDealerHUD != null)
        {
            Image img = contenedorImagenDealerHUD.GetComponent<Image>();
            if (img == null) img = contenedorImagenDealerHUD.GetComponentInChildren<Image>();

            if (img != null)
            {
                img.sprite = listaEnemigos[0].spriteRival;
            }
        }
    }

    void Update()
    {
        if (!juegoIniciado && Input.GetMouseButtonDown(0))
        {
            juegoIniciado = true;
            PrepararSiguienteEnemigo();
        }
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
            NotificacionManager.Instance.MostrarMensaje("¡HAS SOBREVIVIDO A TODOS!", false);
        }
    }

    private void IniciarFlujoPartida(EnemigoData enemigo)
    {
        NotificacionManager.Instance.OcultarMensaje();

        if (panelHUDJugador != null) panelHUDJugador.SetActive(false);
        if (panelHUDDealer != null) panelHUDDealer.SetActive(false);

        if (contenedorImagenDealerHUD != null)
        {
            Image img = contenedorImagenDealerHUD.GetComponent<Image>();
            if (img == null) img = contenedorImagenDealerHUD.GetComponentInChildren<Image>();

            if (img != null)
            {
                img.sprite = enemigo.spriteRival;
            }
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

        contenedorCartas.SetActive(false);
        contenedorDisparo.SetActive(false);
        NotificacionManager.Instance.OcultarMensaje();
        
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