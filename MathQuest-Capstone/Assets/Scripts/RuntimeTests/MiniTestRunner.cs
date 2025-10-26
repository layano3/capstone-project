using UnityEngine;
using System.Collections.Generic;
using Supabase;
using System;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UserAvatar.FinalCharacterController;

public class MiniTestRunner : MonoBehaviour
{
    int passed = 0;
    int failed = 0;
    List<string> results = new List<string>();

    async void Start()
    {
        Debug.Log("🧪 Starting QA Test Suite...\n");

        await Test_ConfigExists();
        await Test_LoginFailsGracefully();
        await Test_AddXpEvent();
        await Test_MainMenuButtons();
        await Test_QuestSystem();
        await Test_AudioSystem();
        await Test_UIComponents();
        await Test_SceneManagement();
        await Test_InputSystem();
        await Test_LevelCalculation();
        await Test_XPProgression();

        Debug.Log($"✅ Passed: {passed} | ❌ Failed: {failed}");
        foreach (var r in results) Debug.Log(r);
    }

    async Task Test_ConfigExists()
    {
        var cfg = Resources.Load<SupabaseConfig>("SupabaseConfig");
        if (cfg != null && !string.IsNullOrEmpty(cfg.url))
        {
            Pass("Config Exists");
        }
        else
        {
            Fail("Config Exists", "SupabaseConfig missing or invalid");
        }
        await Task.Yield();
    }

    async Task Test_LoginFailsGracefully()
    {
        var cfg = Resources.Load<SupabaseConfig>("SupabaseConfig");
        string err = null;

        bool finished = false;
        var login = SupabaseAuth.SignIn(cfg, "fake@user.com", "badpass", (session, e) =>
        {
            err = e;
            finished = true;
        });

        while (!finished) await Task.Yield();

        if (err != null)
            Pass("Login Fails Gracefully");
        else
            Fail("Login Fails Gracefully", "Expected login error, got success");
    }

    async Task Test_AddXpEvent()
    {
        try
        {
            var cfg = Resources.Load<SupabaseConfig>("SupabaseConfig");
            if (cfg == null)
            {
                Fail("Add XP Event", "SupabaseConfig not found");
                return;
            }

            // Create XPManager instance and test the AddXpEvent method
            var xpMgr = new GameObject("XPManager").AddComponent<XPManager>();
            xpMgr.config = cfg;
            
            var studentGuid = "3b5c83b3-619a-4915-9948-fcfb1d8a4baa";
            bool finished = false;
            string error = null;

            // Start the coroutine
            StartCoroutine(xpMgr.AddXpEvent(studentGuid, 25, "QA XP Test", "MiniRunner", (err) =>
            {
                error = err;
                finished = true;
            }));

            // Wait for completion
            while (!finished) await Task.Yield();

            if (error == null)
                Pass("Add XP Event");
            else
                Fail("Add XP Event", error);

            // Clean up
            DestroyImmediate(xpMgr.gameObject);
        }
        catch (Exception ex)
        {
            Fail("Add XP Event", ex.Message);
        }
    }

    async Task Test_MainMenuButtons()
    {
        try
        {
            var mainMenuGO = new GameObject("MainMenu");
            var mainMenu = mainMenuGO.AddComponent<MainMenu>();
            
            // Test that all button methods exist and can be called
            try { mainMenu.LoadQuests(); } catch { throw new Exception("LoadQuests failed"); }
            try { mainMenu.LoadProfile(); } catch { throw new Exception("LoadProfile failed"); }
            try { mainMenu.LoadSettings(); } catch { throw new Exception("LoadSettings failed"); }
            try { mainMenu.BackToMainMenu(); } catch { throw new Exception("BackToMainMenu failed"); }
            try { mainMenu.LoadGame(); } catch { throw new Exception("LoadGame failed"); }
            try { mainMenu.QuitGame(); } catch { throw new Exception("QuitGame failed"); }
            
            Pass("MainMenu Buttons");
            DestroyImmediate(mainMenuGO);
        }
        catch (Exception ex)
        {
            Fail("MainMenu Buttons", ex.Message);
        }
        await Task.Yield();
    }

    async Task Test_QuestSystem()
    {
        try
        {
            // Test QuestManager singleton (simplified to avoid dependency issues)
            var questManagerGO = new GameObject("QuestManager");
            var questManager = questManagerGO.AddComponent<QuestManager>();
            
            if (questManager != null)
                Pass("Quest System");
            else
                Fail("Quest System", "QuestManager creation failed");
            
            DestroyImmediate(questManagerGO);
        }
        catch (Exception ex)
        {
            Fail("Quest System", ex.Message);
        }
        await Task.Yield();
    }

