using UnityEngine;
using System.Collections.Generic;

public class Embranchement : MonoBehaviour
{
    public VoteManager voteManager;
    public PlayerController player;
    public int voteDuration = 10;

    public List<Transform> cheminA;
    public List<Transform> cheminB;
    public List<Transform> cheminC;

    public string bonChoix = "B";
    private bool used = false;

    /* Prefabs des decors du tunnel (assigné par LevelGenerator) */
    public GameObject decorBonPrefab;
    public GameObject decorMauvaisPrefab;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!used && other.CompareTag("Player"))
        {
            used = true;
            player.canMove = false;
            voteManager.LancerVote(voteDuration, this);
        }
    }

    public List<Transform> GetChemin(string resultat)
    {
        if (resultat == "A") return cheminA;
        if (resultat == "B") return cheminB;
        return cheminC;
    }

    public bool EstBonChoix(string resultat)
    {
        return resultat == bonChoix;
    }

    /* Retourne le prefab du decor correspondant au choix */
    public GameObject GetDecorPrefab(string resultat)
    {
        return EstBonChoix(resultat) ? decorBonPrefab : decorMauvaisPrefab;
    }
}