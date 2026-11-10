using UnityEngine;

// Comportamiento de jefe de la nave nodriza (nivel 2).
// Se activa cuando ya no quedan aliens: se mueve por la pantalla y dispara.
public class NaveNodriza : MonoBehaviour
{
    public int vidaMaxima = 10;
    public float velocidad = 5f;
    public float intervaloDisparo = 1.3f;
    public System.Action AlMorir;

    public bool Activa { get; private set; }

    private int vidaActual;
    private float temporizador;
    private Vector2 direccion = new Vector2(1f, 0.6f);
    private float limiteX = 12f;
    private float limiteYMin = 2f;
    private float limiteYMax = 6f;
    private float altura = 1.15f;

    private Transform jugador;
    private GameObject prefabBala;
    private AudioSource audioDisparo;
    private SpriteRenderer sr;
    private Color colorBase = Color.white;
    private AudioClip sonidoDisparo;
    private float volumenDisparo = 1f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) colorBase = sr.color;

        audioDisparo = GetComponent<AudioSource>();
        if (audioDisparo == null) audioDisparo = gameObject.AddComponent<AudioSource>();
        audioDisparo.playOnAwake = false;
        audioDisparo.spatialBlend = 0f;
    }

    public void Activar()
    {
        Activa = true;
        vidaActual = vidaMaxima;

        GameObject p = GameObject.Find("Player");
        if (p != null) jugador = p.transform;

        DisparoEnemigo de = FindFirstObjectByType<DisparoEnemigo>();
        if (de != null)
        {
            prefabBala = de.prefabBala;
            sonidoDisparo = de.sonidoDisparo;
            if (sonidoDisparo == null) sonidoDisparo = Resources.Load<AudioClip>("Sounds/InvasorShoot");
            volumenDisparo = Mathf.Max(de.volumenDisparo, 1f);
        }
        else
        {
            sonidoDisparo = Resources.Load<AudioClip>("Sounds/InvasorShoot");
            volumenDisparo = 1f;
        }

        Camera cam = Camera.main;
        if (cam != null && cam.orthographic && sr != null)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            float ew = sr.bounds.extents.x;
            altura = sr.bounds.extents.y;
            limiteX = Mathf.Max(1f, halfW - ew - 0.3f);
        }

        limiteYMin = 2f;
        limiteYMax = 6f;

        direccion = new Vector2(Random.value < 0.5f ? -1f : 1f, Random.Range(0.35f, 0.8f));
        direccion.Normalize();
        temporizador = intervaloDisparo;
        gameObject.name = "NaveNodriza";
    }

    private void Update()
    {
        if (!Activa) return;
        Mover();
        Disparar();
    }

    private void Mover()
    {
        Vector3 p = transform.position + (Vector3)(direccion * velocidad * Time.deltaTime);

        if (p.x < -limiteX) { p.x = -limiteX; direccion.x = Mathf.Abs(direccion.x); }
        if (p.x > limiteX) { p.x = limiteX; direccion.x = -Mathf.Abs(direccion.x); }
        if (p.y < limiteYMin) { p.y = limiteYMin; direccion.y = Mathf.Abs(direccion.y); }
        if (p.y > limiteYMax) { p.y = limiteYMax; direccion.y = -Mathf.Abs(direccion.y); }

        transform.position = p;
    }

    private void Disparar()
    {
        temporizador -= Time.deltaTime;
        if (temporizador > 0f) return;
        temporizador = intervaloDisparo;
        if (prefabBala == null) return;

        Vector3 origen = transform.position + Vector3.down * (altura * 0.7f);

        Vector2 baseDir = Vector2.down;
        if (jugador != null)
        {
            baseDir = ((Vector2)(jugador.position - origen)).normalized;
            if (baseDir.y > -0.4f) baseDir.y = -0.4f;
            baseDir.Normalize();
        }

        for (int i = -1; i <= 1; i++)
        {
            Vector2 d = Quaternion.Euler(0f, 0f, i * 12f) * baseDir;
            GameObject bala = Instantiate(prefabBala, origen + (Vector3)(d * 0.3f), Quaternion.FromToRotation(Vector2.down, d));
            LaserEnemigo laser = bala.GetComponent<LaserEnemigo>();
            if (laser != null) laser.direccion = d;
        }

        if (audioDisparo != null && sonidoDisparo != null && ControlEfectos.Activados)
        {
            audioDisparo.PlayOneShot(sonidoDisparo, volumenDisparo);
        }
    }

    public void RecibirDano(int dano)
    {
        if (!Activa) return;

        vidaActual -= dano;
        StartCoroutine(Flash());

        if (vidaActual <= 0)
        {
            Activa = false;
            if (audioDisparo != null && ControlEfectos.Activados)
            {
                AudioClip boom = Resources.Load<AudioClip>("Sounds/Explosion");
                if (boom != null) audioDisparo.PlayOneShot(boom, volumenDisparo);
            }
            Morir();
        }
    }

    private System.Collections.IEnumerator Flash()
    {
        if (sr == null) yield break;
        sr.color = new Color(1f, 0.4f, 0.4f, 1f);
        yield return new WaitForSeconds(0.05f);
        if (sr != null) sr.color = colorBase;
    }

    private void Morir()
    {
        for (int i = 0; i < 8; i++)
        {
            Vector3 off = new Vector3(Random.Range(-2.2f, 2.2f), Random.Range(-0.9f, 0.9f), 0f);
            Laser.CrearExplosion(transform.position + off, Random.Range(0.8f, 1.5f), new Color(0.8f, 0.8f, 0.8f, 1f));
        }

        Destroy(gameObject);
        if (AlMorir != null) AlMorir.Invoke();
    }
}
