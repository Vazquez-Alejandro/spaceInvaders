using UnityEngine;
using UnityEngine.SceneManagement;

public class BunkerSpawner : MonoBehaviour
{
    private static BunkerSpawner instancia;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;

        if (instancia == null)
        {
            GameObject go = new GameObject("BunkerSpawner");
            instancia = go.AddComponent<BunkerSpawner>();
            DontDestroyOnLoad(go);
        }

        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            instancia.CrearBunkers();
        }
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (escena.name != "GameScene") return;
        if (instancia == null)
        {
            GameObject go = new GameObject("BunkerSpawner");
            instancia = go.AddComponent<BunkerSpawner>();
            DontDestroyOnLoad(go);
        }
        instancia.CrearBunkers();
    }

    private void CrearBunkers()
    {
        foreach (Bunker b in Object.FindObjectsByType<Bunker>(FindObjectsSortMode.None))
        {
            if (b != null) Destroy(b.gameObject);
        }

        int cantidad = (GestorNiveles.NivelActual >= 2) ? 1 : 2;
        float y = -7f;
        float[] xs = (cantidad == 1) ? new float[] { 0f } : new float[] { -4f, 4f };

        foreach (float x in xs)
        {
            GameObject go = new GameObject("Bunker");
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = new Vector3(5f, 5f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 1;
            sr.color = Color.white;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            col.size = new Vector2(0.26f, 0.12f);

            go.AddComponent<Bunker>();
        }
    }
}
