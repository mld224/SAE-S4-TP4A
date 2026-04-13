using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    /* Vitesse de deplacement du vehicule */
    public float speed = 5f;

    /* Liste des points que le vehicule doit suivre dans l'ordre
       On les place dans la scene Unity (des GameObjects vides)
       et on les glisse dans cette liste dans l'inspecteur */
    public List<Transform> waypoints;

    /* Index du waypoint actuel (celui vers lequel on se dirige) */
    private int currentWaypoint = 0;

    /* Si false, le vehicule s'arrete (pendant un vote par ex) */
    public bool canMove = true;

    void Update()
    {
        if (!canMove) return;
        if (currentWaypoint >= waypoints.Count) return;

        /* On deplace le vehicule vers le waypoint actuel
           MoveTowards avance d'un pas (speed * deltaTime) vers la cible
           sans jamais la depasser */
        Transform target = waypoints[currentWaypoint];
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        /* Rotation du vehicule pour qu'il regarde vers le waypoint
           Atan2 calcule l'angle entre la position actuelle et la cible
           On soustrait 90 car le sprite pointe vers le haut par defaut */
        Vector3 dir = target.position - transform.position;
        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        /* Si on est arrive au waypoint (distance < 0.1)
           on passe au suivant */
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentWaypoint++;
        }
    }

    /* Appele par VoteManager pour inserer les waypoints du chemin choisi
       waypointsChoisis = la liste de points du chemin A, B ou C */
    public void SuivreChemin(List<Transform> waypointsChoisis)
    {
        /* On insere les nouveaux waypoints juste apres la position actuelle */
        waypoints.InsertRange(currentWaypoint, waypointsChoisis);
    }
}