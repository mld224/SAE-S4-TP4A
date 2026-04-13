using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    public PlayerController player;
    public VoteManager voteManager;
    public GameObject embranchementPrefab;

    public float distanceEntreEmbranchements = 30f;
    public float ecartLateral = 5f;
    public float longueurChemin = 10f;
    private float prochainY;
    public float distanceGeneration = 40f;

    private List<GameObject> anciensObjets = new List<GameObject>();
    private int nbEmbranchements = 0;

    /* Position X du dernier chemin pris (pour generer le prochain segment droit) */
    private float dernierX = 0f;

    void Start()
    {
        prochainY = player.transform.position.y + distanceEntreEmbranchements;
        /* Le segment droit va JUSQU'A l'embranchement (pas -5) */
        GenererSegmentDroit(player.transform.position.y + 5f, prochainY, 0);
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

    void GenererEmbranchement(float posY)
    {
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

        emb.cheminA = CreerChemin(currentX - ecartLateral, currentX, posY + 2f, apresY);
        emb.cheminB = CreerChemin(currentX, currentX, posY + 2f, apresY);
        emb.cheminC = CreerChemin(currentX + ecartLateral, currentX, posY + 2f, apresY);

        anciensObjets.Add(embObj);
    }

    List<Transform> CreerChemin(float targetX, float startX, float startY, float endY)
    {
        List<Transform> waypoints = new List<Transform>();

        GameObject wp1 = new GameObject("Chemin_" + targetX + "_start");
        wp1.transform.position = new Vector3((startX + targetX) / 2f, startY + 2f, 0);
        waypoints.Add(wp1.transform);
        anciensObjets.Add(wp1);

        GameObject wp2 = new GameObject("Chemin_" + targetX + "_mid");
        wp2.transform.position = new Vector3(targetX, (startY + endY) / 2f, 0);
        waypoints.Add(wp2.transform);
        anciensObjets.Add(wp2);

        GameObject wp3 = new GameObject("Chemin_" + targetX + "_end");
        wp3.transform.position = new Vector3(targetX, endY, 0);
        waypoints.Add(wp3.transform);
        anciensObjets.Add(wp3);

        /* Continue tout droit sur ce cote JUSQU'AU prochain embranchement
           On genere des waypoints droits sur la position X choisie */
        float nextEmbrY = endY + distanceEntreEmbranchements - longueurChemin;
        for (float y = endY + 5f; y <= nextEmbrY; y += 5f)
        {
            GameObject wp = new GameObject("Suite_" + targetX + "_" + y);
            wp.transform.position = new Vector3(targetX, y, 0);
            waypoints.Add(wp.transform);
            anciensObjets.Add(wp);
        }

        return waypoints;
    }

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