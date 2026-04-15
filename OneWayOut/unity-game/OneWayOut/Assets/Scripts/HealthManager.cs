using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public float vieMax = 100f;
    public float vieCourante;
    public float pertesParSeconde = 1f;
    public float bonusVie = 20f;
    public float malusVie = 15f;
    public Slider barreDeVie;

    /* Reference vers le SpriteRenderer du vehicule */
    public SpriteRenderer vehiculeRenderer;

    /* Tableau de sprites : du plus petit au plus grand
       Element 0 = petit (mauvais etat)
       Element 1 = moyen (etat normal)
       Element 2 = grand (bon etat) */
    public Sprite[] vaisseauSprites;

    void Start()
    {
        vieCourante = vieMax;
        MettreAJourSprite();
    }

    void Update()
    {
        vieCourante -= pertesParSeconde * Time.deltaTime;
        vieCourante = Mathf.Clamp(vieCourante, 0, vieMax);

        if (barreDeVie != null)
            barreDeVie.value = vieCourante / vieMax;

        if (vieCourante <= 0)
            GameOver();
    }

    public void BonChoix()
    {
        vieCourante += bonusVie;
        MettreAJourSprite();
    }

    public void MauvaisChoix()
    {
        vieCourante -= malusVie;
        MettreAJourSprite();
    }

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
        Time.timeScale = 0;
        Debug.Log("GAME OVER");
    }
}