using UnityEngine;
using UnityEngine.EventSystems;

public class UIControlCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Texture2D cursorManoApuntar;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (cursorManoApuntar != null)
        {
            Cursor.SetCursor(cursorManoApuntar, Vector2.zero, CursorMode.Auto);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}