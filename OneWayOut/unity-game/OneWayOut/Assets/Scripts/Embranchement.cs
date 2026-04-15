using UnityEngine;
using System.Collections.Generic;

public class Embranchement : MonoBehaviour
{
    /* References vers les managers (fournies par LevelGenerator) */
    public VoteManager voteManager;
    public PlayerController player;

    /* Duree du vote en secondes */
    public int voteDuration = 10;

    /* Les 3 chemins possibles : un par choix (A, B, C) */
    public List<Transform> cheminA;
    public List<Transform> cheminB;
    public List<Transform> cheminC;

    /* Le bon choix parmi les 3 (donne un bonus de vie) */
    public string bonChoix = "B";

    /* Le mauvais choix parmi les 3 (cause une perte de vie)
       Le 3eme choix (ni bon ni mauvais) est NEUTRE : rien de special */
    public string mauvaisChoix = "A";

    /* Flag pour eviter de declencher plusieurs fois le meme embranchement */
    private bool used = false;

    /* Prefabs des 3 decors possibles (assignes par LevelGenerator) */
    public GameObject decorBonPrefab;
    public GameObject decorMauvaisPrefab;
    public GameObject decorNeutrePrefab;

    /* Se declenche quand le vaisseau entre dans la zone de vote */
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!used && other.CompareTag("Player"))
        {
            used = true;
            /* Arrete le vaisseau pendant le vote */
            player.canMove = false;
            voteManager.LancerVote(voteDuration, this);
        }
    }

    /* Retourne les waypoints correspondant au choix vote */
    public List<Transform> GetChemin(string resultat)
    {
        if (resultat == "A") return cheminA;
        if (resultat == "B") return cheminB;
        return cheminC;
    }

    /* Retourne true si le resultat est le bon choix (bonus de vie) */
    public bool EstBonChoix(string resultat)
    {
        return resultat == bonChoix;
    }

    /* Retourne true si le resultat est le mauvais choix (perte de vie) */
    public bool EstMauvaisChoix(string resultat)
    {
        return resultat == mauvaisChoix;
    }

    /* Retourne le prefab du decor adapte au choix :
       - Bon → decor bon (vert)
       - Mauvais → decor mauvais (rouge/violet)
       - Autre → decor neutre (bleu) */
    public GameObject GetDecorPrefab(string resultat)
    {
        if (EstBonChoix(resultat)) return decorBonPrefab;
        if (EstMauvaisChoix(resultat)) return decorMauvaisPrefab;
        return decorNeutrePrefab;
    }
}