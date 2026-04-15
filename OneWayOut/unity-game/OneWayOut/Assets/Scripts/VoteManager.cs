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
            /* CORRECTION DU BUG : if/else bien separes
               Si bon choix → bonus vie + bonus score
               Sinon → perte de vie */
            if (resultat == currentEmbranchement.bonChoix)
            {
                health.BonChoix();
                if (scoreManager != null)
                    scoreManager.BonChoix();
            }
            else
            {
                health.MauvaisChoix();
            }

            /* Detruit l'ancien decor avant d'en creer un nouveau
               (evite l'empilement des fonds qu'on avait avant) */
            if (decorActuel != null)
            {
                Destroy(decorActuel);
            }

            /* Recupere le decor prefab associe au choix (bon/mauvais) */
            GameObject decorPrefab = currentEmbranchement.GetDecorPrefab(resultat);
            var chemin = currentEmbranchement.GetChemin(resultat);

            /* Instancie le decor au milieu du chemin choisi (au milieu du tunnel)
               pour que le joueur voit le decor au moment ou il passe dedans */
            if (decorPrefab != null && chemin.Count >= 4)
            {
                int indexMilieu = chemin.Count / 2;
                Vector3 positionDecor = chemin[indexMilieu].position;
                positionDecor.z = 5f; /* Force le decor derriere le vaisseau */
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