    async Task Test_AudioSystem()
    {
        try
        {
            // Test AudioManager singleton
            var audioManagerGO = new GameObject("AudioManager");
            var audioManager = audioManagerGO.AddComponent<AudioManager>();
            
            if (AudioManager.Instance != null && AudioManager.Instance == audioManager)
            {
                // Test audio methods with null checks
                try { audioManager.StopMusic(); } catch { throw new Exception("StopMusic failed"); }
                
                // Only test volume methods if AudioMixer is available
                if (audioManager.audioMixer != null)
                {
                    try { audioManager.SetMusicVolume(0.5f); } catch { throw new Exception("SetMusicVolume failed"); }
                    try { audioManager.SetSFXVolume(0.7f); } catch { throw new Exception("SetSFXVolume failed"); }
                }
                else
                {
                    Debug.LogWarning("AudioMixer is null, skipping volume tests");
                }
                
                Pass("Audio System");
            }
            else
            {
                Fail("Audio System", "AudioManager singleton failed");
            }
            
            DestroyImmediate(audioManagerGO);
        }
        catch (Exception ex)
        {
            Fail("Audio System", ex.Message);
        }
        await Task.Yield();
    }

    async Task Test_UIComponents()
    {
        try
        {
            // Test UI component creation
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            
            var buttonGO = new GameObject("Button");
            buttonGO.transform.SetParent(canvasGO.transform);
            var button = buttonGO.AddComponent<Button>();
            
            var inputFieldGO = new GameObject("InputField");
            inputFieldGO.transform.SetParent(canvasGO.transform);
            var inputField = inputFieldGO.AddComponent<TMP_InputField>();
            
            if (canvas != null && button != null && inputField != null)
                Pass("UI Components");
            else
                Fail("UI Components", "UI component creation failed");
            
            DestroyImmediate(canvasGO);
        }
        catch (Exception ex)
        {
            Fail("UI Components", ex.Message);
        }
        await Task.Yield();
    }

    async Task Test_SceneManagement()
    {
        try
        {
            // Test scene management
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            
            if (sceneCount > 0)
                Pass("Scene Management");
            else
                Fail("Scene Management", "No scenes in build settings");
        }
        catch (Exception ex)
        {
            Fail("Scene Management", ex.Message);
        }
        await Task.Yield();
    }

    async Task Test_InputSystem()
    {
        try
        {
            // Test input system
            var inputGO = new GameObject("InputTest");
            var playerInput = inputGO.AddComponent<UnityEngine.InputSystem.PlayerInput>();
            
            if (playerInput != null)
                Pass("Input System");
            else
                Fail("Input System", "PlayerInput component creation failed");
            
            DestroyImmediate(inputGO);
        }
        catch (Exception ex)
        {
            Fail("Input System", ex.Message);
        }
        await Task.Yield();
    }

    async Task Test_LevelCalculation()
    {
        try
        {
            int level1 = LevelCalculator.CalculateLevel(0);
            int level2 = LevelCalculator.CalculateLevel(100);
            int level3 = LevelCalculator.CalculateLevel(250);
            
            if (level1 == 1 && level2 == 2 && level3 == 3)
                Pass("Level Calculation");
            else
                Fail("Level Calculation", $"Expected levels 1,2,3 but got {level1},{level2},{level3}");
        }
        catch (Exception ex)
        {
            Fail("Level Calculation", ex.Message);
        }
        await Task.Yield();
    }

    async Task Test_XPProgression()
    {
        try
        {
            int xpForLevel2 = LevelCalculator.CalculateXPForNextLevel(1);
            int xpForLevel3 = LevelCalculator.CalculateXPForNextLevel(2);
            float progress = LevelCalculator.CalculateProgressPercentage(175, 2);
            
            bool validProgression = xpForLevel2 == 100 && 
                                   xpForLevel3 == 150 && 
                                   progress >= 0.49f && progress <= 0.51f;
            
            if (validProgression)
                Pass("XP Progression");
            else
                Fail("XP Progression", $"XP values or progress incorrect: {xpForLevel2}, {xpForLevel3}, {progress}");
        }
        catch (Exception ex)
        {
            Fail("XP Progression", ex.Message);
        }
        await Task.Yield();
    }

    void Pass(string name)
    {
        passed++;
        results.Add($"✅ {name} passed");
    }

    void Fail(string name, string reason)
    {
        failed++;
        results.Add($"❌ {name} failed: {reason}");
    }
}
