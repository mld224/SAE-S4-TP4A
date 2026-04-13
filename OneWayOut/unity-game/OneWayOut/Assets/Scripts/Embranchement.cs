using UnityEngine;
using System.Collections.Generic;

public class Embranchement : MonoBehaviour
{
    /* Reference vers le VoteManager */
    public VoteManager voteManager;

    /* Reference vers le PlayerController */
    public PlayerController player;

    /* Duree du vote pour cet embranchement */
    public int voteDuration = 10;

    /* Les 3 chemins possibles : chacun est une liste de waypoints
       On cree 3 GameObjects vides "CheminA", "CheminB", "CheminC"
       et on met leurs enfants (des points) dans ces listes */
    public List<Transform> cheminA;
    public List<Transform> cheminB;
    public List<Transform> cheminC;

    /* Quel chemin est le "bon" pour gagner de la vie */
    public string bonChoix = "B";

    /* Evite de declencher 2 fois le meme embranchement */
    private bool used = false;

    /* Se declenche quand le vehicule entre dans la zone
       Necessite : un Collider2D (Is Trigger = true) sur cet objet
                   un Rigidbody2D + Collider2D sur le vehicule */
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!used && other.CompareTag("Player"))
        {
            used = true;

            /* On arrete le vehicule pendant le vote */
            player.canMove = false;

            /* On lance le vote */
            voteManager.LancerVote(voteDuration, this);
        }
    }

    /* Appele par VoteManager quand le vote est termine
       Donne les waypoints du chemin choisi au vehicule */
    public List<Transform> GetChemin(string resultat)
    {
        if (resultat == "A") return cheminA;
        if (resultat == "B") return cheminB;
        return cheminC;
    }
}