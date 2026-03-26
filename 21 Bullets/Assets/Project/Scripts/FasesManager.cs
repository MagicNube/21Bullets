using UnityEngine;
using TMPro;

public class FasesManager : MonoBehaviour
{
    public GameObject contenedorCartas;
    public GameObject contenedorDisparo;

    public CartasManager cartasManager;
    public DisparoManager disparoManager;
    public AnimacionTambor animacionTambor;

    public TextMeshProUGUI textoPantalla;

    private bool juegoIniciado = false;

    void Start()
    {
        contenedorCartas.SetActive(false);
        contenedorDisparo.SetActive(false);
        textoPantalla.text = "(Click para iniciar el juego)";
    }

    void Update()
    {
        if (!juegoIniciado)
        {
            if (Input.GetMouseButtonDown(0))
            {
                juegoIniciado = true;
                IniciarFaseRecarga();
            }
        }
    }

    // FASE 1: El Dealer muestra las balas y carga el arma
    public void IniciarFaseRecarga()
    {
        contenedorCartas.SetActive(false);
        contenedorDisparo.SetActive(false);
        textoPantalla.text = "";

        // Le pedimos al DisparoManager que calcule las balas SIN empezar a disparar
        disparoManager.PrepararNuevaRonda();

        int numReales = disparoManager.ObtenerTotalRealesCalculadas();
        int numFogueo = disparoManager.ObtenerTotalFogueosCalculadas();

        // Lanzamos la animación visual
        animacionTambor.IniciarSecuencia(numReales, numFogueo, AlTerminarRecarga);
    }

    // Se llama automáticamente cuando el tambor termina de girar y desaparece
    private void AlTerminarRecarga()
    {
        IniciarFaseCartas();
    }

    // FASE 2: Se juegan las cartas para decidir quién tiene el turno
    public void IniciarFaseCartas()
    {
        contenedorDisparo.SetActive(false);
        contenedorCartas.SetActive(true);
        textoPantalla.text = "Gana la mano para tener el primer turno.";
        cartasManager.ResetSequence();
    }

    // FASE 3: El ganador de las cartas empieza a disparar
    public void IniciarFaseDisparo(bool ganaJugador)
    {
        contenedorCartas.SetActive(false);
        contenedorDisparo.SetActive(true);
        disparoManager.IniciarFase(ganaJugador);
    }

    // Función para reiniciar el ciclo completo si el tambor se vacía sin que nadie muera
    public void ReiniciarRondaCompleta()
    {
        textoPantalla.text = "Tambor vacío. Siguiente ronda...";
        Invoke("IniciarFaseRecarga", 2f); // Espera 2 segundos y vuelve a cargar
    }
}
