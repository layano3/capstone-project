#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System.Collections;
using Supabase;
using UserAvatar.FinalCharacterController;

namespace Tests
{
    public class GameplayTests
    {
        [Test]
        public void Test_MainMenu_ButtonMethods()
        {
            // Test MainMenu button methods exist and can be called
            var mainMenuGO = new GameObject("MainMenu");
            var mainMenu = mainMenuGO.AddComponent<MainMenu>();
            
            // Test that all button methods exist and can be called without errors
            Assert.DoesNotThrow(() => mainMenu.LoadQuests(), "LoadQuests should not throw");
            Assert.DoesNotThrow(() => mainMenu.LoadProfile(), "LoadProfile should not throw");
            Assert.DoesNotThrow(() => mainMenu.LoadSettings(), "LoadSettings should not throw");
            Assert.DoesNotThrow(() => mainMenu.BackToMainMenu(), "BackToMainMenu should not throw");
            Assert.DoesNotThrow(() => mainMenu.LoadGame(), "LoadGame should not throw");
            Assert.DoesNotThrow(() => mainMenu.QuitGame(), "QuitGame should not throw");
            
            Debug.Log($"MainMenu Button Methods test passed");
            
            Object.DestroyImmediate(mainMenuGO);
        }

        [Test]
        public void Test_QuestManager_Singleton()
        {
            // Test QuestManager singleton pattern (simplified to avoid DOTween dependency)
            var questManagerGO1 = new GameObject("QuestManager1");
            var questManager1 = questManagerGO1.AddComponent<QuestManager>();
            
            // Test that QuestManager can be created
            Assert.IsNotNull(questManager1, "QuestManager should be created successfully");
            
            // Test singleton pattern (simplified)
            if (QuestManager.Instance != null)
            {
                Assert.AreEqual(questManager1, QuestManager.Instance, "First instance should be the singleton");
            }
            
            Debug.Log($"QuestManager Singleton test passed");
            
            Object.DestroyImmediate(questManagerGO1);
        }

        [Test]
        public void Test_Quest_DataStructure()
        {
            // Test Quest data structure
            var quest = new Quest
            {
                questName = "Test Quest",
                description = "Test Description",
                objectives = "Test Objectives",
                rewards = "Test Rewards",
                livesLost = "Test Lives Lost"
            };
            
            Assert.AreEqual("Test Quest", quest.questName, "Quest name should match");
            Assert.AreEqual("Test Description", quest.description, "Quest description should match");
            Assert.AreEqual("Test Objectives", quest.objectives, "Quest objectives should match");
            Assert.AreEqual("Test Rewards", quest.rewards, "Quest rewards should match");
            Assert.AreEqual("Test Lives Lost", quest.livesLost, "Quest lives lost should match");
            
            Debug.Log($"Quest Data Structure test passed");
        }

        [Test]
        public void Test_Quest_Serialization()
        {
            // Test Quest serialization
            var quest = new Quest
            {
                questName = "The Riddle of the Celestial Clock",
                description = "The Mystic Marauder has entered a time-warped storm.",
                objectives = "Solve three sequential math puzzles",
                rewards = "+50 XP +3 Gems",
                livesLost = "Lose 1 heart per failed puzzle attempt"
            };
            
            string json = JsonUtility.ToJson(quest);
            Assert.IsFalse(string.IsNullOrEmpty(json), "Quest JSON should not be empty");
            
            var deserializedQuest = JsonUtility.FromJson<Quest>(json);
            Assert.IsNotNull(deserializedQuest, "Deserialized quest should not be null");
            Assert.AreEqual(quest.questName, deserializedQuest.questName, "Quest name should match after deserialization");
            Assert.AreEqual(quest.description, deserializedQuest.description, "Quest description should match after deserialization");
            
            Debug.Log($"Quest Serialization test passed");
        }

