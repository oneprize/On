using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GoalTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        DefenseManager monster = other.GetComponent<DefenseManager>();
        if (monster == null) return;

        if (DefenseGameManager.Instance != null)
        {
            DefenseGameManager.Instance.OnMonsterReachedGoal(other.gameObject);
        }
    }
}
