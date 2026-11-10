using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Botón táctil en pantalla. Mantiene presionado mientras el dedo está
// encima (para moverse o disparar en forma continua) y lo suelta al
// levantar el dedo o si se sale del botón.
public class BotonTouch : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum Accion { Izquierda, Derecha, Disparo }
    public Accion accion = Accion.Disparo;

    private PlayerController jugador;
    private bool presionado;
    private Image imagen;
    private Color colorBase;
    private bool colorListo;

    private PlayerController J()
    {
        if (jugador == null) jugador = FindFirstObjectByType<PlayerController>();
        return jugador;
    }

    private void PrepararColor()
    {
        if (colorListo) return;
        imagen = GetComponent<Image>();
        if (imagen != null) colorBase = imagen.color;
        colorListo = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        presionado = true;
        PrepararColor();
        if (imagen != null) imagen.color = colorBase * 1.6f;
        Aplicar(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        presionado = false;
        PrepararColor();
        if (imagen != null) imagen.color = colorBase;
        Aplicar(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!presionado) return;
        presionado = false;
        PrepararColor();
        if (imagen != null) imagen.color = colorBase;
        Aplicar(false);
    }

    private void OnDisable()
    {
        presionado = false;
        if (imagen != null) imagen.color = colorBase;
        Aplicar(false);
    }

    private void Aplicar(bool valor)
    {
        PlayerController p = J();
        if (p == null) return;

        switch (accion)
        {
            case Accion.Izquierda: p.tocIzquierda = valor; break;
            case Accion.Derecha: p.tocDerecha = valor; break;
            case Accion.Disparo: p.tocDisparo = valor; break;
        }
    }
}
