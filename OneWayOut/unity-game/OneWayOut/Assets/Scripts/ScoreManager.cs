using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    /* Score actuel du jeu */
    public int score = 0;

    /* Points gagnes a chaque bon choix */
    public int pointsBonChoix = 100;

    /* Points gagnes par seconde de survie */
    public float pointsParSeconde = 5f;

    /* Reference vers VoteManager pour ne pas compter pendant un vote */
    public VoteManager voteManager;

    /* Texte UI pour afficher le score */
    public TMP_Text scoreText;

    /* Compteur interne pour gerer les points/seconde */
    private float compteurTemps = 0f;

    void Start()
    {
        /* Reset du score au demarrage (au cas ou) */
        score = 0;
        compteurTemps = 0f;
        if (scoreText != null)
            scoreText.text = "Score : 0";
    }

    void Update()
    {
        /* Pas de score pendant un vote */
        if (voteManager != null && voteManager.isVoting) return;

        /* Chaque seconde ecoulee, on ajoute les points de survie */
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

