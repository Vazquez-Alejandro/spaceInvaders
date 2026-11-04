using UnityEngine;

// Movimiento del disparo de los enemigos.
public class LaserEnemigo : Proyectil
{
    public float limiteInferior = -8.5f;

    private void Update()
    {
        base.Update();

        if (transform.position.y < limiteInferior || Mathf.Abs(transform.position.x) > 12f)
        {
            Destroy(gameObject);
        }
    }
}
