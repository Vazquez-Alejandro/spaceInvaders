using UnityEngine;
using UnityEngine.UI;

public class ControlEfectos : MonoBehaviour
{
    public static bool Activados { get; private set; } = true;

    [SerializeField] private Image imagenEstado;
    [SerializeField] private Sprite spriteOn;
    [SerializeField] private Sprite spriteOff;

    private void Start()
    {
        Actualizar();
    }

    public void CambiarEstado()
    {
        Activados = !Activados;
        Actualizar();
    }

    private void Actualizar()
    {
        if (imagenEstado == null) return;
        imagenEstado.sprite = Activados ? spriteOn : spriteOff;
    }
}