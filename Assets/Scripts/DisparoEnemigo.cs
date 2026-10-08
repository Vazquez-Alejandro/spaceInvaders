using System.Collections.Generic;
using UnityEngine;

// Disparo de los enemigos del nivel 1: ritmo tranquilo y algo aleatorio.
public class DisparoEnemigo : MonoBehaviour
{
    [Header("Disparo")]
    public GameObject prefabBala;
    public AudioClip sonidoDisparo;
    [Range(0f, 1f)] public float volumenDisparo = 0.8f;

    [Header("Ritmo (nivel 1 tranquilo)")]
    public float intervaloMinimo = 1.5f;
    public float intervaloMaximo = 3f;
    public int balasMaximas = 3;

    [Header("Apuntado al jugador")]
    public Transform jugador;
    [Range(0f, 1f)] public float probabilidadApuntar = 0.5f;
    public float desvioHorizontal = 1.5f;

    private readonly List<Transform> enemigos = new List<Transform>();
    private float temporizador;
    private AudioSource audioDisparo;

    private void Start()
    {
        audioDisparo = GetComponent<AudioSource>();

        if (jugador == null)
        {
            GameObject jugadorObj = GameObject.Find("Player");
            if (jugadorObj != null) jugador = jugadorObj.transform;
        }

        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.name.StartsWith("Invasor") && t.GetComponent<SpriteRenderer>() != null)
            {
                enemigos.Add(t);
            }
        }

        temporizador = Random.Range(intervaloMinimo, intervaloMaximo);
    }

    private void Update()
    {
        temporizador -= Time.deltaTime;
        if (temporizador <= 0f)
        {
            Disparar();
            temporizador = Random.Range(intervaloMinimo, intervaloMaximo);
        }
    }

    private void Disparar()
    {
        if (prefabBala == null) return;
        if (FindObjectsByType<LaserEnemigo>(FindObjectsSortMode.None).Length >= balasMaximas) return;

        Transform enemigo = ElegirEnemigo();
        if (enemigo == null) return;

        Vector3 posicion = enemigo.position;
        Vector2 direccion = Vector2.down;

        if (jugador != null && Random.value <= probabilidadApuntar)
        {
            Vector3 objetivo = jugador.position;
            objetivo.x += Random.Range(-desvioHorizontal, desvioHorizontal);

            direccion = ((Vector2)(objetivo - posicion)).normalized;
            if (direccion.y > -0.3f) direccion.y = -0.3f;
            direccion.Normalize();
        }

        GameObject bala = Instantiate(prefabBala, posicion, Quaternion.FromToRotation(Vector2.down, direccion));

        LaserEnemigo laser = bala.GetComponent<LaserEnemigo>();
        if (laser != null) laser.direccion = direccion;

        if (audioDisparo != null && sonidoDisparo != null && ControlEfectos.Activados)
        {
            audioDisparo.PlayOneShot(sonidoDisparo, volumenDisparo);
        }
    }

    private Transform ElegirEnemigo()
    {
        enemigos.RemoveAll(e => e == null);

        List<Transform> vivos = enemigos.FindAll(e => e.gameObject.activeInHierarchy);
        if (vivos.Count == 0) return null;

        return vivos[Random.Range(0, vivos.Count)];
    }
}
