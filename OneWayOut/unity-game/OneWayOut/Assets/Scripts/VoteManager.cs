using UnityEngine;
using TMPro;

public class VoteManager : MonoBehaviour
{
    public WebSocketClient ws;
    public TMP_Text timerText;
    public TMP_Text resultText;
    public PlayerController player;
    public HealthManager health;

    public float voteDuration = 10f;
    private float timer;
    private bool isVoting = false;

    /* L'embranchement qui a declenche le vote en cours */
    private Embranchement currentEmbranchement;

    void Update()
    {
        if (isVoting)
        {
            timer -= Time.deltaTime;
            timerText.text = "Vote : " + Mathf.Ceil(timer);

            if (timer <= 0)
            {
                timerText.text = "Decompte des votes...";
            }
        }
    }

    /* Appele par Embranchement quand le vehicule entre dans la zone */
    public void LancerVote(int duree, Embranchement embranchement)
    {
        currentEmbranchement = embranchement;
        voteDuration = duree;
        timer = duree;
        isVoting = true;
        resultText.text = "";
        timerText.text = "Vote : " + duree;

        ws.DemanderVote(duree);
        Debug.Log("Vote lance pour " + duree + " secondes");
    }

    /* Appele par WebSocketClient quand le serveur envoie vote_result */
    public void OnVoteResult(string resultat, VotesData details)
    {
        isVoting = false;

        resultText.text = "Resultat : " + resultat
            + " (A:" + details.A + " B:" + details.B + " C:" + details.C + ")";

        /* Gestion de la vie : bon ou mauvais choix */
        if (currentEmbranchement != null && health != null)
        {
            if (resultat == currentEmbranchement.bonChoix)
                health.BonChoix();
            else
                health.MauvaisChoix();

            /* On donne le chemin choisi au vehicule */
            var chemin = currentEmbranchement.GetChemin(resultat);
            player.SuivreChemin(chemin);
        }

        /* On remet le vehicule en mouvement */
        player.canMove = true;

        Invoke(nameof(ClearResult), 3f);
    }

    void ClearResult()
    {
        resultText.text = "";
        timerText.text = "";
    }
}