using UnityEngine;
using UnityEngine.SceneManagement;

public static class GestorNiveles
{
    public static int NivelActual = 1;

    public static void AvanzarNivel()
    {
        NivelActual++;
        SceneManager.LoadScene("GameScene");
    }

    public static void Reiniciar()
    {
        NivelActual = 1;
        SceneManager.LoadScene("GameScene");
    }

    public static void VolverMenu()
    {
        NivelActual = 1;
        SceneManager.LoadScene("MainMenu");
    }
}
