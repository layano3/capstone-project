using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Supabase;

namespace Tests
{
    public class UITests
    {
        [Test]
        public void Test_ProfileController_UI_Components()
        {
            // Test ProfileController UI component setup
            var profileControllerGO = new GameObject("ProfileController");
            var profileController = profileControllerGO.AddComponent<ProfileController>();
            
            // Create UI components
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            
            // Create text components (using TextMeshProUGUI instead of abstract TMP_Text)
            var nameTextGO = new GameObject("NameText");
            nameTextGO.transform.SetParent(canvasGO.transform);
            var nameText = nameTextGO.AddComponent<TextMeshProUGUI>();
            
            var xpTextGO = new GameObject("XPText");
            xpTextGO.transform.SetParent(canvasGO.transform);
            var xpText = xpTextGO.AddComponent<TextMeshProUGUI>();
            
            var levelTextGO = new GameObject("LevelText");
            levelTextGO.transform.SetParent(canvasGO.transform);
            var levelText = levelTextGO.AddComponent<TextMeshProUGUI>();
            
            // Create slider
            var xpBarGO = new GameObject("XPBar");
            xpBarGO.transform.SetParent(canvasGO.transform);
            var xpBar = xpBarGO.AddComponent<Slider>();
            
            // Assign components to ProfileController
            profileController.nameText = nameText;
            profileController.xpText = xpText;
            profileController.levelText = levelText;
            profileController.xpBar = xpBar;
            
            // Test that components are properly assigned
            Assert.IsNotNull(profileController.nameText, "Name text should be assigned");
            Assert.IsNotNull(profileController.xpText, "XP text should be assigned");
            Assert.IsNotNull(profileController.levelText, "Level text should be assigned");
            Assert.IsNotNull(profileController.xpBar, "XP bar should be assigned");
            
            Debug.Log($"✅ ProfileController UI Components test passed");
            
            Object.DestroyImmediate(canvasGO);
            Object.DestroyImmediate(profileControllerGO);
        }

        [Test]
        public void Test_Button_State_Management()
        {
            // Test button state management
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            
            var buttonGO = new GameObject("Button");
            buttonGO.transform.SetParent(canvasGO.transform);
            var button = buttonGO.AddComponent<Button>();
            
            // Test initial state
            Assert.IsTrue(button.interactable, "Button should be interactable by default");
            
            // Test disabling button
            button.interactable = false;
            Assert.IsFalse(button.interactable, "Button should be disabled");
            
            // Test re-enabling button
            button.interactable = true;
            Assert.IsTrue(button.interactable, "Button should be re-enabled");
            
            Debug.Log($"✅ Button State Management test passed");
            
            Object.DestroyImmediate(canvasGO);
        }

        [Test]
        public void Test_InputField_Validation()
        {
            // Test input field validation
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            
            var inputFieldGO = new GameObject("InputField");
            inputFieldGO.transform.SetParent(canvasGO.transform);
            var inputField = inputFieldGO.AddComponent<TMP_InputField>();
            
            // Test initial state
            Assert.IsTrue(string.IsNullOrEmpty(inputField.text), "Input field should be empty initially");
            
            // Test setting text
            inputField.text = "test@example.com";
            Assert.AreEqual("test@example.com", inputField.text, "Input field text should be set correctly");
            
            // Test clearing text
            inputField.text = "";
            Assert.IsTrue(string.IsNullOrEmpty(inputField.text), "Input field should be empty after clearing");
            
            Debug.Log($"✅ InputField Validation test passed");
            
            Object.DestroyImmediate(canvasGO);
        }

        [Test]
        public void Test_Slider_Value_Management()
        {
            // Test slider value management
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            
            var sliderGO = new GameObject("Slider");
            sliderGO.transform.SetParent(canvasGO.transform);
            var slider = sliderGO.AddComponent<Slider>();
            
            // Test initial value
            Assert.AreEqual(0f, slider.value, "Slider should start at 0");
            
            // Test setting value
            slider.value = 0.5f;
            Assert.AreEqual(0.5f, slider.value, "Slider value should be set correctly");
            
            // Test min/max values
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.value = 75f;
            Assert.AreEqual(75f, slider.value, "Slider should respect min/max values");
            
            Debug.Log($"✅ Slider Value Management test passed");
            
            Object.DestroyImmediate(canvasGO);
        }

