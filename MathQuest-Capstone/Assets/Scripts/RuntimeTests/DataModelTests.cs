using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using Supabase;

namespace Tests
{
    public class DataModelTests
    {
        [Test]
        public void Test_Student_Serialization()
        {
            // Test Student class serialization/deserialization
            var student = new Student
            {
                id = "test-id-123",
                name = "Test Student",
                xp = 100,
                xp_goal = 200,
                level = 5,
                behaviour = "on-track",
                last_active = "2024-01-01T00:00:00Z"
            };

            // Test JSON serialization
            string json = JsonUtility.ToJson(student);
            Assert.IsFalse(string.IsNullOrEmpty(json), "JSON should not be empty");
            
            // Test JSON deserialization
            var deserializedStudent = JsonUtility.FromJson<Student>(json);
            Assert.IsNotNull(deserializedStudent, "Deserialized student should not be null");
            Assert.AreEqual(student.id, deserializedStudent.id, "ID should match");
            Assert.AreEqual(student.name, deserializedStudent.name, "Name should match");
            Assert.AreEqual(student.xp, deserializedStudent.xp, "XP should match");
            Assert.AreEqual(student.xp_goal, deserializedStudent.xp_goal, "XP goal should match");
            Assert.AreEqual(student.level, deserializedStudent.level, "Level should match");
            Assert.AreEqual(student.behaviour, deserializedStudent.behaviour, "Behaviour should match");
            Assert.AreEqual(student.last_active, deserializedStudent.last_active, "Last active should match");
            
            Debug.Log($"✅ Student Serialization test passed");
        }

        [Test]
        public void Test_StudentData_Serialization()
        {
            // Test StudentData class serialization/deserialization
            var studentData = new StudentData
            {
                name = "Test Student Data",
                xp = 150,
                xp_goal = 300,
                level = 3
            };

            // Test JSON serialization
            string json = JsonUtility.ToJson(studentData);
            Assert.IsFalse(string.IsNullOrEmpty(json), "JSON should not be empty");
            
            // Test JSON deserialization
            var deserializedStudentData = JsonUtility.FromJson<StudentData>(json);
            Assert.IsNotNull(deserializedStudentData, "Deserialized student data should not be null");
            Assert.AreEqual(studentData.name, deserializedStudentData.name, "Name should match");
            Assert.AreEqual(studentData.xp, deserializedStudentData.xp, "XP should match");
            Assert.AreEqual(studentData.xp_goal, deserializedStudentData.xp_goal, "XP goal should match");
            Assert.AreEqual(studentData.level, deserializedStudentData.level, "Level should match");
            
            Debug.Log($"✅ StudentData Serialization test passed");
        }

        [Test]
        public void Test_SupabaseConfig_Properties()
        {
            // Test SupabaseConfig properties
            var config = ScriptableObject.CreateInstance<SupabaseConfig>();
            
            // Test default values
            Assert.AreEqual("/auth/v1", config.authPath, "Default auth path should be /auth/v1");
            Assert.AreEqual("/rest/v1", config.restPath, "Default rest path should be /rest/v1");
            
            // Test setting values
            config.url = "https://test.supabase.co";
            config.anonKey = "test-anon-key";
            
            Assert.AreEqual("https://test.supabase.co", config.url, "URL should be set correctly");
            Assert.AreEqual("test-anon-key", config.anonKey, "Anon key should be set correctly");
            
            Debug.Log($"✅ SupabaseConfig Properties test passed");
        }

        [Test]
        public void Test_Student_EdgeCases()
        {
            // Test Student class with edge cases
            var student = new Student
            {
                id = "", // Empty ID
                name = null, // Null name
                xp = -1, // Negative XP
                xp_goal = 0, // Zero XP goal
                level = 0, // Zero level
                behaviour = "", // Empty behaviour
                last_active = null // Null last active
            };

            // Test JSON serialization with edge cases
            string json = JsonUtility.ToJson(student);
            Assert.IsFalse(string.IsNullOrEmpty(json), "JSON should not be empty even with edge cases");
            
            // Test JSON deserialization
            var deserializedStudent = JsonUtility.FromJson<Student>(json);
            Assert.IsNotNull(deserializedStudent, "Deserialized student should not be null");
            Assert.AreEqual(student.xp, deserializedStudent.xp, "Negative XP should be preserved");
            Assert.AreEqual(student.xp_goal, deserializedStudent.xp_goal, "Zero XP goal should be preserved");
            Assert.AreEqual(student.level, deserializedStudent.level, "Zero level should be preserved");
            
            Debug.Log($"✅ Student Edge Cases test passed");
        }

        [Test]
        public void Test_StudentData_EdgeCases()
        {
            // Test StudentData class with edge cases
            var studentData = new StudentData
            {
                name = "", // Empty name
                xp = int.MinValue, // Minimum int value
                xp_goal = int.MaxValue, // Maximum int value
                level = 0 // Zero level
            };

            // Test JSON serialization with edge cases
            string json = JsonUtility.ToJson(studentData);
            Assert.IsFalse(string.IsNullOrEmpty(json), "JSON should not be empty even with edge cases");
            
            // Test JSON deserialization
            var deserializedStudentData = JsonUtility.FromJson<StudentData>(json);
            Assert.IsNotNull(deserializedStudentData, "Deserialized student data should not be null");
            Assert.AreEqual(studentData.xp, deserializedStudentData.xp, "Minimum int XP should be preserved");
            Assert.AreEqual(studentData.xp_goal, deserializedStudentData.xp_goal, "Maximum int XP goal should be preserved");
            Assert.AreEqual(studentData.level, deserializedStudentData.level, "Zero level should be preserved");
            
            Debug.Log($"✅ StudentData Edge Cases test passed");
        }

