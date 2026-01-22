using UnityEngine;

public class PhaseCStepTrigger : MonoBehaviour
{
    [SerializeField] private string npcName;
    [SerializeField] private int stepIndex;

    public void Initialize(string npcName, int stepIndex)
    {
        this.npcName = npcName;
        this.stepIndex = stepIndex;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PhaseCAssemblyController controller = PhaseCAssemblyController.Instance;
        if (controller != null)
        {
            controller.NotifyNpcInRange(npcName, stepIndex);
        }
    }
}
