using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class DefenseManager : MonoBehaviour
{
    public Transform goal;
    
    [SerializeField]private NavMeshAgent agent;

    private float moveSpeed = 5f;

    public NavMeshSurface surface;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
    }

    void Update()
    {
        HandleChase();       
    }

    private void HandleChase()
    {
        if (goal != null)
        {
            // 목표 지점의 위치를 목적지로 설정                       
            agent.SetDestination(goal.position);
        }
    }

    public void RebuildNavMesh()
    {
        surface.BuildNavMesh();
        Debug.Log("NavMesh Rebuild");
    }
}
