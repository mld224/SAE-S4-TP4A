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
        prochainY = player.transform.position.y + distanceEntreEmbranchements;
        GenererSegmentDroit(player.transform.position.y + 5f, prochainY - 5f, 0);
    }

    void Update()
    {
        if (player.transform.position.y + distanceGeneration >= prochainY)
        {
            GenererEmbranchement(prochainY);
            prochainY += distanceEntreEmbranchements;
            nbEmbranchements++;
            NettoyerAnciensObjets();
        }
    }

    /* Genere des waypoints en ligne droite entre startY et endY
       et les ajoute au chemin du vehicule */
    void GenererSegmentDroit(float startY, float endY, float posX)
    {
        List<Transform> waypoints = new List<Transform>();

        for (float y = startY; y <= endY; y += 5f)
        {
            GameObject wp = new GameObject("WP_" + y);
            wp.transform.position = new Vector3(posX, y, 0);
            waypoints.Add(wp.transform);
            anciensObjets.Add(wp);
        }

        player.SuivreChemin(waypoints);
    }

    /* Genere un embranchement a la position Y donnee */
    void GenererEmbranchement(float posY)
    {
        /* On prend la position X actuelle du vaisseau */
        float currentX = player.transform.position.x;

        GameObject embObj = new GameObject("Embranchement_" + nbEmbranchements);
        embObj.transform.position = new Vector3(currentX, posY, 0);

        BoxCollider2D col = embObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(10f, 2f);

        Embranchement emb = embObj.AddComponent<Embranchement>();
        emb.voteManager = voteManager;
        emb.player = player;

        emb.voteDuration = Mathf.Max(3, 10 - nbEmbranchements);

        string[] choix = { "A", "B", "C" };
        emb.bonChoix = choix[Random.Range(0, 3)];

        float apresY = posY + longueurChemin;

        /* Les chemins partent de la position actuelle du vaisseau */
        emb.cheminA = CreerChemin(currentX - ecartLateral, currentX, posY + 2f, apresY);
        emb.cheminB = CreerChemin(currentX, currentX, posY + 2f, apresY);
        emb.cheminC = CreerChemin(currentX + ecartLateral, currentX, posY + 2f, apresY);

        anciensObjets.Add(embObj);
    }

    /* Cree une liste de waypoints pour un chemin (A, B ou C)
       decalageX = position horizontale (-5 pour gauche, 0 pour centre, 5 pour droite) */
    List<Transform> CreerChemin(float targetX, float startX, float startY, float endY)
    {
        List<Transform> waypoints = new List<Transform>();

        /* Transition vers le cote choisi */
        GameObject wp1 = new GameObject("Chemin_" + targetX + "_start");
        wp1.transform.position = new Vector3((startX + targetX) / 2f, startY + 2f, 0);
        waypoints.Add(wp1.transform);
        anciensObjets.Add(wp1);

        /* Arrive sur la trajectoire choisie */
        GameObject wp2 = new GameObject("Chemin_" + targetX + "_mid");
        wp2.transform.position = new Vector3(targetX, (startY + endY) / 2f, 0);
        waypoints.Add(wp2.transform);
        anciensObjets.Add(wp2);

        /* Continue tout droit */
        GameObject wp3 = new GameObject("Chemin_" + targetX + "_end");
        wp3.transform.position = new Vector3(targetX, endY, 0);
        waypoints.Add(wp3.transform);
        anciensObjets.Add(wp3);

        /* Continue encore */
        GameObject wp4 = new GameObject("Chemin_" + targetX + "_far");
        wp4.transform.position = new Vector3(targetX, endY + 10f, 0);
        waypoints.Add(wp4.transform);
        anciensObjets.Add(wp4);

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