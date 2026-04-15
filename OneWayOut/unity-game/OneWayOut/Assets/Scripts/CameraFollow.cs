using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2f, -10f);
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        /* Position cible : centre en X (pour voir le vaisseau se deplacer lateralement) */
        Vector3 targetPosition = new Vector3(
            0f,
            target.position.y + offset.y,
            offset.z
        );

        /* Si CameraShake est actif, on ajoute son offset
           Ca fait trembler la camera sans casser le suivi */
        if (CameraShake.Instance != null)
            targetPosition += CameraShake.Instance.shakeOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}

