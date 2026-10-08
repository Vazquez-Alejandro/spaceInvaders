using UnityEngine;

// Movimiento del disparo de los enemigos.
public class LaserEnemigo : MonoBehaviour
{
    public float velocidad = 7f;
    public float limiteInferior = -8.5f;
    public Vector2 direccion = Vector2.down;

    private void Update()
    {
        transform.Translate(direccion.normalized * velocidad * Time.deltaTime, Space.World);

        if (transform.position.y < limiteInferior || Mathf.Abs(transform.position.x) > 12f)
        {
            Destroy(gameObject);
        }
    }
}
