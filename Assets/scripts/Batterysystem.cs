using System.Collections;
using UnityEngine;

public class BatterySystem : MonoBehaviour
{
    [Header("State")]
    public bool startBroken = true;

    [Header("Fire VFX")]
    public ParticleSystem fireParticles;
    public ParticleSystem smokeParticles;

    [Header("Audio")]
    public AudioSource alarmAudio;
    public AudioClip repairSound;
    public AudioSource sfxSource;

    //[Header("Repair VFX")]
    //public ParticleSystem repairSparks;

    [Header("Materials")]
    public Renderer systemRenderer;
    public Material brokenMaterial;
    public Material fixedMaterial;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;

    [Header("UI Prompt")]
    public GameObject repairPrompt;   // "Press E to repair" world-space or canvas text

    private bool isBroken = false;
    private bool playerInRange = false;
    private PlayerInventory playerInventory;

    /// <summary>Other scripts (e.g. PipeGrabSystem, GrabbablePipe) check this flag.</summary>
    public static bool IsRepaired { get; private set; } = false;

    // ── Prompt state ────────────────────────────────────────────────────
    private bool   showPrompt = false;
    private string promptMessage = "";
    private Color  promptColor = Color.white;
    private Color  promptBadge = new Color(0.95f, 0.65f, 0.15f);
    private float  noToolFlashTimer = 0f;

    void Start()
    {
        if (repairPrompt) repairPrompt.SetActive(false);
        if (startBroken) BreakSystem();
    }

    void Update()
    {
        if (noToolFlashTimer > 0f)
            noToolFlashTimer -= Time.deltaTime;

        showPrompt = false;

        if (isBroken && playerInRange)
        {
            showPrompt = true;
            bool hasTool = playerInventory != null && playerInventory.hasScrewdriver;
            string key = PromptUI.KeyName(interactKey);

            if (noToolFlashTimer > 0f)
            {
                promptMessage = "You need the Spanner Wrench!";
                promptColor   = new Color(1f, 0.35f, 0.35f);
                promptBadge   = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            }
            else if (hasTool)
            {
                promptMessage = "Repair Battery";
                promptColor   = Color.white;
                promptBadge   = new Color(0.95f, 0.65f, 0.15f);
            }
            else
            {
                promptMessage = "Need Spanner Wrench";
                promptColor   = new Color(1f, 0.75f, 0.4f);
                promptBadge   = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            }
        }

        if (!isBroken || !playerInRange) return;

        if (Input.GetKeyDown(interactKey))
        {
            if (playerInventory != null && playerInventory.hasScrewdriver)
                RepairSystem();
            else
                ShowNoToolMessage();
        }
    }

    public void BreakSystem()
    {
        isBroken = true;
        IsRepaired = false;
        fireParticles?.Play();
        smokeParticles?.Play();
        alarmAudio?.Play();

        if (systemRenderer && brokenMaterial)
            systemRenderer.material = brokenMaterial;
    }

    void RepairSystem()
    {
        isBroken = false;
        IsRepaired = true;
        showPrompt = false;

        StartCoroutine(StopFireAfterDelay(2f));
        smokeParticles?.Stop();
        alarmAudio?.Stop();

        if (sfxSource && repairSound)
        {
            sfxSource.clip = repairSound;
            sfxSource.Play();
        }

        if (systemRenderer && fixedMaterial)
            systemRenderer.material = fixedMaterial;

        if (repairPrompt) repairPrompt.SetActive(false);

        QuestManager.Instance?.UpdateQuest("fix_battery");

        // Remove spanner wrench from player after 1 second
        StartCoroutine(RemoveWrenchAfterDelay(1f));

        Debug.Log("Battery repaired!");
    }

    IEnumerator RemoveWrenchAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (playerInventory != null)
            playerInventory.RemoveScrewdriver();
    }

    IEnumerator StopFireAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        fireParticles?.Stop();
    }

    void ShowNoToolMessage()
    {
        noToolFlashTimer = 2f;
        Debug.Log("You need a screwdriver to fix this.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        playerInventory = other.GetComponent<PlayerInventory>();

        if (isBroken && repairPrompt) repairPrompt.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        if (repairPrompt) repairPrompt.SetActive(false);
    }

    void OnGUI()
    {
        if (!showPrompt) return;
        PromptUI.Draw(PromptUI.KeyName(interactKey), promptMessage, promptColor, promptBadge);
    }
}