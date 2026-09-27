using UnityEngine;

public class FriesFillZone : MonoBehaviour
{
    [Header("AUDIO")]
    [SerializeField] private AudioSource fillAudioSource;


    private void Awake()
    {
        // Si no se asignó manualmente,
        // busca el AudioSource en el objeto padre.
        if (fillAudioSource == null)
        {
            fillAudioSource =
                GetComponentInParent<AudioSource>();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        FriesBag bag =
            other.GetComponent<FriesBag>();

        // Por si el collider está en un hijo de la bolsa.
        if (bag == null)
        {
            bag =
                other.GetComponentInParent<FriesBag>();
        }

        if (bag == null)
        {
            return;
        }


        // Guardamos si ya estaba llena.
        bool wasFilled =
            bag.IsFilled;


        // Lógica original: llenar las papas.
        bag.FillBag();


        // El sonido solo se reproduce
        // si la bolsa realmente pasó de vacía a llena.
        if (!wasFilled &&
            bag.IsFilled &&
            fillAudioSource != null)
        {
            fillAudioSource.Play();
        }
    }
}