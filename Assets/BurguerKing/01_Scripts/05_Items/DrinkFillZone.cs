using UnityEngine;

public class DrinkFillZone : MonoBehaviour
{
    [Header("AUDIO")]
    [SerializeField] private AudioSource fillAudioSource;


    private void Awake()
    {
        // Si no se asignó manualmente,
        // busca el AudioSource en este objeto o en el padre.
        if (fillAudioSource == null)
        {
            fillAudioSource =
                GetComponent<AudioSource>();

            if (fillAudioSource == null)
            {
                fillAudioSource =
                    GetComponentInParent<AudioSource>();
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        DrinkCup cup =
            other.GetComponent<DrinkCup>();

        // Por si el collider está en un hijo del vaso.
        if (cup == null)
        {
            cup =
                other.GetComponentInParent<DrinkCup>();
        }

        if (cup == null)
        {
            return;
        }


        // Guardamos si el vaso ya estaba lleno.
        bool wasFilled =
            cup.IsFilled;


        // Lógica original: llenar el vaso.
        cup.FillCup();


        // Solo reproduce el sonido si realmente
        // pasó de vacío a lleno.
        if (!wasFilled &&
            cup.IsFilled &&
            fillAudioSource != null)
        {
            fillAudioSource.Play();
        }
    }
}