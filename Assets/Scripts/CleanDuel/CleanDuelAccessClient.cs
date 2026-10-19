using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace IcarusGame.CleanDuel
{
    public static class CleanDuelAccessClient
    {
        public static IEnumerator Check(
            string endpoint,
            string user,
            string password,
            string gameId,
            string environment,
            Action<CleanDuelAccessResult> completed)
        {
            // De coroutine moet eerst starten voordat een lokale validatiefout terugkomt.
            yield return null;

            Uri endpointUri;
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out endpointUri) || endpointUri.Scheme != Uri.UriSchemeHttps)
            {
                completed(new CleanDuelAccessResult
                {
                    Success = false,
                    Message = "Inloggen vereist een beveiligde HTTPS-verbinding met de gameserver."
                });
                yield break;
            }

            string normalizedPassword = (password ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(user) || normalizedPassword.Length < 12 || normalizedPassword.Length > 128)
            {
                completed(new CleanDuelAccessResult
                {
                    Success = false,
                    Message = "Vul je e-mailadres of gebruikers-ID en een wachtwoord van 12 tot 128 tekens in."
                });
                yield break;
            }

            CleanDuelAccessRequest payload = new CleanDuelAccessRequest
            {
                user = user.Trim(),
                // Dit veld blijft gelijk voor compatibiliteit met de bestaande gameserver en accounts.
                accessCode = normalizedPassword,
                gameId = gameId,
                environment = environment,
                clientVersion = Application.version
            };
            byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload));

            using (UnityWebRequest request = new UnityWebRequest(endpoint, UnityWebRequest.kHttpVerbPOST))
            {
                request.uploadHandler = new UploadHandlerRaw(body);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = 15;
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");
                request.SetRequestHeader("X-Request-Id", Guid.NewGuid().ToString("N"));
                yield return request.SendWebRequest();

                string responseText = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
                CleanDuelAccessResponse response = ParseResponse(responseText);
                if (request.result != UnityWebRequest.Result.Success)
                {
                    completed(new CleanDuelAccessResult
                    {
                        Success = false,
                        Response = response,
                        Message = ErrorMessage(request, response)
                    });
                    yield break;
                }

                if (response == null)
                {
                    completed(new CleanDuelAccessResult
                    {
                        Success = false,
                        Message = "De gameserver stuurde een onleesbaar antwoord."
                    });
                    yield break;
                }

                completed(new CleanDuelAccessResult
                {
                    Success = true,
                    Response = response,
                    Message = string.Empty
                });
            }
        }

        private static CleanDuelAccessResponse ParseResponse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonUtility.FromJson<CleanDuelAccessResponse>(json);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static string ErrorMessage(UnityWebRequest request, CleanDuelAccessResponse response)
        {
            if (response != null)
            {
                if (!string.IsNullOrWhiteSpace(response.error)) return response.error;
                if (response.decision != null && !string.IsNullOrWhiteSpace(response.decision.reason)) return response.decision.reason;
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                return "De gameserver is niet bereikbaar. Controleer je internetverbinding en probeer opnieuw.";
            }

            return request.responseCode >= 500
                ? "De gameserver heeft tijdelijk een probleem. Probeer het zo opnieuw."
                : "De toegangscontrole is mislukt.";
        }
    }
}
