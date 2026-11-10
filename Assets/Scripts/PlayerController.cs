using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float velocidadSprint = 1.5f;

    [Header("Disparo")]
    public GameObject prefabBala;
    public Transform puntoDisparo;
    [SerializeField] private float intervaloDisparo = 0.6f;

    public float IntervaloDisparo
    {
        get => intervaloDisparo;
        set => intervaloDisparo = Mathf.Max(0.01f, value);
    }

    [Header("Atributos")]
    [SerializeField] private int vida = 5;
    [SerializeField] private int vidaMaxima = 5;
    [SerializeField] private int monedas = 0;

    public int Vida
    {
        get => vida;
        set => vida = Mathf.Clamp(value, 0, VidaMaxima);
    }

    public int VidaMaxima
    {
        get => vidaMaxima;
        set => vidaMaxima = Mathf.Max(1, value);
    }

    public int Monedas
    {
        get => monedas;
        set => monedas = Mathf.Max(0, value);
    }

    [Header("Mejora por moneda")]
    [SerializeField] private float factorMejoraMoneda = 0.85f;
    [SerializeField] private float intervaloMinimo = 0.25f;
    [SerializeField] private int puntosPorMoneda = 50;

    [Header("Sonido de daño")]
    [SerializeField] private AudioClip sonidoRecibirDano;
    [Range(0f, 1f)] [SerializeField] private float volumenDano = 0.8f;

    [Header("Límites de movimiento")]
    [SerializeField] private float nivelBunkeres = -7f;

    [Header("Inclinación al moverse")]
    [SerializeField] private float inclinacionMaxima = 25f;
    [SerializeField] private float velocidadInclinacion = 150f;

    private Rigidbody2D rb;
    private AudioSource audioDisparo;
    private Vector2 movimiento;

    // Controles táctiles (botones en pantalla) — los setea BotonTouch
    [HideInInspector] public bool tocIzquierda;
    [HideInInspector] public bool tocDerecha;
    [HideInInspector] public bool tocDisparo;

    private Camera camara;
    private float proximoDisparo;
    private float intervaloEfectivo;
    private float topeSuperior;
    private float inclinacionActual;
    private Vector3 offsetDisparo;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioDisparo = GetComponent<AudioSource>();
        camara = Camera.main;
        VidaMaxima = Mathf.Max(1, vidaMaxima);
        Vida = Mathf.Clamp(vida, 0, VidaMaxima);
        intervaloEfectivo = IntervaloDisparo > 0.01f ? IntervaloDisparo : 0.6f;
        if (factorMejoraMoneda <= 0f || factorMejoraMoneda >= 1f) factorMejoraMoneda = 0.85f;
        if (intervaloMinimo <= 0f) intervaloMinimo = 0.25f;

        // Continuar las monedas acumuladas de la partida: los niveles suman.
        monedas = Mathf.Max(0, ScoreManager.Monedas);
        for (int i = 0; i < monedas; i++)
            intervaloEfectivo = Mathf.Max(intervaloMinimo, intervaloEfectivo * factorMejoraMoneda);

        // Offset del punto de disparo sin inclinación, para que la bala
        // salga siempre recta desde el centro aunque la nave esté inclinada.
        offsetDisparo = puntoDisparo != null
            ? puntoDisparo.position - transform.position
            : new Vector3(0f, 0.25f, 0f);

        // La nave no puede pasar de los bunkeres: tope = nivel bunkeres
        // menos media altura del bunker menos media altura de la nave.
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        float mediaJugador = (col != null ? col.size.y * Mathf.Abs(transform.lossyScale.y) : 0.3f) * 0.5f;
        const float mediaBunker = 0.3f; // escala 5 sobre sprite de 12 px (PPU 100) = 0.6 de alto
        topeSuperior = nivelBunkeres - mediaBunker - mediaJugador;

        Debug.Log($"[PlayerController] intervaloDisparo={intervaloDisparo} -> efectivo={intervaloEfectivo}");
    }

    void Update()
    {
        ProcesarEntradaTeclado();
        DetectarImpactos();
        ActualizarInclinacion();
    }

    // Inclinación lateral: se inclina hacia el lado que se mueve y
    // vuelve al centro al soltar la tecla.
    private void ActualizarInclinacion()
    {
        float objetivo = 0f;
        if (movimiento.x < -0.01f) objetivo = inclinacionMaxima;      // izquierda
        else if (movimiento.x > 0.01f) objetivo = -inclinacionMaxima; // derecha

        inclinacionActual = Mathf.MoveTowards(inclinacionActual, objetivo, velocidadInclinacion * Time.deltaTime);
    }

    void FixedUpdate()
    {
        MoverJugador();

        // La rotación va por el Rigidbody2D: autoSyncTransforms está
        // desactivado, así que transform.rotation lo pisaría la física.
        if (rb != null) rb.rotation = inclinacionActual;
    }

    // Maneja teclado y controles táctiles (en el celular no hay teclado:
    // Keyboard.current es null, así que no se puede cortar antes).
    private void ProcesarEntradaTeclado()
    {
        var keyboard = Keyboard.current;

        float moveX = 0f;
        float moveY = 0f;

        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveX = -1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveX = 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveY = 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveY = -1f;
        }

        if (tocIzquierda) moveX = -1f;
        if (tocDerecha) moveX = 1f;

        movimiento = new Vector2(moveX, moveY).normalized;

        // Disparo: espacio en PC o botón táctil (mantener = disparo seguido)
        bool disparar = tocDisparo || (keyboard != null && keyboard.spaceKey.wasPressedThisFrame);
        if (disparar && Time.time >= proximoDisparo)
        {
            Disparar();
            proximoDisparo = Time.time + intervaloEfectivo;
        }
    }

    // Función para calcular y aplicar el movimiento físico
    private void MoverJugador()
    {
        var keyboard = Keyboard.current;
        float velocidadActual = velocidad;

        if (keyboard != null && keyboard.leftShiftKey.isPressed)
        {
            velocidadActual *= velocidadSprint;
        }

        Vector2 destino = rb.position + movimiento * velocidadActual * Time.fixedDeltaTime;

        if (camara != null)
        {
            float mitadAlto = camara.orthographicSize;
            float mitadAncho = mitadAlto * camara.aspect;
            const float margen = 0.5f;
            Vector3 centro = camara.transform.position;

            destino.x = Mathf.Clamp(destino.x, centro.x - mitadAncho + margen, centro.x + mitadAncho - margen);

            float limiteInferior = centro.y - mitadAlto + margen;
            float limiteSuperior = Mathf.Min(centro.y + mitadAlto - margen, topeSuperior);
            destino.y = Mathf.Clamp(destino.y, limiteInferior, limiteSuperior);
        }
        else
        {
            destino.y = Mathf.Min(destino.y, topeSuperior);
        }

        rb.MovePosition(destino);
    }

    // Detección por superposición visual: donde la bala toque la nave
    // (ala incluida), resta vida. Caja un poco más grande que la nave
    // para que las balas que la raspan también registren.
    private void DetectarImpactos()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null) return;

        Bounds nave = col.bounds;
        nave.Expand(0.2f);

        foreach (LaserEnemigo bala in FindObjectsByType<LaserEnemigo>(FindObjectsSortMode.None))
        {
            if (bala == null) continue;

            SpriteRenderer sr = bala.GetComponent<SpriteRenderer>();
            bool toca = nave.Contains(bala.transform.position) ||
                        (sr != null && sr.bounds.Intersects(nave));
            if (!toca) continue;

            AplicarDano(1);
            Destroy(bala.gameObject);
            return; // un impacto por frame
        }
    }

    // Función para instanciar la bala
    private void Disparar()
    {
        if (prefabBala != null)
        {
            // Sin inclinación: la bala sale recta desde el centro de la nave.
            Vector3 pos = transform.position + offsetDisparo;
            Instantiate(prefabBala, pos, Quaternion.identity);

            if (audioDisparo != null && ControlEfectos.Activados)
            {
                audioDisparo.Play();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Portal"))
        {
            GestorNiveles.AvanzarNivel();
        }
        // El daño por láser lo maneja DetectarImpactos() (muestreo por frame).
    }

    // Reduce la vida y reproduce el sonido de impacto
    private void AplicarDano(int cantidad)
    {
        Vida = Mathf.Max(0, Vida - cantidad);
        BarraVida.RefrescarBarra();
        if (Vida <= 0)
        {
            if (audioDisparo != null && ControlEfectos.Activados)
            {
                AudioClip boom = Resources.Load<AudioClip>("Sounds/Explosion");
                if (boom != null) audioDisparo.PlayOneShot(boom, volumenDano);
            }
            SpawnExplosion(transform.position);
            Destroy(gameObject);
        }

        if (sonidoRecibirDano != null && ControlEfectos.Activados)
        {
            AudioSource.PlayClipAtPoint(sonidoRecibirDano, transform.position, volumenDano);
        }
    }

    // La moneda llama a este método al tocar la nave
    public void RecogerMoneda()
    {
        Monedas++;
        intervaloEfectivo = Mathf.Max(intervaloMinimo, intervaloEfectivo * factorMejoraMoneda);
        IntervaloDisparo = intervaloEfectivo;
        ScoreManager.Sumar(Mathf.Max(0, puntosPorMoneda));
        ScoreManager.ActualizarMonedas(Monedas);
        Debug.Log($"[PlayerController] moneda recogida ({Monedas}) +{puntosPorMoneda} pts -> intervaloDisparo={IntervaloDisparo:0.00}");
    }


    private void SpawnExplosion(Vector3 pos)
    {
        GameObject go = new GameObject("Explosion");
        go.SetActive(false);
        go.transform.position = pos;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.playOnAwake = false;
        main.duration = 0.5f;
        main.startLifetime = 0.4f;
        main.startSpeed = 4f;
        main.startSize = 0.2f;
        main.startColor = new Color(1f, 0.3f, 0.1f, 1f);
        main.maxParticles = 30;
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.5f, 0.1f), 0f), new GradientColorKey(Color.red, 1f) },
                  new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = g;

        go.SetActive(true);
        ps.Play();
        Destroy(go, 1.5f);
    }

    private void CambiarDeNivel(string nombreEscena)
    {
        SceneManager.LoadScene(nombreEscena);
    }
}