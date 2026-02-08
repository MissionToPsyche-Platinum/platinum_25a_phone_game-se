using UnityEngine;

public class PhaseCStepTrigger : MonoBehaviour
{
    [SerializeField] private string npcName;

    public void Initialize(string npcName)
    {
        this.npcName = npcName;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        PhaseCAssemblyController controller = PhaseCAssemblyController.Instance;
        if (controller != null)
            controller.NotifyNpcInRange(npcName);
    }
}
