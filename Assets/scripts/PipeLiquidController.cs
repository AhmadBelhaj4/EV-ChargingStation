using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PipeLiquidController : MonoBehaviour
{
    public Image liquidFillImage;
    public Text liquidText;
    [Header("Progress Bar")]
    public Slider liquidProgressBar;

    [Header("Pipe Renderers")]
    public Renderer[] pipeRenderers;

    [Header("Liquid Settings")]
    public float fillDuration = 2f;

    [Header("Colors")]
    public Color emptyColor  = new Color(0.2f, 0.2f, 0.2f);
    public Color filledColor = new Color(0.1f, 0.5f, 1f);      // cold blue

    private static readonly int FillID     = Shader.PropertyToID("_Fill");
    private static readonly int ColorID    = Shader.PropertyToID("_Color");
    private static readonly int FresnelID  = Shader.PropertyToID("_Fresnel_Color");
    private static readonly int TopColorID = Shader.PropertyToID("_Top_Color");

    private float currentFill = 1f;
    private MaterialPropertyBlock[] blocks;
    private Coroutine currentRoutine;

    void Awake()
    {
        blocks = new MaterialPropertyBlock[pipeRenderers.Length];
        for (int i = 0; i < pipeRenderers.Length; i++)
            blocks[i] = new MaterialPropertyBlock();
    }

    public void RemoveLiquid()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        // Remove doesn't care about color — just drain the fill
        currentRoutine = StartCoroutine(AnimateFill(currentFill, 0f, useColor: false));
    }

    public void AddLiquid()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        // Add forces cold blue color as it fills
        currentRoutine = StartCoroutine(AnimateFill(currentFill, 1f, useColor: true));
    }

    public float GetCurrentFill()
    {
        return currentFill;
    }

    IEnumerator AnimateFill(float startFill, float targetFill, bool useColor)
    {
        float elapsed = 0f;

        while (elapsed < fillDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fillDuration);

            currentFill = Mathf.Lerp(startFill, targetFill, t);

            if (useColor)
            {
                // Gradually shift from emptyColor to filledColor (cold blue) as liquid rises
                Color currentColor = Color.Lerp(emptyColor, filledColor, currentFill);
                ApplyToAllPipes(currentFill, currentColor);
            }
            else
            {
                // No color change — just update fill level
                ApplyToAllPipes(currentFill);
            }

            yield return null;
        }

        // Snap to final values
        currentFill = targetFill;

        if (useColor)
            ApplyToAllPipes(targetFill, filledColor);  // force pure cold blue at the end
        else
            ApplyToAllPipes(targetFill);

        // Trigger quests when drain/fill completes
        if (targetFill <= 0.01f)
            QuestManager.Instance?.UpdateQuest("drain_pipes");
        else if (targetFill >= 0.99f)
            QuestManager.Instance?.UpdateQuest("refill_liquid");
    }

// ── With color (used by AddLiquid) ────────────────────────────────────
void ApplyToAllPipes(float fill, Color col)
{
    Color glow = col * 2.5f;

    for (int i = 0; i < pipeRenderers.Length; i++)
    {
        if (pipeRenderers[i] == null) continue;

        pipeRenderers[i].GetPropertyBlock(blocks[i]);
        blocks[i].SetFloat(FillID,     fill);
        blocks[i].SetColor(ColorID,    col);
        blocks[i].SetColor(FresnelID,  glow);
        blocks[i].SetColor(TopColorID, glow);
        pipeRenderers[i].SetPropertyBlock(blocks[i]);
    }

    if (liquidProgressBar != null) liquidProgressBar.value = fill;  // ← added
    UpdateUI(fill); // ← added
}

// ── Without color (used by RemoveLiquid) ──────────────────────────────
void ApplyToAllPipes(float fill)
{
    for (int i = 0; i < pipeRenderers.Length; i++)
    {
        if (pipeRenderers[i] == null) continue;

        pipeRenderers[i].GetPropertyBlock(blocks[i]);
        blocks[i].SetFloat(FillID, fill);
        pipeRenderers[i].SetPropertyBlock(blocks[i]);
    }

    if (liquidProgressBar != null) liquidProgressBar.value = fill;  // ← added
    UpdateUI(fill); // ← added
}
void UpdateUI(float fill)
{
    float clamped = Mathf.Clamp01(fill);

    if (liquidFillImage != null)
        liquidFillImage.fillAmount = clamped;

    if (liquidText != null)
        liquidText.text = Mathf.RoundToInt(clamped * 100f) + "%";
}
}