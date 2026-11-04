using UnityEngine;

public class Bunker : MonoBehaviour
{
    private Texture2D tex;
    private SpriteRenderer sr;
    private Sprite spriteBase;
    private int totalOpacos;
    private const int RADIO = 4;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        Sprite src = Resources.Load<Sprite>("Bunkers/Bunker");
        if (src == null || src.texture == null)
        {
            Debug.LogWarning("Bunker: sprite no encontrado");
            return;
        }

        Color[] srcPixels = src.texture.GetPixels();
        tex = new Texture2D(src.texture.width, src.texture.height, TextureFormat.RGBA32, false);
        Color verde = new Color(0.3f, 0.8f, 0.2f, 1f);
        Color[] nuevos = new Color[srcPixels.Length];
        for (int i = 0; i < srcPixels.Length; i++)
        {
            nuevos[i] = (srcPixels[i].a > 0.1f) ? verde : Color.clear;
        }
        tex.SetPixels(nuevos);
        tex.Apply();

        spriteBase = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), src.pixelsPerUnit);
        sr.sprite = spriteBase;

        totalOpacos = ContarOpacos();
    }

    private void Update()
    {
        if (tex == null) return;
        // Poll for overlapping lasers (trigger detection misses fast-moving lasers)
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null) return;
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, col.size * transform.lossyScale, 0f);
        foreach (Collider2D c in hits)
        {
            if (c == null) continue;
            if (c.gameObject == gameObject) continue;
            Laser laser = c.GetComponent<Laser>();
            LaserEnemigo laserEnemigo = c.GetComponent<LaserEnemigo>();
            if (laser == null && laserEnemigo == null) continue;
            // erode
            if (laser != null) { Destroy(laser.gameObject); }
            else if (laserEnemigo != null) { Destroy(laserEnemigo.gameObject); }
            Vector3 local = transform.InverseTransformPoint(c.transform.position);
            float ppu = (spriteBase != null) ? spriteBase.pixelsPerUnit : 100f;
            int px = Mathf.Clamp(Mathf.RoundToInt(local.x * ppu + tex.width * 0.5f), 0, tex.width - 1);
            int py = Mathf.Clamp(Mathf.RoundToInt(local.y * ppu + tex.height * 0.5f), 0, tex.height - 1);
            Borrar(px, py, RADIO);
            tex.Apply();
            int restantes = ContarOpacos();
            if (totalOpacos > 0 && restantes < totalOpacos * 0.12f)
            {
                Destroy(gameObject);
                return;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        bool esLaser = other.GetComponent<Laser>() != null || other.GetComponent<LaserEnemigo>() != null;
        if (!esLaser) return;

        Vector3 _local = transform.InverseTransformPoint(other.transform.position);

        Destroy(other.gameObject);

        Vector3 local = transform.InverseTransformPoint(other.transform.position);
        float ppu = (spriteBase != null) ? spriteBase.pixelsPerUnit : 100f;
        int px = Mathf.Clamp(Mathf.RoundToInt(local.x * ppu + tex.width * 0.5f), 0, tex.width - 1);
        // Determine entry side based on laser velocity direction
        Vector2 vel = Vector2.zero;
        Laser l = other.GetComponent<Laser>();
        LaserEnemigo le = other.GetComponent<LaserEnemigo>();
        if (l != null) vel = Vector2.up; // player laser moves up -> enters from bottom
        else if (le != null) vel = le.direccion; // enemy laser moves in its direction
        int py;
        if (vel.y > 0) py = 0; // moving up -> hit bottom
        else if (vel.y < 0) py = tex.height - 1; // moving down -> hit top
        else py = Mathf.Clamp(Mathf.RoundToInt(local.y * ppu + tex.height * 0.5f), 0, tex.height - 1);

        Borrar(px, py, RADIO);
        tex.Apply();

        int restantes = ContarOpacos();
        if (totalOpacos > 0 && restantes < totalOpacos * 0.12f)
        {
            Destroy(gameObject);
        }
    }

    private void Borrar(int cx, int cy, int r)
    {
        for (int y = cy - r; y <= cy + r; y++)
        {
            for (int x = cx - r; x <= cx + r; x++)
            {
                if (x < 0 || y < 0 || x >= tex.width || y >= tex.height) continue;
                int dx = x - cx;
                int dy = y - cy;
                if (dx * dx + dy * dy <= r * r)
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
    }

    private int ContarOpacos()
    {
        int c = 0;
        foreach (Color p in tex.GetPixels())
        {
            if (p.a > 0.01f) c++;
        }
        return c;
    }
}
