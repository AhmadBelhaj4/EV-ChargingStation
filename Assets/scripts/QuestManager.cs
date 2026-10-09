using UnityEngine;
using System.Collections.Generic;

// ── Plain C# enum — NOT a MonoBehaviour ──
public enum QuestStatus { Locked, Active, Completed }

// ── Plain C# class — NOT a MonoBehaviour ──
[System.Serializable]
public class Quest
{
    public string id;
    public string title;
    public string description;
    public QuestStatus status;
    public int requiredAmount;
    public int currentAmount;

    public bool IsCompleted => currentAmount >= requiredAmount;

    public string ProgressText =>
        requiredAmount > 1
        ? $"{currentAmount}/{requiredAmount}"
        : (IsCompleted ? "Done" : "Incomplete");
}

// ── This one IS a MonoBehaviour — attach this to QuestSystemGO ──
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    // ── Static flags checked by other systems ────────────────────────────
    /// <summary>True after drain_pipes quest is completed. Gates pipe grabbing.</summary>
    public static bool IsDrainDone { get; private set; } = false;

    /// <summary>True after fix_pipes quest is completed.</summary>
    public static bool IsWeldingDone { get; private set; } = false;

    /// <summary>True after return_pipe quest is completed. Gates FILL button.</summary>
    public static bool IsReturnPipeDone { get; private set; } = false;

    [Header("Quests — edit these in Inspector")]
    [SerializeField] private List<Quest> quests = new List<Quest>()
    {
        new Quest {
            id             = "talk_npc",
            title          = "Talk to Greg",
            description    = "Talk to the engineer about the cooling system",
            status         = QuestStatus.Active,
            requiredAmount = 1
        },
        new Quest {
            id             = "fix_battery",
            title          = "Repair the Battery",
            description    = "Use the spanner wrench to fix the broken battery",
            status         = QuestStatus.Locked,
            requiredAmount = 1
        },
        new Quest {
            id             = "drain_pipes",
            title          = "Drain the Pipes",
            description    = "Drain the cooling liquid from the pipes",
            status         = QuestStatus.Locked,
            requiredAmount = 1
        },
        new Quest {
            id             = "fix_pipes",
            title          = "Fix the Pipes",
            description    = "Weld the broken pipe holes on the welding table",
            status         = QuestStatus.Locked,
            requiredAmount = 1
        },
        new Quest {
            id             = "return_pipe",
            title          = "Return the Pipe",
            description    = "Place the repaired pipe back in its original position",
            status         = QuestStatus.Locked,
            requiredAmount = 1
        },
        new Quest {
            id             = "refill_liquid",
            title          = "Refill Liquid Coolant",
            description    = "Refill the cooling liquid in the pipes",
            status         = QuestStatus.Locked,
            requiredAmount = 1
        },
        new Quest {
            id             = "test_voltage",
            title          = "Test the Voltage",
            description    = "Use the voltage slider to test the cooling system",
            status         = QuestStatus.Locked,
            requiredAmount = 1
        },
    };

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Force-rebuild the quest list from code so stale Inspector data is overridden
        quests = new List<Quest>()
        {
            new Quest {
                id             = "talk_npc",
                title          = "Talk to Greg",
                description    = "Talk to the engineer about the cooling system",
                status         = QuestStatus.Active,
                requiredAmount = 1
            },
            new Quest {
                id             = "fix_battery",
                title          = "Repair the Battery",
                description    = "Use the spanner wrench to fix the broken battery",
                status         = QuestStatus.Locked,
                requiredAmount = 1
            },
            new Quest {
                id             = "drain_pipes",
                title          = "Drain the Pipes",
                description    = "Drain the cooling liquid from the pipes",
                status         = QuestStatus.Locked,
                requiredAmount = 1
            },
            new Quest {
                id             = "fix_pipes",
                title          = "Fix the Pipes",
                description    = "Weld all 5 broken pipe holes on the welding table",
                status         = QuestStatus.Locked,
                requiredAmount = 5
            },
            new Quest {
                id             = "return_pipe",
                title          = "Return the Pipe",
                description    = "Place the repaired pipe back in its original position",
                status         = QuestStatus.Locked,
                requiredAmount = 1
            },
            new Quest {
                id             = "refill_liquid",
                title          = "Refill Liquid Coolant",
                description    = "Refill the cooling liquid in the pipes",
                status         = QuestStatus.Locked,
                requiredAmount = 1
            },
            new Quest {
                id             = "test_voltage",
                title          = "Test the Voltage",
                description    = "Use the voltage slider to test the cooling system",
                status         = QuestStatus.Locked,
                requiredAmount = 1
            },
        };
    }

    void Start()
    {
        QuestUI.Instance?.RefreshUI(quests);
    }

    /// <summary>
    /// Call this from anywhere to add progress to a quest.
    /// Example: QuestManager.Instance.UpdateQuest("fix_pipes");
    /// </summary>
    public void UpdateQuest(string questId, int amount = 1)
    {
        Quest q = quests.Find(q => q.id == questId);

        if (q == null)
        {
            Debug.LogWarning($"Quest '{questId}' not found!");
            return;
        }

        if (q.status != QuestStatus.Active)
        {
            Debug.Log($"Quest '{questId}' is not active — ignoring.");
            return;
        }

        q.currentAmount += amount;
        q.currentAmount = Mathf.Clamp(q.currentAmount, 0, q.requiredAmount);

        Debug.Log($"Quest '{q.title}': {q.currentAmount}/{q.requiredAmount}");

        if (q.IsCompleted)
            CompleteQuest(q);
        else
            QuestUI.Instance?.RefreshUI(quests);
    }

    void CompleteQuest(Quest q)
    {
        q.status = QuestStatus.Completed;
        Debug.Log($"✓ Quest completed: {q.title}");

        // ── Update static flags for other systems ──
        switch (q.id)
        {
            case "drain_pipes":   IsDrainDone      = true; break;
            case "fix_pipes":     IsWeldingDone    = true; break;
            case "return_pipe":   IsReturnPipeDone = true; break;
        }

        // Unlock the next quest automatically
        int index = quests.IndexOf(q);
        if (index + 1 < quests.Count)
        {
            quests[index + 1].status = QuestStatus.Active;
            Debug.Log($"New quest unlocked: {quests[index + 1].title}");
        }

        QuestUI.Instance?.ShowCompletionBanner(q.title);
        QuestUI.Instance?.RefreshUI(quests);

        // Check if ALL quests are now completed → stop timer & show score
        if (AreAllQuestsCompleted())
        {
            Debug.Log("All quests completed!");
            GameTimer.Instance?.StopTimer();
        }
    }

    /// <summary>
    /// Returns true when every quest in the list has been completed.
    /// </summary>
    public bool AreAllQuestsCompleted()
    {
        foreach (Quest q in quests)
        {
            if (q.status != QuestStatus.Completed)
                return false;
        }
        return quests.Count > 0;
    }

    public List<Quest> GetQuests() => quests;
}