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

    /* Public pour que d'autres scripts puissent verifier l'etat du vote
       (ex: HealthManager pour ne pas baisser la vie pendant le vote) */
    public bool isVoting = false;

    private Embranchement currentEmbranchement;
    public ScoreManager scoreManager;

    /* Reference vers le decor actuellement affiche
       Permet de le detruire avant d'en instancier un nouveau */
    private GameObject decorActuel;

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

    public void OnVoteResult(string resultat, VotesData details)
    {
        isVoting = false;

        resultText.text = "Resultat : " + resultat
            + " (A:" + details.A + " B:" + details.B + " C:" + details.C + ")";

        if (currentEmbranchement != null && health != null)
        {
            if (resultat == currentEmbranchement.bonChoix)
                health.BonChoix();
            if (scoreManager != null && resultat == currentEmbranchement.bonChoix)
                scoreManager.BonChoix();
            else
                health.MauvaisChoix();

            /* DETRUIRE L'ANCIEN DECOR avant d'en creer un nouveau */
            if (decorActuel != null)
            {
                Destroy(decorActuel);
            }

            GameObject decorPrefab = currentEmbranchement.GetDecorPrefab(resultat);
            var chemin = currentEmbranchement.GetChemin(resultat);

            if (decorPrefab != null && chemin.Count >= 4)
            {
                int indexMilieu = chemin.Count / 2;
                Vector3 positionDecor = chemin[indexMilieu].position;
                positionDecor.z = 5f;
                /* On stocke la reference pour pouvoir le detruire plus tard */
                decorActuel = Instantiate(decorPrefab, positionDecor, Quaternion.identity);
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