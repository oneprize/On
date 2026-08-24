using UnityEngine;
using UnityEngine.UI;

public class DefenseGameManager : MonoBehaviour
{
    public static DefenseGameManager Instance { get; private set; }

    [Header("패배 조건")]
    [Tooltip("이 개수만큼의 몬스터가 Goal에 도달하면 게임 오버")]
    [SerializeField] private int monsterCountToLose = 5;

    [Header("UI (선택, 비워두면 기본 문구로 표시)")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text gameOverText;

    private int reachedCount;
    private bool isGameOver;

    void Awake()
    {
        Instance = this;
    }

    public void OnMonsterReachedGoal(GameObject monster)
    {
        if (isGameOver) return;

        reachedCount++;
        Destroy(monster);

        Debug.Log($"몬스터 도착: {reachedCount}/{monsterCountToLose}");

        if (reachedCount >= monsterCountToLose)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        isGameOver = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER";
        }

        Time.timeScale = 0f;
    }

    void OnGUI()
    {
        if (!isGameOver || gameOverPanel != null) return;

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 60,
            alignment = TextAnchor.MiddleCenter
        };
        style.normal.textColor = Color.red;
        GUI.Label(new Rect(0, Screen.height / 2f - 50, Screen.width, 100), "GAME OVER", style);
    }
}