        [Test]
        public void Test_Student_LevelCalculation()
        {
            // Test level calculation logic
            var student = new Student
            {
                xp = 100,
                xp_goal = 200,
                level = 1
            };

            // Test XP progress calculation
            float progress = (float)student.xp / student.xp_goal;
            Assert.AreEqual(0.5f, progress, "XP progress should be 50%");
            
            // Test if student needs more XP
            bool needsMoreXp = student.xp < student.xp_goal;
            Assert.IsTrue(needsMoreXp, "Student should need more XP");
            
            // Test level up scenario
            student.xp = student.xp_goal;
            progress = (float)student.xp / student.xp_goal;
            Assert.AreEqual(1.0f, progress, "XP progress should be 100% when at goal");
            
            Debug.Log($"Student Level Calculation test passed");
        }

        [Test]
        public void Test_StatsData_Model_Exists_TDD()
        {
            var statsDataType = System.Type.GetType("StatsData");
            
            Assert.IsNotNull(statsDataType, "TDD: StatsData class should exist to represent stats table from database");
            
            if (statsDataType != null)
            {
                Assert.IsNotNull(statsDataType.GetField("total_quests_completed"), "TDD: StatsData should have total_quests_completed field");
                Assert.IsNotNull(statsDataType.GetField("total_xp_earned"), "TDD: StatsData should have total_xp_earned field");
                Assert.IsNotNull(statsDataType.GetField("accuracy_percentage"), "TDD: StatsData should have accuracy_percentage field");
                Assert.IsNotNull(statsDataType.GetField("student_id"), "TDD: StatsData should have student_id field");
                Assert.IsNotNull(statsDataType.GetField("time_played"), "TDD: StatsData should have time_played field");
            }
        }

        [Test]
        public void Test_AchievementData_Model_Exists_TDD()
        {
            var achievementDataType = System.Type.GetType("AchievementData");
            
            Assert.IsNotNull(achievementDataType, "TDD: AchievementData class should exist to represent achievements table");
            
            if (achievementDataType != null)
            {
                Assert.IsNotNull(achievementDataType.GetField("id"), "TDD: AchievementData should have id field");
                Assert.IsNotNull(achievementDataType.GetField("title"), "TDD: AchievementData should have title field");
                Assert.IsNotNull(achievementDataType.GetField("description"), "TDD: AchievementData should have description field");
                Assert.IsNotNull(achievementDataType.GetField("icon_url"), "TDD: AchievementData should have icon_url field");
                Assert.IsNotNull(achievementDataType.GetField("date_earned"), "TDD: AchievementData should have date_earned field");
                Assert.IsNotNull(achievementDataType.GetField("student_id"), "TDD: AchievementData should have student_id field");
            }
        }

        [Test]
        public void Test_CaptainLogData_Model_Exists_TDD()
        {
            var captainLogDataType = System.Type.GetType("CaptainLogData");
            
            Assert.IsNotNull(captainLogDataType, "TDD: CaptainLogData class should exist to represent captain_log table");
            
            if (captainLogDataType != null)
            {
                Assert.IsNotNull(captainLogDataType.GetField("id"), "TDD: CaptainLogData should have id field");
                Assert.IsNotNull(captainLogDataType.GetField("entry_date"), "TDD: CaptainLogData should have entry_date field");
                Assert.IsNotNull(captainLogDataType.GetField("entry_text"), "TDD: CaptainLogData should have entry_text field");
                Assert.IsNotNull(captainLogDataType.GetField("quest_id"), "TDD: CaptainLogData should have quest_id field");
                Assert.IsNotNull(captainLogDataType.GetField("student_id"), "TDD: CaptainLogData should have student_id field");
            }
        }

        [Test]
        public void Test_StudentData_HasExtendedFields_TDD()
        {
            var studentData = new StudentData();
            var studentDataType = studentData.GetType();
            
            var behaviourField = studentDataType.GetField("behaviour");
            var lastActiveField = studentDataType.GetField("last_active");
            var streakField = studentDataType.GetField("current_streak");
            
            Assert.IsNotNull(behaviourField, "TDD: StudentData should have behaviour field for tracking student status");
            Assert.IsNotNull(lastActiveField, "TDD: StudentData should have last_active field to track last login");
            Assert.IsNotNull(streakField, "TDD: StudentData should have current_streak field to track daily login streak");
        }

        [Test]
        public void Test_QuestProgressData_Model_Exists_TDD()
        {
            var questProgressType = System.Type.GetType("QuestProgressData");
            
            Assert.IsNotNull(questProgressType, "TDD: QuestProgressData class should exist to track quest completion");
            
            if (questProgressType != null)
            {
                Assert.IsNotNull(questProgressType.GetField("quest_id"), "TDD: QuestProgressData should have quest_id field");
                Assert.IsNotNull(questProgressType.GetField("student_id"), "TDD: QuestProgressData should have student_id field");
                Assert.IsNotNull(questProgressType.GetField("is_completed"), "TDD: QuestProgressData should have is_completed field");
                Assert.IsNotNull(questProgressType.GetField("completion_date"), "TDD: QuestProgressData should have completion_date field");
                Assert.IsNotNull(questProgressType.GetField("score"), "TDD: QuestProgressData should have score field");
                Assert.IsNotNull(questProgressType.GetField("attempts"), "TDD: QuestProgressData should have attempts field");
            }
        }
    }
}
