using System.Collections;
using UnityEngine;

public class DealerIANovato : MonoBehaviour
{
    public DisparoManager disparoManager;
    
    public float tiempoPensamiento = 1.5f;
    public float probabilidadError = 0.3f;

    public void IniciarTurno()
    {
        StartCoroutine(TomarDecisionRoutine());
    }

    private IEnumerator TomarDecisionRoutine()
    {
        yield return new WaitForSeconds(tiempoPensamiento);

        int reales = disparoManager.ObtenerRealesRestantes();
        int fogueos = disparoManager.ObtenerFogueosRestantes();

        bool dispararAJugador = true;

        if (reales == 0)
        {
            dispararAJugador = false;
        }
        else if (fogueos == 0)
        {
            dispararAJugador = true;
        }
        else
        {
            if (reales > fogueos) dispararAJugador = true;
            else if (fogueos > reales) dispararAJugador = false;
            else dispararAJugador = (Random.value > 0.5f);

            if (Random.value < probabilidadError)
            {
                dispararAJugador = !dispararAJugador;
            }
        }

        disparoManager.RecibirDecisionDealer(dispararAJugador);
    }
}