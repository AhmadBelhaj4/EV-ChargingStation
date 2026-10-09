using UnityEngine;
using TMPro;

public class NPCInteract : MonoBehaviour
{
    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Dialogue Lines")]
    [TextArea] public string[] dialogueLines;

    [Header("UI (legacy — kept for Inspector references)")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI promptText;

    [Header("Audio")]
    public AudioSource npcAudio;
    public AudioClip[] voiceClips;

    [Header("Tool Gift")]
    public GameObject screwdriverPickupObject;
    public int giftAtLine = 1;

    private bool playerInRange = false;
    private bool dialogueOpen = false;
    private bool toolGiven = false;
    private int currentLine = 0;
    private string currentDialogueText = "";

    PlayerInventory playerInventory;

    void Start()
    {
        // Hide legacy UI — we draw dialogue with OnGUI now
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (promptText) promptText.gameObject.SetActive(false);
        if (screwdriverPickupObject) screwdriverPickupObject.SetActive(false);
    }

    void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(interactKey))
        {
            if (!dialogueOpen)
                OpenDialogue();
            else
                AdvanceLine();
        }
    }

    void OpenDialogue()
    {
        dialogueOpen = true;
        currentLine = 0;
        ShowLine(currentLine);
        QuestManager.Instance?.UpdateQuest("talk_npc");
    }

    void AdvanceLine()
    {
        currentLine++;

        if (currentLine == giftAtLine && !toolGiven)
            GiveTool();

        if (currentLine >= dialogueLines.Length)
            CloseDialogue();
        else
            ShowLine(currentLine);
    }

    void ShowLine(int index)
    {
        currentDialogueText = dialogueLines[index];

        if (npcAudio && voiceClips != null && index < voiceClips.Length && voiceClips[index] != null)
        {
            npcAudio.clip = voiceClips[index];
            npcAudio.Play();
        }
    }

    void GiveTool()
    {
        toolGiven = true;

        if (playerInventory != null)
            playerInventory.PickUpScrewdriver();

        if (screwdriverPickupObject != null)
            screwdriverPickupObject.SetActive(true);

        Debug.Log("Screwdriver given to player.");
    }

    void CloseDialogue()
    {
        dialogueOpen = false;
        currentDialogueText = "";
        if (npcAudio != null)
        {
            npcAudio.Stop();
            npcAudio.clip = null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        playerInventory = other.GetComponent<PlayerInventory>();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        if (dialogueOpen) CloseDialogue();
    }

    void OnGUI()
    {
        if (!playerInRange) return;

        string key = PromptUI.KeyName(interactKey);

        if (!dialogueOpen)
        {
            // "Talk to Greg" prompt
            PromptUI.Draw(key, "Talk to Greg");
        }
        else
        {
            // ── Dialogue box (dark panel, same style as PromptUI) ──
            float panelW = 700f;
            float panelH = 120f;
            float panelX = Screen.width / 2f - panelW / 2f;
            float panelY = Screen.height - 200f;

            // Dark background
            Texture2D bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, new Color(0.06f, 0.06f, 0.1f, 0.92f));
            bgTex.Apply();
            GUI.DrawTexture(new Rect(panelX, panelY, panelW, panelH), bgTex);

            // Accent bar at top of dialogue box
            Texture2D accentTex = new Texture2D(1, 1);
            accentTex.SetPixel(0, 0, new Color(0.4f, 0.75f, 1f, 0.8f));
            accentTex.Apply();
            GUI.DrawTexture(new Rect(panelX, panelY, panelW, 3f), accentTex);

            // Speaker name
            GUIStyle nameStyle = new GUIStyle();
            nameStyle.fontSize = 16;
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.normal.textColor = new Color(0.4f, 0.75f, 1f);
            nameStyle.alignment = TextAnchor.UpperLeft;
            GUI.Label(new Rect(panelX + 20f, panelY + 12f, 200f, 24f), "GREG", nameStyle);

            // Dialogue text
            GUIStyle textStyle = new GUIStyle();
            textStyle.fontSize = 18;
            textStyle.normal.textColor = new Color(0.92f, 0.93f, 0.96f);
            textStyle.alignment = TextAnchor.UpperLeft;
            textStyle.wordWrap = true;
            GUI.Label(new Rect(panelX + 20f, panelY + 38f, panelW - 40f, panelH - 50f),
                currentDialogueText, textStyle);

            // "Continue" prompt at bottom-right
            PromptUI.Draw(key, "Continue...", Color.white, new Color(0.4f, 0.75f, 1f));
        }
    }
}