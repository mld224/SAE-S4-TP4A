using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2f, -10f);
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        /* La camera suit le vaisseau UNIQUEMENT en Y
           En X elle reste fixe a 0 (au centre de la map)
           Comme ca le vaisseau bouge visiblement a gauche/droite */
        Vector3 targetPosition = new Vector3(
            0f,                           /* X fixe au centre */
            target.position.y + offset.y, /* Y suit le vaisseau */
            offset.z                       /* Z fixe (camera) */
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}