using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    /* Vitesse de deplacement du vehicule (en unites par seconde) */
    public float speed = 5f;

    /* Liste des points que le vehicule doit suivre dans l'ordre
       Remplie dynamiquement par LevelGenerator au fur et a mesure */
    public List<Transform> waypoints;

    /* Index du waypoint actuel (celui vers lequel on se dirige) */
    private int currentWaypoint = 0;

    /* Si false, le vehicule s'arrete (pendant un vote par ex) */
    public bool canMove = true;

    void Awake()
    {
        /* Awake() est appele AVANT tous les Start() des autres scripts
           Donc on s'assure que la liste existe et que l'index est a 0
           AVANT que LevelGenerator commence a ajouter des waypoints */
        if (waypoints == null)
            waypoints = new List<Transform>();
        else
            waypoints.Clear();

        currentWaypoint = 0;
        canMove = true;
    }

    void Update()
    {
        /* Si le vaisseau est arrete (ex: pendant un vote), on ne bouge pas */
        if (!canMove) return;

        /* Si on a atteint le dernier waypoint, on ne bouge plus */
        if (currentWaypoint >= waypoints.Count) return;

        /* Recupere le waypoint vers lequel on va */
        Transform target = waypoints[currentWaypoint];

        /* MoveTowards avance vers la cible sans jamais la depasser
           speed * Time.deltaTime = distance parcourue cette frame */
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        /* Calcule la direction et oriente le sprite vers la cible
           Atan2 retourne l'angle en radians, on le convertit en degres
           On soustrait 90 car le sprite par defaut pointe vers le haut */
        Vector3 dir = target.position - transform.position;
        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        /* Quand on est assez proche du waypoint (0.1 unite), on passe au suivant */
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentWaypoint++;
        }
    }

    /* Appele par VoteManager apres un vote pour donner les nouveaux waypoints
       waypointsChoisis = liste de points du chemin A, B ou C */
    public void SuivreChemin(List<Transform> waypointsChoisis)
    {
        /* On supprime tous les waypoints a partir de la position actuelle
           Ca evite que le vaisseau termine l'ancien chemin apres avoir vote */
        if (currentWaypoint < waypoints.Count)
        {
            waypoints.RemoveRange(currentWaypoint, waypoints.Count - currentWaypoint);
        }

        /* Puis on ajoute les nouveaux waypoints du chemin choisi a la suite */
        waypoints.AddRange(waypointsChoisis);
    }
}