using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CursorObjeto2D : MonoBehaviour
{
    public Texture2D cursorManoApuntar;

    void OnMouseEnter()
    {
        if (cursorManoApuntar != null)
        {
            Cursor.SetCursor(cursorManoApuntar, Vector2.zero, CursorMode.Auto);
        }
    }

    void OnMouseExit()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    void OnDisable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}