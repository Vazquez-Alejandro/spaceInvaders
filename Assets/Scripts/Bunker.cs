using UnityEngine;

// Bunker con frames de daño del sprite sheet (4 estados y destrucción total).
public class Bunker : MonoBehaviour
{
    private static readonly string[] RUTAS =
    {
        "Bunkers/Bunker0",
        "Bunkers/Bunker1",
        "Bunkers/Bunker2",
        "Bunkers/Bunker3"
    };

    // Altura visible de cada frame (sobre 12 px del recorte) para ajustar el colisionador.
    private static readonly float[] ALTURAS_LOCALES = { 0.12f, 0.12f, 0.08f, 0.04f };

    private SpriteRenderer sr;
    private Sprite[] frames;
    private int dano;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        frames = new Sprite[RUTAS.Length];
        for (int i = 0; i < RUTAS.Length; i++)
        {
            Sprite src = Resources.Load<Sprite>(RUTAS[i]);
            if (src == null || src.texture == null)
            {
                Debug.LogWarning("Bunker: sprite no encontrado " + RUTAS[i]);
                continue;
            }
            frames[i] = CrearSprite(src);
        }

        dano = 0;
        AplicarFrame();
    }

    // Replica el enfoque que ya funcionaba: textura RGBA32 en tiempo de ejecución
    // con el color sólido del bunker, en vez del sprite importado.
    private static Sprite CrearSprite(Sprite src)
    {
        Color[] px = src.texture.GetPixels();
        Color verde = new Color(0.3f, 0.8f, 0.2f, 1f);
        Color[] nuevos = new Color[px.Length];
        for (int i = 0; i < px.Length; i++)
            nuevos[i] = (px[i].a > 0.1f) ? verde : Color.clear;

        Texture2D tex = new Texture2D(src.texture.width, src.texture.height, TextureFormat.RGBA32, false);
        tex.SetPixels(nuevos);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), src.pixelsPerUnit);
    }

    private void Update()
    {
        if (frames == null) return;

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null) return;

        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, col.size * transform.lossyScale, 0f);
        foreach (Collider2D c in hits)
        {
            if (c == null || c.gameObject == gameObject) continue;

            bool esLaser = c.GetComponent<Laser>() != null || c.GetComponent<LaserEnemigo>() != null;
            if (!esLaser) continue;

            Destroy(c.gameObject);
            if (!Danar()) return;
        }
    }

    // Avanza un nivel de daño. Devuelve false si el bunker quedó destruido.
    private bool Danar()
    {
        dano++;
        if (dano >= frames.Length)
        {
            Destroy(gameObject);
            return false;
        }

        AplicarFrame();
        return true;
    }

    private void AplicarFrame()
    {
        if (frames == null || dano < 0 || dano >= frames.Length) return;

        Sprite f = frames[dano];
        if (f == null) return;

        if (sr != null)
        {
            sr.color = Color.white;
            sr.sprite = f;
        }

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            float h = (dano < ALTURAS_LOCALES.Length) ? ALTURAS_LOCALES[dano] : 0.12f;
            // recorte 26x12 con pivot centrado: el contenido va pegado al fondo (y = -0.06)
            col.size = new Vector2(0.26f, h);
            col.offset = new Vector2(0f, -0.06f + h * 0.5f);
        }
    }
}
