using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhaseCAssemblyController : MonoBehaviour
{
    public static PhaseCAssemblyController Instance { get; private set; }

    private const string TargetSceneName = "MinigameC";

    private const string NpcInstruments = "Dr. Sarah Chen";
    private const string NpcBus = "Dr. Marcus Rodriguez";
    private const string NpcReview = "Dr. Priya Patel";
    private const string NpcIntegration = "Dr. James Thompson";

    private int currentStepIndex;
    private List<PhaseCStep> steps;
    private readonly Dictionary<string, npc> npcByName = new Dictionary<string, npc>();

    public event Action<StepInfo> StepChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureController()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != TargetSceneName)
        {
            return;
        }

        if (FindFirstObjectByType<PhaseCAssemblyController>() == null)
        {
            GameObject controllerObject = new GameObject("PhaseCAssemblyController");
            controllerObject.AddComponent<PhaseCAssemblyController>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != TargetSceneName)
        {
            Destroy(gameObject);
            return;
        }

        BuildSteps();
        CacheNpcs();
        AttachStepTriggers();
        UpdateNpcDialogue();
        EnsureGuideUi();
        NotifyStepChanged();
    }

    public StepInfo GetCurrentStepInfo()
    {
        if (steps == null || steps.Count == 0)
        {
            return StepInfo.Empty;
        }

        int safeIndex = Mathf.Clamp(currentStepIndex, 0, steps.Count - 1);
        PhaseCStep step = steps[safeIndex];
        return new StepInfo(step.Id, step.Title, step.CompletionNpc, step.Summary, safeIndex + 1, steps.Count);
    }

    public string[] GetDialogueLinesForNpc(string npcName)
    {
        if (string.IsNullOrWhiteSpace(npcName) || steps == null || steps.Count == 0)
        {
            return null;
        }

        int safeIndex = Mathf.Clamp(currentStepIndex, 0, steps.Count - 1);
        return steps[safeIndex].GetNpcLines(npcName);
    }

    public void NotifyDialogueClosed(string npcName)
    {
        if (steps == null || steps.Count == 0)
        {
            return;
        }

        if (currentStepIndex >= steps.Count)
        {
            return;
        }

        PhaseCStep currentStep = steps[currentStepIndex];
        if (currentStep.IsCompletionNpc(npcName))
        {
            AdvanceStep();
        }
    }

    public void NotifyNpcInRange(string npcName, int stepIndex)
    {
        if (currentStepIndex != stepIndex)
        {
            return;
        }

        if (npcByName.TryGetValue(npcName, out npc npcComponent))
        {
            npcComponent.RefreshDialogueLines();
        }
    }

    private void AdvanceStep()
    {
        if (currentStepIndex < steps.Count - 1)
        {
            currentStepIndex++;
            UpdateNpcDialogue();
            NotifyStepChanged();
        }
    }

    private void UpdateNpcDialogue()
    {
        foreach (npc npcComponent in npcByName.Values)
        {
            npcComponent.RefreshDialogueLines();
        }
    }

    private void NotifyStepChanged()
    {
        StepChanged?.Invoke(GetCurrentStepInfo());
    }

    private void EnsureGuideUi()
    {
        if (GetComponent<PhaseCGuideUI>() == null)
        {
            gameObject.AddComponent<PhaseCGuideUI>();
        }
    }

    private void CacheNpcs()
    {
        npcByName.Clear();
        npc[] npcComponents = FindObjectsByType<npc>(FindObjectsSortMode.None);

        foreach (npc npcComponent in npcComponents)
        {
            if (npcComponent.dialogueData == null)
            {
                continue;
            }

            string npcName = npcComponent.dialogueData.npcName;
            if (string.IsNullOrWhiteSpace(npcName))
            {
                continue;
            }

            if (!npcByName.ContainsKey(npcName))
            {
                npcByName.Add(npcName, npcComponent);
            }
        }
    }

    private void AttachStepTriggers()
    {
        foreach (KeyValuePair<string, npc> entry in npcByName)
        {
            int stepIndex = GetStepIndexForCompletionNpc(entry.Key);
            if (stepIndex < 0)
            {
                continue;
            }

            PhaseCStepTrigger trigger = entry.Value.GetComponent<PhaseCStepTrigger>();
            if (trigger == null)
            {
                trigger = entry.Value.gameObject.AddComponent<PhaseCStepTrigger>();
            }

            trigger.Initialize(entry.Key, stepIndex);
        }
    }

    private int GetStepIndexForCompletionNpc(string npcName)
    {
        for (int i = 0; i < steps.Count; i++)
        {
            if (steps[i].IsCompletionNpc(npcName))
            {
                return i;
            }
        }

        return -1;
    }

    private void BuildSteps()
    {
        steps = new List<PhaseCStep>
        {
            new PhaseCStep(
                "C1",
                "Instrument Build",
                NpcInstruments,
                "Build and confirm the instrument suite.",
                new Dictionary<string, string[]>
                {
                    {
                        NpcInstruments,
                        new[]
                        {
                            "Phase C is final design and building the instruments.",
                            "Psyche uses a magnetometer, multispectral imager, and gamma ray and neutron spectrometer.",
                            "Confirm the instruments are built so we can move on."
                        }
                    },
                    {
                        NpcBus,
                        new[]
                        {
                            "We cannot finish the spacecraft bus until the instruments are ready.",
                            "Check in with Dr. Sarah Chen first."
                        }
                    },
                    {
                        NpcReview,
                        new[]
                        {
                            "Critical Design Review comes after the builds.",
                            "Finish the instrument build step first."
                        }
                    },
                    {
                        NpcIntegration,
                        new[]
                        {
                            "Systems integration happens after design review.",
                            "We are not there yet."
                        }
                    }
                }),
            new PhaseCStep(
                "C2",
                "Spacecraft Bus Complete",
                NpcBus,
                "Confirm the spacecraft bus is completed.",
                new Dictionary<string, string[]>
                {
                    {
                        NpcInstruments,
                        new[]
                        {
                            "The instruments are ready to mount on the spacecraft bus.",
                            "Meet Dr. Marcus Rodriguez to complete the bus."
                        }
                    },
                    {
                        NpcBus,
                        new[]
                        {
                            "By May 2020 the spacecraft bus was completed.",
                            "The bus is the main body that holds subsystems and instruments.",
                            "Mark the bus complete to proceed."
                        }
                    },
                    {
                        NpcReview,
                        new[]
                        {
                            "After the bus is complete, we hold the Critical Design Review.",
                            "Finish the bus step before we review."
                        }
                    },
                    {
                        NpcIntegration,
                        new[]
                        {
                            "Integration happens after the design review.",
                            "Come back after the bus is signed off."
                        }
                    }
                }),
            new PhaseCStep(
                "C3",
                "Critical Design Review",
                NpcReview,
                "Complete the Critical Design Review.",
                new Dictionary<string, string[]>
                {
                    {
                        NpcInstruments,
                        new[]
                        {
                            "With the bus complete, we can move into design review.",
                            "Visit Dr. Priya Patel for the Critical Design Review."
                        }
                    },
                    {
                        NpcBus,
                        new[]
                        {
                            "Design review verifies the final design before full system integration.",
                            "Talk to Dr. Priya Patel next."
                        }
                    },
                    {
                        NpcReview,
                        new[]
                        {
                            "In May 2020 we completed the Critical Design Review.",
                            "It verifies the design meets requirements before full integration.",
                            "We also confirm plans for X-band radio and the laser comm demo.",
                            "Approve the review to continue."
                        }
                    },
                    {
                        NpcIntegration,
                        new[]
                        {
                            "Systems integration review happens after critical design review.",
                            "Complete the CDR first."
                        }
                    }
                }),
            new PhaseCStep(
                "C4",
                "Systems Integration Review",
                NpcIntegration,
                "Verify readiness to integrate the full system.",
                new Dictionary<string, string[]>
                {
                    {
                        NpcInstruments,
                        new[]
                        {
                            "Integration review checks the full system readiness.",
                            "Meet Dr. James Thompson to complete it."
                        }
                    },
                    {
                        NpcBus,
                        new[]
                        {
                            "After CDR we verify the integrated system is ready for assembly and test.",
                            "Go to Dr. James Thompson for the integration review."
                        }
                    },
                    {
                        NpcReview,
                        new[]
                        {
                            "The Systems Integration Review happened in January 2021.",
                            "Finish it to reach the final approval."
                        }
                    },
                    {
                        NpcIntegration,
                        new[]
                        {
                            "In January 2021 we completed the Systems Integration Review.",
                            "It confirms subsystems can be combined and tested as a whole.",
                            "Mark the integration review complete."
                        }
                    }
                }),
            new PhaseCStep(
                "C5",
                "Key Decision Point D",
                NpcInstruments,
                "Finalize Phase C with KDP-D approval.",
                new Dictionary<string, string[]>
                {
                    {
                        NpcInstruments,
                        new[]
                        {
                            "Key Decision Point D approves the mission to move forward.",
                            "With instruments built, bus complete, and reviews done, Phase C wraps up.",
                            "Confirm KDP-D to finish Phase C."
                        }
                    },
                    {
                        NpcBus,
                        new[]
                        {
                            "We are waiting on KDP-D approval.",
                            "Check with Dr. Sarah Chen to finalize Phase C."
                        }
                    },
                    {
                        NpcReview,
                        new[]
                        {
                            "KDP-D is the final approval before the next phase.",
                            "Talk to Dr. Sarah Chen to close out Phase C."
                        }
                    },
                    {
                        NpcIntegration,
                        new[]
                        {
                            "Final approval happens after all reviews are complete.",
                            "Go to Dr. Sarah Chen for KDP-D."
                        }
                    }
                })
        };
    }

    public readonly struct StepInfo
    {
        public static readonly StepInfo Empty = new StepInfo("", "", "", "", 0, 0);

        public string Id { get; }
        public string Title { get; }
        public string CompletionNpc { get; }
        public string Summary { get; }
        public int StepNumber { get; }
        public int StepCount { get; }

        public StepInfo(string id, string title, string completionNpc, string summary, int stepNumber, int stepCount)
        {
            Id = id;
            Title = title;
            CompletionNpc = completionNpc;
            Summary = summary;
            StepNumber = stepNumber;
            StepCount = stepCount;
        }
    }

    private class PhaseCStep
    {
        private readonly string id;
        private readonly string title;
        private readonly string completionNpc;
        private readonly string summary;
        private readonly Dictionary<string, string[]> npcLines;

        public PhaseCStep(string id, string title, string completionNpc, string summary, Dictionary<string, string[]> npcLines)
        {
            this.id = id;
            this.title = title;
            this.completionNpc = completionNpc;
            this.summary = summary;
            this.npcLines = npcLines ?? new Dictionary<string, string[]>();
        }

        public string Id => id;
        public string Title => title;
        public string CompletionNpc => completionNpc;
        public string Summary => summary;

        public bool IsCompletionNpc(string npcName)
        {
            return !string.IsNullOrWhiteSpace(npcName) && npcName == completionNpc;
        }

        public string[] GetNpcLines(string npcName)
        {
            if (string.IsNullOrWhiteSpace(npcName))
            {
                return null;
            }

            if (npcLines.TryGetValue(npcName, out string[] lines))
            {
                return lines;
            }

            return null;
        }
    }
}
