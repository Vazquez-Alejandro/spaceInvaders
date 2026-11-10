using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static int Score { get; private set; }
    public static int Monedas { get; private set; }
    private static ScoreManager instancia;

    private RectTransform contenedor;
    private Image etiqueta;
    private readonly List<Image> digitos = new List<Image>();
    private Sprite[] spritesDigitos;
    private Sprite spriteEtiqueta;
    private Text textoMonedas;
    private Font fuenteMonedas;

    private const float ALTO = 20f;
    private const float ESPACIO = 2f;
    private const float PADDING = 6f;
    private const int DIGITOS_MINIMOS = 4;
    private static readonly Color COLOR = new Color(0.196f, 1f, 0.196f, 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
        AlCargarEscena(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (escena.name == "GameScene")
        {
            // Reiniciar solo al empezar una partida nueva. Al pasar de nivel
            // (NivelActual ya avanzó) el puntaje y las monedas se acumulan.
            if (GestorNiveles.NivelActual == GestorNiveles.NivelInicial)
            {
                Score = 0;
                ActualizarMonedas(0);
            }

            if (instancia == null)
            {
                GameObject go = new GameObject("ScoreManager");
                instancia = go.AddComponent<ScoreManager>();
                DontDestroyOnLoad(go);
            }
            instancia.CrearUI();
            instancia.Actualizar();
            ActualizarMonedas(Monedas); // refresca el texto con el valor acumulado
        }
        else
        {
            if (instancia != null && instancia.contenedor != null)
                instancia.contenedor.gameObject.SetActive(false);
        }
    }

    private void CrearUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        if (spritesDigitos == null)
        {
            spritesDigitos = new Sprite[10];
            for (int i = 0; i < 10; i++)
                spritesDigitos[i] = Resources.Load<Sprite>("ScoreUI/D" + i);
            spriteEtiqueta = Resources.Load<Sprite>("ScoreUI/SCORE");
        }

        if (fuenteMonedas == null)
            fuenteMonedas = Resources.Load<Font>("Fonts/MGS1 HUD");

        GameObject panel = GameObject.Find("PanelScore");
        if (panel != null)
        {
            Transform estatico = panel.transform.Find("TextoScore");
            if (estatico != null) estatico.gameObject.SetActive(false);
        }

        digitos.Clear();
        etiqueta = null;
        contenedor = null;

        GameObject go = new GameObject("ScoreJuego", typeof(RectTransform));
        go.transform.SetParent(panel != null ? panel.transform : canvas.transform, false);
        contenedor = go.GetComponent<RectTransform>();

        if (panel != null)
        {
            contenedor.anchorMin = Vector2.zero;
            contenedor.anchorMax = Vector2.one;
            contenedor.offsetMin = Vector2.zero;
            contenedor.offsetMax = Vector2.zero;
        }
        else
        {
            contenedor.anchorMin = new Vector2(1, 1);
            contenedor.anchorMax = new Vector2(1, 1);
            contenedor.pivot = new Vector2(1, 1);
            contenedor.anchoredPosition = new Vector2(-20, -92);
            contenedor.sizeDelta = new Vector2(260, 50);
        }

        CrearEtiqueta();
        CrearDigitos(DIGITOS_MINIMOS);
        CrearContadorMonedas(canvas, panel);
    }

    private void CrearContadorMonedas(Canvas canvas, GameObject panelScore)
    {
        textoMonedas = null;

        Transform padre = panelScore != null && panelScore.transform.parent != null
            ? panelScore.transform.parent
            : canvas.transform;

        GameObject panel = new GameObject("PanelMonedas", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(padre, false);

        RectTransform prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(1, 1);
        prt.anchorMax = new Vector2(1, 1);
        prt.pivot = new Vector2(1, 1);
        prt.sizeDelta = new Vector2(260, 50);

        float y = -150f;
        if (panelScore != null)
        {
            RectTransform srt = panelScore.GetComponent<RectTransform>();
            if (srt != null) y = srt.anchoredPosition.y - srt.sizeDelta.y - 8f;
        }
        prt.anchoredPosition = new Vector2(-20, y);

        Image fondo = panel.GetComponent<Image>();
        fondo.color = new Color(0f, 0f, 0f, 0.8f);
        fondo.raycastTarget = false;

        GameObject go = new GameObject("TextoMonedas", typeof(RectTransform));
        go.transform.SetParent(panel.transform, false);

        RectTransform trt = go.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(PADDING, 0);
        trt.offsetMax = new Vector2(-PADDING, 0);

        textoMonedas = go.AddComponent<Text>();
        textoMonedas.font = fuenteMonedas != null
            ? fuenteMonedas
            : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoMonedas.fontSize = 44;
        textoMonedas.color = COLOR;
        textoMonedas.alignment = TextAnchor.MiddleLeft;
        textoMonedas.horizontalOverflow = HorizontalWrapMode.Overflow;
        textoMonedas.verticalOverflow = VerticalWrapMode.Overflow;
        textoMonedas.raycastTarget = false;
        textoMonedas.text = "COIN: 0";
    }

    private void CrearEtiqueta()
    {
        GameObject go = new GameObject("EtiquetaScore", typeof(RectTransform));
        go.transform.SetParent(contenedor, false);
        etiqueta = go.AddComponent<Image>();
        etiqueta.sprite = spriteEtiqueta;
        etiqueta.color = COLOR;
        etiqueta.raycastTarget = false;

        RectTransform rt = etiqueta.rectTransform;
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.sizeDelta = new Vector2(Ancho(etiqueta), ALTO);
    }

    private void CrearDigitos(int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            GameObject go = new GameObject("Digito" + (digitos.Count), typeof(RectTransform));
            go.transform.SetParent(contenedor, false);
            Image img = go.AddComponent<Image>();
            img.color = COLOR;
            img.raycastTarget = false;

            RectTransform rt = img.rectTransform;
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            digitos.Add(img);
        }
    }

    private float Ancho(Image img)
    {
        if (img == null || img.sprite == null) return ALTO;
        float relacion = img.sprite.rect.width / img.sprite.rect.height;
        return ALTO * relacion;
    }

    public static void Sumar(int cantidad)
    {
        Score += cantidad;
        if (instancia != null) instancia.Actualizar();
    }

    public static void ActualizarMonedas(int cantidad)
    {
        Monedas = Mathf.Max(0, cantidad);
        if (instancia != null && instancia.textoMonedas != null)
            instancia.textoMonedas.text = "COIN: " + Monedas;
    }

    private void Actualizar()
    {
        if (contenedor == null || etiqueta == null) return;

        string texto = Mathf.Max(0, Score).ToString();
        while (texto.Length < DIGITOS_MINIMOS) texto = "0" + texto;
        if (texto.Length > digitos.Count) CrearDigitos(texto.Length - digitos.Count);

        float x = PADDING;

        float anchoEtiqueta = Ancho(etiqueta);
        etiqueta.rectTransform.sizeDelta = new Vector2(anchoEtiqueta, ALTO);
        etiqueta.rectTransform.anchoredPosition = new Vector2(x, 0);
        x += anchoEtiqueta + ESPACIO;

        for (int i = 0; i < digitos.Count; i++)
        {
            Image digito = digitos[i];
            if (i < texto.Length)
            {
                digito.gameObject.SetActive(true);
                digito.sprite = spritesDigitos[texto[i] - '0'];
                float ancho = Ancho(digito);
                digito.rectTransform.sizeDelta = new Vector2(ancho, ALTO);
                digito.rectTransform.anchoredPosition = new Vector2(x, 0);
                x += ancho + ESPACIO;
            }
            else
            {
                digito.gameObject.SetActive(false);
            }
        }
    }
}
