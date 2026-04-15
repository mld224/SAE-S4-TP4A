using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    public PlayerController player;
    public VoteManager voteManager;
    public GameObject decorBonPrefab;
    public GameObject decorMauvaisPrefab;


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
        /* L'embranchement est TOUJOURS au centre maintenant */
        GameObject embObj = new GameObject("Embranchement_" + nbEmbranchements);
        embObj.transform.position = new Vector3(0, posY, 0);

        BoxCollider2D col = embObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(10f, 2f);

        Embranchement emb = embObj.AddComponent<Embranchement>();
        emb.voteManager = voteManager;
        emb.player = player;
        emb.voteDuration = Mathf.Max(3, 10 - nbEmbranchements);

        /* On passe les prefabs de decor a l'embranchement */
        emb.decorBonPrefab = decorBonPrefab;
        emb.decorMauvaisPrefab = decorMauvaisPrefab;

        string[] choix = { "A", "B", "C" };
        emb.bonChoix = choix[Random.Range(0, 3)];

        float apresY = posY + longueurChemin;

        /* Les chemins partent de X=0 (centre) */
        emb.cheminA = CreerChemin(-ecartLateral, 0, posY + 2f, apresY);
        emb.cheminB = CreerChemin(0, 0, posY + 2f, apresY);
        emb.cheminC = CreerChemin(ecartLateral, 0, posY + 2f, apresY);

        anciensObjets.Add(embObj);
    }

    List<Transform> CreerChemin(float targetX, float startX, float startY, float endY)
    {
        List<Transform> waypoints = new List<Transform>();

        /* Entree du tunnel */
        GameObject wp1 = new GameObject("Tunnel_" + targetX + "_entree");
        wp1.transform.position = new Vector3((startX + targetX) / 2f, startY + 2f, 0);
        waypoints.Add(wp1.transform);
        anciensObjets.Add(wp1);

        /* Milieu du tunnel (c'est ici qu'apparait le decor) */
        GameObject wp2 = new GameObject("Tunnel_" + targetX + "_milieu");
        wp2.transform.position = new Vector3(targetX, (startY + endY) / 2f, 0);
        waypoints.Add(wp2.transform);
        anciensObjets.Add(wp2);

        /* Reste dans le tunnel un moment (pour voir la consequence) */
        GameObject wp3 = new GameObject("Tunnel_" + targetX + "_fin");
        wp3.transform.position = new Vector3(targetX, endY, 0);
        waypoints.Add(wp3.transform);
        anciensObjets.Add(wp3);

        /* Sortie du tunnel : retour progressif vers le centre */
        GameObject wp4 = new GameObject("Tunnel_" + targetX + "_sortie");
        wp4.transform.position = new Vector3(targetX * 0.5f, endY + 5f, 0);
        waypoints.Add(wp4.transform);
        anciensObjets.Add(wp4);

        /* Retour complet au centre (X=0) */
        GameObject wp5 = new GameObject("Tunnel_" + targetX + "_centre");
        wp5.transform.position = new Vector3(0, endY + 10f, 0);
        waypoints.Add(wp5.transform);
        anciensObjets.Add(wp5);

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