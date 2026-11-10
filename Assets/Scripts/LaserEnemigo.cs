using UnityEngine;

// Movimiento del disparo de los enemigos.
public class LaserEnemigo : Proyectil
{
    public float limiteInferior = -11f;

    protected override void Update()
    {
        base.Update();

        // Morir solo cuando queda realmente fuera de pantalla: la cámara
        // llega hasta ~-9.6 en Y y ~±17 en X, y la nave llega hasta -9.1.
        if (transform.position.y < limiteInferior || Mathf.Abs(transform.position.x) > 20f)
        {
            Destroy(gameObject);
        }
    }
}
