using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    /* Score actuel du jeu */
    public int score = 0;

    /* Points gagnes a chaque bon choix */
    public int pointsBonChoix = 100;

    /* Points gagnes a chaque seconde de survie */
    public float pointsParSeconde = 5f;

    /* Reference vers le VoteManager pour savoir si on vote */
    public VoteManager voteManager;

    /* Texte UI pour afficher le score */
    public TMP_Text scoreText;

    /* Compteur interne pour les points/seconde */
    private float compteurTemps = 0f;

    void Update()
    {
        /* Pas de score pendant un vote */
        if (voteManager != null && voteManager.isVoting) return;

        /* Score base sur le temps de survie */
        compteurTemps += Time.deltaTime;
        if (compteurTemps >= 1f)
        {
            score += Mathf.RoundToInt(pointsParSeconde);
            compteurTemps = 0f;
        }

        if (scoreText != null)
            scoreText.text = "Score : " + score;
    }

    /* Appele par VoteManager quand le joueur fait un bon choix */
    public void BonChoix()
    {
        score += pointsBonChoix;
    }
}