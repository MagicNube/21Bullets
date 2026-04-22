using UnityEngine;

public class EfectoLatido : MonoBehaviour
{
    private Vector3 escalaOriginal;

    void Awake()
    {
        escalaOriginal = transform.localScale;
    }

    void Update()
    {
        float scale = 1f + Mathf.Sin(Time.time * 8f) * 0.15f;
        transform.localScale = escalaOriginal * scale;
    }

    void OnDisable()
    {
        transform.localScale = escalaOriginal;
    }

    void OnDestroy()
    {
        transform.localScale = escalaOriginal;
    }
}