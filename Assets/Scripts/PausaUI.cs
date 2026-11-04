using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PausaUI : MonoBehaviour
{
    private static PausaUI instancia;
    private GameObject panel;
    private GameObject botonPausa;
    private bool pausado;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
        Adjuntar();
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (escena.name == "GameScene") Adjuntar();
        else Destruir();
    }

    private static void Adjuntar()
    {
        if (instancia != null) return;
        if (SceneManager.GetActiveScene().name != "GameScene") return;

        GameObject go = new GameObject("PausaUI");
        instancia = go.AddComponent<PausaUI>();
        DontDestroyOnLoad(go);
        instancia.CrearUI();
    }

    private static void Destruir()
    {
        if (instancia != null)
        {
            if (instancia.panel != null) Destroy(instancia.panel);
            Destroy(instancia.gameObject);
            instancia = null;
        }
        Time.timeScale = 1f;
    }

    private void CrearUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject cgo = new GameObject("CanvasPausa");
            canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cgo.AddComponent<CanvasScaler>();
            cgo.AddComponent<GraphicRaycaster>();
        }

        panel = new GameObject("PanelPausa");
        panel.transform.SetParent(canvas.transform, false);
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.75f);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        GameObject titulo = CrearTexto(panel.transform, "PAUSA", 48, new Vector2(0, 120));
        GameObject btnReanudar = CrearBoton(panel.transform, "Reanudar", new Vector2(0, 20), Reanudar);
        GameObject btnMenu = CrearBoton(panel.transform, "Menu Principal", new Vector2(0, -60), IrMenu);
        GameObject btnSalir = CrearBoton(panel.transform, "Salir", new Vector2(0, -140), Salir);

        CrearBotonPausa(canvas.transform);

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
        rt.sizeDelta = new Vector2(400, 60);
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
        rt.sizeDelta = new Vector2(260, 50);

        GameObject txt = new GameObject("Texto");
        txt.transform.SetParent(go.transform, false);
        Text t = txt.AddComponent<Text>();
        t.text = texto;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 24;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = new Color(0.196f, 1f, 0.196f, 1f);
        RectTransform trt = txt.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        return go;
    }

    private void CrearBotonPausa(Transform canvasTransform)
    {
        botonPausa = new GameObject("BotonPausa");
        botonPausa.transform.SetParent(canvasTransform, false);
        Image img = botonPausa.AddComponent<Image>();
        img.color = new Color(0f, 0.12f, 0f, 0.8f);
        Button btn = botonPausa.AddComponent<Button>();
        btn.onClick.AddListener(Pausar);
        RectTransform rt = botonPausa.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(270, -20);
        rt.sizeDelta = new Vector2(60, 40);

        GameObject txt = new GameObject("Texto");
        txt.transform.SetParent(botonPausa.transform, false);
        Text t = txt.AddComponent<Text>();
        t.text = "||";
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

    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (pausado) Reanudar();
            else Pausar();
        }
    }

    private void Pausar()
    {
        pausado = true;
        Time.timeScale = 0f;
        if (panel != null) panel.SetActive(true);
        if (botonPausa != null) botonPausa.SetActive(false);
    }

    private void Reanudar()
    {
        pausado = false;
        Time.timeScale = 1f;
        if (panel != null) panel.SetActive(false);
        if (botonPausa != null) botonPausa.SetActive(true);
    }

    private void IrMenu()
    {
        Time.timeScale = 1f;
        pausado = false;
        GestorNiveles.VolverMenu();
    }

    private void Salir()
    {
        Application.Quit();
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
