using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    [SerializeField] private string nombreNivel = "GameScene";

    public void Empezar()
    {
        SceneManager.LoadScene(nombreNivel);
    }
}