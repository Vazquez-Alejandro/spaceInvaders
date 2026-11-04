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

    private Rigidbody2D rb;
    private AudioSource audioDisparo;
    private Vector2 movimiento;
    private Camera camara;
    private float proximoDisparo;
    private float intervaloEfectivo;

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
        Debug.Log($"[PlayerController] intervaloDisparo={intervaloDisparo} -> efectivo={intervaloEfectivo}");
    }

    void Update()
    {
        ProcesarEntradaTeclado();
    }

    void FixedUpdate()
    {
        MoverJugador();
    }

    // Función para manejar las teclas y las acciones de entrada
    private void ProcesarEntradaTeclado()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Captura el movimiento
        float moveX = 0f;
        float moveY = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveX = -1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveX = 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveY = 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveY = -1f;

        movimiento = new Vector2(moveX, moveY).normalized;

        // Disparo (Espacio)
        if (keyboard.spaceKey.wasPressedThisFrame && Time.time >= proximoDisparo)
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
            destino.y = Mathf.Clamp(destino.y, centro.y - mitadAlto + margen, centro.y + mitadAlto - margen);
        }

        rb.MovePosition(destino);
    }

    // Función para instanciar la bala
    private void Disparar()
    {
        if (prefabBala != null)
        {
            Vector3 pos = puntoDisparo != null ? puntoDisparo.position : transform.position;
            Instantiate(prefabBala, pos, transform.rotation);

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

        if (collision.gameObject.name.StartsWith("EnemyLaser") &&
            collision.GetComponent<SpriteRenderer>() != null)
        {
            AplicarDano(1);
            Destroy(collision.gameObject);
        }
    }

    // Reduce la vida y reproduce el sonido de impacto
    private void AplicarDano(int cantidad)
    {
        Vida = Mathf.Max(0, Vida - cantidad);
        BarraVida.RefrescarBarra();
        if (Vida <= 0)
        {
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

    private void CambiarDeNivel(string nombreEscena)
    {
        SceneManager.LoadScene(nombreEscena);
    }
}