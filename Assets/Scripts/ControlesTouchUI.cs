using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Botones táctiles (◀ ▶ DISPARO) + opciones del menú: BOTONES y MODO (fácil/difícil).
// Las opciones se ven igual en el menú y en el juego.
public class ControlesTouchUI : MonoBehaviour
{
    private const string CLAVE_BOTONES = "ControlesTouch";

    private static readonly Color COLOR_ON = new Color(0.196f, 1f, 0.196f, 1f);
    private static readonly Color COLOR_OFF = new Color(0.6f, 0.66f, 0.6f, 1f);
    private static readonly Color COLOR_FACIL = new Color(0.196f, 1f, 0.196f, 1f);
    private static readonly Color COLOR_DIFICIL = new Color(1f, 0.62f, 0.2f, 1f);
    private static readonly Color COLOR_TEXTO = new Color(0.196f, 1f, 0.196f, 0.95f);
    private static readonly Color COLOR_BORDE = new Color(0.196f, 1f, 0.196f, 0.8f);

    private static readonly Vector2 TAM_OPCION = new Vector2(340f, 60f);

    private static bool activos;
    private static Text textoBotones;
    private static Text textoModo;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
        AlCargarEscena(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        Destruir("ControlesTouch");
        Destruir("OpcionBotones");
        Destruir("OpcionModo");

        if (escena.name != "GameScene" && escena.name != "MainMenu") return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        activos = PlayerPrefs.GetInt(CLAVE_BOTONES, SystemInfo.deviceType == DeviceType.Handheld ? 1 : 0) == 1;

        // Mismas dos opciones en menú y juego; en el juego además aparecen los botones.
        float y1 = escena.name == "MainMenu" ? -20f : -150f;
        float y2 = escena.name == "MainMenu" ? -90f : -220f;
        textoBotones = CrearOpcion(canvas, "OpcionBotones", y1, AlternarBotones);
        textoModo = CrearOpcion(canvas, "OpcionModo", y2, AlternarModo);

        if (escena.name == "GameScene" && activos) CrearBotones(canvas);

        ActualizarTextos();
    }

    private static void Destruir(string nombre)
    {
        GameObject g = GameObject.Find(nombre);
        if (g != null) Object.Destroy(g);
    }

    private static void AlternarBotones()
    {
        activos = !activos;
        PlayerPrefs.SetInt(CLAVE_BOTONES, activos ? 1 : 0);
        PlayerPrefs.Save();

        Destruir("ControlesTouch");
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (activos && canvas != null && SceneManager.GetActiveScene().name == "GameScene")
            CrearBotones(canvas);

        ActualizarTextos();
    }

    private static void AlternarModo()
    {
        Dificultad.Facil = !Dificultad.Facil;
        ActualizarTextos();
    }

    private static void ActualizarTextos()
    {
        if (textoBotones != null)
        {
            textoBotones.text = "BOTONES: " + (activos ? "ON" : "OFF");
            textoBotones.color = activos ? COLOR_ON : COLOR_OFF;
        }
        if (textoModo != null)
        {
            textoModo.text = "MODO: " + (Dificultad.Facil ? "FACIL" : "DIFICIL");
            textoModo.color = Dificultad.Facil ? COLOR_FACIL : COLOR_DIFICIL;
        }
    }

    // Opción tipo botón: mismo rectángulo del sheet para todas, arriba a la izquierda.
    private static Text CrearOpcion(Canvas canvas, string nombre, float y, UnityAction accion)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);

        Image fondo = go.AddComponent<Image>();
        fondo.sprite = Resources.Load<Sprite>("BotonTouch");
        fondo.type = Image.Type.Simple;
        fondo.color = Color.white;
        fondo.raycastTarget = true;

        Outline borde = go.AddComponent<Outline>();
        borde.effectColor = COLOR_BORDE;
        borde.effectDistance = new Vector2(2f, -2f);

        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(accion);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(20f, y);
        rt.sizeDelta = TAM_OPCION;

        GameObject txt = new GameObject("Texto", typeof(RectTransform));
        txt.transform.SetParent(go.transform, false);
        Text t = txt.AddComponent<Text>();
        t.font = Resources.Load<Font>("Fonts/MGS1 HUD");
        if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.alignment = TextAnchor.MiddleCenter;
        t.color = COLOR_TEXTO;
        t.raycastTarget = false;
        // Letras lo más grandes posible dentro de la caja (sin salirse)
        t.resizeTextForBestFit = true;
        t.resizeTextMinSize = 16;
        t.resizeTextMaxSize = 44;

        RectTransform trt = txt.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(12f, 6f);
        trt.offsetMax = new Vector2(-12f, -6f);

        return t;
    }

    private static void CrearBotones(Canvas canvas)
    {
        GameObject raiz = new GameObject("ControlesTouch", typeof(RectTransform));
        raiz.transform.SetParent(canvas.transform, false);
        RectTransform rt = raiz.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Flechas abajo-izquierda, disparo abajo-derecha
        CrearBoton(raiz.transform, "<", new Vector2(0f, 0f), new Vector2(55f, 55f),
            new Vector2(105f, 105f), 54f, BotonTouch.Accion.Izquierda);
        CrearBoton(raiz.transform, ">", new Vector2(0f, 0f), new Vector2(175f, 55f),
            new Vector2(105f, 105f), 54f, BotonTouch.Accion.Derecha);
        CrearBoton(raiz.transform, "DISPARO", new Vector2(1f, 0f), new Vector2(-70f, 55f),
            new Vector2(160f, 105f), 26f, BotonTouch.Accion.Disparo);
    }

    private static void CrearBoton(Transform padre, string texto, Vector2 anclaje,
        Vector2 posicion, Vector2 tam, float fuente, BotonTouch.Accion accion)
    {
        GameObject go = new GameObject("Boton_" + accion, typeof(RectTransform));
        go.transform.SetParent(padre, false);

        Image img = go.AddComponent<Image>();
        img.sprite = Resources.Load<Sprite>("BotonTouch");
        img.type = Image.Type.Simple;
        img.color = Color.white;
        img.raycastTarget = true;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anclaje;
        rt.anchorMax = anclaje;
        rt.pivot = anclaje;
        rt.sizeDelta = tam;
        rt.anchoredPosition = posicion;

        BotonTouch toque = go.AddComponent<BotonTouch>();
        toque.accion = accion;

        GameObject txt = new GameObject("Texto", typeof(RectTransform));
        txt.transform.SetParent(go.transform, false);
        Text t = txt.AddComponent<Text>();
        t.text = texto;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = Mathf.RoundToInt(fuente);
        t.alignment = TextAnchor.MiddleCenter;
        t.color = COLOR_TEXTO;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;

        RectTransform trt = txt.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
    }
}
