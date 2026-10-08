#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildGame
{
    private static readonly string[] Escenas = { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/GameScene.unity" };

    [MenuItem("SpaceInvaders/Compilar Juego (Linux x86_64)")]
    public static void CompilarLinux()
    {
        Compilar(BuildTarget.StandaloneLinux64, "Builds/Linux");
    }

    [MenuItem("SpaceInvaders/Compilar Juego (Windows x86_64)")]
    public static void CompilarWindows()
    {
        Compilar(BuildTarget.StandaloneWindows64, "Builds/Windows");
    }

    private static void Compilar(BuildTarget plataforma, string carpeta)
    {
        BuildPlayerOptions opciones = new BuildPlayerOptions
        {
            scenes = Escenas,
            locationPathName = carpeta + "/SpaceInvaders",
            target = plataforma,
            options = BuildOptions.None
        };

        BuildReport reporte = BuildPipeline.BuildPlayer(opciones);
        BuildSummary resumen = reporte.summary;

        if (resumen.result == BuildResult.Succeeded)
        {
            Debug.Log("Build OK en: " + carpeta + " | " + resumen.totalSize + " bytes");
            EditorUtility.RevealInFinder(carpeta);
        }
        else
        {
            Debug.LogError("Falló el build: " + resumen.result);
        }
    }
}
#endif