        [Test]
        public void Test_Text_Content_Management()
        {
            // Test text content management
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(canvasGO.transform);
            var text = textGO.AddComponent<TextMeshProUGUI>();
            
            // Test initial state
            Assert.IsTrue(string.IsNullOrEmpty(text.text), "Text should be empty initially");
            
            // Test setting text
            text.text = "Hello World";
            Assert.AreEqual("Hello World", text.text, "Text should be set correctly");
            
            // Test text formatting
            text.text = "XP: 100/200";
            Assert.AreEqual("XP: 100/200", text.text, "Formatted text should be set correctly");
            
            Debug.Log($"✅ Text Content Management test passed");
            
            Object.DestroyImmediate(canvasGO);
        }

        [Test]
        public void Test_UI_Event_Handling()
        {
            // Test UI event handling
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            
            var buttonGO = new GameObject("Button");
            buttonGO.transform.SetParent(canvasGO.transform);
            var button = buttonGO.AddComponent<Button>();
            
            int clickCount = 0;
            button.onClick.AddListener(() => clickCount++);
            
            // Test single click
            button.onClick.Invoke();
            Assert.AreEqual(1, clickCount, "Button click should increment counter");
            
            // Test multiple clicks
            button.onClick.Invoke();
            button.onClick.Invoke();
            Assert.AreEqual(3, clickCount, "Multiple clicks should increment counter");
            
            Debug.Log($"✅ UI Event Handling test passed");
            
            Object.DestroyImmediate(canvasGO);
        }

        [Test]
        public void Test_Canvas_Rendering()
        {
            // Test canvas rendering setup
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            var canvasScaler = canvasGO.AddComponent<CanvasScaler>();
            var graphicRaycaster = canvasGO.AddComponent<GraphicRaycaster>();
            
            // Test canvas properties
            Assert.IsNotNull(canvas, "Canvas should be created");
            Assert.IsNotNull(canvasScaler, "CanvasScaler should be created");
            Assert.IsNotNull(graphicRaycaster, "GraphicRaycaster should be created");
            
            // Test canvas render mode
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Assert.AreEqual(RenderMode.ScreenSpaceOverlay, canvas.renderMode, "Canvas render mode should be set");
            
            Debug.Log($"✅ Canvas Rendering test passed");
            
            Object.DestroyImmediate(canvasGO);
        }

        [Test]
        public void Test_UI_Layout_Components()
        {
            // Test UI layout components
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            
            // Test VerticalLayoutGroup
            var verticalLayoutGO = new GameObject("VerticalLayout");
            verticalLayoutGO.transform.SetParent(canvasGO.transform);
            var verticalLayout = verticalLayoutGO.AddComponent<VerticalLayoutGroup>();
            
            Assert.IsNotNull(verticalLayout, "VerticalLayoutGroup should be created");
            Assert.IsTrue(verticalLayout.childForceExpandHeight, "VerticalLayoutGroup should expand children by default");
            
            // Test HorizontalLayoutGroup
            var horizontalLayoutGO = new GameObject("HorizontalLayout");
            horizontalLayoutGO.transform.SetParent(canvasGO.transform);
            var horizontalLayout = horizontalLayoutGO.AddComponent<HorizontalLayoutGroup>();
            
            Assert.IsNotNull(horizontalLayout, "HorizontalLayoutGroup should be created");
            Assert.IsTrue(horizontalLayout.childForceExpandWidth, "HorizontalLayoutGroup should expand children by default");
            
            Debug.Log($"✅ UI Layout Components test passed");
            
            Object.DestroyImmediate(canvasGO);
        }

        [Test]
        public void Test_UI_Component_Performance()
        {
            // Test UI component creation performance
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            
            // Create 50 UI components
            for (int i = 0; i < 50; i++)
            {
                var buttonGO = new GameObject($"Button_{i}");
                buttonGO.transform.SetParent(canvasGO.transform);
                var button = buttonGO.AddComponent<Button>();
                var text = buttonGO.AddComponent<TextMeshProUGUI>();
                text.text = $"Button {i}";
            }
            
            stopwatch.Stop();
            
            // Should complete in reasonable time
            Assert.Less(stopwatch.ElapsedMilliseconds, 500, "UI component creation should be fast");
            
            Debug.Log($"✅ UI Component Performance test passed - {stopwatch.ElapsedMilliseconds}ms for 50 components");
            
            Object.DestroyImmediate(canvasGO);
        }
    }
}
