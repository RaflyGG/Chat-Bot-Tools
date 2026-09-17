using UnityEngine;

public class ChatBackendTester : MonoBehaviour
{
    public ChatRouter chatRouter;
    
    [Header("Test Input")]
    public string testMessage = "Hello!";
    
    [Header("Controls")]
    [Tooltip("Click this checkbox in the inspector while in Play Mode to send a message")]
    public bool sendNow = false;

    private void Update()
    {
        // Simple inspector button hack
        if (sendNow)
        {
            sendNow = false;
            SendTestMessage();
        }
    }

    public void SendTestMessage()
    {
        if (chatRouter == null)
        {
            chatRouter = FindObjectOfType<ChatRouter>();
            if (chatRouter == null)
            {
                Debug.LogError("ChatRouter is not assigned and could not be found!");
                return;
            }
        }

        Debug.Log($"[Tester] Sending message: {testMessage}");
        chatRouter.ProcessMessage(testMessage, OnSuccess, OnError);
    }

    private void OnSuccess(string response)
    {
        Debug.Log($"<color=green>[Tester] Bot Reply: {response}</color>");
    }

    private void OnError(string error)
    {
        Debug.LogError($"<color=red>[Tester] Error: {error}</color>");
    }
}
