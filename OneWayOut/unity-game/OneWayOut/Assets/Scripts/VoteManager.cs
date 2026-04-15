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

        if (currentEmbranchement != null && health != null)
        {
            if (resultat == currentEmbranchement.bonChoix)
                health.BonChoix();
            else
                health.MauvaisChoix();

            /* Instancier le decor au milieu du chemin choisi */
            GameObject decorPrefab = currentEmbranchement.GetDecorPrefab(resultat);
            var chemin = currentEmbranchement.GetChemin(resultat);

            if (decorPrefab != null && chemin.Count >= 4)
            {
                /* Position au milieu du tunnel : on prend un waypoint vers le milieu de la liste */
                int indexMilieu = chemin.Count / 2;
                Vector3 positionDecor = chemin[indexMilieu].position;
                GameObject decorInstance = Instantiate(decorPrefab, positionDecor, Quaternion.identity);
                Destroy(decorInstance, 25f);
            }

            player.SuivreChemin(chemin);
        }

        player.canMove = true;
        Invoke(nameof(ClearResult), 3f);
    }

    void ClearResult()
    {
        resultText.text = "";
        timerText.text = "";
    }
}