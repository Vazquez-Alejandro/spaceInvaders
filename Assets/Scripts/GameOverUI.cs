using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    private static GameOverUI instancia;
    private GameObject panel;
    private bool activo;
    private bool guardado;
    private Text textoPuntaje;
    private Text textoMonedas;
    private Text textoMensaje;
    private InputField inputNombre;
    private Button botonGuardar;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (escena.name == "GameScene")
        {
            if (instancia == null)
            {
                GameObject go = new GameObject("GameOverUI");
                instancia = go.AddComponent<GameOverUI>();
                DontDestroyOnLoad(go);
            }
            instancia.CrearUI();
            instancia.Ocultar();
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

        panel = new GameObject("PanelGameOver");
        panel.transform.SetParent(canvas.transform, false);
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.85f);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        CrearTexto(panel.transform, "GAME OVER", 52, new Vector2(0, 230), Color.red);
        textoPuntaje = CrearTexto(panel.transform, "PUNTAJE: 0", 30, new Vector2(0, 160), new Color(0.196f, 1f, 0.196f, 1f));
        textoMonedas = CrearTexto(panel.transform, "MONEDAS: 0", 26, new Vector2(0, 115), new Color(0.196f, 1f, 0.196f, 1f));
        CrearTexto(panel.transform, "INGRESA TU NOMBRE:", 22, new Vector2(0, 60), new Color(0.196f, 1f, 0.196f, 1f));

        inputNombre = CrearInput(panel.transform, new Vector2(0, 15));
        botonGuardar = CrearBoton(panel.transform, "Guardar en Ranking", new Vector2(0, -45), GuardarEnRanking).GetComponent<Button>();
        textoMensaje = CrearTexto(panel.transform, "", 22, new Vector2(0, -95), new Color(0.196f, 1f, 0.196f, 1f));

        CrearBoton(panel.transform, "Reiniciar", new Vector2(0, -160), Reiniciar);
        CrearBoton(panel.transform, "Salir", new Vector2(0, -220), Salir);

        panel.SetActive(false);
    }

    private Text CrearTexto(Transform padre, string texto, int tamaño, Vector2 pos, Color color)
    {
        GameObject go = new GameObject("Texto");
        go.transform.SetParent(padre, false);
        Text t = go.AddComponent<Text>();
        t.text = texto;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = tamaño;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(700, 60);
        return t;
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
        rt.sizeDelta = new Vector2(300, 50);

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

    private InputField CrearInput(Transform padre, Vector2 pos)
    {
        GameObject go = new GameObject("NombreInput", typeof(RectTransform), typeof(Image), typeof(InputField));
        go.transform.SetParent(padre, false);

        Image img = go.GetComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(420, 50);

        InputField input = go.GetComponent<InputField>();
        input.targetGraphic = img;
        input.lineType = InputField.LineType.SingleLine;
        input.characterLimit = 12;

        Text texto = CrearTextoHijo(go.transform, new Color(0.196f, 1f, 0.196f, 1f), "");
        input.textComponent = texto;

        Text placeholder = CrearTextoHijo(go.transform, new Color(1f, 1f, 1f, 0.35f), "TU NOMBRE");
        input.placeholder = placeholder;

        input.text = "";
        input.ForceLabelUpdate();

        return input;
    }

    private Text CrearTextoHijo(Transform padre, Color color, string contenido)
    {
        GameObject go = new GameObject("Texto", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(padre, false);

        Text t = go.GetComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 24;
        t.color = color;
        t.alignment = TextAnchor.MiddleLeft;
        t.supportRichText = false;
        t.text = contenido;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(12, 0);
        rt.offsetMax = new Vector2(-12, 0);
        return t;
    }

    public static void Mostrar()
    {
        if (instancia == null) return;
        if (instancia.panel == null) instancia.CrearUI();
        instancia.activo = true;
        Time.timeScale = 0f;
        instancia.ActualizarDatos();
        if (instancia.panel != null)
        {
            instancia.panel.transform.SetAsLastSibling();
            instancia.panel.SetActive(true);
        }
    }

    private void ActualizarDatos()
    {
        guardado = false;

        if (textoPuntaje != null) textoPuntaje.text = "PUNTAJE: " + Mathf.Max(0, ScoreManager.Score);
        if (textoMonedas != null) textoMonedas.text = "MONEDAS: " + Mathf.Max(0, ScoreManager.Monedas);
        if (textoMensaje != null) textoMensaje.text = "";

        if (inputNombre != null)
        {
            inputNombre.interactable = true;
            inputNombre.text = "";
            inputNombre.Select();
            inputNombre.ActivateInputField();
        }
        if (botonGuardar != null) botonGuardar.interactable = true;
    }

    private void GuardarEnRanking()
    {
        if (guardado) return;
        guardado = true;

        string nombre = inputNombre != null ? inputNombre.text : "";
        Ranking.Agregar(nombre, ScoreManager.Score, ScoreManager.Monedas, GestorNiveles.NivelActual);

        if (inputNombre != null) inputNombre.interactable = false;
        if (botonGuardar != null) botonGuardar.interactable = false;
        if (textoMensaje != null) textoMensaje.text = "PUNTAJE GUARDADO EN EL RANKING";
    }

    private void Ocultar()
    {
        activo = false;
        Time.timeScale = 1f;
        if (panel != null) panel.SetActive(false);
    }

    private void Reiniciar()
    {
        Time.timeScale = 1f;
        activo = false;
        GestorNiveles.Reiniciar();
    }

    private void Salir()
    {
        Time.timeScale = 1f;
        GestorNiveles.VolverMenu();
    }
}
