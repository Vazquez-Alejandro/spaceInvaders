using UnityEngine;

public class Laser : MonoBehaviour
{
    public float speed = 10f;
    public float topLimit = 6f;

    public AudioClip sonidoImpacto;
    [Range(0f, 1f)] public float volumenImpacto = 0.8f;

    private void Update()
    {
        MoverLaser();
        ComprobarLímite();
    }

    private void MoverLaser()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    private void ComprobarLímite()
    {
        if (transform.position.y > topLimit)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D colision)
    {
        if (colision.gameObject.name.StartsWith("Invasor") &&
            colision.GetComponent<SpriteRenderer>() != null)
        {
            if (sonidoImpacto != null && ControlEfectos.Activados)
            {
                AudioSource.PlayClipAtPoint(sonidoImpacto, transform.position, volumenImpacto);
            }

            Destroy(colision.gameObject);
            Destroy(gameObject);
        }
    }
}