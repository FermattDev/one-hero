using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Placed once per boss level: loads the next level when every boss is defeated.
public class LevelProgression : MonoBehaviour
{
    [SerializeField] private string levelTitle;
    [SerializeField] private string nextSceneName;
    [SerializeField] private string firstSceneName;
    [SerializeField] private float loadDelay = 2.5f;

    private readonly HashSet<EnemyController> bosses = new HashSet<EnemyController>();
    private bool completed;

    private bool IsFinalLevel => string.IsNullOrEmpty(nextSceneName);

    private void Start()
    {
        foreach (var enemy in FindObjectsByType<EnemyController>())
        {
            if (enemy.IsBoss)
            {
                bosses.Add(enemy);
            }
        }
    }

    private void OnEnable()
    {
        EnemyController.BossDefeated += OnBossDefeated;
    }

    private void OnDisable()
    {
        EnemyController.BossDefeated -= OnBossDefeated;
    }

    private void OnBossDefeated(EnemyController boss)
    {
        bosses.Remove(boss);
        if (completed || bosses.Count > 0)
        {
            return;
        }

        completed = true;
        if (!IsFinalLevel)
        {
            StartCoroutine(LoadAfterDelay(nextSceneName));
        }
    }

    private IEnumerator LoadAfterDelay(string sceneName)
    {
        yield return new WaitForSeconds(loadDelay);
        SceneManager.LoadScene(sceneName);
    }

    private void Update()
    {
        // After the last boss falls the player can start over.
        if (completed && IsFinalLevel && Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene(firstSceneName);
        }
    }

    private void OnGUI()
    {
        var title = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
        title.normal.textColor = Color.white;
        GUI.Label(new Rect(16, 8, 600, 34), levelTitle, title);

        if (!completed)
        {
            return;
        }

        var banner = new GUIStyle(GUI.skin.label) { fontSize = 48, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        banner.normal.textColor = Color.yellow;
        GUI.Label(new Rect(0, Screen.height * 0.35f, Screen.width, 70), IsFinalLevel ? "YOU WIN!" : "BOSS DEFEATED", banner);

        if (IsFinalLevel)
        {
            var hint = new GUIStyle(banner) { fontSize = 22 };
            hint.normal.textColor = Color.white;
            GUI.Label(new Rect(0, Screen.height * 0.35f + 70, Screen.width, 40), "Press Enter to play again", hint);
        }
    }
}
