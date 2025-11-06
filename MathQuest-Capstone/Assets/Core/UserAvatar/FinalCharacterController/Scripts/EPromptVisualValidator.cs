using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Helper script to validate and show what your EPromptVisual prefab should have.
/// Attach this to your prefab to check if it's set up correctly.
/// </summary>
public class EPromptVisualValidator : MonoBehaviour
{
    [Header("Validation")]
    [Tooltip("Click the button in Inspector to validate this prefab")]
    public bool runValidation = false;

    void OnValidate()
    {
        if (runValidation)
        {
            runValidation = false;
            ValidatePrefab();
        }
    }

    [ContextMenu("Validate Prefab Structure")]
    public void ValidatePrefab()
    {
        Debug.Log("=== EPromptVisual Prefab Validation ===");
        
        // Check RectTransform
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Debug.Log("✓ RectTransform found on root");
            Debug.Log($"  Size: {rectTransform.sizeDelta}");
            Debug.Log($"  Anchors: Min({rectTransform.anchorMin}) Max({rectTransform.anchorMax})");
        }
        else
        {
            Debug.LogError("✗ RectTransform NOT found! This prefab needs a RectTransform to work with UI Canvas.");
        }

        // Check for TextMeshProUGUI
        TextMeshProUGUI text = GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            Debug.Log($"✓ TextMeshProUGUI found on '{text.gameObject.name}'");
            Debug.Log($"  Font Size: {text.fontSize}");
            Debug.Log($"  Text: '{text.text}'");
            Debug.Log($"  Alignment: {text.alignment}");
        }
        else
        {
            Debug.LogError("✗ TextMeshProUGUI NOT found! InteractionUI needs this to display the prompt text.");
        }

        // Check for Image components
        Image[] images = GetComponentsInChildren<Image>();
        if (images.Length > 0)
        {
            Debug.Log($"✓ Found {images.Length} Image component(s):");
            foreach (Image img in images)
            {
                Debug.Log($"  - '{img.gameObject.name}' (Color: {img.color})");
            }
        }
        else
        {
            Debug.LogWarning("⚠ No Image components found. Consider adding a background for better visibility.");
        }

        // Check Canvas Group (optional but useful)
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            Debug.Log("✓ CanvasGroup found (good for fading)");
        }
        else
        {
            Debug.LogWarning("⚠ No CanvasGroup found. InteractionUI will add one to the canvas, but you can add one here too.");
        }

        Debug.Log("=== Validation Complete ===");
    }

    [ContextMenu("Auto-Setup Basic Structure")]
    public void AutoSetupBasicStructure()
    {
        Debug.Log("=== Auto-Setting Up EPromptVisual ===");

        // Ensure RectTransform
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("Cannot setup: This GameObject needs to be a UI element with RectTransform!");
            return;
        }

        // Set recommended size
        rectTransform.sizeDelta = new Vector2(250f, 80f);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        Debug.Log("✓ RectTransform configured");

        // Add background image if none exists
        Image bgImage = GetComponent<Image>();
        if (bgImage == null)
        {
            bgImage = gameObject.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Dark semi-transparent
            Debug.Log("✓ Background Image added");
        }

        // Find or create text
        TextMeshProUGUI text = GetComponentInChildren<TextMeshProUGUI>();
        if (text == null)
        {
            GameObject textGO = new GameObject("InteractionText");
            textGO.transform.SetParent(transform, false);
            
            text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = "Press E to interact";
            text.fontSize = 28;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10); // Padding
            textRect.offsetMax = new Vector2(-10, -10);

            Debug.Log("✓ TextMeshProUGUI created");
        }
        else
        {
            Debug.Log("✓ TextMeshProUGUI already exists");
        }

        Debug.Log("=== Auto-Setup Complete! ===");
        Debug.Log("Your prefab is now ready to use with InteractionUI!");
    }
}

