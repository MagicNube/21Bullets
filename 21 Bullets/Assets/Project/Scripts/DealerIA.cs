using System.Collections;
using UnityEngine;

public class DealerIA : MonoBehaviour
{
    public DisparoManager disparoManager;
    public ObjetosManager objetosManager;
    public FasesManager fasesManager;
    public CartasManager cartasManager;

    public float tiempoPensamiento = 2.5f;
    public float tiempoEntreAcciones = 3.0f;

    private bool sabeBalaActual = false;
    private bool esBalaReal = false;

    public void IniciarTurno()
    {
        StartCoroutine(TurnoRoutine());
    }

    private IEnumerator TurnoRoutine()
    {
        yield return new WaitForSeconds(tiempoPensamiento);

        if (objetosManager.enemigoRecibeObjetos && disparoManager.objetosUsadosEsteDisparo < 1)
        {
            yield return StartCoroutine(FaseObjetosRoutine());
        }

        if (!disparoManager.turnoDelJugadorActual)
        {
            DecidirDisparo();
        }
    }

    private IEnumerator FaseObjetosRoutine()
    {
        bool decidio = false;
        TipoObjeto objetoAUsar = TipoObjeto.Botiquin;

        int reales = disparoManager.ObtenerRealesRestantes();
        int fogueos = disparoManager.ObtenerFogueosRestantes();
        bool probReal = reales > fogueos;

        if (objetosManager.DealerTieneObjeto(TipoObjeto.Botiquin) && disparoManager.vidasDealer == 1) { objetoAUsar = TipoObjeto.Botiquin; decidio = true; }
        else if (sabeBalaActual && !esBalaReal && objetosManager.DealerTieneObjeto(TipoObjeto.Inversor)) { objetoAUsar = TipoObjeto.Inversor; esBalaReal = true; decidio = true; }
        else if (sabeBalaActual && !esBalaReal && objetosManager.DealerTieneObjeto(TipoObjeto.Muñeco)) { objetoAUsar = TipoObjeto.Muñeco; sabeBalaActual = false; decidio = true; }
        else if ((sabeBalaActual && esBalaReal) || probReal) {
            if (objetosManager.DealerTieneObjeto(TipoObjeto.Polvora)) { objetoAUsar = TipoObjeto.Polvora; decidio = true; }
            else if (objetosManager.DealerTieneObjeto(TipoObjeto.Esposas) && !disparoManager.jugadorInmuneAEsposas) { objetoAUsar = TipoObjeto.Esposas; decidio = true; }
        }
        
        if (!decidio && !sabeBalaActual) {
            if (objetosManager.DealerTieneObjeto(TipoObjeto.Lupa)) {
                sabeBalaActual = true;
                esBalaReal = disparoManager.BalaActualEsReal();
                objetoAUsar = TipoObjeto.Lupa;
                decidio = true;
            } else if (objetosManager.DealerTieneObjeto(TipoObjeto.Telefono)) {
                objetoAUsar = TipoObjeto.Telefono;
                decidio = true;
            }
        }
        
        if (!decidio && objetosManager.DealerTieneObjeto(TipoObjeto.Guante) && objetosManager.JugadorTieneObjetosFaseDisparo()) {
            objetoAUsar = TipoObjeto.Guante;
            decidio = true;
        }
        
        if (!decidio && objetosManager.DealerTieneObjeto(TipoObjeto.Botiquin) && disparoManager.vidasDealer < disparoManager.vidasMaximasDealer) {
            objetoAUsar = TipoObjeto.Botiquin;
            decidio = true;
        }

        if (decidio)
        {
            objetosManager.IntentarUsarObjetoDealer(objetoAUsar);
            yield return new WaitForSeconds(tiempoEntreAcciones);
        }
    }

    private void DecidirDisparo()
    {
        int reales = disparoManager.ObtenerRealesRestantes();
        int fogueos = disparoManager.ObtenerFogueosRestantes();
        bool dispararAJugador = true;

        if (sabeBalaActual)
        {
            dispararAJugador = esBalaReal;
        }
        else
        {
            if (reales == 0) dispararAJugador = false;
            else if (fogueos == 0) dispararAJugador = true;
            else
            {
                if (reales > fogueos) dispararAJugador = true;
                else if (fogueos > reales) dispararAJugador = false;
                else dispararAJugador = (Random.value > 0.5f);
            }
        }

        sabeBalaActual = false;
        disparoManager.RecibirDecisionDealer(dispararAJugador);
    }
    
    // --- NUEVAS FUNCIONES PARA CUANDO ROBA OBJETOS ---
    public void ForzarOlvidoBala()
    {
        sabeBalaActual = false;
    }

    public void RecibirInfoLupa(bool esReal)
    {
        sabeBalaActual = true;
        esBalaReal = esReal;
    }

    public void InvertirConocimientoBala()
    {
        if (sabeBalaActual)
        {
            esBalaReal = !esBalaReal;
        }
    }

    public IEnumerator PensarObjetosCartas()
    {
        if (cartasManager.objetosUsadosEnCartas >= 2) yield break;

        if (objetosManager.DealerTieneObjeto(TipoObjeto.Botiquin) && disparoManager.vidasDealer < disparoManager.vidasMaximasDealer)
        {
            objetosManager.IntentarUsarObjetoDealer(TipoObjeto.Botiquin);
            yield return new WaitForSeconds(tiempoEntreAcciones);
        }

        if (cartasManager.objetosUsadosEnCartas >= 2) yield break;

        if (objetosManager.DealerTieneObjeto(TipoObjeto.Guante)) 
        {
            bool jugadorTieneUtil = false;
            foreach(var obj in objetosManager.objetosJugador) 
            {
               if(obj != null) 
               {
                  TipoObjeto t = obj.GetComponent<ObjetoInteractuable>().miTipo;
                  if(t == TipoObjeto.Tijeras || t == TipoObjeto.Lupa || t == TipoObjeto.Botiquin) jugadorTieneUtil = true;
               }
            }
            if (jugadorTieneUtil) 
            {
                objetosManager.IntentarUsarObjetoDealer(TipoObjeto.Guante);
                yield return new WaitForSeconds(tiempoEntreAcciones);
            }
        }

        if (cartasManager.objetosUsadosEnCartas >= 2) yield break;

        if (objetosManager.DealerTieneObjeto(TipoObjeto.Lupa)) 
        {
            objetosManager.IntentarUsarObjetoDealer(TipoObjeto.Lupa);
            yield return new WaitForSeconds(tiempoEntreAcciones);
        }

        if (cartasManager.objetosUsadosEnCartas >= 2) yield break;

        int score = cartasManager.CalculateHandValue(cartasManager.dealerHandIndices);
        if (score >= 12 && score < cartasManager.dealerThreshold && objetosManager.DealerTieneObjeto(TipoObjeto.Tijeras))
        {
            objetosManager.IntentarUsarObjetoDealer(TipoObjeto.Tijeras);
            yield return new WaitForSeconds(tiempoEntreAcciones);
        }
    }
}