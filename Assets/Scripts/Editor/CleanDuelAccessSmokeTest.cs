using System;
using System.Collections;
using System.Reflection;
using IcarusGame.CleanDuel;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace IcarusGame.EditorTools
{
    public static class CleanDuelAccessSmokeTest
    {
        [MenuItem("Tools/Icarus/Clean Duel/Test Password Login")]
        public static void Run()
        {
            GameObject root = new GameObject("Password Login Test");
            root.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                int submissions = 0;
                string submittedUser = null;
                string submittedPassword = null;
                CleanDuelAccessGateView view = new CleanDuelAccessGateView();
                view.Build(root.transform, "player@example.test", (user, password) =>
                {
                    submissions++;
                    submittedUser = user;
                    submittedPassword = password;
                }, "https://gameserver.rene007.nl/password-reset.html");

                InputField userInput = Field<InputField>(view, "userInput");
                InputField passwordInput = Field<InputField>(view, "passwordInput");
                Button submit = Field<Button>(view, "submitButton");
                Button visibility = Field<Button>(view, "passwordVisibilityButton");
                Button recovery = Field<Button>(view, "recoveryButton");

                Require(userInput.text == "player@example.test", "Remembered user was not restored.");
                Require(passwordInput.text.Length == 0, "Password must start empty.");
                Require(passwordInput.inputType == InputField.InputType.Password, "Password must be masked initially.");
                passwordInput.text = "short";
                submit.onClick.Invoke();
                Require(submissions == 0, "Short password must not be submitted.");

                userInput.text = " player@example.test ";
                passwordInput.text = "example-player-password";
                visibility.onClick.Invoke();
                Require(passwordInput.inputType == InputField.InputType.Standard, "Show password toggle failed.");
                visibility.onClick.Invoke();
                Require(passwordInput.inputType == InputField.InputType.Password, "Hide password toggle failed.");
                passwordInput.onSubmit.Invoke(passwordInput.text);
                Require(submissions == 1, "Enter must submit a valid login once.");
                Require(submittedUser == "player@example.test" && submittedPassword == "example-player-password", "Login credentials did not match the existing server contract.");
                Require(passwordInput.text.Length == 0, "Submitted password must be cleared from the input.");
                Require(!submit.interactable && !visibility.interactable && !recovery.interactable, "Busy login must disable controls.");
                passwordInput.text = "example-player-password";
                submit.onClick.Invoke();
                Require(submissions == 1, "Busy login must reject duplicate submissions.");

                view.ShowError("Test login rejected");
                Require(submit.interactable && passwordInput.text.Length == 0, "A rejected login must be retryable with an empty password.");
                Require(passwordInput.inputType == InputField.InputType.Password, "A rejected login must restore masking.");
                passwordInput.text = "example-player-password";
                submit.onClick.Invoke();
                Require(submissions == 2, "Retry after a rejected login failed.");

                AssertLocalRejection("http://gameserver.rene007.nl/api/game/v1/access/check", "player@example.test", "example-player-password");
                AssertLocalRejection("https://gameserver.rene007.nl/api/game/v1/access/check", "", "example-player-password");
                AssertLocalRejection("https://gameserver.rene007.nl/api/game/v1/access/check", "player@example.test", new string('x', 129));
                CleanDuelAccessSession session;
                string error;
                Require(!CleanDuelAccessSession.TryCreate(new CleanDuelAccessResponse
                {
                    decision = new CleanDuelAccessDecision { allowed = false },
                    user = new CleanDuelAccessUser { id = "player-1" }
                }, out session, out error), "A denied server response must never grant a game session.");
                Debug.Log("Password login smoke test passed: masking, show/hide, Enter, duplicate prevention, retry, HTTPS requirement and denied sessions.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void AssertLocalRejection(string endpoint, string user, string password)
        {
            CleanDuelAccessResult result = null;
            IEnumerator routine = CleanDuelAccessClient.Check(endpoint, user, password, "master-duel", "Production", value => result = value);
            int frames = 0;
            while (routine.MoveNext())
            {
                Require(routine.Current == null && ++frames < 3, "Invalid credentials must be rejected locally without a network request.");
            }
            Require(result != null && !result.Success, "Invalid login must return a failed result.");
        }

        private static T Field<T>(object instance, string name)
        {
            return (T)instance.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
