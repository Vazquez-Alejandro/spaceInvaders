using UnityEngine;

// Dificultad elegida en el menú (se guarda por dispositivo).
// Difícil = velocidad actual. Fácil = los aliens se mueven más lento.
public static class Dificultad
{
    private const string CLAVE = "Dificultad";

    // Multiplica el tiempo entre pasos de los aliens (más grande = más lento).
    public const float MULTIPLICADOR_FACIL = 1.8f;

    public static bool Facil
    {
        get => PlayerPrefs.GetInt(CLAVE, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(CLAVE, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static float MultiplicadorIntervalo => Facil ? MULTIPLICADOR_FACIL : 1f;
}
