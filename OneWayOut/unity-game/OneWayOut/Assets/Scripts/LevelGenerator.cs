using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    /* References */
    public PlayerController player;
    public VoteManager voteManager;

    /* Les 3 prefabs de decor (glissés dans l'inspecteur) */
    public GameObject decorBonPrefab;
    public GameObject decorMauvaisPrefab;
    public GameObject decorNeutrePrefab;

    /* Distance en Y entre 2 embranchements */
    public float distanceEntreEmbranchements = 60f;

    /* Decalage lateral des chemins gauche/droite */
    public float ecartLateral = 6f;

    /* Longueur en Y du tunnel (temps ou le decor est visible)
       Plus c'est grand, plus le joueur a le temps de voir le decor */
    public float longueurTunnel = 25f;

    /* Distance a partir de laquelle on genere le prochain embranchement */
    public float distanceGeneration = 30f;

    /* Position Y du prochain embranchement a generer */
    private float prochainY;

    /* Liste des objets generes pour pouvoir les nettoyer */
    private List<GameObject> anciensObjets = new List<GameObject>();

    /* Compteur d'embranchements (pour ajuster la difficulte) */
    private int nbEmbranchements = 0;

    void Start()
    {
        prochainY = player.transform.position.y + distanceEntreEmbranchements;
        /* Segment droit du depart jusqu'au premier embranchement */
        GenererSegmentDroit(player.transform.position.y + 5f, prochainY, 0);
    }

    void Update()
    {
        /* Quand le vaisseau s'approche du prochain point de generation,
           on cree un nouvel embranchement */
        if (player.transform.position.y + distanceGeneration >= prochainY)
        {
            GenererEmbranchement(prochainY);
            prochainY += distanceEntreEmbranchements;
            nbEmbranchements++;
            NettoyerAnciensObjets();
        }
    }

    /* Genere des waypoints en ligne droite pour le debut du jeu */
    void GenererSegmentDroit(float startY, float endY, float posX)
    {
        List<Transform> waypoints = new List<Transform>();

        for (float y = startY; y <= endY; y += 5f)
        {
            GameObject wp = new GameObject("WP_droit_" + y);
            wp.transform.position = new Vector3(posX, y, 0);
            waypoints.Add(wp.transform);
            anciensObjets.Add(wp);
        }

        player.SuivreChemin(waypoints);
    }

    /* Genere un nouvel embranchement a la position Y donnee */
    void GenererEmbranchement(float posY)
    {
        /* Cree le GameObject de l'embranchement au centre */
        GameObject embObj = new GameObject("Embranchement_" + nbEmbranchements);
        embObj.transform.position = new Vector3(0, posY, 0);

        /* Collider en trigger pour detecter le passage du vaisseau */
        BoxCollider2D col = embObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(10f, 2f);

        /* Ajoute le script Embranchement */
        Embranchement emb = embObj.AddComponent<Embranchement>();
        emb.voteManager = voteManager;
        emb.player = player;

        /* Duree du vote diminue avec le nombre d'embranchements (pression) */
        emb.voteDuration = Mathf.Max(3, 10 - nbEmbranchements);

        /* Passe les 3 decors prefabs */
        emb.decorBonPrefab = decorBonPrefab;
        emb.decorMauvaisPrefab = decorMauvaisPrefab;
        emb.decorNeutrePrefab = decorNeutrePrefab;

        /* Choisit aleatoirement 2 lettres differentes : le bon et le mauvais
           La 3eme est automatiquement le chemin neutre */
        List<string> choixDisponibles = new List<string> { "A", "B", "C" };
        int indexBon = Random.Range(0, choixDisponibles.Count);
        emb.bonChoix = choixDisponibles[indexBon];
        choixDisponibles.RemoveAt(indexBon);

        int indexMauvais = Random.Range(0, choixDisponibles.Count);
        emb.mauvaisChoix = choixDisponibles[indexMauvais];
        /* Le 3eme (celui qui reste) est neutre */

        /* Genere les 3 chemins complets (tunnel + retour + segment droit) */
        emb.cheminA = CreerCheminComplet(-ecartLateral, posY);
        emb.cheminB = CreerCheminComplet(0, posY);
        emb.cheminC = CreerCheminComplet(ecartLateral, posY);

        anciensObjets.Add(embObj);
    }

    /* Cree un chemin complet : entree tunnel + tunnel + sortie + segment droit
       jusqu'au prochain embranchement */
    List<Transform> CreerCheminComplet(float targetX, float startY)
    {
        List<Transform> waypoints = new List<Transform>();

        /* Entree du tunnel : transition douce vers le cote */
        GameObject wp1 = new GameObject("Tunnel_entree_" + targetX);
        wp1.transform.position = new Vector3(targetX * 0.5f, startY + 4f, 0);
        waypoints.Add(wp1.transform);
        anciensObjets.Add(wp1);

        /* Arrivee plein cote */
        GameObject wp2 = new GameObject("Tunnel_arrive_" + targetX);
        wp2.transform.position = new Vector3(targetX, startY + 8f, 0);
        waypoints.Add(wp2.transform);
        anciensObjets.Add(wp2);

        /* Reste sur le cote pendant longueurTunnel
           C'est ici que le decor est visible */
        for (float y = startY + 12f; y <= startY + 8f + longueurTunnel; y += 5f)
        {
            GameObject wp = new GameObject("Tunnel_long_" + targetX + "_" + y);
            wp.transform.position = new Vector3(targetX, y, 0);
            waypoints.Add(wp.transform);
            anciensObjets.Add(wp);
        }

        float yApresTunnel = startY + 8f + longueurTunnel;

        /* Sortie du tunnel : retour progressif vers le centre */
        GameObject wpSortie = new GameObject("Tunnel_sortie_" + targetX);
        wpSortie.transform.position = new Vector3(targetX * 0.5f, yApresTunnel + 5f, 0);
        waypoints.Add(wpSortie.transform);
        anciensObjets.Add(wpSortie);

        /* Retour complet au centre */
        GameObject wpCentre = new GameObject("Tunnel_centre_" + targetX);
        wpCentre.transform.position = new Vector3(0, yApresTunnel + 10f, 0);
        waypoints.Add(wpCentre.transform);
        anciensObjets.Add(wpCentre);

        /* Segment droit jusqu'au prochain embranchement
           (evite que le vaisseau s'arrete) */
        float yProchainEmbranchement = startY + distanceEntreEmbranchements;
        for (float y = yApresTunnel + 15f; y <= yProchainEmbranchement; y += 5f)
        {
            GameObject wp = new GameObject("Apres_tunnel_" + targetX + "_" + y);
            wp.transform.position = new Vector3(0, y, 0);
            waypoints.Add(wp.transform);
            anciensObjets.Add(wp);
        }

        return waypoints;
    }

    /* Supprime les objets trop loin derriere le vaisseau (optimisation memoire) */
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

