using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ObjetoInteractuable : MonoBehaviour
{
    public TipoObjeto miTipo;
    private ObjetosManager manager;
    private Vector3 posicionInicialSlot;
    private bool arrastrando = false;

    private SpriteRenderer sr;
    private Color colorOriginal;
    
    public bool esUsable = true;

    private Coroutine rutinaHover;
    private readonly float tiempoHover = 0.3f;

    public void Configurar(TipoObjeto tipo, ObjetosManager refManager)
    {
        miTipo = tipo;
        manager = refManager;
        sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            colorOriginal = sr.color;
        }
    }

    public void SetActivo(bool activo)
    {
        if (esUsable == activo) return;
        esUsable = activo;
        
        if (sr != null)
        {
            Color c = colorOriginal;
            if (!activo) c.a = 0.6f; 
            sr.color = c;
        }
    }

    void OnMouseEnter()
    {
        if (!esUsable || arrastrando) return;
        rutinaHover = StartCoroutine(RutinaMostrarTooltip());
    }

    void OnMouseExit()
    {
        if (rutinaHover != null)
        {
            StopCoroutine(rutinaHover);
            rutinaHover = null;
        }
        if (!arrastrando)
        {
            NotificacionManager.Instance.OcultarMensaje();
        }
    }

    private IEnumerator RutinaMostrarTooltip()
    {
        yield return new WaitForSeconds(tiempoHover);
        string descripcion = ObtenerDescripcion(miTipo);
        NotificacionManager.Instance.MostrarMensaje(descripcion, false);
    }

    private string ObtenerDescripcion(TipoObjeto tipo)
    {
        switch (tipo)
        {
            case TipoObjeto.Botiquin: return "Botiquín: Recupera 1 vida.";
            case TipoObjeto.Esposas: return "Esposas: El enemigo pierde su próximo turno.";
            case TipoObjeto.Guante: return "Guante: Roba un objeto a elegir del rival y se usa.";
            case TipoObjeto.Inversor: return "Inversor: Cambia la bala de Real a Fogueo o viceversa.";
            case TipoObjeto.Lupa: return "Lupa: Revela la carta de arriba del mazo o la bala actual.";
            case TipoObjeto.Muñeco: return "Vudú: Dispara una bala al muñeco y mantén el turno.";
            case TipoObjeto.Polvora: return "Pólvora: El siguiente disparo hará doble daño.";
            case TipoObjeto.RayosX: return "Rayos X: Revela la carta oculta del Dealer.";
            case TipoObjeto.Telefono: return "Teléfono: Te revela la posición de una bala.";
            case TipoObjeto.Tijeras: return "Tijeras: Elimina una carta a elegir.";
            default: return "";
        }
    }

    void OnMouseDown()
    {
        if (!esUsable) return;
        if (manager != null && manager.objetoCajaVisual != null && manager.objetoCajaVisual.activeSelf) return;

        if (rutinaHover != null)
        {
            StopCoroutine(rutinaHover);
            rutinaHover = null;
        }
        NotificacionManager.Instance.OcultarMensaje();

        posicionInicialSlot = transform.position;
        arrastrando = true;

        if (manager != null)
        {
            if (manager.cursorManoCerrada != null)
            {
                Cursor.SetCursor(manager.cursorManoCerrada, Vector2.zero, CursorMode.Auto);
            }
            manager.ResaltarZonasValidas(miTipo, true);
        }
    }

    void OnMouseDrag()
    {
        if (!esUsable || !arrastrando) return;

        Vector3 posRaton = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        posRaton.z = transform.position.z;
        transform.position = posRaton;
    }

    void OnMouseUp()
    {
        if (!arrastrando) return;
        arrastrando = false;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        bool exito = false;
        if (manager != null)
        {
            manager.ResaltarZonasValidas(miTipo, false);
            exito = manager.IntentarUsarObjetoJugador(this);
        }

        if (!exito)
        {
            transform.position = posicionInicialSlot;
        }
    }
}