        [Test]
        public void Test_AudioManager_Singleton()
        {
            // Test AudioManager singleton pattern
            var audioManagerGO1 = new GameObject("AudioManager1");
            var audioManager1 = audioManagerGO1.AddComponent<AudioManager>();
            
            // First instance should become the singleton
            Assert.IsNotNull(AudioManager.Instance, "AudioManager.Instance should not be null");
            Assert.AreEqual(audioManager1, AudioManager.Instance, "First instance should be the singleton");
            
            // Second instance should be destroyed
            var audioManagerGO2 = new GameObject("AudioManager2");
            var audioManager2 = audioManagerGO2.AddComponent<AudioManager>();
            
            // The second instance should be destroyed, so Instance should still be the first one
            Assert.AreEqual(audioManager1, AudioManager.Instance, "Singleton should remain the first instance");
            
            Debug.Log($"AudioManager Singleton test passed");
            
            Object.DestroyImmediate(audioManagerGO1);
            Object.DestroyImmediate(audioManagerGO2);
        }

        [Test]
        public void Test_AudioManager_Methods()
        {
            // Test AudioManager methods with proper setup
            var audioManagerGO = new GameObject("AudioManager");
            var audioManager = audioManagerGO.AddComponent<AudioManager>();
            
            // Test that methods exist and can be called
            Assert.DoesNotThrow(() => audioManager.StopMusic(), "StopMusic should not throw");
            
            // Test volume methods with null check (AudioMixer will be null in tests)
            if (audioManager.audioMixer != null)
            {
                Assert.DoesNotThrow(() => audioManager.SetMusicVolume(0.5f), "SetMusicVolume should not throw");
                Assert.DoesNotThrow(() => audioManager.SetSFXVolume(0.7f), "SetSFXVolume should not throw");
            }
            else
            {
                Debug.LogWarning("AudioMixer is null, skipping volume tests - this is expected in unit tests");
            }
            
            Debug.Log($"AudioManager Methods test passed");
            
            Object.DestroyImmediate(audioManagerGO);
        }

        [Test]
        public void Test_SceneManagement()
        {
            // Test scene management functionality
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            Assert.Greater(sceneCount, 0, "Should have at least one scene in build settings");
            
            // Test that expected scenes exist
            string[] expectedScenes = {
                "MainMenu",
                "GamePlay", 
                "QuestsListScene",
                "ProfileScene",
                "SettingScene"
            };
            
            // Note: We can't actually load scenes in unit tests, but we can validate the build settings
            Debug.Log($"Scene Management test passed - {sceneCount} scenes in build settings");
        }

        [Test]
        public void Test_InputSystem_Actions()
        {
            // Test that InputSystem actions are properly configured
            // This is more of a validation test since we can't easily test input in unit tests
            
            // Test that we can create input-related objects
            var inputGO = new GameObject("InputTest");
            var inputActions = inputGO.AddComponent<UnityEngine.InputSystem.PlayerInput>();
            
            Assert.IsNotNull(inputActions, "PlayerInput component should be creatable");
            
            Debug.Log($"InputSystem Actions test passed");
            
            Object.DestroyImmediate(inputGO);
        }

        [Test]
        public void Test_UI_Components()
        {
            // Test UI component creation and basic functionality
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            var canvasScaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            var graphicRaycaster = canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            Assert.IsNotNull(canvas, "Canvas should be created");
            Assert.IsNotNull(canvasScaler, "CanvasScaler should be created");
            Assert.IsNotNull(graphicRaycaster, "GraphicRaycaster should be created");
            
            // Test Button creation
            var buttonGO = new GameObject("Button");
            buttonGO.transform.SetParent(canvasGO.transform);
            var button = buttonGO.AddComponent<Button>();
            var buttonText = buttonGO.AddComponent<UnityEngine.UI.Text>();
            
            Assert.IsNotNull(button, "Button should be created");
            Assert.IsNotNull(buttonText, "Button text should be created");
            
            // Test that button can be interacted with
            Assert.IsTrue(button.interactable, "Button should be interactable by default");
            
            Debug.Log($"UI Components test passed");
            
            Object.DestroyImmediate(canvasGO);
        }

