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
    public float intervaloMaximo = 0.45f;
    public float intervaloMinimo = 0.15f;

    // En pantallas anchas (WebGL 960x600) los aliens tardaban una eternidad
    // en cruzar: se acota el campo de marcha para que el ritmo no dependa
    // del ancho de la pantalla.
    public float anchoMaximoMarcha = 24f;

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
    private bool nivelSpawn;
    private bool spawning;
    private bool oleadaActiva;
    private bool jefeActivo;
    private GameObject ufo;

    private Vector3 desplazamiento;
    private readonly Dictionary<Transform, Vector3> slotDe = new Dictionary<Transform, Vector3>();
    private readonly Dictionary<Transform, Vector3> salidaDe = new Dictionary<Transform, Vector3>();
    private readonly Dictionary<Transform, float> emergeDe = new Dictionary<Transform, float>();
    private const float EMERGE_DURACION = 0.3f;

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
        StopAllCoroutines();
        if (ufo != null) Destroy(ufo);
        ufo = null;
        spawning = false;
        oleadaActiva = false;
        jefeActivo = false;

        enemigos.Clear();
        filaDe.Clear();
        direccionFila.Clear();
        slotDe.Clear();
        salidaDe.Clear();
        emergeDe.Clear();
        desplazamiento = Vector3.zero;

        nivelSpawn = (GestorNiveles.NivelActual == 2);
        spawning = false;

        if (nivelSpawn)
        {
            // Desactivar aliens pre-colocados
            foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (t.name.StartsWith("Invasor") && t.GetComponent<SpriteRenderer>() != null)
                {
                    t.gameObject.SetActive(false);
                }
            }

            totalInicial = 0;
            temporizador = intervaloMaximo;
            direccion = 1;
            pasoActual = pasoHorizontal;
            nivelCompletado = false;
            movimientoAlternado = false;
            formacionAjustada = false;

            CrearUFO();
            StartCoroutine(DescenderUFO());
            return;
        }

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

        float limite = CalcularLimite(halfWidth);

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


    private void CrearUFO()
    {
        Sprite src = Resources.Load<Sprite>("UFO");
        if (src == null)
        {
            Debug.LogWarning("UFO sprite no encontrado en Resources/UFO");
            return;
        }
        GameObject go = new GameObject("UFO");
        go.transform.position = new Vector3(0f, 14f, 0f);
        go.transform.localScale = new Vector3(33f, 33f, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = src;
        sr.sortingOrder = 5;
        sr.color = Color.white;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = src.bounds.size;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        go.AddComponent<NaveNodriza>();
        ufo = go;
    }

    private System.Collections.IEnumerator DescenderUFO()
    {
        spawning = true;
        Vector3 inicio = new Vector3(0f, 14f, 0f);
        Vector3 fin = new Vector3(0f, 6f, 0f);
        float duracion = 2f;
        float t = 0f;
        while (t < duracion)
        {
            if (ufo == null) { spawning = false; yield break; }
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duracion));
            ufo.transform.position = Vector3.Lerp(inicio, fin, k);
            yield return null;
        }
        if (ufo != null) ufo.transform.position = fin;
        yield return new WaitForSeconds(0.7f);
        StartCoroutine(SpawnAliens());
    }

    private System.Collections.IEnumerator SpawnAliens()
    {
        if (oleadaActiva) yield break;
        oleadaActiva = true;
        spawning = true;
        string[] prefabs = { "Invasor1", "Invasor2", "Invasor3", "Invasor4", "Invasor5" };
        int filas = 3;
        int cols = 6;
        float gapX = 0.9f;
        float gapY = 0.75f;
        float startX = -(cols - 1) * gapX * 0.5f;
        float startY = 3f;
        Vector3 baseSalida = (ufo != null ? ufo.transform.position : new Vector3(0f, 6f, 0f)) - new Vector3(0f, 1.2f, 0f);
        totalInicial = filas * cols;
        int idx = 0;
        for (int f = 0; f < filas; f++)
        {
            for (int c = 0; c < cols; c++)
            {
                string pname = prefabs[idx % prefabs.Length];
                int numero = idx + 1;
                idx++;
                GameObject prefab = Resources.Load<GameObject>("Prefabs/" + pname);
                if (prefab == null) prefab = Resources.Load<GameObject>(pname);
                if (prefab == null) continue;

                Vector3 slot = new Vector3(startX + c * gapX, startY - f * gapY, 0f);
                Vector3 salida = new Vector3(slot.x * 0.4f, baseSalida.y, 0f);

                GameObject go = Instantiate(prefab, salida, Quaternion.identity);
                go.name = "Invasor" + numero;
                go.transform.localScale = new Vector3(3f, 3f, 1f);
                enemigos.Add(go.transform);
                slotDe[go.transform] = slot;
                salidaDe[go.transform] = salida;
                emergeDe[go.transform] = 0f;

                yield return new WaitForSeconds(0.3f);
            }
        }
        spawning = false;
        oleadaActiva = false;
    }

    private void Update()
    {
        if (enemigos.RemoveAll(e => e == null) > 0)
        {
            LimpiarDiccionariosMuertos();
        }

        if (nivelSpawn)
        {
            UpdateSpawn();
            return;
        }

        if (enemigos.Count == 0 && !spawning)
        {
            if (totalInicial > 1 && !nivelCompletado)
            {
                nivelCompletado = true;
                StartCoroutine(AvanzarNivel());
            }
            return;
        }

        if (!formacionAjustada && !spawning)
        {
            AjustarFormacion();
            formacionAjustada = true;
        }

        if (spawning) return;

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

    private void LimpiarDiccionariosMuertos()
    {
        var muertos = new List<Transform>();
        foreach (var kv in slotDe)
        {
            if (kv.Key == null) muertos.Add(kv.Key);
        }
        foreach (Transform t in muertos)
        {
            slotDe.Remove(t);
            salidaDe.Remove(t);
            emergeDe.Remove(t);
        }
    }

    private void AvanzarEmergencia()
    {
        foreach (Transform e in enemigos)
        {
            if (e == null) continue;
            if (!emergeDe.TryGetValue(e, out float em)) continue;
            if (em < 1f) emergeDe[e] = Mathf.Min(1f, em + Time.deltaTime / EMERGE_DURACION);
        }
    }

    private void AplicarPosiciones()
    {
        foreach (Transform e in enemigos)
        {
            if (e == null) continue;
            if (!slotDe.TryGetValue(e, out Vector3 slot)) continue;
            float em = emergeDe.TryGetValue(e, out float v) ? v : 1f;
            Vector3 objetivo = slot + desplazamiento;
            Vector3 salida = salidaDe.TryGetValue(e, out Vector3 s) ? s : objetivo;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(em));
            e.position = Vector3.Lerp(salida, objetivo, k);
        }
        Physics2D.SyncTransforms();
    }

    private float HalfWidth(Transform e)
    {
        SpriteRenderer sr = e.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return 0.5f;
        return sr.bounds.extents.x;
    }

    private void UpdateSpawn()
    {
        AvanzarEmergencia();
        AplicarPosiciones();

        if (enemigos.Count == 0)
        {
            if (spawning) return;
            if (totalInicial > 1 && !nivelCompletado)
            {
                if (nivelSpawn && ufo != null && !jefeActivo)
                {
                    IniciarJefe();
                }
                else if (!jefeActivo)
                {
                    nivelCompletado = true;
                    StartCoroutine(AvanzarNivel());
                }
            }
            return;
        }

        ActualizarLimites();

        temporizador -= Time.deltaTime;
        if (temporizador > 0f) return;
        temporizador = CalcularIntervalo(enemigos.Count);
        float intervalo = temporizador;

        float minX = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        foreach (Transform e in enemigos)
        {
            if (e == null) continue;
            if (!slotDe.TryGetValue(e, out Vector3 slot)) continue;
            float half = HalfWidth(e);
            float cx = (slot + desplazamiento).x;
            minX = Mathf.Min(minX, cx - half);
            maxX = Mathf.Max(maxX, cx + half);
        }
        if (float.IsInfinity(minX)) return;

        bool tocaLimite = (direccion > 0 && maxX + pasoActual > limiteDerecho + EPSILON_LIMITE) ||
                          (direccion < 0 && minX - pasoActual < limiteIzquierdo - EPSILON_LIMITE);

        if (tocaLimite)
        {
            float dx = (direccion > 0) ? (limiteDerecho - maxX) : (limiteIzquierdo - minX);
            desplazamiento.x += dx;
            desplazamiento.y -= pasoVertical;
            direccion = -direccion;
        }
        else
        {
            desplazamiento.x += pasoActual * direccion;
        }

        AplicarPosiciones();
        AnimarPaso(enemigos, intervalo);
    }

    private void IniciarJefe()
    {
        if (ufo == null) return;
        NaveNodriza nave = ufo.GetComponent<NaveNodriza>();
        if (nave == null) return;

        jefeActivo = true;
        nave.AlMorir = AlMorirJefe;
        nave.Activar();
    }

    private void AlMorirJefe()
    {
        ufo = null;
        jefeActivo = false;
        if (!nivelCompletado)
        {
            nivelCompletado = true;
            StartCoroutine(AvanzarNivel());
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
        if (GestorNiveles.NivelActual >= GestorNiveles.NivelMaximo)
        {
            // Victoria: pedir las iniciales antes de ir a los créditos.
            CreditosUI.MostrarAlIniciar = true;
            GameOverUI.MostrarVictoria();
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
        float limite = CalcularLimite(halfWidth);
        limiteIzquierdo = centroX - limite;
        limiteDerecho = centroX + limite;
    }

    // Medio ancho máximo de marcha: usa el de la pantalla si es menor,
    // o el tope fijo en pantallas muy anchas.
    private float CalcularLimite(float halfWidth)
    {
        float limite = Mathf.Max(1f, halfWidth - MARGEN_LATERAL);
        return Mathf.Min(limite, anchoMaximoMarcha * 0.5f);
    }

    private float CalcularIntervalo(int vivos)
    {
        float factor = (float)vivos / totalInicial;
        return Mathf.Lerp(intervaloMinimo, intervaloMaximo, factor) * Dificultad.MultiplicadorIntervalo;
    }
}
