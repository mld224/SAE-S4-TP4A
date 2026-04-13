using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    /* References */
    public PlayerController player;
    public VoteManager voteManager;

    /* Prefab de l'embranchement (un modele reutilisable)
       On le cree une fois dans Unity et le script le duplique a l'infini */
    public GameObject embranchementPrefab;

    /* Distance entre 2 embranchements en unites Y */
    public float distanceEntreEmbranchements = 30f;

    /* Ecart lateral des chemins A et C par rapport au centre */
    public float ecartLateral = 5f;

    /* Longueur des chemins A/B/C apres un embranchement */
    public float longueurChemin = 10f;

    /* Position Y du prochain embranchement a generer */
    private float prochainY;

    /* Distance en avance : on genere quand le vehicule est a moins
       de cette distance du prochain embranchement */
    public float distanceGeneration = 40f;

    /* Liste des anciens objets generes pour les supprimer */
    private List<GameObject> anciensObjets = new List<GameObject>();

    /* Compteur d'embranchements (pour augmenter la difficulte) */
    private int nbEmbranchements = 0;

    void Start()
    {
        /* On place le premier embranchement devant le vehicule */
        prochainY = player.transform.position.y + distanceEntreEmbranchements;

        /* On ajoute quelques waypoints droits pour le debut */
        GenererSegmentDroit(player.transform.position.y + 5f, prochainY - 5f);
    }

    void Update()
    {
        /* Si le vehicule approche du prochain point de generation
           on cree un nouvel embranchement */
        if (player.transform.position.y + distanceGeneration >= prochainY)
        {
            GenererEmbranchement(prochainY);
            prochainY += distanceEntreEmbranchements;
            nbEmbranchements++;

            /* Nettoyage : supprime les objets trop loin derriere */
            NettoyerAnciensObjets();
        }
    }

    /* Genere des waypoints en ligne droite entre startY et endY
       et les ajoute au chemin du vehicule */
    void GenererSegmentDroit(float startY, float endY)
    {
        List<Transform> waypoints = new List<Transform>();

        /* On place un waypoint tous les 5 unites */
        for (float y = startY; y <= endY; y += 5f)
        {
            GameObject wp = new GameObject("WP_" + y);
            wp.transform.position = new Vector3(0, y, 0);
            waypoints.Add(wp.transform);
            anciensObjets.Add(wp);
        }

        player.SuivreChemin(waypoints);
    }

    /* Genere un embranchement a la position Y donnee */
    void GenererEmbranchement(float posY)
    {
        /* Cree le GameObject de l'embranchement */
        GameObject embObj = new GameObject("Embranchement_" + nbEmbranchements);
        embObj.transform.position = new Vector3(0, posY, 0);

        /* Ajoute un Collider2D en trigger pour detecter le vehicule */
        BoxCollider2D col = embObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(10f, 2f);

        /* Ajoute le script Embranchement */
        Embranchement emb = embObj.AddComponent<Embranchement>();
        emb.voteManager = voteManager;
        emb.player = player;

        /* Duree du vote : diminue avec le nombre d'embranchements
           Commence a 10s, descend jusqu'a 3s minimum */
        emb.voteDuration = Mathf.Max(3, 10 - nbEmbranchements);

        /* Le bon choix est aleatoire a chaque embranchement */
        string[] choix = { "A", "B", "C" };
        emb.bonChoix = choix[Random.Range(0, 3)];

        /* Position apres l'embranchement */
        float apresY = posY + longueurChemin;

        /* Genere les 3 chemins */
        emb.cheminA = CreerChemin(-ecartLateral, posY + 2f, apresY);
        emb.cheminB = CreerChemin(0, posY + 2f, apresY);
        emb.cheminC = CreerChemin(ecartLateral, posY + 2f, apresY);

        anciensObjets.Add(embObj);

        /* Genere le segment droit apres la convergence
           Les 3 chemins se rejoignent au centre puis continuent tout droit */
        float convergenceY = apresY + 3f;
        GenererPointConvergence(convergenceY);
        GenererSegmentDroit(convergenceY + 5f, prochainY + distanceEntreEmbranchements - 5f);
    }

    /* Cree une liste de waypoints pour un chemin (A, B ou C)
       decalageX = position horizontale (-5 pour gauche, 0 pour centre, 5 pour droite) */
    List<Transform> CreerChemin(float decalageX, float startY, float endY)
    {
        List<Transform> waypoints = new List<Transform>();

        /* Point de depart du chemin (le vehicule s'ecarte du centre) */
        GameObject wp1 = new GameObject("Chemin_" + decalageX + "_start");
        wp1.transform.position = new Vector3(decalageX, startY + 3f, 0);
        waypoints.Add(wp1.transform);
        anciensObjets.Add(wp1);

        /* Point milieu */
        GameObject wp2 = new GameObject("Chemin_" + decalageX + "_mid");
        wp2.transform.position = new Vector3(decalageX, (startY + endY) / 2f, 0);
        waypoints.Add(wp2.transform);
        anciensObjets.Add(wp2);

        /* Point de fin (avant convergence) */
        GameObject wp3 = new GameObject("Chemin_" + decalageX + "_end");
        wp3.transform.position = new Vector3(decalageX, endY, 0);
        waypoints.Add(wp3.transform);
        anciensObjets.Add(wp3);

        return waypoints;
    }

    /* Cree un waypoint de convergence (les 3 chemins reviennent au centre) */
    void GenererPointConvergence(float posY)
    {
        List<Transform> waypoints = new List<Transform>();

        GameObject wp = new GameObject("Convergence_" + posY);
        wp.transform.position = new Vector3(0, posY, 0);
        waypoints.Add(wp.transform);
        anciensObjets.Add(wp);

        player.SuivreChemin(waypoints);
    }

    /* Supprime les GameObjects trop loin derriere le vehicule
       pour ne pas surcharger la memoire */
    void NettoyerAnciensObjets()
    {
        float limiteY = player.transform.position.y - 50f;

        for (int i = anciensObjets.Count - 1; i >= 0; i--)
        {
            if (anciensObjets[i] != null
                && anciensObjets[i].transform.position.y < limiteY)
            {
                Destroy(anciensObjets[i]);
                anciensObjets.RemoveAt(i);
            }
        }
    }
}