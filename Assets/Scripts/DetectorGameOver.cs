using UnityEngine;
using UnityEngine.SceneManagement;

public class DetectorGameOver : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (escena.name != "GameScene") return;
        GameObject go = new GameObject("DetectorGameOver");
        go.AddComponent<DetectorGameOver>();
    }

    private void Update()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            GameOverUI.Mostrar();
            enabled = false;
            return;
        }

        // Aliens reached player line (y <= player y + margin)
        float lineaJugador = player.transform.position.y;
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (t.name.StartsWith("Invasor") && t.GetComponent<SpriteRenderer>() != null)
            {
                if (t.position.y <= lineaJugador + 0.5f)
                {
                    // destroy player
                    Destroy(player);
                    GameOverUI.Mostrar();
                    enabled = false;
                    return;
                }
            }
        }
    }
}
