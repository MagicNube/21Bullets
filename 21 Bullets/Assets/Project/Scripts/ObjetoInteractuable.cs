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
    private Color colorTransparente;

    public void Configurar(TipoObjeto tipo, ObjetosManager refManager)
    {
        miTipo = tipo;
        manager = refManager;
        sr = GetComponent<SpriteRenderer>();
        
        if (sr != null)
        {
            colorOriginal = sr.color;
            colorTransparente = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, 0.5f);
        }
    }


    void OnMouseDown()
    {
        if (manager != null && manager.objetoCajaVisual != null && manager.objetoCajaVisual.activeSelf) return;

        posicionInicialSlot = transform.position;
        arrastrando = true;
        
        if (sr != null) sr.color = colorTransparente;
        
        if (manager != null && manager.cursorManoCerrada != null)
        {
            Cursor.SetCursor(manager.cursorManoCerrada, Vector2.zero, CursorMode.Auto);
        }
    }

    void OnMouseDrag()
    {
        if (arrastrando)
        {
            Vector3 posRaton = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            posRaton.z = transform.position.z;
            transform.position = posRaton;

            if (manager != null)
            {
                bool enZonaValida = manager.ComprobarZonaValida(miTipo, transform.position);
                if (sr != null) sr.color = enZonaValida ? colorOriginal : colorTransparente;
            }
        }
    }

    void OnMouseUp()
    {
        if (!arrastrando) return;

        arrastrando = false;
        
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        
        if (sr != null) sr.color = colorOriginal;
        
        bool exito = false;
        if (manager != null)
        {
            exito = manager.IntentarUsarObjetoJugador(this);
        }
        
        if (!exito)
        {
            transform.position = posicionInicialSlot;
        }
    }
}