using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ChatUIManager : MonoBehaviour
{
    [Header("Backend Reference")]
    public ChatRouter chatRouter;

    [Header("UI Elements")]
    public TMP_InputField inputField;
    public Button sendButton;
    public ScrollRect scrollRect;
    public Transform contentParent;

    [Header("Prefabs")]
    [Tooltip("Prefab untuk pesan yang dikirim pemain")]
    public GameObject playerBubblePrefab;
    
    [Tooltip("Prefab untuk pesan balasan dari Bot")]
    public GameObject botBubblePrefab;

    private void Start()
    {
        // Hubungkan tombol kirim
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendButtonClicked);
        }

        // Hubungkan jika pemain menekan 'Enter' di keyboard
        if (inputField != null)
        {
            inputField.onSubmit.AddListener(delegate { OnSendButtonClicked(); });
        }
    }

    public void OnSendButtonClicked()
    {
        string message = inputField.text.Trim();
        if (string.IsNullOrEmpty(message)) return;

        // 1. Munculkan chat bubble pemain di UI
        AddMessageBubble(message, playerBubblePrefab);

        // 2. Kosongkan input dan matikan sementara sambil menunggu bot mengetik
        inputField.text = "";
        inputField.interactable = false;
        sendButton.interactable = false;

        // 3. Kirim ke Backend (Router)
        chatRouter.ProcessMessage(message, OnBotReplyReceived, OnBotError);
    }

    private void OnBotReplyReceived(string reply)
    {
        // Nyalakan kembali input
        inputField.interactable = true;
        sendButton.interactable = true;
        inputField.ActivateInputField(); // Fokus otomatis ke kolom teks lagi

        // Munculkan chat bubble bot
        AddMessageBubble(reply, botBubblePrefab);
    }

    private void OnBotError(string errorMsg)
    {
        // Nyalakan kembali input
        inputField.interactable = true;
        sendButton.interactable = true;
        inputField.ActivateInputField();

        // Munculkan pesan error (pakai bubble bot tapi bisa diberi warna berbeda jika mau)
        AddMessageBubble("<i>Maaf, terjadi kesalahan: " + errorMsg + "</i>", botBubblePrefab);
    }

    private void AddMessageBubble(string text, GameObject prefab)
    {
        if (prefab == null || contentParent == null)
        {
            Debug.LogError("Prefab atau Content Parent belum di-assign di Inspector!");
            return;
        }

        // Spawn prefab chat bubble ke dalam Scroll View Content
        GameObject bubble = Instantiate(prefab, contentParent);
        
        // Cari komponen TextMeshPro di dalam prefab tersebut (bisa di parent atau child)
        TextMeshProUGUI textComp = bubble.GetComponentInChildren<TextMeshProUGUI>();
        if (textComp != null)
        {
            textComp.text = text;
        }
        else
        {
            Debug.LogWarning("Komponen TextMeshProUGUI tidak ditemukan di dalam prefab Chat Bubble!");
        }

        // Paksa UI untuk merender ulang dan scroll otomatis ke paling bawah
        StartCoroutine(ScrollToBottom());
    }

    private IEnumerator ScrollToBottom()
    {
        // Tunggu hingga UI selesai me-layout pesan baru
        yield return new WaitForEndOfFrame();
        
        RectTransform rt = contentParent.GetComponent<RectTransform>();
        if (rt != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
        
        yield return new WaitForEndOfFrame();
        
        // Gulir scroll view ke paling bawah (0.0 = bawah, 1.0 = atas)
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
