using UnityEngine;

public class Laser : Proyectil
{
    public float topLimit = 7f;

    public AudioClip sonidoImpacto;
    [Range(0f, 1f)] public float volumenImpacto = 0.9f;

    [Header("Monedas")]
    [Range(0f, 1f)] public float probabilidadMoneda = 0.3f;

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
        Physics2D.SyncTransforms();
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.6f);

        foreach (Collider2D c in hits)
        {
            if (c == null) continue;
            NaveNodriza nave = c.GetComponent<NaveNodriza>();
            if (nave != null && nave.Activa)
            {
                destruido = true;
                CrearExplosion(transform.position, 0.5f, new Color(0.9f, 0.9f, 0.9f, 1f));
                nave.RecibirDano(1);
                Destroy(gameObject);
                return;
            }
        }

        Collider2D objetivo = null;
        float mejorDist = float.MaxValue;
        foreach (Collider2D c in hits)
        {
            if (c == null) continue;
            if (!c.gameObject.name.StartsWith("Invasor")) continue;
            if (c.GetComponent<SpriteRenderer>() == null) continue;

            float dist = ((Vector2)c.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (dist < mejorDist)
            {
                mejorDist = dist;
                objetivo = c;
            }
        }
        if (objetivo == null) return;

        destruido = true;
        if (sonidoImpacto != null && ControlEfectos.Activados)
        {
            AudioSource.PlayClipAtPoint(sonidoImpacto, transform.position, volumenImpacto);
        }
        SpawnExplosion(objetivo);
        ScoreManager.Sumar(10);
        SoltarMoneda(objetivo.transform.position);
        Destroy(objetivo.gameObject);
        Destroy(gameObject);
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

        NaveNodriza nave = colision.GetComponent<NaveNodriza>();
        if (nave != null && nave.Activa)
        {
            destruido = true;
            CrearExplosion(transform.position, 0.5f, new Color(0.9f, 0.9f, 0.9f, 1f));
            nave.RecibirDano(1);
            Destroy(gameObject);
            return;
        }

        if (!colision.gameObject.name.StartsWith("Invasor")) return;
        if (colision.GetComponent<SpriteRenderer>() == null) return;

        destruido = true;
        if (sonidoImpacto != null && ControlEfectos.Activados)
        {
            AudioSource.PlayClipAtPoint(sonidoImpacto, transform.position, volumenImpacto);
        }
        SpawnExplosion(colision);
        ScoreManager.Sumar(10);
        SoltarMoneda(colision.transform.position);
        Destroy(colision.gameObject);
        Destroy(gameObject);
    }


    private static readonly Color ColorExplosion = new Color(0.78f, 0.78f, 0.78f, 1f);

    private void SpawnExplosion(Collider2D colision)
    {
        SpriteRenderer enemySr = colision != null ? colision.GetComponent<SpriteRenderer>() : null;
        float tamano = enemySr != null ? enemySr.bounds.size.x : 0.33f;
        Vector3 pos = colision != null ? colision.transform.position : transform.position;
        CrearExplosion(pos, tamano, ColorExplosion);
    }

    public static void CrearExplosion(Vector3 pos, float tamano, Color color)
    {
        GameObject go = new GameObject("Explosion");
        go.transform.position = pos;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        Sprite src = Resources.Load<Sprite>("Explosion");
        float escala = 6f;
        if (src != null)
        {
            sr.sprite = src;
            float baseW = src.bounds.size.x;
            if (baseW > 0.0001f) escala = tamano / baseW;
        }
        go.transform.localScale = Vector3.one * escala;
        sr.sortingOrder = 6;
        sr.color = color;

        Destroy(go, 0.25f);
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
