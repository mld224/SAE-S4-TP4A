using UnityEngine;
using TMPro;

public class VoteManager : MonoBehaviour
{
    /* Reference vers le WebSocketClient pour envoyer START_VOTE */
    public WebSocketClient ws;

    /* Textes UI */
    public TMP_Text timerText;
    public TMP_Text resultText;

    /* Duree du vote en secondes */
    public float voteDuration = 10f;
    private float timer;

    /* true = un vote est en cours, false = le vehicule roule */
    private bool isVoting = false;

    /* Reference vers le joueur (le vehicule) */
    public PlayerController player;

    void Start()
    {
        /* Lance un premier vote 3 secondes apres le demarrage
           puis relance un vote 5 secondes apres chaque resultat
           C'est temporaire, a remplacer par la detection d'embranchement */
        Invoke(nameof(LancerVoteAuto), 3f);
    }

    void LancerVoteAuto()
    {
        LancerVote(10);
    }

    void Update()
    {
        /* Si un vote est en cours, on decremente le timer
           et on l'affiche a l'ecran */
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

    /* Appelee quand le vehicule arrive a un embranchement
       C'est cette fonction que tu appelleras depuis ton code de niveau
       Exemple : quand le vehicule entre dans une zone de vote,
       tu fais voteManager.LancerVote() */
    public void LancerVote(int duree = 10)
    {
        voteDuration = duree;
        timer = duree;
        isVoting = true;
        resultText.text = "";
        timerText.text = "Vote : " + duree;

        /* On demande au serveur de lancer le vote
           Le serveur va broadcast "vote_start" aux telephones */
        ws.DemanderVote(duree);

        Debug.Log("Vote lance pour " + duree + " secondes");
    }

    /* Appelee par WebSocketClient quand le serveur envoie "vote_result"
       resultat = "A", "B" ou "C" (le choix gagnant)
       details = le nombre de votes pour chaque choix */
    public void OnVoteResult(string resultat, VotesData details)
    {
        isVoting = false;

        resultText.text = "Resultat : " + resultat
            + " (A:" + details.A + " B:" + details.B + " C:" + details.C + ")";

        Debug.Log("Vote termine -> " + resultat);

        /* On bouge le vehicule selon le resultat */
        ApplyDecision(resultat);

        /* On efface le texte apres 3 secondes */
        Invoke(nameof(ClearResult), 3f);
    }

    void ApplyDecision(string result)
    {
        if (result == "A")
            player.MoveLeft();
        else if (result == "B")
            player.MoveForward();
        else if (result == "C")
            player.MoveRight();
    }

    void ClearResult()
    {
        resultText.text = "";
        timerText.text = "";
        /* Relance un vote 5 secondes apres le resultat (temporaire) */
        Invoke(nameof(LancerVoteAuto), 5f);
    }
}