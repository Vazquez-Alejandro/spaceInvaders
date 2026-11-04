using UnityEngine;

public class Laser : Proyectil
{
    public float topLimit = 7f;

    public AudioClip sonidoImpacto;
    [Range(0f, 1f)] public float volumenImpacto = 0.8f;

    [Header("Monedas")]
    [Range(0f, 1f)] public float probabilidadMoneda = 0.12f;

    private bool destruido = false;

    void Start()
    {
        velocidad = 10f;
        direccion = Vector2.up;
    }

    protected override void Update()
    {
        base.Update();
        ComprobarLímite();
        ComprobarSolapamiento();
    }

    private void ComprobarLímite()
    {
        if (transform.position.y > topLimit)
        {
            Destroy(gameObject);
        }
    }

    private void ComprobarSolapamiento()
    {
        if (destruido) return;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.6f);
        foreach (Collider2D c in hits)
        {
            if (c == null) continue;
            if (!c.gameObject.name.StartsWith("Invasor")) continue;
            if (c.GetComponent<SpriteRenderer>() == null) continue;

            destruido = true;
            if (sonidoImpacto != null && ControlEfectos.Activados)
            {
                AudioSource.PlayClipAtPoint(sonidoImpacto, transform.position, volumenImpacto);
            }
            ScoreManager.Sumar(10);
            SoltarMoneda(c.transform.position);
            Destroy(c.gameObject);
            Destroy(gameObject);
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D colision)
    {
        IntentarDestruir(colision);
    }

    private void OnTriggerStay2D(Collider2D colision)
    {
        IntentarDestruir(colision);
    }

    private void IntentarDestruir(Collider2D colision)
    {
        if (destruido) return;
        if (colision == null) return;
        if (!colision.gameObject.name.StartsWith("Invasor")) return;
        if (colision.GetComponent<SpriteRenderer>() == null) return;

        destruido = true;
        if (sonidoImpacto != null && ControlEfectos.Activados)
        {
            AudioSource.PlayClipAtPoint(sonidoImpacto, transform.position, volumenImpacto);
        }
        ScoreManager.Sumar(10);
        SoltarMoneda(colision.transform.position);
        Destroy(colision.gameObject);
        Destroy(gameObject);
    }

    private void SoltarMoneda(Vector3 posicion)
    {
        if (Random.value > probabilidadMoneda) return;

        GameObject moneda = new GameObject("Moneda");
        moneda.tag = "Pickup";
        moneda.transform.position = posicion;
        moneda.transform.localScale = Vector3.one * 6f;

        SpriteRenderer sr = moneda.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 3;

        Rigidbody2D rb = moneda.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        CircleCollider2D col = moneda.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.1f;

        moneda.AddComponent<Moneda>();
    }
}
