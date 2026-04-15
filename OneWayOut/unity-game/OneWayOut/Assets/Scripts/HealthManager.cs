using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    /* Valeurs de vie (modifiables dans l'inspecteur) */
    public float vieMax = 100f;
public float vieCourante;

/* Vitesse de perte de vie (par seconde de jeu) */
public float pertesParSeconde = 1f;

/* Bonus et malus appliques lors d'un bon/mauvais choix */
public float bonusVie = 20f;
public float malusVie = 15f;

/* === NOUVELLE BARRE DE VIE ===
   Au lieu d'un Slider, on utilise une Image en mode "Filled"
   qui se vide/remplit selon le pourcentage de vie.
   Tu dois glisser l'Image "BarFill" dans ce champ */
public Image barreDeVieFill;

/* Reference vers GameOverManager pour declencher l'ecran Game Over */
public GameOverManager gameOverManager;

/* Reference vers VoteManager pour savoir si un vote est en cours */
public VoteManager voteManager;

/* Reference vers le SpriteRenderer du vaisseau */
public SpriteRenderer vehiculeRenderer;

/* 3 sprites du vaisseau : petit / moyen / grand */
public Sprite[] vaisseauSprites;

/* Sons joues lors d'un bon/mauvais choix (optionnels) */
public AudioSource audioSource;
public AudioClip sonBonChoix;
public AudioClip sonMauvaisChoix;

/* Flag pour eviter de declencher Game Over plusieurs fois */
private bool gameOverDeclenche = false;

void Start()
{
    vieCourante = vieMax;
    MettreAJourSprite();
    MettreAJourBarre();
}

void Update()
{
    /* La vie ne baisse pas pendant un vote */
    if (voteManager != null && voteManager.isVoting)
    {
        MettreAJourBarre();
        return;
    }

    /* Perte de vie continue */
    vieCourante -= pertesParSeconde * Time.deltaTime;
    vieCourante = Mathf.Clamp(vieCourante, 0, vieMax);

    MettreAJourBarre();

    if (vieCourante <= 0 && !gameOverDeclenche)
    {
        gameOverDeclenche = true;
        GameOver();
    }
}

/* Met a jour le Fill Amount de la barre (entre 0 et 1)
   fillAmount = 0 → barre vide
   fillAmount = 1 → barre pleine */
void MettreAJourBarre()
{
    if (barreDeVieFill != null)
    {
        barreDeVieFill.fillAmount = vieCourante / vieMax;

            float pourcentage = vieCourante / vieMax;

            Color rouge = new Color(1f, 0f, 0f);
            Color jaune = new Color(1f, 0.85f, 0.2f);
            Color vert = new Color(0f, 1f, 0f);

            if (pourcentage > 0.5f)
            {
                float t = (pourcentage - 0.5f) / 0.5f;
                barreDeVieFill.color = Color.Lerp(jaune, vert, t);
            }
            else
            {
                float t = pourcentage / 0.5f;
                barreDeVieFill.color = Color.Lerp(rouge, jaune, t);
            }
        }
}

public void BonChoix()
{
    vieCourante += bonusVie;
    vieCourante = Mathf.Clamp(vieCourante, 0, vieMax);
    MettreAJourSprite();
    MettreAJourBarre();

    if (audioSource != null && sonBonChoix != null)
        audioSource.PlayOneShot(sonBonChoix);
}

public void MauvaisChoix()
{
    vieCourante -= malusVie;
    vieCourante = Mathf.Clamp(vieCourante, 0, vieMax);
    MettreAJourSprite();
    MettreAJourBarre();

    if (audioSource != null && sonMauvaisChoix != null)
        audioSource.PlayOneShot(sonMauvaisChoix);
}

/* Change le sprite du vaisseau selon le pourcentage de vie */
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

void GameOver()
{
    if (gameOverManager != null)
        gameOverManager.DeclencherGameOver();
    else
        Time.timeScale = 0;
}
}