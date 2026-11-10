using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RankingUI : MonoBehaviour
{
    private static RankingUI instancia;
    private GameObject panel;
    private Text textoEntradas;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
        AlCargarEscena(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (escena.name == "MainMenu")
        {
            if (instancia == null)
            {
                GameObject go = new GameObject("RankingUI");
                instancia = go.AddComponent<RankingUI>();
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

    public static void MostrarRanking()
    {
        if (instancia == null)
        {
            GameObject go = new GameObject("RankingUI");
            instancia = go.AddComponent<RankingUI>();
            DontDestroyOnLoad(go);
            instancia.CrearUI();
        }
        instancia.Mostrar();
    }

    private void CrearUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        if (panel != null) Destroy(panel);

        panel = new GameObject("PanelRanking");
        panel.transform.SetParent(canvas.transform, false);
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.9f);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        CrearTexto(panel.transform, "RANKING", 44, new Vector2(0, 250), new Color(0.196f, 1f, 0.196f, 1f));

        GameObject go = new GameObject("TextoEntradas", typeof(RectTransform));
        go.transform.SetParent(panel.transform, false);
        textoEntradas = go.AddComponent<Text>();
        textoEntradas.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoEntradas.fontSize = 24;
        textoEntradas.alignment = TextAnchor.UpperCenter;
        textoEntradas.color = new Color(0.196f, 1f, 0.196f, 1f);
        textoEntradas.horizontalOverflow = HorizontalWrapMode.Overflow;
        textoEntradas.verticalOverflow = VerticalWrapMode.Overflow;
        textoEntradas.supportRichText = false;
        RectTransform trt = go.GetComponent<RectTransform>();
        trt.anchoredPosition = new Vector2(0, 0);
        trt.sizeDelta = new Vector2(900, 400);

        CrearBoton(panel.transform, "Volver", new Vector2(0, -270), Ocultar);
        panel.SetActive(false);
    }

    private GameObject CrearTexto(Transform padre, string texto, int tamaño, Vector2 pos, Color color)
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
        rt.sizeDelta = new Vector2(900, 60);
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

    private void Mostrar()
    {
        if (panel == null) return;

        if (textoEntradas != null)
        {
            List<EntradaRanking> entradas = Ranking.Obtener();
            if (entradas.Count == 0)
            {
                textoEntradas.text = "Todavia no hay puntajes.\nJuga una partida y guarda tu puntaje.";
            }
            else
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < entradas.Count; i++)
                {
                    EntradaRanking e = entradas[i];
                    string nombre = string.IsNullOrEmpty(e.nombre) ? "JUGADOR" : e.nombre;
                    if (nombre.Length > 12) nombre = nombre.Substring(0, 12);
                    sb.AppendLine($"{i + 1,2}. {nombre,-12}  {e.puntaje,6} PTS   {e.monedas,3} COIN   NIVEL {e.nivel}");
                }
                textoEntradas.text = sb.ToString();
            }
        }

        if (panel != null)
        {
            panel.transform.SetAsLastSibling();
            panel.SetActive(true);
        }
    }

    private void Ocultar()
    {
        if (panel != null) panel.SetActive(false);
    }
}
