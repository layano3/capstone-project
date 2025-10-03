using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[Serializable] public class AuthSession
{
    public string access_token;
    public string token_type;
    public int    expires_in;
    public string refresh_token;
    // "user" comes back too but we don't need it for login flow
}

public static class SupabaseAuth
{
    static string AuthUrl(SupabaseConfig cfg, string path) => $"{cfg.url}{cfg.authPath}{path}";

    // Coroutine SignIn
    public static IEnumerator SignIn(SupabaseConfig cfg, string email, string password,
                                     Action<AuthSession, string> done)
    {
        var payload = $"{{\"email\":\"{email}\",\"password\":\"{password}\"}}";
        using var req = new UnityWebRequest(AuthUrl(cfg, "/token?grant_type=password"), "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("apikey", cfg.anonKey);
        req.SetRequestHeader("Authorization", $"Bearer {cfg.anonKey}");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success || req.responseCode >= 400)
        {
            done?.Invoke(null, $"SignIn failed [{req.responseCode}]: {req.downloadHandler.text}");
            yield break;
        }

        var json = req.downloadHandler.text;
        var session = JsonUtility.FromJson<AuthSession>(json);
        done?.Invoke(session, null);
    }

    // Optional helper: decode user id (sub) from JWT
    [Serializable] class JwtPayload { public string sub; }
    public static string GetUserIdFromJwt(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2) return null;
            string payload = parts[1]
                .Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4) { case 2: payload += "=="; break; case 3: payload += "="; break; }
            var bytes = Convert.FromBase64String(payload);
            var json  = Encoding.UTF8.GetString(bytes);
            var data  = JsonUtility.FromJson<JwtPayload>(json);
            return data.sub;
        }
        catch { return null; }
    }
}
