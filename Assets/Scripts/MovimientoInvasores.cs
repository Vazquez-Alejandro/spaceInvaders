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

    private const float MARGEN_LATERAL = 1f;
    private const float EPSILON_LIMITE = 0.001f;

    private readonly List<Transform> enemigos = new List<Transform>();
    private int totalInicial;
    private float temporizador;
    private int direccion = 1;
    private float limiteIzquierdo;
    private float limiteDerecho;
    private float pasoActual;

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
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.name.StartsWith("Invasor") && t.GetComponent<SpriteRenderer>() != null)
            {
                enemigos.Add(t);
            }
        }
        totalInicial = Mathf.Max(1, enemigos.Count);
        temporizador = intervaloMaximo;
        AjustarFormacion();
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
        if (enemigos.Count == 0) { Recoger(); return; }

        enemigos.RemoveAll(e => e == null);

        List<Transform> vivos = enemigos.FindAll(e => e.gameObject.activeInHierarchy);
        if (vivos.Count == 0) return;

        temporizador -= Time.deltaTime;
        if (temporizador > 0f) return;

        temporizador = CalcularIntervalo(vivos.Count);
        float intervalo = temporizador;

        float minX = vivos.Min(e => e.position.x);
        float maxX = vivos.Max(e => e.position.x);

        bool tocaLimite = (direccion > 0 && maxX + pasoActual > limiteDerecho + EPSILON_LIMITE) ||
                          (direccion < 0 && minX - pasoActual < limiteIzquierdo - EPSILON_LIMITE);

        if (tocaLimite)
        {
            foreach (Transform e in vivos)
            {
                Vector3 p = e.position;
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

    private float CalcularIntervalo(int vivos)
    {
        float factor = (float)vivos / totalInicial;
        return Mathf.Lerp(intervaloMinimo, intervaloMaximo, factor);
    }
}
