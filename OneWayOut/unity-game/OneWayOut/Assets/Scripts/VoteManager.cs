using UnityEngine;
using TMPro;

public class VoteManager : MonoBehaviour
{
    /* References des managers (relies dans l'inspecteur) */
    public WebSocketClient ws;
    public TMP_Text timerText;
    public TMP_Text resultText;
    public PlayerController player;
    public HealthManager health;
    public ScoreManager scoreManager;

    /* Duree du vote en secondes (modifiable dans l'inspecteur) */
    public float voteDuration = 10f;

    /* Timer interne du vote en cours */
    private float timer;

    /* Public pour que d'autres scripts puissent verifier l'etat du vote
       (ex: HealthManager pour ne pas baisser la vie pendant le vote) */
    public bool isVoting = false;

    /* L'embranchement qui a declenche le vote en cours */
    private Embranchement currentEmbranchement;

    /* Reference vers le decor actuellement affiche
       Permet de le detruire avant d'en instancier un nouveau */
    private GameObject decorActuel;

    void Update()
    {
        /* Pendant un vote, on decremente le timer et on l'affiche */
        if (isVoting)
        {
            timer -= Time.deltaTime;
            timerText.text = "Vote : " + Mathf.Ceil(timer);

            /* Quand le temps est ecoule, on affiche un message
               en attendant le resultat du serveur */
            if (timer <= 0)
            {
                timerText.text = "Decompte des votes...";
            }
        }
    }

    /* Appele par Embranchement quand le vaisseau entre dans la zone de vote */
    public void LancerVote(int duree, Embranchement embranchement)
    {
        currentEmbranchement = embranchement;
        voteDuration = duree;
        timer = duree;
        isVoting = true;
        resultText.text = "";
        timerText.text = "Vote : " + duree;

        /* Demande au serveur de broadcast "vote_start" aux telephones */
        ws.DemanderVote(duree);
        Debug.Log("Vote lance pour " + duree + " secondes");
    }

    /* Appele par WebSocketClient quand le serveur envoie vote_result */
    public void OnVoteResult(string resultat, VotesData details)
    {
        isVoting = false;

        resultText.text = "Resultat : " + resultat
            + " (A:" + details.A + " B:" + details.B + " C:" + details.C + ")";

        if (currentEmbranchement != null && health != null)
        {
            /* 3 cas possibles :
               1. Bon choix → bonus de vie + bonus de score
               2. Mauvais choix → perte de vie
               3. Neutre → rien de special (juste la vie qui baisse normalement) */
            if (currentEmbranchement.EstBonChoix(resultat))
            {
                health.BonChoix();
                if (scoreManager != null)
                    scoreManager.BonChoix();
            }
            else if (currentEmbranchement.EstMauvaisChoix(resultat))
            {
                health.MauvaisChoix();
            }
            /* Si c'est le chemin neutre, on ne fait rien (pas de bonus, pas de malus)
               La vie continue de baisser normalement dans Update() */

            /* Detruit l'ancien decor avant d'en creer un nouveau */
            if (decorActuel != null)
            {
                Destroy(decorActuel);
            }

            /* Recupere le decor prefab associe au choix (bon/mauvais/neutre) */
            GameObject decorPrefab = currentEmbranchement.GetDecorPrefab(resultat);
            var chemin = currentEmbranchement.GetChemin(resultat);

            /* Instancie le decor AU CENTRE DE L'ECRAN (X=0)
               pour que le joueur le voie en plein milieu quel que soit
               le chemin choisi (gauche, centre ou droite) */
            if (decorPrefab != null && chemin.Count >= 4)
            {
                int indexMilieu = chemin.Count / 2;
                Vector3 positionDecor = chemin[indexMilieu].position;
                /* CORRECTION : force X=0 pour centrer le decor
                   (la camera est a X=0, donc le decor est pile au milieu) */
                positionDecor.x = 0f;
                positionDecor.z = 5f;
                decorActuel = Instantiate(decorPrefab, positionDecor, Quaternion.identity);
            }

            /* Donne les waypoints du chemin choisi au vaisseau */
            player.SuivreChemin(chemin);
        }

        /* Le vaisseau reprend sa progression */
        player.canMove = true;

        /* Efface le texte de resultat apres 3 secondes */
        Invoke(nameof(ClearResult), 3f);
    }

    /* Efface les textes apres la fin du vote */
    void ClearResult()
    {
        resultText.text = "";
        timerText.text = "";
    }
}