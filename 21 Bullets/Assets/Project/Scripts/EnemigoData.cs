using UnityEngine;

[CreateAssetMenu(fileName = "NuevoEnemigo", menuName = "21 Bullets/Enemigo")]
public class EnemigoData : ScriptableObject
{
    public string nombre;
    public Sprite spriteRival;
    public int vidasMaximas = 3;
    public int umbralPlantadoBlackjack = 14;
    public bool puedeUsarObjetos = false;
}