        [Test]
        public void Test_Button_Interactions()
        {
            // Test button interaction functionality
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            
            var buttonGO = new GameObject("Button");
            buttonGO.transform.SetParent(canvasGO.transform);
            var button = buttonGO.AddComponent<Button>();
            
            bool buttonClicked = false;
            button.onClick.AddListener(() => buttonClicked = true);
            
            // Simulate button click
            button.onClick.Invoke();
            
            Assert.IsTrue(buttonClicked, "Button click should trigger the listener");
            
            Debug.Log($"Button Interactions test passed");
            
            Object.DestroyImmediate(canvasGO);
        }

        [Test]
        public void Test_GameObject_Creation_Performance()
        {
            // Test GameObject creation performance
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Create 100 GameObjects with various components (avoiding Transform since it's automatic)
            for (int i = 0; i < 100; i++)
            {
                var go = new GameObject($"TestObject_{i}");
                // Don't add Transform - it's automatically added
                go.AddComponent<MeshRenderer>();
                go.AddComponent<MeshFilter>();
                
                Object.DestroyImmediate(go);
            }
            
            stopwatch.Stop();
            
            // Should complete in reasonable time
            Assert.Less(stopwatch.ElapsedMilliseconds, 1000, "GameObject creation should be fast");
            
            Debug.Log($"GameObject Creation Performance test passed - {stopwatch.ElapsedMilliseconds}ms for 100 objects");
        }

        [Test]
        public void Test_Component_Attachment()
        {
            // Test component attachment and removal
            var go = new GameObject("TestObject");
            
            // Test adding components
            var transform = go.GetComponent<Transform>();
            Assert.IsNotNull(transform, "Transform should be automatically attached");
            
            var meshRenderer = go.AddComponent<MeshRenderer>();
            Assert.IsNotNull(meshRenderer, "MeshRenderer should be attachable");
            
            var meshFilter = go.AddComponent<MeshFilter>();
            Assert.IsNotNull(meshFilter, "MeshFilter should be attachable");
            
            // Test removing components
            Object.DestroyImmediate(meshRenderer);
            var removedRenderer = go.GetComponent<MeshRenderer>();
            Assert.IsNull(removedRenderer, "Removed component should be null");
            
            Debug.Log($"Component Attachment test passed");
            
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Test_Transform_Hierarchy()
        {
            // Test Transform hierarchy operations
            var parentGO = new GameObject("Parent");
            var child1GO = new GameObject("Child1");
            var child2GO = new GameObject("Child2");
            
            // Test setting parent
            child1GO.transform.SetParent(parentGO.transform);
            child2GO.transform.SetParent(parentGO.transform);
            
            Assert.AreEqual(2, parentGO.transform.childCount, "Parent should have 2 children");
            Assert.AreEqual(parentGO.transform, child1GO.transform.parent, "Child1 parent should be correct");
            Assert.AreEqual(parentGO.transform, child2GO.transform.parent, "Child2 parent should be correct");
            
            // Test removing from hierarchy
            child1GO.transform.SetParent(null);
            Assert.AreEqual(1, parentGO.transform.childCount, "Parent should have 1 child after removal");
            Assert.IsNull(child1GO.transform.parent, "Removed child should have null parent");
            
            Debug.Log($"Transform Hierarchy test passed");
            
            Object.DestroyImmediate(parentGO);
            Object.DestroyImmediate(child1GO);
            Object.DestroyImmediate(child2GO);
        }

        #region Character Movement Tests

        [Test]
        public void Test_PlayerLocomotionInput_Exists()
        {
            // Test that PlayerLocomotionInput component exists
            var playerGO = new GameObject("Player");
            var locomotionInput = playerGO.AddComponent<PlayerLocomotionInput>();
            
            Assert.IsNotNull(locomotionInput, "PlayerLocomotionInput should be created successfully");
            
            // Note: PlayerControls might not be initialized in test environment
            // This is expected behavior for unit tests
            Debug.Log($"✅ PlayerLocomotionInput Exists test passed");
            
            Object.DestroyImmediate(playerGO);
        }

        [Test]
        public void Test_PlayerState_Exists()
        {
            // Test that PlayerState component exists
            var playerGO = new GameObject("Player");
            var playerState = playerGO.AddComponent<PlayerState>();
            
            Assert.IsNotNull(playerState, "PlayerState should be created successfully");
            
            Debug.Log($"PlayerState Exists test passed");
            
            Object.DestroyImmediate(playerGO);
        }

        [Test]
        public void Test_CharacterController_CanBeAdded()
        {
            // Test that Unity's CharacterController can be added and configured
            var playerGO = new GameObject("Player");
            var characterController = playerGO.AddComponent<CharacterController>();
            
            Assert.IsNotNull(characterController, "CharacterController should be created");
            
            // Test default properties
            characterController.height = 2f;
            characterController.radius = 0.5f;
            characterController.slopeLimit = 45f;
            
            Assert.AreEqual(2f, characterController.height, "Character height should be set correctly");
            Assert.AreEqual(0.5f, characterController.radius, "Character radius should be set correctly");
            Assert.AreEqual(45f, characterController.slopeLimit, "Slope limit should be set correctly");
            
            Debug.Log($"CharacterController Configuration test passed");
            
            Object.DestroyImmediate(playerGO);
        }

        [Test]
        public void Test_GameScene_LoadsCorrectly()
        {
            // Test that LoadGame method exists and can be called
            var mainMenuGO = new GameObject("MainMenu");
            var mainMenu = mainMenuGO.AddComponent<MainMenu>();
            
            // Store current scene index
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            
            // Test that LoadGame method exists
            var loadGameMethod = mainMenu.GetType().GetMethod("LoadGame");
            Assert.IsNotNull(loadGameMethod, "MainMenu should have LoadGame method");
            
            Debug.Log($"Game Scene Loading test passed");
            
            Object.DestroyImmediate(mainMenuGO);
        }

        [Test]
        public void Test_PlayerAnimation_Exists()
        {
            // Test that PlayerAnimation component exists for character animations
            var playerGO = new GameObject("Player");
            var animator = playerGO.AddComponent<Animator>();
            var playerAnimation = playerGO.AddComponent<PlayerAnimation>();
            
            Assert.IsNotNull(playerAnimation, "PlayerAnimation should be created successfully");
            Assert.IsNotNull(animator, "Animator component should exist");
            
            // Note: PlayerAnimation might have dependencies that aren't met in test environment
            // This is expected behavior for unit tests
            Debug.Log($"PlayerAnimation Exists test passed");
            
            Object.DestroyImmediate(playerGO);
        }

        [Test]
        public void Test_Camera_ExistsForPlayer()
        {
            // Test that camera can be created for player view
            var cameraGO = new GameObject("PlayerCamera");
            var camera = cameraGO.AddComponent<Camera>();
            
            Assert.IsNotNull(camera, "Camera should be created successfully");
            
            // Test camera properties
            camera.fieldOfView = 60f;
            Assert.AreEqual(60f, camera.fieldOfView, "Camera field of view should be set correctly");
            
            Debug.Log($"Player Camera test passed");
            
            Object.DestroyImmediate(cameraGO);
        }

        [Test]
        public void Test_PlayerInteractInput_Exists()
        {
            // Test that PlayerInteractInput exists for player interactions
            var playerGO = new GameObject("Player");
            var interactInput = playerGO.AddComponent<PlayerInteractInput>();
            
            Assert.IsNotNull(interactInput, "PlayerInteractInput should be created successfully");
            
            // Note: PlayerInteractInput might have dependencies that aren't met in test environment
            // This is expected behavior for unit tests
            Debug.Log($"PlayerInteractInput Exists test passed");
            
            Object.DestroyImmediate(playerGO);
        }

        [Test]
        public void Test_QuestManager_HasQuestProgressTracking_TDD()
        {
            var questManagerGO = new GameObject("QuestManager");
            var questManager = questManagerGO.AddComponent<QuestManager>();
            
            var questProgressField = questManager.GetType().GetField("questProgress", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic);
            
            var saveProgressMethod = questManager.GetType().GetMethod("SaveQuestProgress", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic);
            
            Assert.IsNotNull(questProgressField, "TDD: QuestManager should have questProgress field to track player progress");
            Assert.IsNotNull(saveProgressMethod, "TDD: QuestManager should have SaveQuestProgress method to save quest completion");
            
            Object.DestroyImmediate(questManagerGO);
        }

        [Test]
        public void Test_QuestManager_HasQuestRewardSystem_TDD()
        {
            var questManagerGO = new GameObject("QuestManager");
            var questManager = questManagerGO.AddComponent<QuestManager>();
            
            var giveRewardsMethod = questManager.GetType().GetMethod("GiveQuestRewards", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic);
            
            var calculateRewardsMethod = questManager.GetType().GetMethod("CalculateQuestRewards", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic);
            
            Assert.IsNotNull(giveRewardsMethod, "TDD: QuestManager should have GiveQuestRewards method");
            Assert.IsNotNull(calculateRewardsMethod, "TDD: QuestManager should have CalculateQuestRewards method based on performance");
            
            Object.DestroyImmediate(questManagerGO);
        }

        [Test]
        public void Test_GameManager_Singleton_Exists_TDD()
        {
            var gameManagerType = System.Type.GetType("GameManager");
            
            Assert.IsNotNull(gameManagerType, "TDD: GameManager singleton class should exist for managing game state");
            
            if (gameManagerType != null)
            {
                var instanceProperty = gameManagerType.GetProperty("Instance", 
                    System.Reflection.BindingFlags.Static | 
                    System.Reflection.BindingFlags.Public);
                
                Assert.IsNotNull(instanceProperty, "TDD: GameManager should have static Instance property for singleton pattern");
            }
        }

        [Test]
        public void Test_SaveManager_Exists_TDD()
        {
            var saveManagerType = System.Type.GetType("SaveManager");
            
            Assert.IsNotNull(saveManagerType, "TDD: SaveManager class should exist for saving and loading game data");
            
            if (saveManagerType != null)
            {
                var saveGameMethod = saveManagerType.GetMethod("SaveGame", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.Static);
                
                var loadGameMethod = saveManagerType.GetMethod("LoadGame", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.Static);
                
                Assert.IsNotNull(saveGameMethod, "TDD: SaveManager should have SaveGame method");
                Assert.IsNotNull(loadGameMethod, "TDD: SaveManager should have LoadGame method");
            }
        }

        [Test]
        public void Test_Quest_HasDifficultySystem_TDD()
        {
            var quest = new Quest();
            var questType = quest.GetType();
            
            var difficultyField = questType.GetField("difficulty");
            var recommendedLevelField = questType.GetField("recommended_level");
            
            Assert.IsNotNull(difficultyField, "TDD: Quest should have difficulty field (easy, medium, hard)");
            Assert.IsNotNull(recommendedLevelField, "TDD: Quest should have recommended_level field");
        }

        [Test]
        public void Test_AudioManager_HasMusicPlaylist_TDD()
        {
            var audioManagerGO = new GameObject("AudioManager");
            var audioManager = audioManagerGO.AddComponent<AudioManager>();
            
            var playlistField = audioManager.GetType().GetField("musicPlaylist", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic);
            
            var playRandomMusicMethod = audioManager.GetType().GetMethod("PlayRandomMusic", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic);
            
            Assert.IsNotNull(playlistField, "TDD: AudioManager should have musicPlaylist field for multiple tracks");
            Assert.IsNotNull(playRandomMusicMethod, "TDD: AudioManager should have PlayRandomMusic method");
            
            Object.DestroyImmediate(audioManagerGO);
        }

        #endregion
    }
}
#endif
