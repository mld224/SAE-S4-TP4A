using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    /* Le panneau Game Over (un Panel UI cache au depart) */
    public GameObject gameOverPanel;

    /* Texte du score final affiche sur le panel */
    public TMP_Text scoreFinalText;

    /* Reference vers le ScoreManager pour recuperer le score */
    public ScoreManager scoreManager;

    void Start()
    {
        /* Au demarrage, le panel Game Over doit etre cache */
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        /* On s'assure que le jeu n'est pas en pause au demarrage
           (au cas ou on vient de recharger la scene apres un Game Over) */
        Time.timeScale = 1;
    }

    /* Appele par HealthManager quand la vie atteint 0 */
    public void DeclencherGameOver()
    {
        /* Affiche le panel */
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        /* Affiche le score final sur le panel */
        if (scoreFinalText != null && scoreManager != null)
            scoreFinalText.text = "Score final : " + scoreManager.score;

        /* Met le jeu en pause (timeScale = 0 arrete tout Time.deltaTime) */
        Time.timeScale = 0;
    }

    /* Appele par le bouton "Recommencer" du panel */
    public void Recommencer()
    {
        /* Remet le jeu en vitesse normale */
        Time.timeScale = 1;
        /* Recharge la scene courante a zero */
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}