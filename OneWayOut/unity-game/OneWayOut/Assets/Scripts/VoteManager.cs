using UnityEngine;
using TMPro;

public class VoteManager : MonoBehaviour
{
    /* ===== REFERENCES ===== */
    public WebSocketClient ws;
    public TMP_Text timerText;
    public TMP_Text resultText;
    public PlayerController player;
    public HealthManager health;
    public ScoreManager scoreManager;

    /* ===== VOTE ===== */
    public float voteDuration = 10f;
    private float timer;
    public bool isVoting = false;

    /* L'embranchement qui a declenche le vote en cours */
    private Embranchement currentEmbranchement;

    /* Reference vers le decor actuellement affiche */
    private GameObject decorActuel;

    /* ===== SONS =====
       audioSource : peut etre le meme que celui du Player ou un separe
       sonBonChoix : glisse GoodWaySound.mp3
       sonMauvaisChoix : glisse BadWaySound.mp3
       sonNeutre : glisse NormalWaySound.mp3 */
    public AudioSource audioSource;
    public AudioClip sonBonChoix;
    public AudioClip sonMauvaisChoix;
    public AudioClip sonNeutre;

    void Update()
    {
        /* Pendant un vote, on decremente le timer et on l'affiche */
        if (isVoting)
        {
            timer -= Time.deltaTime;
            timerText.text = "Vote : " + Mathf.Ceil(timer);

            if (timer <= 0)
                timerText.text = "Decompte des votes...";
        }
    }

    /* ===== LANCER UN VOTE =====
       Appele par Embranchement quand le vaisseau entre dans la zone */
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

    /* ===== RESULTAT DU VOTE =====
       Appele par WebSocketClient quand le serveur envoie vote_result */
    public void OnVoteResult(string resultat, VotesData details)
    {
        isVoting = false;

        resultText.text = " ← :" + details.A + " ↑ :" + details.B + " → :" + details.C;

        if (currentEmbranchement != null && health != null)
        {
            /* 3 cas avec chacun son son */
            if (currentEmbranchement.EstBonChoix(resultat))
            {
                health.BonChoix();
                if (scoreManager != null)
                    scoreManager.BonChoix();
                JouerSon(sonBonChoix);
            }
            else if (currentEmbranchement.EstMauvaisChoix(resultat))
            {
                health.MauvaisChoix();
                JouerSon(sonMauvaisChoix);
            }
            else
            {
                /* Chemin neutre : pas de bonus/malus, juste un son */
                JouerSon(sonNeutre);
            }

            /* Detruit l'ancien decor avant d'en creer un nouveau */
            if (decorActuel != null)
                Destroy(decorActuel);

            GameObject decorPrefab = currentEmbranchement.GetDecorPrefab(resultat);
            var chemin = currentEmbranchement.GetChemin(resultat);

            if (decorPrefab != null && chemin.Count >= 4)
            {
                int indexMilieu = chemin.Count / 2;
                Vector3 positionDecor = chemin[indexMilieu].position;
                positionDecor.x = 0f;
                positionDecor.z = 5f;
                decorActuel = Instantiate(decorPrefab, positionDecor, Quaternion.identity);
            }

            player.SuivreChemin(chemin);
        }

        player.canMove = true;
        Invoke(nameof(ClearResult), 3f);
    }

    /* Utilitaire pour jouer un son en securite */
    void JouerSon(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    void ClearResult()
    {
        resultText.text = "";
        timerText.text = "";
    }
}