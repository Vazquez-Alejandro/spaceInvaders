using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPrincipal : MonoBehaviour
{
    [SerializeField] private string nombreNivel = "GameScene";

    private void Start()
    {
        GameObject boton = GameObject.Find("BotonRanking");
        if (boton == null) return;

        Button btn = boton.GetComponent<Button>();
        if (btn == null) return;

        btn.onClick.RemoveListener(AbrirRanking);
        btn.onClick.AddListener(AbrirRanking);
    }

    public void Empezar()
    {
        GestorNiveles.Reiniciar();
    }

    public void AbrirRanking()
    {
        RankingUI.MostrarRanking();
    }

    public void AbrirCreditos()
    {
        CreditosUI ui = FindFirstObjectByType<CreditosUI>();
        if (ui == null)
        {
            GameObject go = new GameObject("CreditosUI");
            ui = go.AddComponent<CreditosUI>();
            DontDestroyOnLoad(go);
        }
        ui.Mostrar();
    }
}
