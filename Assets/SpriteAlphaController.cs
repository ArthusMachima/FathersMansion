using UnityEngine;
using UnityEngine.Tilemaps;

public class RendererGroupAlpha : MonoBehaviour
{
    [Range(0f, 1f)]
    public float alpha = 1f;

    private SpriteRenderer[] childSprites;
    private Tilemap[] childTilemaps;
    private int spriteCount;
    private int tilemapCount;
    public bool doLog;

    public float GroupAlpha
    {
        get => alpha;
        set
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(alpha, value)) return;
            alpha = value;
            lastAlpha = value;
            ApplyAlpha();
        }
    }

    private float lastAlpha = -1f;

    void OnEnable()
    {
        RefreshRenderers();
    }

    void Update()
    {
        if (Mathf.Approximately(alpha, lastAlpha)) return;
        alpha = Mathf.Clamp01(alpha);
        ApplyAlpha();
        lastAlpha = alpha;
    }

    /// <summary>
    /// Re-scans the hierarchy. Call this if you add or remove
    /// child renderers or colliders at runtime.
    /// </summary>
    public void RefreshRenderers()
    {
        childSprites = null;
        childTilemaps = null;
        childSprites = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        childTilemaps = GetComponentsInChildren<Tilemap>(includeInactive: true);
        spriteCount = childSprites.Length;
        tilemapCount = childTilemaps.Length;
        lastAlpha = -1f;
        ApplyAlpha();
    }

    void ApplyAlpha()
    {
        float a = alpha;
        bool visible = a > 0f;

        

        for (int i = 0; i < spriteCount; i++)
        {
            SpriteRenderer sr = childSprites[i];
            if (sr == null) continue;
            Color c = sr.color;
            c.a = a;
            sr.color = c;
        }

        for (int i = 0; i < tilemapCount; i++)
        {
            Tilemap tm = childTilemaps[i];
            if (tm == null) continue;
            Color c = tm.color;
            c.a = a;
            tm.color = c;
        }

    }

#if UNITY_EDITOR
    void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            alpha = Mathf.Clamp01(alpha);
            RefreshRenderers();
        };
    }
#endif
}