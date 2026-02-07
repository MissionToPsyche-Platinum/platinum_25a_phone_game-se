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

    // Item IDs (1-based; must match ItemDictionary prefab list order)
    private static readonly int IdMagnetometerParts = 1;
    private static readonly int IdWiring = 2;
    private static readonly int IdCameraSensor = 3;
    private static readonly int IdSpectrometerCore = 4;
    private static readonly int IdInsulation = 5;

    private int currentStepIndex;
    private List<PhaseCStep> steps;
    private readonly Dictionary<string, npc> npcByName = new Dictionary<string, npc>();

    /// <summary>Step 1 (Instrument Build): 0 = none, 1 = magnetometer, 2 = imager, 3 = spectrometer. Step completes when 3 and dialogue closed.</summary>
    private int instrumentsBuilt;

    private InventoryController inventoryController;
    private ItemDictionary itemDictionary;

    public event Action<StepInfo> StepChanged;

    /// <summary>Fired when the player completes the final step (KDP-D) and Phase C is complete.</summary>
    public event Action PhaseCComplete;

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

        inventoryController = FindFirstObjectByType<InventoryController>();
        itemDictionary = FindFirstObjectByType<ItemDictionary>();
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
            return StepInfo.Empty;

        if (currentStepIndex >= steps.Count)
            return new StepInfo("", "Phase C complete", "", "Thank you for playing.", null, 0, steps.Count);

        int safeIndex = Mathf.Clamp(currentStepIndex, 0, steps.Count - 1);
        PhaseCStep step = steps[safeIndex];
        string collectObjective = GetCollectObjective(safeIndex, step);
        return new StepInfo(step.Id, step.Title, step.CompletionNpc, step.Summary, collectObjective, safeIndex + 1, steps.Count);
    }

    private string GetCollectObjective(int stepIndex, PhaseCStep step)
    {
        if (stepIndex != 0 || itemDictionary == null) return null;
        if (instrumentsBuilt >= 3) return null;

        int[][] recipes = GetInstrumentRecipes();
        int next = instrumentsBuilt;
        if (next < 0 || next >= recipes.Length) return null;

        int[] ids = recipes[next];
        var names = new List<string>();
        foreach (int id in ids)
            names.Add(itemDictionary.GetDisplayName(id));
        return "Collect: " + string.Join(", ", names) + ". Bring to " + step.CompletionNpc + ".";
    }

    public string[] GetDialogueLinesForNpc(string npcName)
    {
        if (string.IsNullOrWhiteSpace(npcName) || steps == null || steps.Count == 0)
        {
            return null;
        }

        int safeIndex = Mathf.Clamp(currentStepIndex, 0, steps.Count - 1);

        // Step 1 Instrument Build: deliver items to Sarah when player has the next recipe
        if (safeIndex == 0 && npcName == NpcInstruments)
        {
            TryDeliverInstrumentBuild();
            string[] instrumentLines = GetInstrumentBuildDialogueForSarah();
            if (instrumentLines != null && instrumentLines.Length > 0)
                return instrumentLines;
        }

        return steps[safeIndex].GetNpcLines(npcName);
    }

    private static int[][] GetInstrumentRecipes()
    {
        return new[]
        {
            new[] { IdMagnetometerParts, IdWiring },
            new[] { IdCameraSensor, IdWiring },
            new[] { IdSpectrometerCore, IdInsulation }
        };
    }

    /// <summary>If player has items for the next instrument, consume them and increment instrumentsBuilt. Called when opening dialogue with Sarah on step 1.</summary>
    private void TryDeliverInstrumentBuild()
    {
        if (currentStepIndex != 0 || inventoryController == null) return;
        if (instrumentsBuilt >= 3) return;

        int[][] recipes = GetInstrumentRecipes();
        int[] nextRecipe = recipes[instrumentsBuilt];
        if (inventoryController.HasAllItems(nextRecipe) && inventoryController.RemoveItems(nextRecipe))
        {
            instrumentsBuilt++;
            NotifyStepChanged();
        }
    }

    private string[] GetInstrumentBuildDialogueForSarah()
    {
        if (instrumentsBuilt == 0)
        {
            return new[]
            {
                "Welcome to the instrument team. We're building three science instruments for Psyche.",
                "First up: the Magnetometer. It'll look for evidence of an ancient magnetic field. I need Magnetometer Parts and Wiring. Gather those and bring them to me.",
                "Parts spawn around the facility. Once you have them, come back and we'll lock in the first instrument."
            };
        }
        if (instrumentsBuilt == 1)
        {
            return new[]
            {
                "Magnetometer's done. Next is the Multispectral Imager. It'll characterize the surface in visible and near-infrared.",
                "I need a Camera Sensor and Wiring. Find them and bring them here.",
                "Come back when you've got the parts."
            };
        }
        if (instrumentsBuilt == 2)
        {
            return new[]
            {
                "Imager's complete. Last one: the Gamma-Ray and Neutron Spectrometer for elemental composition.",
                "I need the Spectrometer Core and Insulation. Bring those and we'll close out the instrument suite.",
                "See you when you have them."
            };
        }
        if (instrumentsBuilt == 3)
        {
            return new[]
            {
                "All three instruments are complete. We've locked in the instrument suite for Psyche.",
                "Once we've got these signed off, we move on to the bus. Come back when you're ready and we'll close out this step."
            };
        }
        return null;
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
        if (!currentStep.IsCompletionNpc(npcName))
            return;

        // Step 1 (Instrument Build) only completes when all three instruments have been delivered
        if (currentStepIndex == 0 && instrumentsBuilt < 3)
            return;

        bool wasLastStep = currentStepIndex == steps.Count - 1;
        AdvanceStep();
        if (wasLastStep)
            PhaseCComplete?.Invoke();
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

        UpdateObjectiveIndicators();
    }

    private void UpdateObjectiveIndicators()
    {
        if (npcByName == null || npcByName.Count == 0)
            return;

        if (steps == null || steps.Count == 0 || currentStepIndex >= steps.Count)
        {
            foreach (npc n in npcByName.Values)
                n.SetIsCurrentObjective(false);
            return;
        }

        string completionNpc = steps[currentStepIndex].CompletionNpc;
        foreach (KeyValuePair<string, npc> kvp in npcByName)
            kvp.Value.SetIsCurrentObjective(kvp.Key == completionNpc);
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
            // Step 1: Instrument Build – Dr. Sarah Chen (enthusiastic, science-focused)
            new PhaseCStep(
                "C1",
                "Instrument Build",
                NpcInstruments,
                "Help Sarah lock in the instrument suite for Psyche.",
                new Dictionary<string, string[]>
                {
                    {
                        NpcInstruments,
                        new[]
                        {
                            "Welcome to the instrument team. We're building three science instruments for Psyche: a Magnetometer, a Multispectral Imager, and a Gamma-Ray and Neutron Spectrometer.",
                            "The Magnetometer will look for evidence of an ancient magnetic field at Psyche. The Multispectral Imager will characterize the surface in visible and near-infrared wavelengths. The Gamma-Ray and Neutron Spectrometer will help us determine the asteroid's elemental composition.",
                            "Once we've got these locked in, we move on to the bus. Come back when you're ready and we'll close out this step."
                        }
                    },
                    {
                        NpcBus,
                        new[]
                        {
                            "The bus can't move forward until the instruments are set. Sarah's got that in hand. Go see her when you get a chance; she'll get you up to speed."
                        }
                    },
                    {
                        NpcReview,
                        new[]
                        {
                            "Critical Design Review comes after we finish the builds. Right now we're still in the instrument phase. Sarah's the one to talk to."
                        }
                    },
                    {
                        NpcIntegration,
                        new[]
                        {
                            "Systems integration is down the road. For now we're focused on getting the instruments right. Sarah would love to walk you through it."
                        }
                    }
                }),
            // Step 2: Spacecraft Bus Complete – Dr. Marcus Rodriguez (practical, organized)
            new PhaseCStep(
                "C2",
                "Spacecraft Bus Complete",
                NpcBus,
                "Help Marcus close out the spacecraft bus.",
                new Dictionary<string, string[]>
                {
                    {
                        NpcInstruments,
                        new[]
                        {
                            "The instruments are ready to mount. Marcus is wrapping up the bus so we have somewhere to put them. Go see him when you can."
                        }
                    },
                    {
                        NpcBus,
                        new[]
                        {
                            "By May 2020 we had the spacecraft bus finished. It's the main structure that holds all the subsystems and science instruments.",
                            "Once we sign off on the bus, we move to the Critical Design Review. Come back when you're ready and we'll close out this step."
                        }
                    },
                    {
                        NpcReview,
                        new[]
                        {
                            "We do the Critical Design Review after the bus is done. Marcus is closing that out. Once he's signed off, we're on."
                        }
                    },
                    {
                        NpcIntegration,
                        new[]
                        {
                            "Integration comes after design review. Right now we need the bus signed off. Marcus is your person for that."
                        }
                    }
                }),
            // Step 3: Critical Design Review – Dr. Priya Patel (analytical, calm)
            new PhaseCStep(
                "C3",
                "Critical Design Review",
                NpcReview,
                "Work with Priya to complete the Critical Design Review.",
                new Dictionary<string, string[]>
                {
                    {
                        NpcInstruments,
                        new[]
                        {
                            "With the bus complete, we're ready for design review. Priya runs the CDR. She'll walk you through what we're locking in."
                        }
                    },
                    {
                        NpcBus,
                        new[]
                        {
                            "The CDR verifies the design before we integrate everything. Priya's got it under control. Go talk to her when you're ready."
                        }
                    },
                    {
                        NpcReview,
                        new[]
                        {
                            "In May 2020 we completed the Critical Design Review. It verifies that the design meets every requirement before we move to full system integration.",
                            "We also locked in plans for X-band telecommunications and technology demonstrations such as laser communications. When you're ready, we'll approve the review and move forward."
                        }
                    },
                    {
                        NpcIntegration,
                        new[]
                        {
                            "Systems integration review happens after we pass the CDR. Priya's running the review. Once she gives the nod, we're on to integration."
                        }
                    }
                }),
            // Step 4: Systems Integration Review – Dr. James Thompson (collaborative, big picture)
            new PhaseCStep(
                "C4",
                "Systems Integration Review",
                NpcIntegration,
                "Work with James to complete the Systems Integration Review.",
                new Dictionary<string, string[]>
                {
                    {
                        NpcInstruments,
                        new[]
                        {
                            "We're at the integration review now. James is making sure the whole system is ready to come together. He's the one to see."
                        }
                    },
                    {
                        NpcBus,
                        new[]
                        {
                            "After the CDR we verify the integrated system is ready for assembly and test. James has the full picture. Go talk to him."
                        }
                    },
                    {
                        NpcReview,
                        new[]
                        {
                            "The Systems Integration Review happened in January 2021. James runs it. Once that's done, we're set for final approval."
                        }
                    },
                    {
                        NpcIntegration,
                        new[]
                        {
                            "In January 2021 we completed the Systems Integration Review. It confirms that all subsystems can be combined and tested as a whole.",
                            "When you're ready, we'll close out integration. Then it's on to final approval."
                        }
                    }
                }),
            // Step 5: Key Decision Point D – Dr. Sarah Chen (closing the loop)
            new PhaseCStep(
                "C5",
                "Key Decision Point D",
                NpcInstruments,
                "Finalize Phase C with Sarah and KDP-D approval.",
                new Dictionary<string, string[]>
                {
                    {
                        NpcInstruments,
                        new[]
                        {
                            "Key Decision Point D is NASA's approval to proceed to the next phase. Instruments built, bus complete, reviews passed.",
                            "When you're ready, we'll close out Phase C together. Two years of design and build, and we're ready for what comes next."
                        }
                    },
                    {
                        NpcBus,
                        new[]
                        {
                            "We're waiting on KDP-D to wrap Phase C. Sarah's leading the close-out. Go see her when you're ready to finish this chapter."
                        }
                    },
                    {
                        NpcReview,
                        new[]
                        {
                            "KDP-D is the final approval before the next phase. Sarah's handling the sign-off. Talk to her when you're ready to close Phase C."
                        }
                    },
                    {
                        NpcIntegration,
                        new[]
                        {
                            "Final approval happens after all the reviews are in. Sarah's got the last word on Phase C. Go see her when you're ready."
                        }
                    }
                })
        };
    }

    public readonly struct StepInfo
    {
        public static readonly StepInfo Empty = new StepInfo("", "", "", "", null, 0, 0);

        public string Id { get; }
        public string Title { get; }
        public string CompletionNpc { get; }
        public string Summary { get; }
        /// <summary>When set (e.g. Step 1 Instrument Build), show this as the objective instead of Summary.</summary>
        public string CollectObjective { get; }
        public int StepNumber { get; }
        public int StepCount { get; }

        public StepInfo(string id, string title, string completionNpc, string summary, string collectObjective, int stepNumber, int stepCount)
        {
            Id = id;
            Title = title;
            CompletionNpc = completionNpc;
            Summary = summary;
            CollectObjective = collectObjective;
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
