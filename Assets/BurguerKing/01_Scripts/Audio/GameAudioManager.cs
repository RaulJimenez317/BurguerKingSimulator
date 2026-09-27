using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance;

    [Header("SONIDOS DE PEDIDOS")]
    public AudioClip pedidoCorrecto;
    public AudioClip pedidoFallido;
    public AudioClip seAcabaElTiempo;
    public AudioClip gameOver;

    [Header("AUDIO SOURCE")]
    public AudioSource audioSource;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayPedidoCorrecto()
    {
        audioSource.PlayOneShot(pedidoCorrecto);
    }

    public void PlayPedidoFallido()
    {
        audioSource.PlayOneShot(pedidoFallido);
    }

    public void PlayTiempoAgotado()
    {
        audioSource.PlayOneShot(seAcabaElTiempo);
    }

    public void PlayGameOver()
    {
        audioSource.PlayOneShot(gameOver);
    }
}