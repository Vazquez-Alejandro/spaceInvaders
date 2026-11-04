using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(1000)]
public class MovimientoInvasores : MonoBehaviour
{
    [Header("Movimiento")]
    public float pasoHorizontal = 1.5f;
    public float pasoVertical = 1f;
    public float intervaloMaximo = 0.9f;
    public float intervaloMinimo = 0.1f;

    private const float MARGEN_LATERAL = 0.3f;
    private const float EPSILON_LIMITE = 0.001f;

    private readonly List<Transform> enemigos = new List<Transform>();
    private readonly Dictionary<Transform, int> filaDe = new Dictionary<Transform, int>();
    private readonly Dictionary<int, int> direccionFila = new Dictionary<int, int>();

    private int totalInicial;
    private float temporizador;
    private int direccion = 1;
    private float limiteIzquierdo;
    private float limiteDerecho;
    private float pasoActual;
    private bool movimientoAlternado;
    private bool nivelCompletado;
    private bool formacionAjustada;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        if (FindFirstObjectByType<MovimientoInvasores>() != null) return;
        GameObject go = new GameObject("MovimientoInvasores");
        DontDestroyOnLoad(go);
        go.AddComponent<MovimientoInvasores>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    private void Start()
    {
        Recoger();
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        Recoger();
    }

