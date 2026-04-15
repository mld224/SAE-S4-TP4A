using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    /* Le panneau Game Over (un GameObject UI cache au depart) */
    public GameObject gameOverPanel;

    /* Texte du score final */
    public TMP_Text scoreFinalText;

    /* Reference vers le ScoreManager */
    public ScoreManager scoreManager;

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    /* Appele par HealthManager quand la vie atteint 0 */
    public void DeclencherGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (scoreFinalText != null && scoreManager != null)
            scoreFinalText.text = "Score final : " + scoreManager.score;

        Time.timeScale = 0;
    }

    /* Appele par le bouton "Recommencer" */
    public void Recommencer()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}