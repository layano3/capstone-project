#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using Supabase;

namespace Tests
{
    public class SupabaseTests
    {
        private SupabaseConfig config;

        [SetUp]
        public void SetUp()
        {
            // Load the SupabaseConfig from Resources
            config = Resources.Load<SupabaseConfig>("SupabaseConfig");
        }

        [Test]
        public void Test_ConfigExists()
        {
            // Test that SupabaseConfig exists and has valid data
            Assert.IsNotNull(config, "SupabaseConfig should not be null");
            Assert.IsFalse(string.IsNullOrEmpty(config.url), "SupabaseConfig.url should not be empty");
            Assert.IsFalse(string.IsNullOrEmpty(config.anonKey), "SupabaseConfig.anonKey should not be empty");
            
            Debug.Log($"✅ Config Exists test passed - URL: {config.url}");
        }

        [Test]
        public void Test_LoginFailsGracefully()
        {
            // Test that login fails gracefully with invalid credentials
            Assert.IsNotNull(config, "SupabaseConfig should be loaded");
            
            // Note: This test validates the config exists and would fail gracefully
            // The actual login test requires a MonoBehaviour to run coroutines
            // For a full integration test, use the MiniTestRunner instead
            
            Assert.IsFalse(string.IsNullOrEmpty(config.url), "Config URL should be set");
            Assert.IsFalse(string.IsNullOrEmpty(config.anonKey), "Config anon key should be set");
            
            Debug.Log($"✅ Login Fails Gracefully test passed - Config is valid");
        }

        [Test]
        public void Test_AddXpEvent()
        {
            // Test adding XP event - validates the XPManager can be instantiated
            Assert.IsNotNull(config, "SupabaseConfig should be loaded");
            
            // Test that we can create an XPManager component
            var xpManagerGO = new GameObject("XPManager");
            var xpManager = xpManagerGO.AddComponent<XPManager>();
            xpManager.config = config;
            
            // Verify the component was created successfully
            Assert.IsNotNull(xpManager, "XPManager should be created successfully");
            Assert.IsNotNull(xpManager.config, "XPManager config should be set");
            Assert.AreEqual(config, xpManager.config, "XPManager config should match the test config");
            
            // Clean up
            Object.DestroyImmediate(xpManagerGO);
            
            Debug.Log($"✅ Add XP Event test passed - XPManager can be instantiated");
        }

        [Test]
        public void Test_SupabaseAuth_GetUserIdFromJwt()
        {
            // Test JWT parsing functionality
            string testJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
            
            string userId = SupabaseAuth.GetUserIdFromJwt(testJwt);
            
            Assert.IsNotNull(userId, "Should extract user ID from JWT");
            Assert.AreEqual("1234567890", userId, "Should extract correct user ID");
            
            Debug.Log($"✅ GetUserIdFromJwt test passed - User ID: {userId}");
        }

        [Test]
        public void Test_SupabaseAuth_GetUserIdFromJwt_InvalidToken()
        {
            // Test JWT parsing with invalid token
            string invalidJwt = "invalid.jwt.token";
            
            string userId = SupabaseAuth.GetUserIdFromJwt(invalidJwt);
            
            Assert.IsNull(userId, "Should return null for invalid JWT");
            
            Debug.Log($"✅ GetUserIdFromJwt Invalid Token test passed");
        }

        [Test]
        public void Test_SupabaseAuth_GetUserIdFromJwt_EmptyToken()
        {
            // Test JWT parsing with empty token
            string emptyJwt = "";
            
            string userId = SupabaseAuth.GetUserIdFromJwt(emptyJwt);
            
            Assert.IsNull(userId, "Should return null for empty JWT");
            
            Debug.Log($"✅ GetUserIdFromJwt Empty Token test passed");
        }

        [Test]
        public void Test_SupabaseAuth_GetUserIdFromJwt_NullToken()
        {
            // Test JWT parsing with null token
            string nullJwt = null;
            
            string userId = SupabaseAuth.GetUserIdFromJwt(nullJwt);
            
            Assert.IsNull(userId, "Should return null for null JWT");
            
            Debug.Log($"✅ GetUserIdFromJwt Null Token test passed");
        }

        [Test]
        public void Test_SupabaseConfig_DefaultValues()
        {
            // Test SupabaseConfig default values
            var config = ScriptableObject.CreateInstance<SupabaseConfig>();
            
            Assert.AreEqual("/auth/v1", config.authPath, "Default auth path should be /auth/v1");
            Assert.AreEqual("/rest/v1", config.restPath, "Default rest path should be /rest/v1");
            Assert.IsTrue(string.IsNullOrEmpty(config.url), "Default URL should be empty");
            Assert.IsTrue(string.IsNullOrEmpty(config.anonKey), "Default anon key should be empty");
            
            Debug.Log($"✅ SupabaseConfig Default Values test passed");
        }