    private void Recoger()
    {
        enemigos.Clear();
        filaDe.Clear();
        direccionFila.Clear();

        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.name.StartsWith("Invasor") && t.GetComponent<SpriteRenderer>() != null)
            {
                enemigos.Add(t);
            }
        }

        totalInicial = Mathf.Max(1, enemigos.Count);
        temporizador = intervaloMaximo;
        direccion = 1;
        nivelCompletado = false;

        movimientoAlternado = false;
        formacionAjustada = false;

        if (movimientoAlternado)
        {
            var ordenados = enemigos.OrderByDescending(e => e.position.y).ToList();
            int filaActual = -1;
            float yActual = float.NaN;
            foreach (Transform e in ordenados)
            {
                if (float.IsNaN(yActual) || Mathf.Abs(e.position.y - yActual) > 0.5f)
                {
                    filaActual++;
                    yActual = e.position.y;
                }
                filaDe[e] = filaActual;
            }

            int numeroFilas = filaActual + 1;
            for (int i = 0; i < numeroFilas; i++)
            {
                direccionFila[i] = (i % 2 == 0) ? 1 : -1;
            }
        }

    }

    private void AjustarFormacion()
    {
        Camera cam = Camera.main;
        float halfWidth = cam != null ? cam.orthographicSize * cam.aspect : 5.6f;
        float centroX = cam != null ? cam.transform.position.x : 0f;

        float limite = Mathf.Max(1f, halfWidth - MARGEN_LATERAL);

        float minX = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        foreach (Transform e in enemigos)
        {
            if (e == null) continue;
            minX = Mathf.Min(minX, e.position.x);
            maxX = Mathf.Max(maxX, e.position.x);
        }

        if (float.IsInfinity(minX))
        {
            limiteIzquierdo = centroX - limite;
            limiteDerecho = centroX + limite;
            pasoActual = pasoHorizontal;
            return;
        }

        float centroFormacion = (minX + maxX) * 0.5f;
        float desplazamiento = centroX - centroFormacion;
        foreach (Transform e in enemigos)
        {
            if (e == null) continue;
            Vector3 p = e.position;
            p.x += desplazamiento;
            e.position = p;
        }

        float halfSpan = (maxX - minX) * 0.5f;
        float espaciado = Mathf.Max(0.1f, pasoHorizontal);
        float margenPasos = 2f;

        float necesario = halfSpan + espaciado * margenPasos;
        float factor = necesario > limite ? Mathf.Clamp(limite / necesario, 0.1f, 1f) : 1f;

        if (factor < 0.999f)
        {
            foreach (Transform e in enemigos)
            {
                if (e == null) continue;
                Vector3 p = e.position;
                p.x = centroX + (p.x - centroX) * factor;
                e.position = p;
            }
            espaciado *= factor;
        }

        limiteIzquierdo = centroX - limite;
        limiteDerecho = centroX + limite;
        pasoActual = espaciado;
    }

    private void Update()
    {
        enemigos.RemoveAll(e => e == null);

        if (enemigos.Count == 0)
        {
            if (totalInicial > 1 && !nivelCompletado)
            {
                nivelCompletado = true;
                StartCoroutine(AvanzarNivel());
            }
            return;
        }

        if (!formacionAjustada)
        {
            AjustarFormacion();
            formacionAjustada = true;
        }

        List<Transform> vivos = enemigos.FindAll(e => e.gameObject.activeInHierarchy);
        if (vivos.Count == 0) return;

        temporizador -= Time.deltaTime;
        if (temporizador > 0f) return;

        temporizador = CalcularIntervalo(vivos.Count);
        float intervalo = temporizador;

        ActualizarLimites();

        if (movimientoAlternado)
        {
            var filas = new Dictionary<int, List<Transform>>();
            foreach (Transform e in vivos)
            {
                int f = filaDe.ContainsKey(e) ? filaDe[e] : 0;
                if (!filas.ContainsKey(f)) filas[f] = new List<Transform>();
                filas[f].Add(e);
            }

            foreach (var kv in filas)
            {
                int f = kv.Key;
                List<Transform> miembros = kv.Value;
                if (miembros.Count == 0) continue;

                int dir = direccionFila.ContainsKey(f) ? direccionFila[f] : 1;
                float minX = miembros.Min(e => ObtenerBordeIzquierdo(e));
                float maxX = miembros.Max(e => ObtenerBordeDerecho(e));

                bool tocaLimite = (dir > 0 && maxX + pasoActual > limiteDerecho + EPSILON_LIMITE) ||
                                  (dir < 0 && minX - pasoActual < limiteIzquierdo - EPSILON_LIMITE);

                if (tocaLimite)
                {
                    float desplazamientoX = (dir > 0) ? (limiteDerecho - maxX) : (limiteIzquierdo - minX);
                    foreach (Transform e in miembros)
                    {
                        Vector3 p = e.position;
                        p.x += desplazamientoX;
                        p.y -= pasoVertical;
                        e.position = p;
                    }
                    direccionFila[f] = -dir;
                }
                else
                {
                    foreach (Transform e in miembros)
                    {
                        Vector3 p = e.position;
                        p.x += pasoActual * dir;
                        e.position = p;
                    }
                }
            }

            AnimarPaso(vivos, intervalo);
        }
        else
        {
            float minX = vivos.Min(e => ObtenerBordeIzquierdo(e));
            float maxX = vivos.Max(e => ObtenerBordeDerecho(e));

            bool tocaLimite = (direccion > 0 && maxX + pasoActual > limiteDerecho + EPSILON_LIMITE) ||
                              (direccion < 0 && minX - pasoActual < limiteIzquierdo - EPSILON_LIMITE);

            if (tocaLimite)
            {
                float desplazamientoX = (direccion > 0) ? (limiteDerecho - maxX) : (limiteIzquierdo - minX);
                foreach (Transform e in vivos)
                {
                    Vector3 p = e.position;
                    p.x += desplazamientoX;
                    p.y -= pasoVertical;
                    e.position = p;
                }
                direccion = -direccion;
            }
            else
            {
                foreach (Transform e in vivos)
                {
                    Vector3 p = e.position;
                    p.x += pasoActual * direccion;
                    e.position = p;
                }
                AnimarPaso(vivos, intervalo);
            }
        }
    }

    private void AnimarPaso(List<Transform> vivos, float duracion)
    {
        foreach (Transform e in vivos)
        {
            Animator anim = e.GetComponent<Animator>();
            if (anim == null) continue;

            var clips = anim.GetCurrentAnimatorClipInfo(0);
            float largo = 1f;
            if (clips != null && clips.Length > 0 && clips[0].clip != null)
            {
                largo = clips[0].clip.length;
            }

            anim.speed = largo / Mathf.Max(0.05f, duracion);

            AnimatorStateInfo estado = anim.GetCurrentAnimatorStateInfo(0);
            if (estado.fullPathHash != 0)
            {
                anim.Play(estado.fullPathHash, 0, 0f);
            }
        }
    }

    private System.Collections.IEnumerator AvanzarNivel()
    {
        yield return new WaitForSeconds(1.5f);
        if (GestorNiveles.NivelActual >= 2)
        {
            CreditosUI.MostrarAlIniciar = true;
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            GestorNiveles.AvanzarNivel();
        }
    }

    private float ObtenerBordeIzquierdo(Transform e)
    {
        if (e == null) return 0f;
        SpriteRenderer sr = e.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return e.position.x - 0.5f;
        Bounds b = sr.bounds;
        if (b.size.x < 0.0001f) return e.position.x - 0.5f;
        return b.min.x;
    }

    private float ObtenerBordeDerecho(Transform e)
    {
        if (e == null) return 0f;
        SpriteRenderer sr = e.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return e.position.x + 0.5f;
        Bounds b = sr.bounds;
        if (b.size.x < 0.0001f) return e.position.x + 0.5f;
        return b.max.x;
    }

    private float ObtenerMitadAncho(Transform e)
    {
        return (ObtenerBordeDerecho(e) - ObtenerBordeIzquierdo(e)) * 0.5f;
    }

    private void ActualizarLimites()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        float halfWidth = cam.orthographicSize * cam.aspect;
        float centroX = cam.transform.position.x;
        float limite = Mathf.Max(1f, halfWidth - MARGEN_LATERAL);
        limiteIzquierdo = centroX - limite;
        limiteDerecho = centroX + limite;
    }

    private float CalcularIntervalo(int vivos)
    {
        float factor = (float)vivos / totalInicial;
        return Mathf.Lerp(intervaloMinimo, intervaloMaximo, factor);
    }
}
