using System.Collections;
using TMPro;
using UnityEngine;

public enum WriteEffectType
{
    None,
    FadeIn,
    SlideIn,
    ScaleUp,
    ScaleDown,
    TypewriterCursor,
    Wave,
    Pop,
    DropIn,
    GlitchIn,
    BlinkOn
}

[RequireComponent(typeof(TMP_Text))]
public class Writter : MonoBehaviour
{
    public TMP_Text tmpText;
    public TextEffectsHandler effectsHandler;

    [SerializeField] private float writeSpeed = 0.05f;
    [SerializeField] private float effectDuration = 0.3f;
    [SerializeField] private float waveAmplitude = 12f;
    [SerializeField] private float waveFrequency = 6f;

    private Coroutine writeRoutine;
    private string textoCompleto;
    private int letrasEscritas = 0;

    private void Awake()
    {
        if (tmpText == null)
            tmpText = GetComponent<TMP_Text>();

        if (effectsHandler == null)
            effectsHandler = GetComponent<TextEffectsHandler>();

    }

    public void Escrever(string textoComTags, WriteEffectType efeito = WriteEffectType.None)
    {
        if (writeRoutine != null)
            StopCoroutine(writeRoutine);

        tmpText.text = "";
        if (effectsHandler != null)
            effectsHandler.enabled = false;

        writeRoutine = StartCoroutine(EscreverCoroutine(textoComTags, efeito));
    }

    private IEnumerator EscreverCoroutine(string textoComTags, WriteEffectType efeito)
    {
        textoCompleto = RemoveTags(textoComTags);
        letrasEscritas = 0;

        while (letrasEscritas <= textoCompleto.Length)
        {
            tmpText.text = textoCompleto.Substring(0, letrasEscritas);

            if (efeito == WriteEffectType.TypewriterCursor)
            {
                if (letrasEscritas < textoCompleto.Length && Time.time % 1f < 0.5f)
                    tmpText.text += "_";
            }
            else if (efeito != WriteEffectType.None)
            {
                int idx = letrasEscritas - 1;
                AplicarEfeitoDeEscrita(idx, efeito, 1f);
                AplicarEfeitoDeEscrita(idx - 1, efeito, 0.5f);
            }

            letrasEscritas++;
            yield return new WaitForSeconds(writeSpeed);
        }

        if (effectsHandler != null)
        {
            effectsHandler.enabled = true;
            effectsHandler.ParseText(textoComTags);
        }
    }

    private void AplicarEfeitoDeEscrita(int letraIndex, WriteEffectType efeito, float ampMul = 1f)
    {
        var info = tmpText.textInfo;
        if (letraIndex < 0 || letraIndex >= info.characterCount) return;
        if (!info.characterInfo[letraIndex].isVisible) return;

        tmpText.ForceMeshUpdate();

        int vtx = info.characterInfo[letraIndex].vertexIndex;
        int mat = info.characterInfo[letraIndex].materialReferenceIndex;
        Vector3[] verts = info.meshInfo[mat].vertices;
        Color32[] cols = info.meshInfo[mat].colors32;

        float rawT = (Time.time * 1000f % (effectDuration * 1000f)) / (effectDuration * 1000f);
        float t = Mathf.Clamp01(rawT);

        switch (efeito)
        {
            case WriteEffectType.FadeIn:
                {
                    byte alpha = (byte)(255 * Mathf.Clamp01(t * ampMul * 2f));
                    for (int i = 0; i < 4; i++)
                        cols[vtx + i].a = alpha;
                }
                break;

            case WriteEffectType.SlideIn:
                {
                    Vector3 offset = new Vector3(Mathf.Lerp(-200f, 0f, t) * ampMul, 0, 0);
                    for (int i = 0; i < 4; i++)
                        verts[vtx + i] += offset;
                }
                break;

            case WriteEffectType.ScaleUp:
                {
                    Vector3 cen = (verts[vtx] + verts[vtx + 2]) / 2f;
                    float s = Mathf.Lerp(0.2f, 1f, t) * ampMul + (1 - ampMul);
                    for (int i = 0; i < 4; i++)
                        verts[vtx + i] = cen + (verts[vtx + i] - cen) * s;
                }
                break;

            case WriteEffectType.ScaleDown:
                {
                    Vector3 cen = (verts[vtx] + verts[vtx + 2]) / 2f;
                    float s = Mathf.Lerp(5f, 1f, t) * ampMul + (1 - ampMul);
                    for (int i = 0; i < 4; i++)
                        verts[vtx + i] = cen + (verts[vtx + i] - cen) * s;
                }
                break;

            case WriteEffectType.Wave:
                {
                    float w = Mathf.Sin(Time.time * waveFrequency + letraIndex * 0.5f) * waveAmplitude * ampMul;
                    Vector3 offset = new Vector3(0, w, 0);
                    for (int i = 0; i < 4; i++)
                        verts[vtx + i] += offset;
                }
                break;

            case WriteEffectType.Pop:
                {
                    Vector3 cen = (verts[vtx] + verts[vtx + 2]) / 2f;
                    float s = Mathf.Sin(t * Mathf.PI) * ampMul * 2.5f + 1f;
                    for (int i = 0; i < 4; i++)
                        verts[vtx + i] = cen + (verts[vtx + i] - cen) * s;
                }
                break;

            case WriteEffectType.DropIn:
                {
                    float drop = Mathf.Lerp(300f, 0f, t) * ampMul;
                    float bounce = Mathf.Sin(t * Mathf.PI) * 40f * ampMul;
                    Vector3 offset = new Vector3(0, drop + bounce, 0);
                    for (int i = 0; i < 4; i++)
                        verts[vtx + i] += offset;
                }
                break;

            case WriteEffectType.GlitchIn:
                {
                    float inten = Mathf.Lerp(20f, 0f, t) * ampMul;
                    Vector3 offset = new Vector3(
                        Random.Range(-inten, inten),
                        Random.Range(-inten, inten),
                        0
                    );
                    for (int i = 0; i < 4; i++)
                        verts[vtx + i] += offset;
                }
                break;

            case WriteEffectType.BlinkOn:
                {
                    float speed = 60f * ampMul;
                    bool show = Mathf.FloorToInt(t * speed) % 2 == 0;
                    byte a = (byte)(show ? 255 : 0);
                    for (int i = 0; i < 4; i++)
                        cols[vtx + i].a = a;
                }
                break;
        }

        tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32);
    }

    private string RemoveTags(string s)
        => System.Text.RegularExpressions.Regex.Replace(s, @"<.*?>", "");
}