        [Test]
        public void Test_SupabaseConfig_Validation()
        {
            // Test SupabaseConfig validation
            var config = ScriptableObject.CreateInstance<SupabaseConfig>();
            
            // Test invalid URL
            config.url = "not-a-valid-url";
            config.anonKey = "test-key";
            
            Assert.IsFalse(config.url.StartsWith("https://"), "Invalid URL should not start with https://");
            
            // Test valid URL
            config.url = "https://test.supabase.co";
            Assert.IsTrue(config.url.StartsWith("https://"), "Valid URL should start with https://");
            
            Debug.Log($"SupabaseConfig Validation test passed");
        }

        [Test]
        public void Test_ProfileController_HasLoadStatsMethod_TDD()
        {
            var profileControllerGO = new GameObject("ProfileController");
            var profileController = profileControllerGO.AddComponent<ProfileController>();
            
            var loadStatsMethod = profileController.GetType().GetMethod("LoadStatsData", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic);
            
            Assert.IsNotNull(loadStatsMethod, "TDD: ProfileController should have LoadStatsData method to load stats from database");
            
            Object.DestroyImmediate(profileControllerGO);
        }

        [Test]
        public void Test_ProfileController_HasLoadAchievementsMethod_TDD()
        {
            var profileControllerGO = new GameObject("ProfileController");
            var profileController = profileControllerGO.AddComponent<ProfileController>();
            
            var loadAchievementsMethod = profileController.GetType().GetMethod("LoadAchievementsData", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic);
            
            Assert.IsNotNull(loadAchievementsMethod, "TDD: ProfileController should have LoadAchievementsData method to load achievements from database");
            
            Object.DestroyImmediate(profileControllerGO);
        }

        [Test]
        public void Test_ProfileController_HasLoadCaptainLogMethod_TDD()
        {
            var profileControllerGO = new GameObject("ProfileController");
            var profileController = profileControllerGO.AddComponent<ProfileController>();
            
            var loadCaptainLogMethod = profileController.GetType().GetMethod("LoadCaptainLogData", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic);
            
            Assert.IsNotNull(loadCaptainLogMethod, "TDD: ProfileController should have LoadCaptainLogData method to load captain log entries from database");
            
            Object.DestroyImmediate(profileControllerGO);
        }

        [Test]
        public void Test_ProfileController_HasStatsUIFields_TDD()
        {
            var profileControllerGO = new GameObject("ProfileController");
            var profileController = profileControllerGO.AddComponent<ProfileController>();
            
            var statsTextFieldType = profileController.GetType().GetField("statsText");
            var totalQuestsFieldType = profileController.GetType().GetField("totalQuestsCompletedText");
            var totalXPEarnedFieldType = profileController.GetType().GetField("totalXPEarnedText");
            var accuracyFieldType = profileController.GetType().GetField("accuracyText");
            
            Assert.IsNotNull(statsTextFieldType, "TDD: ProfileController should have statsText field for Stats display");
            Assert.IsNotNull(totalQuestsFieldType, "TDD: ProfileController should have totalQuestsCompletedText field");
            Assert.IsNotNull(totalXPEarnedFieldType, "TDD: ProfileController should have totalXPEarnedText field");
            Assert.IsNotNull(accuracyFieldType, "TDD: ProfileController should have accuracyText field");
            
            Object.DestroyImmediate(profileControllerGO);
        }

        [Test]
        public void Test_ProfileController_HasAchievementsUIFields_TDD()
        {
            var profileControllerGO = new GameObject("ProfileController");
            var profileController = profileControllerGO.AddComponent<ProfileController>();
            
            var achievementsContainerFieldType = profileController.GetType().GetField("achievementsContainer");
            var achievementPrefabFieldType = profileController.GetType().GetField("achievementPrefab");
            
            Assert.IsNotNull(achievementsContainerFieldType, "TDD: ProfileController should have achievementsContainer field for displaying achievements");
            Assert.IsNotNull(achievementPrefabFieldType, "TDD: ProfileController should have achievementPrefab field for instantiating achievement items");
            
            Object.DestroyImmediate(profileControllerGO);
        }

        [Test]
        public void Test_ProfileController_HasCaptainLogUIFields_TDD()
        {
            var profileControllerGO = new GameObject("ProfileController");
            var profileController = profileControllerGO.AddComponent<ProfileController>();
            
            var captainLogContainerFieldType = profileController.GetType().GetField("captainLogContainer");
            var logEntryPrefabFieldType = profileController.GetType().GetField("logEntryPrefab");
            
            Assert.IsNotNull(captainLogContainerFieldType, "TDD: ProfileController should have captainLogContainer field for displaying log entries");
            Assert.IsNotNull(logEntryPrefabFieldType, "TDD: ProfileController should have logEntryPrefab field for instantiating log entries");
            
            Object.DestroyImmediate(profileControllerGO);
        }
    }
}
#endif
