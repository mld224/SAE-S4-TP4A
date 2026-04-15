using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    /* ===== VALEURS DE VIE ===== */
    public float vieMax = 100f;
    public float vieCourante;
    public float pertesParSeconde = 2f;
    public float bonusVie = 20f;
    public float malusVie = 15f;

    /* ===== UI BARRE DE VIE (Image en mode Filled) ===== */
    public Image barreDeVieFill;

    /* ===== REFERENCES AUTRES MANAGERS ===== */
    public GameOverManager gameOverManager;
    public VoteManager voteManager;
    public SpriteRenderer vehiculeRenderer;

    /* 3 sprites du vaisseau (petit / moyen / grand) */
    public Sprite[] vaisseauSprites;

    /* ===== SONS =====
       audioSource : AudioSource sur le Player (Play On Awake decoche)
       sonPowerUp : glisse PowerUpPlayerSound.mp3 (vaisseau qui grandit)
       sonPowerDown : glisse PowerDownPlayerSound.mp3 (vaisseau qui retrecit)
       sonGameOver : glisse GameOverSound.mp3 (mort du vaisseau) */
    public AudioSource audioSource;
    public AudioClip sonPowerUp;
    public AudioClip sonPowerDown;
    public AudioClip sonGameOver;

    /* Empeche de declencher Game Over plusieurs fois */
    private bool gameOverDeclenche = false;

    /* Memorise le dernier sprite affiche pour detecter les changements
       de taille du vaisseau (indispensable pour declencher PowerUp/Down
       UNIQUEMENT au moment du changement) */
    private int dernierIndexSprite = -1;

    void Start()
    {
        vieCourante = vieMax;
        MettreAJourSprite();
        MettreAJourBarre();
    }

    void Update()
    {
        /* Pas de gameplay tant que le presenteur n'a pas lance la partie */
        if (!LobbyManager.GameStarted) return;

        /* La vie ne baisse pas pendant un vote */
        if (voteManager != null && voteManager.isVoting)
        {
            MettreAJourBarre();
            return;
        }

        /* Perte de vie continue (pression constante) */
        vieCourante -= pertesParSeconde * Time.deltaTime;
        vieCourante = Mathf.Clamp(vieCourante, 0, vieMax);

        MettreAJourBarre();
        MettreAJourSprite();

        if (vieCourante <= 0 && !gameOverDeclenche)
        {
            gameOverDeclenche = true;
            GameOver();
        }
    }

    /* ===== MISE A JOUR DE LA BARRE DE VIE =====
       Ajuste le fillAmount (0 a 1) et la couleur selon la vie */
    void MettreAJourBarre()
    {
        if (barreDeVieFill == null) return;

        float pourcentage = vieCourante / vieMax;
        barreDeVieFill.fillAmount = pourcentage;

        if (pourcentage > 0.66f)
            barreDeVieFill.color = new Color(0.3f, 1f, 0.3f); /* vert */
        else if (pourcentage > 0.33f)
            barreDeVieFill.color = new Color(1f, 0.6f, 0f); /* orange */
        else
            barreDeVieFill.color = new Color(1f, 0.2f, 0.2f); /* rouge */
    }

    /* ===== BON CHOIX (appele par VoteManager) ===== */
    public void BonChoix()
    {
        vieCourante += bonusVie;
        vieCourante = Mathf.Clamp(vieCourante, 0, vieMax);
        MettreAJourSprite();
    }

    /* ===== MAUVAIS CHOIX (appele par VoteManager) ===== */
    public void MauvaisChoix()
    {
        vieCourante -= malusVie;
        vieCourante = Mathf.Clamp(vieCourante, 0, vieMax);
        MettreAJourSprite();

        /* Shake de la camera a l'impact */
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(1f, 0.5f);
    }

    /* ===== MISE A JOUR DU SPRITE + SONS POWER UP/DOWN =====
       Change le sprite du vaisseau selon la vie
       Joue PowerUpSound si le vaisseau grandit
       Joue PowerDownSound si le vaisseau retrecit */
    void MettreAJourSprite()
    {
        if (vehiculeRenderer == null || vaisseauSprites.Length == 0) return;

        float pourcentage = vieCourante / vieMax;
        int index;

        if (pourcentage <= 0.33f)
            index = 0; /* petit */
        else if (pourcentage <= 0.66f)
            index = 1; /* moyen */
        else
            index = 2; /* grand */

        if (index < vaisseauSprites.Length)
            vehiculeRenderer.sprite = vaisseauSprites[index];

        /* On joue un son uniquement si le sprite change (pas au premier appel) */
        if (dernierIndexSprite != -1 && index != dernierIndexSprite)
        {
            if (index > dernierIndexSprite)
                JouerSon(sonPowerUp);   /* vaisseau grandit */
            else
                JouerSon(sonPowerDown); /* vaisseau retrecit */
        }

        dernierIndexSprite = index;
    }

    /* Utilitaire : joue un AudioClip s'il existe
       PlayOneShot permet de superposer plusieurs sons */
    void JouerSon(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    /* ===== GAME OVER ===== */
    void GameOver()
    {
        JouerSon(sonGameOver);

        if (gameOverManager != null)
            gameOverManager.DeclencherGameOver();
        else
            Time.timeScale = 0;
    }
}