using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public class PortraitFit : MonoBehaviour
{
    const float TARGET = 9f / 16f;   // Portrait 9:16

    RectTransform rt;           // GameRoot
    RectTransform parentRT;     // SafeArea (parent)
    Vector2 lastParentSize;

    void OnEnable()
    {
        rt = transform as RectTransform;
        parentRT = transform.parent as RectTransform;  // <<< klíčové: počítáme z SafeArea
        Apply();
    }

    void OnTransformParentChanged()  // kdyby ses přepojil v hierarchii
    {
        parentRT = transform.parent as RectTransform;
        Apply();
    }

    void OnRectTransformDimensionsChange()  // editor i runtime změny SafeArea
    {
        Apply();
    }

    void Update()  // fallback pro WebGL/editor
    {
        // Guard proti nekonečnému volání
        if (!parentRT) return;
        if (!rt) return;
        
        Vector2 parentSize = parentRT.rect.size;
        
        // Kontrola, zda se opravdu změnila velikost (s tolerancí pro floating point chyby)
        if (Vector2.Distance(parentSize, lastParentSize) > 0.001f)
        {
            Apply();
        }
    }

    void Apply()
    {
        if (!rt || !parentRT) return;

        Vector2 parentSize = parentRT.rect.size;
        if (parentSize.x <= 0f || parentSize.y <= 0f) return;
        
        // Ochrana proti opakovanému volání se stejnými hodnotami
        if (Vector2.Distance(parentSize, lastParentSize) < 0.001f)
        {
            return; // Už bylo aplikováno
        }

        lastParentSize = parentSize;

        float pw = parentSize.x;
        float ph = parentSize.y;
        float current = pw / ph;

        float boxW, boxH;
        if (current > TARGET)
        {
            // Parent je relativně širší → výška limituje
            boxH = ph;
            boxW = ph * TARGET;
        }
        else
        {
            // Parent je relativně užší → šířka limituje
            boxW = pw;
            boxH = pw / TARGET;
        }

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); // middle-center
        rt.pivot = new Vector2(0.5f, 0.5f);

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, boxW);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   boxH);
        rt.anchoredPosition = Vector2.zero; // centrováno
    }
}
