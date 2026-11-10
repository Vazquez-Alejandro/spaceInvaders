using UnityEngine;
using UnityEngine.SceneManagement;

// Escala y ubica el fondo para que SIEMPRE cubra toda la cámara,
// sin importar el aspecto de la pantalla (editor, WebGL 960x600,
// pantalla completa...). Sin esto, en aspectos anchos se ven las
// bandas del color de limpieza de la cámara.
public class BackgroundCubrir : MonoBehaviour
{
    private SpriteRenderer sr;
    private Camera cam;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
        AlCargarEscena(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (escena.name != "GameScene") return;

        foreach (SpriteRenderer r in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            if (r.sortingOrder < 0 && r.GetComponent<BackgroundCubrir>() == null)
            {
                r.gameObject.AddComponent<BackgroundCubrir>();
            }
        }
    }

    private void LateUpdate()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (cam == null) cam = Camera.main;
        if (sr == null || sr.sprite == null || cam == null) return;

        Vector3 tam = sr.sprite.bounds.size;
        if (tam.x <= 0.0001f || tam.y <= 0.0001f) return;

        float altoVisible = cam.orthographicSize * 2f;
        float anchoVisible = altoVisible * cam.aspect;

        // Escala "cover": la mínima que cubre ancho y alto.
        float escala = Mathf.Max(anchoVisible / tam.x, altoVisible / tam.y);
        transform.localScale = new Vector3(escala, escala, 1f);

        // Anclar la base de la imagen al pie de la pantalla para que
        // la ciudad quede abajo, como en el diseño original.
        float mitadAlto = tam.y * escala * 0.5f;
        float yBase = cam.transform.position.y - cam.orthographicSize;
        transform.position = new Vector3(
            cam.transform.position.x,
            yBase + mitadAlto,
            transform.position.z);
    }
}
