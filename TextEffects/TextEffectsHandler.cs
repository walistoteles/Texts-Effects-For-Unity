using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[RequireComponent(typeof(TMP_Text))]
public class TextEffectsHandler : MonoBehaviour
{
    public TMP_Text tmpText;

    private class TextEffect
    {
        public string type;
        public float speed = 5f;
        public float force = 10f;
    }

    private class EffectRegion
    {
        public int startIndex;
        public int endIndex;
        public List<TextEffect> effects = new List<TextEffect>();
    }

    private List<EffectRegion> regions = new List<EffectRegion>();

    void Awake()
    {
        if (tmpText == null)
            tmpText = GetComponent<TMP_Text>();
    }

    void Start()
    {
        ParseText(tmpText.text);
    }

    void Update()
    {
        ApplyEffects();
    }

    public void ParseText(string text)
    {
        regions.Clear();
        string parsed = ParseRecursive(text, out List<EffectRegion> result);
        tmpText.text = parsed;
        tmpText.ForceMeshUpdate();
        regions = result;
    }

    string ParseRecursive(string input, out List<EffectRegion> collectedEffects, List<TextEffect> inherited = null)
    {
        if (inherited == null) inherited = new List<TextEffect>();
        collectedEffects = new List<EffectRegion>();

        string pattern = @"<(\w+)([^>]*)>(.*?)<\/\1>";
        var matches = Regex.Matches(input, pattern, RegexOptions.Singleline);

        int offset = 0;
        foreach (Match match in matches)
        {
            string tag = match.Groups[1].Value.ToLower();
            string attributes = match.Groups[2].Value;
            string inner = match.Groups[3].Value;

            float speed = 5f, force = 10f;

            var speedMatch = Regex.Match(attributes, @"s\s*=\s*[""']?([\d.]+)[""']?");
            var forceMatch = Regex.Match(attributes, @"f\s*=\s*[""']?([\d.]+)[""']?");
            if (speedMatch.Success) float.TryParse(speedMatch.Groups[1].Value, out speed);
            if (forceMatch.Success) float.TryParse(forceMatch.Groups[1].Value, out force);

            var newEffect = new TextEffect { type = tag, speed = speed, force = force };

            string parsedInner = ParseRecursive(inner, out List<EffectRegion> innerEffects, new List<TextEffect>(inherited) { newEffect });

            int realIndex = match.Index - offset;
            input = input.Remove(realIndex, match.Length);
            input = input.Insert(realIndex, parsedInner);
            offset += match.Length - parsedInner.Length;

            var region = new EffectRegion
            {
                startIndex = realIndex,
                endIndex = realIndex + parsedInner.Length,
                effects = new List<TextEffect>(inherited) { newEffect }
            };

            collectedEffects.Add(region);
            collectedEffects.AddRange(innerEffects);
        }

        return input;
    }

    void ApplyEffects()
    {
        TMP_TextInfo textInfo = tmpText.textInfo;
        tmpText.ForceMeshUpdate();

        foreach (var region in regions)
        {
            for (int i = region.startIndex; i < region.endIndex && i < textInfo.characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;

                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                Vector3[] verts = textInfo.meshInfo[materialIndex].vertices;
                Color32[] colors = textInfo.meshInfo[materialIndex].colors32;

                Vector3 center = (verts[vertexIndex] + verts[vertexIndex + 2]) / 2f;
                Vector3 offset = Vector3.zero;
                float scaleX = 1f, scaleY = 1f;
                float alpha = 1f;
                float rotation = 0f;

                foreach (var effect in region.effects)
                {
                    float t = Time.time * effect.speed;

                    switch (effect.type)
                    {
                        case "wiggle":
                            offset += new Vector3(0, Mathf.Sin(t + i * 0.25f) * effect.force, 0);
                            break;
                        case "shake":
                            offset += new Vector3(
                                (Mathf.PerlinNoise(i * 0.5f, t) - 0.5f) * effect.force,
                                (Mathf.PerlinNoise(i * 0.5f + 100, t) - 0.5f) * effect.force,
                                0
                            );
                            break;
                        case "bounce":
                            offset += new Vector3(0, Mathf.Abs(Mathf.Sin(t + i * 0.2f)) * effect.force, 0);
                            break;
                        case "glitch":
                            offset += new Vector3(
                                Random.Range(-effect.force, effect.force),
                                Random.Range(-effect.force, effect.force),
                                0
                            );
                            break;
                        case "scale":
                            float scale = 1 + Mathf.Sin(t + i * 0.2f) * (effect.force * 0.01f);
                            scaleX *= scale;
                            scaleY *= scale;
                            break;
                        case "squash":
                            scaleX *= 1 + Mathf.Sin(t + i * 0.3f) * (effect.force * 0.01f);
                            scaleY *= 1 - Mathf.Sin(t + i * 0.3f) * (effect.force * 0.01f);
                            break;
                        case "fadewave":
                            float wave = Mathf.Sin(t + i * 0.3f) * 0.5f + 0.5f;
                            alpha *= wave;
                            break;
                        case "wave":
                            offset += new Vector3(0, Mathf.Sin(t + i * 0.5f) * effect.force, 0);
                            break;
                        case "flip":
                            rotation += Mathf.Sin(t + i) * effect.force * 10f;
                            break;
                        case "explode":
                            float explodeDistance = Mathf.Abs(Mathf.Sin(t + i)) * effect.force;
                            Vector3 dir = (verts[vertexIndex] - center).normalized;
                            offset += dir * explodeDistance;
                            break;
                    }
                }

                for (int j = 0; j < 4; j++)
                {
                    Vector3 pos = verts[vertexIndex + j];
                    pos = center + Vector3.Scale(pos - center, new Vector3(scaleX, scaleY, 1));

                    if (Mathf.Abs(rotation) > 0.01f)
                        pos = RotatePointAroundPivot(pos, center, new Vector3(0, 0, rotation));

                    verts[vertexIndex + j] = pos + offset;
                }

                for (int j = 0; j < 4; j++)
                {
                    Color32 c = colors[vertexIndex + j];
                    c.a = (byte)(Mathf.Clamp01(alpha) * 255);
                    colors[vertexIndex + j] = c;
                }
            }
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            meshInfo.mesh.colors32 = meshInfo.colors32;
            tmpText.UpdateGeometry(meshInfo.mesh, i);
        }
    }

    Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot;
        dir = Quaternion.Euler(angles) * dir;
        point = dir + pivot;
        return point;
    }
}
