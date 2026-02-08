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
    private static readonly int IdMetalAlloy = 6;
    private static readonly int IdCircuitBoard = 7;
    private static readonly int IdSolarCells = 10;
    private static readonly int IdBattery = 11;
    private static readonly int IdRadioAntenna = 12;
    private static readonly int IdLaserModule = 13;
    private static readonly int IdNavigationSystem = 14;
    private static readonly int IdPropellant = 15;

    private int currentStepIndex;
    private List<PhaseCStep> steps;
    private readonly Dictionary<string, npc> npcByName = new Dictionary<string, npc>();

    /// <summary>Step 1 (Instrument Build): 0 = none, 1 = magnetometer, 2 = imager, 3 = spectrometer. Step completes when 3 and dialogue closed.</summary>
    private int instrumentsBuilt;

    /// <summary>Step 2 (Communications): 0 = none, 1 = X-band radio, 2 = laser communication. Step completes when 2 and dialogue closed.</summary>
    private int commsBuilt;

    /// <summary>Step 3 (Spacecraft Bus): 0 = none, 1 = bus frame, 2 = power system. Step completes when 2 and dialogue closed.</summary>
    private int busBuilt;

    /// <summary>Step 5 (SIR): 0 = none, 1 = propulsion delivered. Step completes when 1 and dialogue closed.</summary>
    private int propulsionDelivered;

    private InventoryController inventoryController;
    private ItemDictionary itemDictionary;

    public event Action<StepInfo> StepChanged;

    /// <summary>Fired when the  player completes the final step (KDP-D) and Phase C is complete.</summary>
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

    /// <summary>Returns item IDs needed for the next delivery in the current step (for spawn bias). Empty if no collection step.</summary>
    public List<int> GetCurrentStepRequiredItemIds()
    {
        var ids = new List<int>();
        if (steps == null || steps.Count == 0 || itemDictionary == null) return ids;
        int stepIndex = currentStepIndex;
        if (stepIndex == 0 && instrumentsBuilt < 3)
        {
            int[][] r = GetInstrumentRecipes();
            if (instrumentsBuilt >= 0 && instrumentsBuilt < r.Length) ids.AddRange(r[instrumentsBuilt]);
        }
        else if (stepIndex == 1 && commsBuilt < 2)
        {
            int[][] r = GetCommunicationsRecipes();
            if (commsBuilt >= 0 && commsBuilt < r.Length) ids.AddRange(r[commsBuilt]);
        }
        else if (stepIndex == 2 && busBuilt < 2)
        {
            int[][] r = GetBusRecipes();
            if (busBuilt >= 0 && busBuilt < r.Length) ids.AddRange(r[busBuilt]);
        }
        else if (stepIndex == 4 && propulsionDelivered < 1)
            ids.AddRange(GetPropulsionRecipe());
        return ids;
    }

    /// <summary>World position of the current step's completion NPC, or null if none (e.g. game complete). Used by objective arrow.</summary>
    public Vector3? GetCurrentStepCompletionNpcWorldPosition()
    {
        if (steps == null || steps.Count == 0 || currentStepIndex >= steps.Count) return null;
        string npcName = steps[currentStepIndex].CompletionNpc;
        if (string.IsNullOrEmpty(npcName) || !npcByName.TryGetValue(npcName, out npc npcComponent)) return null;
        return npcComponent.transform.position;
    }

    /// <summary>Current step index for arrow target locking. -1 if no step.</summary>
    public int GetCurrentStepIndexForArrow()
    {
        if (steps == null || steps.Count == 0 || currentStepIndex >= steps.Count) return -1;
        return currentStepIndex;
    }

    /// <summary>Sub-progress within current step so arrow knows when to re-pick target (e.g. after delivering one item).</summary>
    public int GetCurrentStepSubProgressForArrow()
    {
        if (steps == null || steps.Count == 0 || currentStepIndex >= steps.Count) return 0;
        int sub = 0;
        if (currentStepIndex == 0) sub = instrumentsBuilt;
        else if (currentStepIndex == 1) sub = commsBuilt;
        else if (currentStepIndex == 2) sub = busBuilt;
        else if (currentStepIndex == 4) sub = propulsionDelivered;
        return sub;
    }

    /// <summary>Completion NPC component for current step (for arrow to lock onto transform). Null if none.</summary>
    public npc GetCompletionNpcComponentForArrow()
    {
        if (steps == null || steps.Count == 0 || currentStepIndex >= steps.Count) return null;
        string npcName = steps[currentStepIndex].CompletionNpc;
        if (string.IsNullOrEmpty(npcName) || !npcByName.TryGetValue(npcName, out npc npcComponent)) return null;
        return npcComponent;
    }

    private string GetCollectObjective(int stepIndex, PhaseCStep step)
    {
        if (itemDictionary == null) return null;

        if (stepIndex == 0 && instrumentsBuilt < 3)
        {
            int[][] recipes = GetInstrumentRecipes();
            int next = instrumentsBuilt;
            if (next < 0 || next >= recipes.Length) return null;
            int[] ids = recipes[next];
            var names = new List<string>();
            foreach (int id in ids)
                names.Add(itemDictionary.GetDisplayName(id));
            return "Collect: " + string.Join(", ", names) + ". Bring to " + step.CompletionNpc + ".";
        }

        if (stepIndex == 1 && commsBuilt < 2)
        {
            int[][] recipes = GetCommunicationsRecipes();
            int next = commsBuilt;
            if (next < 0 || next >= recipes.Length) return null;
            int[] ids = recipes[next];
            var names = new List<string>();
            foreach (int id in ids)
                names.Add(itemDictionary.GetDisplayName(id));
            return "Collect: " + string.Join(", ", names) + ". Bring to " + step.CompletionNpc + ".";
        }

        if (stepIndex == 2 && busBuilt < 2)
        {
            int[][] recipes = GetBusRecipes();
            int next = busBuilt;
            if (next < 0 || next >= recipes.Length) return null;
            int[] ids = recipes[next];
            var names = new List<string>();
            foreach (int id in ids)
                names.Add(itemDictionary.GetDisplayName(id));
            return "Collect: " + string.Join(", ", names) + ". Bring to " + step.CompletionNpc + ".";
        }

        if (stepIndex == 4 && propulsionDelivered < 1)
        {
            int[] ids = GetPropulsionRecipe();
            var names = new List<string>();
            foreach (int id in ids)
                names.Add(itemDictionary.GetDisplayName(id));
            return "Collect: " + string.Join(", ", names) + ". Bring to " + step.CompletionNpc + ".";
        }

        return null;
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

        // Step 2 Communications: deliver X-band radio and laser comm to Priya
        if (safeIndex == 1 && npcName == NpcReview)
        {
            TryDeliverCommunications();
            string[] commsLines = GetCommunicationsDialogueForPriya();
            if (commsLines != null && commsLines.Length > 0)
                return commsLines;
        }

        // Step 3 Bus: deliver bus frame and power system to Marcus
        if (safeIndex == 2 && npcName == NpcBus)
        {
            TryDeliverBus();
            string[] busLines = GetBusDialogueForMarcus();
            if (busLines != null && busLines.Length > 0)
                return busLines;
        }

        // Step 5 SIR: deliver propulsion components to James
        if (safeIndex == 4 && npcName == NpcIntegration)
        {
            TryDeliverPropulsion();
            string[] sirLines = GetSIRDialogueForJames();
            if (sirLines != null && sirLines.Length > 0)
                return sirLines;
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

    private static int[][] GetCommunicationsRecipes()
    {
        return new[]
        {
            new[] { IdRadioAntenna, IdWiring, IdCircuitBoard },
            new[] { IdLaserModule, IdCameraSensor }
        };
    }

    private static int[][] GetBusRecipes()
    {
        return new[]
        {
            new[] { IdMetalAlloy, IdMetalAlloy },
            new[] { IdSolarCells, IdBattery, IdWiring }
        };
    }

    private static int[] GetPropulsionRecipe()
    {
        return new[] { IdPropellant, IdMetalAlloy, IdNavigationSystem };
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
                "All three instruments are complete. We've locked in the instrument suite for Psyche. ",
                "Next up is communications: X-band radio and laser. Priya runs that. Come back when you're ready and we'll close out this step. "
            };
        }
        return null;
    }

    /// <summary>If player has items for the next communications build, consume them and increment commsBuilt. Called when opening dialogue with Priya on step 2.</summary>
    private void TryDeliverCommunications()
    {
        if (currentStepIndex != 1 || inventoryController == null) return;
        if (commsBuilt >= 2) return;

        int[][] recipes = GetCommunicationsRecipes();
        int[] nextRecipe = recipes[commsBuilt];
        if (inventoryController.HasAllItems(nextRecipe) && inventoryController.RemoveItems(nextRecipe))
        {
            commsBuilt++;
            NotifyStepChanged();
        }
    }

    private string[] GetCommunicationsDialogueForPriya()
    {
        if (commsBuilt == 0)
        {
            return new[]
            {
                "I'm Dr. Priya Patel, communications. We need two systems to talk to Earth from deep space: X-band radio and optical laser communication.",
                "First is the X-band radio. It's our primary link for commanding the spacecraft and receiving data. I need a Radio Antenna, Wiring, and a Circuit Board. Bring those and we'll lock it in.",
                "Once you have the parts, come back and we'll get the X-band system ready."
            };
        }
        if (commsBuilt == 1)
        {
            return new[]
            {
                "X-band radio is done. Next is laser communication: we'll send data back with lasers from deep space. Much higher data rates.",
                "I need the Laser Module and a Camera Sensor. Find them and bring them here.",
                "When you've got those, we'll close out the communications suite."
            };
        }
        if (commsBuilt == 2)
        {
            return new[]
            {
                "Both systems are complete. We've locked in X-band radio and laser communication for Psyche.",
                "The bus team can move forward with a clear comms design. Come back when you're ready and we'll close out this step."
            };
        }
        return null;
    }

    private void TryDeliverBus()
    {
        if (currentStepIndex != 2 || inventoryController == null) return;
        if (busBuilt >= 2) return;
        int[][] recipes = GetBusRecipes();
        int[] nextRecipe = recipes[busBuilt];
        if (inventoryController.HasAllItems(nextRecipe) && inventoryController.RemoveItems(nextRecipe))
        {
            busBuilt++;
            NotifyStepChanged();
        }
    }

    private string[] GetBusDialogueForMarcus()
    {
        if (busBuilt == 0)
        {
            return new[]
            {
                "Marcus here. With instruments and comms set, we're building the bus: the structure that holds everything.",
                "First is the bus frame. I need two Metal Alloy units. Bring those and we'll get the frame locked in.",
                "Parts show up around the facility. Come back when you have them."
            };
        }
        if (busBuilt == 1)
        {
            return new[]
            {
                "Frame's done. Next is power: Solar Cells, a Battery, and Wiring for the distribution. That'll keep the whole spacecraft powered.",
                "Gather those and bring them here. Then we can close out the bus."
            };
        }
        if (busBuilt == 2)
        {
            return new[]
            {
                "Bus frame and power system are complete. The spacecraft bus is ready for the next phase.",
                "Priya will run the Critical Design Review. Come back when you're ready and we'll close out this step."
            };
        }
        return null;
    }

    private void TryDeliverPropulsion()
    {
        if (currentStepIndex != 4 || inventoryController == null) return;
        if (propulsionDelivered >= 1) return;
        int[] recipe = GetPropulsionRecipe();
        if (inventoryController.HasAllItems(recipe) && inventoryController.RemoveItems(recipe))
        {
            propulsionDelivered++;
            NotifyStepChanged();
        }
    }

    private string[] GetSIRDialogueForJames()
    {
        if (propulsionDelivered == 0)
        {
            return new[]
            {
                "James Thompson, systems integration. We're at the Systems Integration Review: we need to confirm every subsystem is ready.",
                "Propulsion is part of that. I need Propellant, Metal Alloy, and the Navigation System. Bring those and we'll lock in propulsion for the review.",
                "Once that's in, we can close out SIR and move to final approval."
            };
        }
        if (propulsionDelivered == 1)
        {
            return new[]
            {
                "Propulsion components are in. The Systems Integration Review can confirm all subsystems work together.",
                "Come back when you're ready and we'll close out this step. Then it's Key Decision Point D with Sarah."
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

        // Step 2 (Communications) only completes when both X-band and laser comm are delivered
        if (currentStepIndex == 1 && commsBuilt < 2)
            return;

        // Step 3 (Bus) only completes when both bus frame and power system are delivered
        if (currentStepIndex == 2 && busBuilt < 2)
            return;

        // Step 5 (SIR) only completes when propulsion is delivered
        if (currentStepIndex == 4 && propulsionDelivered < 1)
            return;

        bool wasLastStep = currentStepIndex == steps.Count - 1;
        AdvanceStep();
        if (wasLastStep)
            PhaseCComplete?.Invoke();
    }

    public void NotifyNpcInRange(string npcName)
    {
        if (currentStepIndex >= steps.Count) return;
        if (!steps[currentStepIndex].IsCompletionNpc(npcName)) return;
        if (npcByName.TryGetValue(npcName, out npc npcComponent))
            npcComponent.RefreshDialogueLines();
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
        HashSet<string> completionNpcs = new HashSet<string>();
        foreach (PhaseCStep step in steps)
            completionNpcs.Add(step.CompletionNpc);
        foreach (KeyValuePair<string, npc> entry in npcByName)
        {
            if (!completionNpcs.Contains(entry.Key)) continue;
            PhaseCStepTrigger trigger = entry.Value.GetComponent<PhaseCStepTrigger>();
            if (trigger == null)
                trigger = entry.Value.gameObject.AddComponent<PhaseCStepTrigger>();
            trigger.Initialize(entry.Key);
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
            // Step 1: Instrument Build - Dr. Sarah Chen (enthusiastic, science-focused)
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
            // Step 2: Communications (X-band radio and laser) - Dr. Priya Patel
            new PhaseCStep(
                "C2",
                "Communications",
                NpcReview,
                "Help Priya lock in X-band radio and laser communication for Psyche.",
                new Dictionary<string, string[]>
                {
                    {
                        NpcInstruments,
                        new[]
                        {
                            "Instruments are done. Next we need communications: X-band radio and laser. Priya runs that. Go see her and she'll tell you what to collect."
                        }
                    },
                    {
                        NpcBus,
                        new[]
                        {
                            "The bus waits on communications to be set. Priya's building the X-band and laser systems. Go see her when you get a chance."
                        }
                    },
                    {
                        NpcReview,
                        new[]
                        {
                            "We need X-band radio for commanding and data, and laser communication for high-rate data from deep space. Bring me the parts and we'll lock them in."
                        }
                    },
                    {
                        NpcIntegration,
                        new[]
                        {
                            "Systems integration needs a clear comms design first. Priya's locking in X-band and laser. Talk to her to get the communications step done."
                        }
                    }
                }),
            // Step 3: Spacecraft Bus Complete - Dr. Marcus Rodriguez (practical, organized)
            new PhaseCStep(
                "C3",
                "Spacecraft Bus Complete",
                NpcBus,
                "Help Marcus close out the spacecraft bus.",
                new Dictionary<string, string[]>
                {
                    {
                        NpcInstruments,
                        new[]
                        {
                            "Instruments and communications are set. Marcus is wrapping up the bus so we have somewhere to put everything. Go see him when you can."
                        }
                    },
                    {
                        NpcBus,
                        new[]
                        {
                            "By May 2020 we had the spacecraft bus finished. It's the main structure that holds all the subsystems, science instruments, and communications.",
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
            // Step 4: Critical Design Review - Dr. Priya Patel (analytical, calm)
            new PhaseCStep(
                "C4",
                "Critical Design Review",
                NpcReview,
                "Work with Priya to complete the Critical Design Review.",
                new Dictionary<string, string[]>
                {
                    {
                        NpcInstruments,
                        new[]
                        {
                            "With instruments, communications, and the bus complete, we're ready for design review. Priya runs the CDR. She'll walk you through what we're locking in."
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
                            "We had already locked in X-band radio and laser communication; the CDR confirmed those plans. When you're ready, we'll approve the review and move forward."
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
            // Step 5: Systems Integration Review - Dr. James Thompson (collaborative, big picture)
            new PhaseCStep(
                "C5",
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
            // Step 6: Key Decision Point D - Dr. Sarah Chen (closing the loop)
            new PhaseCStep(
                "C6",
                "Key Decision Point D",
                NpcInstruments,
                "Finalize Phase C with Sarah and KDP-D approval.",
                new Dictionary<string, string[]>
                {
                    {
                        NpcInstruments,
                        new[]
                        {
                            "Key Decision Point D is NASA's approval to proceed to the next phase. Instruments built, X-band and laser communication locked in, bus complete, reviews passed.",
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
