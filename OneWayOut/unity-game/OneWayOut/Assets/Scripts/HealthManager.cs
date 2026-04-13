using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public float vieMax = 100f;
    public float vieCourante;

    /* Vie perdue par seconde (pression constante) */
    public float pertesParSeconde = 2f;

    /* Gain/perte de vie selon le choix */
    public float bonusVie = 20f;
    public float malusVie = 15f;

    /* Barre de vie UI (un Slider dans le Canvas) */
    public Slider barreDeVie;

    void Start()
    {
        vieCourante = vieMax;
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
    }

    public void MauvaisChoix()
    {
        vieCourante -= malusVie;
    }

    void GameOver()
    {
        Time.timeScale = 0;
        Debug.Log("GAME OVER");
    }
}