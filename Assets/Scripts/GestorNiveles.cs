using UnityEngine;
using UnityEngine.SceneManagement;

public static class GestorNiveles
{
    // Pruebas: true = arranca directo en el nivel 2 (modo nave nodriza). false = juego normal.
    public static bool EmpezarEnNivel2 = false;
    public static int NivelInicial => EmpezarEnNivel2 ? 2 : 1;
    public static int NivelActual = NivelInicial;

    // Último nivel visible. El nivel 3 queda oculto por ahora.
    public const int NivelMaximo = 2;

    public static void AvanzarNivel()
    {
        if (NivelActual >= NivelMaximo) return;
        NivelActual++;
        SceneManager.LoadScene("GameScene");
    }

    public static void Reiniciar()
    {
        NivelActual = NivelInicial;
        SceneManager.LoadScene("GameScene");
    }

    public static void VolverMenu()
    {
        NivelActual = NivelInicial;
        SceneManager.LoadScene("MainMenu");
    }
}
