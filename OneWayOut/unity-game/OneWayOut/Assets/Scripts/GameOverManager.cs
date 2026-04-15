using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public TMP_Text scoreFinalText;
    public ScoreManager scoreManager;

    /* Reference vers WebSocketClient pour prevenir le serveur du Game Over */
    public WebSocketClient ws;

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Time.timeScale = 1;
    }

    /* Appele par HealthManager quand la vie atteint 0 */
    public void DeclencherGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (scoreFinalText != null && scoreManager != null)
            scoreFinalText.text = "Score final : " + scoreManager.score;

        /* Previent le serveur pour afficher le bouton Recommencer sur les tel */
        if (ws != null)
            ws.SendGameOver();

        Time.timeScale = 0;
    }

    /* Recommencer via le bouton Unity (garde la compat si vous l'utilisez) */
    public void Recommencer()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}