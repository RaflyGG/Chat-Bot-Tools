using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct KeywordResponse
{
    public string keyword;
    [TextArea(2, 5)]
    public string response;
}

public class OfflineDatabase : MonoBehaviour
{
    [Tooltip("Add your keywords and their corresponding static responses here.")]
    public List<KeywordResponse> offlineData = new List<KeywordResponse>();
    
    private Dictionary<string, string> databaseMap = new Dictionary<string, string>();

    private void Awake()
    {
        // Populate dictionary for faster lookup later
        foreach (var data in offlineData)
        {
            string key = data.keyword.ToLower().Trim();
            if (!databaseMap.ContainsKey(key))
            {
                databaseMap.Add(key, data.response);
            }
        }
    }

    /// <summary>
    /// Checks if a message contains any of the keywords and returns the appropriate response.
    /// Returns null if no keyword is matched.
    /// </summary>
    public string GetResponse(string message)
    {
        string lowerMessage = message.ToLower();
        
        // Simple checking: if the message contains the keyword
        foreach (var kvp in databaseMap)
        {
            if (lowerMessage.Contains(kvp.Key))
            {
                return kvp.Value;
            }
        }
        
        return null; // No match found
    }
}
