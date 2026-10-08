using UnityEngine;
using UnityEngine.UI;

public class ControlMusica : MonoBehaviour
{
    [SerializeField] private AudioSource audioMusica;
    [SerializeField] private Image imagenEstado;
    [SerializeField] private Sprite spriteOn;
    [SerializeField] private Sprite spriteOff;

    private void Start()
    {
        if (audioMusica == null) audioMusica = GetComponent<AudioSource>();
        if (audioMusica != null && !audioMusica.isPlaying) audioMusica.Play();
        Actualizar();
    }

    public void CambiarEstado()
    {
        if (audioMusica == null) return;
        if (audioMusica.isPlaying) audioMusica.Stop();
        else audioMusica.Play();
        Actualizar();
    }

    private void Actualizar()
    {
        if (imagenEstado == null) return;
        bool encendida = audioMusica != null && audioMusica.isPlaying;
        imagenEstado.sprite = encendida ? spriteOn : spriteOff;
    }
}