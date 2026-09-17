using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

public class APIClient : MonoBehaviour
{
    public enum LLMProvider
    {
        OpenAI,
        Gemini
    }

    [Header("Provider Settings")]
    [Tooltip("Choose which AI provider to use")]
    public LLMProvider provider = LLMProvider.OpenAI;

    [Header("API Settings")]
    [Tooltip("Enter your API Key here. Do not expose this in public builds!")]
    public string apiKey = "";
    
    [Tooltip("The API Endpoint URL.\nOpenAI: https://api.openai.com/v1/chat/completions\nGemini: https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash")]
    public string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash";
    
    [Tooltip("The model to use, e.g., gpt-3.5-turbo (OpenAI). Not strictly needed for Gemini if included in URL.")]
    public string model = "gpt-3.5-turbo";
    
    [TextArea(3, 10)]
    public string systemPrompt = "You are a helpful assistant for a game.";

    /// <summary>
    /// Sends a message to the LLM API and returns the response via callback.
    /// </summary>
    public void SendMessageToAPI(string userMessage, Action<string> onSuccess, Action<string> onError)
    {
        if (provider == LLMProvider.OpenAI)
        {
            StartCoroutine(PostRequestOpenAI(userMessage, onSuccess, onError));
        }
        else if (provider == LLMProvider.Gemini)
        {
            StartCoroutine(PostRequestGemini(userMessage, onSuccess, onError));
        }
    }

    private IEnumerator PostRequestOpenAI(string userMessage, Action<string> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            onError?.Invoke("API Key is missing. Please set it in the APIClient component.");
            yield break;
        }

        var requestData = new
        {
            model = this.model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            }
        };

        string jsonData = JsonConvert.SerializeObject(requestData);
        byte[] postData = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                string errorMsg = $"Error {request.responseCode}: {request.error}\n{request.downloadHandler.text}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                try
                {
                    var parsedData = JsonConvert.DeserializeObject<OpenAIResponse>(jsonResponse);
                    if (parsedData != null && parsedData.choices != null && parsedData.choices.Count > 0)
                    {
                        string botReply = parsedData.choices[0].message.content;
                        onSuccess?.Invoke(botReply);
                    }
                    else
                    {
                        onError?.Invoke("Failed to parse response from OpenAI API.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("JSON Parse Error: " + e.Message);
                    onError?.Invoke("Error parsing OpenAI API response.");
                }
            }
        }
    }

    private IEnumerator PostRequestGemini(string userMessage, Action<string> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            onError?.Invoke("API Key is missing. Please set it in the APIClient component.");
            yield break;
        }

        // Build the request body for Gemini format
        var requestData = new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            contents = new[]
            {
                new { 
                    role = "user", 
                    parts = new[] { new { text = userMessage } } 
                }
            }
        };

        string jsonData = JsonConvert.SerializeObject(requestData);
        byte[] postData = Encoding.UTF8.GetBytes(jsonData);

        // Ensure the URL is correctly formatted for Gemini
        string finalUrl = apiUrl.Trim();
        if (!finalUrl.Contains(":generateContent"))
        {
            if (finalUrl.EndsWith("/")) finalUrl = finalUrl.Substring(0, finalUrl.Length - 1);
            finalUrl += ":generateContent";
        }
        
        // Gemini strongly prefers the key in the query string
        if (!finalUrl.Contains("?key="))
        {
            finalUrl += "?key=" + apiKey;
        }

        // --- DEBUG LOGS ---
        Debug.Log($"<color=yellow>[APIClient] Sending Request to URL: {finalUrl.Replace(apiKey, "HIDDEN_KEY")}</color>");
        Debug.Log($"<color=yellow>[APIClient] Payload: {jsonData}</color>");
        // ------------------

        using (UnityWebRequest request = new UnityWebRequest(finalUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                string errorMsg = $"Error {request.responseCode}: {request.error}\n{request.downloadHandler.text}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                try
                {
                    var parsedData = JsonConvert.DeserializeObject<GeminiResponse>(jsonResponse);
                    if (parsedData != null && parsedData.candidates != null && parsedData.candidates.Count > 0)
                    {
                        string botReply = parsedData.candidates[0].content.parts[0].text;
                        onSuccess?.Invoke(botReply);
                    }
                    else
                    {
                        onError?.Invoke("Failed to parse response from Gemini API.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("JSON Parse Error (Gemini): " + e.Message);
                    onError?.Invoke("Error parsing Gemini API response.");
                }
            }
        }
    }

    // --- Helper classes for JSON Deserialization ---

    [Serializable]
    public class OpenAIResponse { public List<Choice> choices; }
    [Serializable]
    public class Choice { public Message message; }
    [Serializable]
    public class Message { public string role; public string content; }

    [Serializable]
    public class GeminiResponse { public List<Candidate> candidates; }
    [Serializable]
    public class Candidate { public GeminiContent content; }
    [Serializable]
    public class GeminiContent { public List<GeminiPart> parts; }
    [Serializable]
    public class GeminiPart { public string text; }
}
