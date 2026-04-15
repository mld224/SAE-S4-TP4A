using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    /* Valeurs de vie (modifiables dans l'inspecteur) */
    public float vieMax = 100f;
    public float vieCourante;

    /* Vitesse de perte de vie (par seconde de jeu) */
    public float pertesParSeconde = 1f;

    /* Bonus et malus applique lors d'un bon/mauvais choix */
    public float bonusVie = 20f;
    public float malusVie = 15f;

    /* Slider UI qui affiche la barre de vie */
    public Slider barreDeVie;

    /* Reference vers GameOverManager pour declencher l'ecran Game Over */
    public GameOverManager gameOverManager;

    /* Reference vers VoteManager pour savoir si un vote est en cours
       (la vie ne baisse pas pendant un vote) */
    public VoteManager voteManager;

    /* Reference vers le SpriteRenderer du vaisseau
       pour changer son sprite selon la vie */
    public SpriteRenderer vehiculeRenderer;

    /* 3 sprites du vaisseau : petit (faible vie), moyen, grand (haute vie) */
    public Sprite[] vaisseauSprites;

    /* Sons joues lors d'un bon/mauvais choix (optionnels) */
    public AudioSource audioSource;
    public AudioClip sonBonChoix;
    public AudioClip sonMauvaisChoix;

    /* Flag pour eviter de declencher Game Over plusieurs fois */
    private bool gameOverDeclenche = false;

    void Start()
    {
        /* Initialise la vie a son maximum */
        vieCourante = vieMax;
        MettreAJourSprite();
    }

    void Update()
    {
        /* Si un vote est en cours, on ne baisse pas la vie
           mais on met quand meme la barre a jour au cas ou */
        if (voteManager != null && voteManager.isVoting)
        {
            if (barreDeVie != null)
                barreDeVie.value = vieCourante / vieMax;
            return;
        }

        /* La vie diminue continuellement pour mettre la pression */
        vieCourante -= pertesParSeconde * Time.deltaTime;

        /* Mathf.Clamp empeche la vie de depasser 0 ou vieMax */
        vieCourante = Mathf.Clamp(vieCourante, 0, vieMax);

        /* Mise a jour de la barre de vie
           Slider.value = valeur entre 0 (vide) et 1 (plein) */
        if (barreDeVie != null)
            barreDeVie.value = vieCourante / vieMax;

        /* Si la vie atteint 0 → Game Over */
        if (vieCourante <= 0 && !gameOverDeclenche)
        {
            gameOverDeclenche = true;
            GameOver();
        }
    }

    /* Appele par VoteManager quand le joueur a fait un bon choix */
    public void BonChoix()
    {
        vieCourante += bonusVie;
        /* Clamp pour pas depasser vieMax */
        vieCourante = Mathf.Clamp(vieCourante, 0, vieMax);
        MettreAJourSprite();

        /* Joue le son de bon choix s'il est configure */
        if (audioSource != null && sonBonChoix != null)
            audioSource.PlayOneShot(sonBonChoix);
    }

    /* Appele par VoteManager quand le joueur a fait un mauvais choix */
    public void MauvaisChoix()
    {
        vieCourante -= malusVie;
        vieCourante = Mathf.Clamp(vieCourante, 0, vieMax);
        MettreAJourSprite();

        if (audioSource != null && sonMauvaisChoix != null)
            audioSource.PlayOneShot(sonMauvaisChoix);
    }

    /* Change le sprite du vaisseau selon le pourcentage de vie
       0-33% → petit vaisseau (element 0)
       34-66% → moyen (element 1)
       67-100% → grand vaisseau (element 2) */
    void MettreAJourSprite()
    {
        if (vehiculeRenderer == null || vaisseauSprites.Length == 0) return;

        float pourcentage = vieCourante / vieMax;
        int index;

        if (pourcentage <= 0.33f)
            index = 0;
        else if (pourcentage <= 0.66f)
            index = 1;
        else
            index = 2;

        if (index < vaisseauSprites.Length)
            vehiculeRenderer.sprite = vaisseauSprites[index];
    }

    /* Declenche le Game Over : affiche le panel et met le jeu en pause */
    void GameOver()
    {
        if (gameOverManager != null)
            gameOverManager.DeclencherGameOver();
        else
            Time.timeScale = 0;
    }


}