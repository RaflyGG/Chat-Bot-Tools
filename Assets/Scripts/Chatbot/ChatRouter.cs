using UnityEngine;
using System;

[RequireComponent(typeof(OfflineDatabase))]
[RequireComponent(typeof(APIClient))]
public class ChatRouter : MonoBehaviour
{
    private OfflineDatabase offlineDatabase;
    private APIClient apiClient;

    private void Awake()
    {
        offlineDatabase = GetComponent<OfflineDatabase>();
        apiClient = GetComponent<APIClient>();
    }

    /// <summary>
    /// Processes the user's message. 
    /// First checks the offline database. If no match is found, falls back to the API.
    /// </summary>
    /// <param name="userMessage">The message from the user.</param>
    /// <param name="onResponseReceived">Callback when the bot has a response.</param>
    /// <param name="onError">Callback if an error occurs.</param>
    public void ProcessMessage(string userMessage, Action<string> onResponseReceived, Action<string> onError)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            return;
        }

        // 1. Preprocessing (Router)
        string cleanMessage = userMessage.Trim();

        // 2. Offline Database Check
        string offlineResponse = offlineDatabase.GetResponse(cleanMessage);

        if (!string.IsNullOrEmpty(offlineResponse))
        {
            // Found a match in offline database!
            Debug.Log("[ChatRouter] Keyword matched. Using Offline Database.");
            onResponseReceived?.Invoke(offlineResponse);
        }
        else
        {
            // 3. Fallback to Online API
            Debug.Log("[ChatRouter] No keyword match. Falling back to API.");
            apiClient.SendMessageToAPI(cleanMessage, onResponseReceived, onError);
        }
    }
}
