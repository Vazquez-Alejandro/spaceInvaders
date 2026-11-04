using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(1100)]
public class BarraVida : MonoBehaviour
{
    private static BarraVida instancia;

    private Image imagen;
    private PlayerController jugador;
    private Sprite[] estados;
    private int vidaAnterior = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
        Adjuntar();
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        Adjuntar();
    }

    private static void Adjuntar()
    {
        if (instancia != null) return;

        GameObject barra = GameObject.Find("HealthBar");
        if (barra == null) return;

        instancia = barra.GetComponent<BarraVida>();
        if (instancia == null)
        {
            instancia = barra.AddComponent<BarraVida>();
        }
    }

    private void OnDisable()
    {
        if (instancia == this) instancia = null;
    }

    private void Start()
    {
        imagen = GetComponent<Image>();
        estados = Resources.LoadAll<Sprite>("SpaceInvaders_Health");
        Array.Sort(estados, (a, b) => string.Compare(a.name, b.name));
        jugador = FindFirstObjectByType<PlayerController>();
        Actualizar(true);
    }

    private void Update()
    {
        Actualizar(false);
    }

    public static void RefrescarBarra()
    {
        if (instancia != null) instancia.Actualizar(true);
    }

    private void Actualizar(bool forzar)
    {
        if (imagen == null || estados == null || estados.Length == 0) return;

        if (jugador == null)
        {
            jugador = FindFirstObjectByType<PlayerController>();
            if (jugador == null) return;
        }

        int maximo = Mathf.Max(1, jugador.VidaMaxima);
        int vida = Mathf.Clamp(jugador.Vida, 0, maximo);

        if (!forzar && vida == vidaAnterior) return;
        vidaAnterior = vida;

        int indice = Mathf.Clamp(maximo - vida, 0, estados.Length - 1);
        imagen.sprite = estados[indice];
    }
}
