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

    // WebGL = se juega en el navegador. Es lo que sube a itch.io.
    [MenuItem("SpaceInvaders/Compilar Juego (WebGL - itch.io)")]
    public static void CompilarWebGL()
    {
        // itch.io no envía cabeceras Content-Encoding, así que sin compresión
        // el juego carga siempre (archivo más grande pero sin problemas).
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = true;
        Compilar(BuildTarget.WebGL, "Builds/WebGL");
    }

    // Ajuste de la página WebGL: el canvas (540x960) se escala para entrar en
    // cualquier iframe (itch.io) conservando la proporción. Sin esto, si el
    // iframe es más bajo, el juego queda recortado arriba y abajo.
    private const string CSS_AJUSTE =
        "<style id=\"ajuste-vertical\">\n" +
        "html,body{width:100%;height:100%;margin:0;overflow:hidden;background:#231F20}\n" +
        "#unity-container.unity-desktop{position:fixed!important;left:0!important;top:0!important;width:100%!important;height:100%!important;transform:none!important;display:flex!important;align-items:center;justify-content:center}\n" +
        "#unity-canvas{width:min(100vw,calc(100vh * 540 / 960))!important;height:auto!important;max-width:100vw!important;max-height:100vh!important;aspect-ratio:540/960}\n" +
        "#unity-footer{display:none!important}\n" +
        "</style>\n";

    private static void PatchearWebGL(string carpeta)
    {
        string index = System.IO.Path.Combine(carpeta, "index.html");
        if (!System.IO.File.Exists(index)) return;
        string html = System.IO.File.ReadAllText(index);
        if (html.Contains("id=\"ajuste-vertical\"")) return;
        html = html.Replace("</head>", CSS_AJUSTE + "  </head>");
        System.IO.File.WriteAllText(index, html);
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
            if (plataforma == BuildTarget.WebGL) PatchearWebGL(carpeta + "/SpaceInvaders");
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