using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    /* Singleton accessible partout via CameraShake.Instance */
    public static CameraShake Instance;

    /* Offset applique a la camera pendant le shake
       Ce sera lu par CameraFollow pour ne pas ecraser le shake */
    public Vector3 shakeOffset = Vector3.zero;

    void Awake()
    {
        Instance = this;
    }

    /* Appele par d'autres scripts (HealthManager par ex)
       duree : combien de temps le shake dure (en secondes)
       intensite : amplitude du tremblement (en unites Unity) */
    public void Shake(float duree = 0.3f, float intensite = 0.3f)
    {
        StartCoroutine(ShakeCoroutine(duree, intensite));
    }

    IEnumerator ShakeCoroutine(float duree, float intensite)
    {
        float timer = 0f;

        while (timer < duree)
        {
            /* Tirage aleatoire d'un offset dans un rayon "intensite" */
            float x = Random.Range(-1f, 1f) * intensite;
            float y = Random.Range(-1f, 1f) * intensite;
            shakeOffset = new Vector3(x, y, 0);

            timer += Time.deltaTime;
            yield return null;
        }

        /* A la fin, on remet l'offset a zero */
        shakeOffset = Vector3.zero;
    }
}