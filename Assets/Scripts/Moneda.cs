using System.Collections.Generic;
using UnityEngine;

public class Moneda : MonoBehaviour
{
    public float velocidadCaida = 2.5f;
    public float fotogramasPorSegundo = 10f;
    public float limiteInferior = -11f;

    private Sprite[] fotogramas;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private float tiempo;
    private int indice;
    private bool recogida;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        List<Sprite> encontrados = new List<Sprite>();
        Sprite[] todos = Resources.LoadAll<Sprite>("Pickups");
        if (todos != null)
        {
            foreach (Sprite s in todos)
            {
                if (s.name.StartsWith("Moneda_")) encontrados.Add(s);
            }
        }
        encontrados.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        fotogramas = encontrados.ToArray();

        if (sr != null && fotogramas.Length > 0) sr.sprite = fotogramas[0];
    }

    void Update()
    {
        if (sr == null || fotogramas == null || fotogramas.Length < 2) return;

        tiempo += Time.deltaTime;
        float duracion = 1f / Mathf.Max(1f, fotogramasPorSegundo);
        while (tiempo >= duracion)
        {
            tiempo -= duracion;
            indice = (indice + 1) % fotogramas.Length;
            sr.sprite = fotogramas[indice];
        }
    }

    void FixedUpdate()
    {
        Vector2 paso = Vector2.down * velocidadCaida * Time.fixedDeltaTime;
        if (rb != null) rb.MovePosition(rb.position + paso);
        else transform.position += (Vector3)paso;

        if (transform.position.y < limiteInferior) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (recogida) return;

        PlayerController jugador = other.GetComponent<PlayerController>();
        if (jugador == null) jugador = other.GetComponentInParent<PlayerController>();
        if (jugador == null) return;

        recogida = true;
        jugador.RecogerMoneda();
        Destroy(gameObject);
    }
}
