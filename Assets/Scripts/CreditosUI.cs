using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreditosUI : MonoBehaviour
{
    public static bool MostrarAlIniciar = false;

    private static CreditosUI instancia;
    private GameObject panel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (escena.name == "MainMenu")
        {
            if (instancia == null)
            {
                GameObject go = new GameObject("CreditosUI");
                instancia = go.AddComponent<CreditosUI>();
                DontDestroyOnLoad(go);
            }
            instancia.CrearUI();
            if (MostrarAlIniciar)
            {
                MostrarAlIniciar = false;
                instancia.Mostrar();
            }
        }
        else
        {
            if (instancia != null && instancia.panel != null) instancia.panel.SetActive(false);
        }
    }

    private void CrearUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        if (panel != null) Destroy(panel);

        panel = new GameObject("PanelCreditos");
        panel.transform.SetParent(canvas.transform, false);
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.9f);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        GameObject titulo = CrearTexto(panel.transform, "CREDITOS", 40, new Vector2(0, 260));

        string[] lineas = new string[]
        {
            "Desarrollo: Alejandro",
            "",
            "Sprites: SpaceInvaders (assets del proyecto)",
            "Audio: SFX y musica del proyecto",
            "Fuentes: MGS1 (HUD/Codec/Ammo)",
            "",
            "Gracias por jugar!"
        };
        float y = 180f;
        foreach (string l in lineas)
        {
            CrearTexto(panel.transform, l, 22, new Vector2(0, y));
            y -= 36f;
        }

        CrearBoton(panel.transform, "Volver", new Vector2(0, -260), Ocultar);
        CrearBotonCreditos(canvas);
        panel.SetActive(false);
    }

    private GameObject CrearTexto(Transform padre, string texto, int tamaño, Vector2 pos)
    {
        GameObject go = new GameObject("Texto");
        go.transform.SetParent(padre, false);
        Text t = go.AddComponent<Text>();
        t.text = texto;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = tamaño;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = new Color(0.196f, 1f, 0.196f, 1f);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(900, 40);
        return go;
    }

    private GameObject CrearBoton(Transform padre, string texto, Vector2 pos, UnityEngine.Events.UnityAction accion)
    {
        GameObject go = new GameObject("Boton");
        go.transform.SetParent(padre, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0f, 0.12f, 0f, 0.9f);
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(accion);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(200, 45);

        GameObject txt = new GameObject("Texto");
        txt.transform.SetParent(go.transform, false);
        Text t = txt.AddComponent<Text>();
        t.text = texto;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 22;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = new Color(0.196f, 1f, 0.196f, 1f);
        RectTransform trt = txt.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        return go;
    }

    private void CrearBotonCreditos(Canvas canvas)
    {
        GameObject go = new GameObject("BotonCreditos");
        go.transform.SetParent(canvas.transform, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0f, 0.12f, 0f, 0.8f);
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(Mostrar);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 200f);
        rt.sizeDelta = new Vector2(260f, 50f);

        GameObject txt = new GameObject("Texto");
        txt.transform.SetParent(go.transform, false);
        Text t = txt.AddComponent<Text>();
        t.text = "Creditos";
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 24;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = new Color(0.196f, 1f, 0.196f, 1f);
        RectTransform trt = txt.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
    }

    public void Mostrar()
    {
        if (panel != null) panel.SetActive(true);
    }

    private void Ocultar()
    {
        if (panel != null) panel.SetActive(false);
    }
}
