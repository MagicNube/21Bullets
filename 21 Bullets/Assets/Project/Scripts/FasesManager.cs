using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FasesManager : MonoBehaviour
{
    [Header("Contenedores de Fase")]
    public GameObject contenedorCartas;
    public GameObject contenedorDisparo;

    [Header("Managers")]
    public CartasManager cartasManager;
    public DisparoManager disparoManager;
    public AnimacionTambor animacionTambor;
    public ObjetosManager objetosManager;
    public IntroAnimacionManager introManager;

    [Header("UI y Textos")]
    public TextMeshProUGUI textoCartas;
    public TextMeshProUGUI textoDisparo;

    public GameObject contenedorImagenDealerHUD;

    [Header("Paneles HUD a Ocultar")]
    public GameObject panelHUDJugador;
    public GameObject panelHUDDealer;

    [Header("Configuración de Progresión")]
    public Sprite spriteProtagonista;
    public EnemigoData[] listaEnemigos;
    private int indiceEnemigoActual = 0;

    private bool juegoIniciado = false;

    void Start()
    {
        contenedorCartas.SetActive(false);
        contenedorDisparo.SetActive(false);
        textoCartas.text = "(Click para iniciar)";

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
            objetosManager.enemigoRecibeObjetos = datos.puedeUsarObjetos;

            IniciarFlujoPartida(datos);
        }
        else
        {
            textoCartas.text = "¡HAS SOBREVIVIDO A TODOS!";
        }
    }

    private void IniciarFlujoPartida(EnemigoData enemigo)
    {
        textoCartas.text = "";

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
        textoCartas.text = "";
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
        textoDisparo.text = "";
        contenedorDisparo.SetActive(false);
        contenedorCartas.SetActive(true);
        textoCartas.text = "Gana la mano para tener el primer turno.";
        cartasManager.ResetSequence();
    }

    public void IniciarFaseDisparo(bool ganaJugador)
    {
        textoCartas.text = "";
        contenedorCartas.SetActive(false);
        contenedorDisparo.SetActive(true);
        disparoManager.IniciarFase(ganaJugador);
    }

    public void ReiniciarRondaCompleta()
    {
        textoDisparo.text = "Tambor vacío. Siguiente ronda...";
        Invoke("IniciarFaseRecarga", 2f);
    }
}
