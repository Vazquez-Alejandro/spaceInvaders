using UnityEngine;

// Clase base para proyectiles (herencia y polimorfismo - Clase 13)
public class Proyectil : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 7f;
    public Vector2 direccion = Vector2.down;

    protected virtual void Update()
    {
        Mover();
    }

    protected virtual void Mover()
    {
        transform.Translate(direccion.normalized * velocidad * Time.deltaTime, Space.World);
    }
}
