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

    [Header("Atributos")]
    public int vida = 100;
    public int vidaMaxima = 100;
    public int monedas = 0;

    [Header("Sonido de daño")]
    [SerializeField] private AudioClip sonidoRecibirDano;
    [Range(0f, 1f)] [SerializeField] private float volumenDano = 0.8f;

    private Rigidbody2D rb;
    private AudioSource audioDisparo;
    private Vector2 movimiento;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioDisparo = GetComponent<AudioSource>();
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
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            Disparar();
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

        rb.MovePosition(rb.position + movimiento * velocidadActual * Time.fixedDeltaTime);
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
        if (collision.CompareTag("Pickup"))
        {
            RecolectarMoneda(collision.gameObject);
        }

        if (collision.CompareTag("Portal"))
        {
            CambiarDeNivel("Nivel2");
        }

        if (collision.gameObject.name.StartsWith("EnemyLaser") &&
            collision.GetComponent<SpriteRenderer>() != null)
        {
            AplicarDano(10);
            Destroy(collision.gameObject);
        }
    }

    // Reduce la vida y reproduce el sonido de impacto
    private void AplicarDano(int cantidad)
    {
        vida = Mathf.Max(0, vida - cantidad);

        if (sonidoRecibirDano != null && ControlEfectos.Activados)
        {
            AudioSource.PlayClipAtPoint(sonidoRecibirDano, transform.position, volumenDano);
        }
    }

    // Funciones auxiliares para delegar responsabilidades de colisión
    private void RecolectarMoneda(GameObject monedaObj)
    {
        monedas++;
        monedaObj.SetActive(false);
    }

    private void CambiarDeNivel(string nombreEscena)
    {
        SceneManager.LoadScene(nombreEscena);
    }
}