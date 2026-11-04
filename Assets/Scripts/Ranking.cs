using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EntradaRanking
{
    public string nombre;
    public int puntaje;
    public int monedas;
    public int nivel;
}

[Serializable]
public class DatosRanking
{
    public List<EntradaRanking> entradas = new List<EntradaRanking>();
}

public static class Ranking
{
    public const int MAX_ENTRADAS = 10;
    private const string CLAVE = "RankingSpaceInvaders";

    public static List<EntradaRanking> Obtener()
    {
        string json = PlayerPrefs.GetString(CLAVE, "");
        if (string.IsNullOrEmpty(json)) return new List<EntradaRanking>();

        DatosRanking datos = JsonUtility.FromJson<DatosRanking>(json);
        if (datos == null || datos.entradas == null) return new List<EntradaRanking>();

        datos.entradas.Sort((a, b) => b.puntaje.CompareTo(a.puntaje));
        return datos.entradas;
    }

    public static bool EntraEnTop(int puntaje)
    {
        List<EntradaRanking> entradas = Obtener();
        if (entradas.Count < MAX_ENTRADAS) return true;
        return puntaje > entradas[entradas.Count - 1].puntaje;
    }

    public static void Agregar(string nombre, int puntaje, int monedas, int nivel)
    {
        List<EntradaRanking> entradas = Obtener();
        entradas.Add(new EntradaRanking
        {
            nombre = string.IsNullOrWhiteSpace(nombre) ? "JUGADOR" : nombre.Trim(),
            puntaje = Mathf.Max(0, puntaje),
            monedas = Mathf.Max(0, monedas),
            nivel = Mathf.Max(1, nivel)
        });
        entradas.Sort((a, b) => b.puntaje.CompareTo(a.puntaje));
        if (entradas.Count > MAX_ENTRADAS)
            entradas.RemoveRange(MAX_ENTRADAS, entradas.Count - MAX_ENTRADAS);

        DatosRanking datos = new DatosRanking { entradas = entradas };
        PlayerPrefs.SetString(CLAVE, JsonUtility.ToJson(datos));
        PlayerPrefs.Save();
    }

    public static void Limpiar()
    {
        PlayerPrefs.DeleteKey(CLAVE);
        PlayerPrefs.Save();
    }
}
