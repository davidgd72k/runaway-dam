using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.Data;
using TMPro;
using UnityEngine;

public class scoreboard : MonoBehaviour
{
    // Lista de referencias a los 5 TMP_Text (compatible con TextMeshPro y TextMeshProUGUI)
    public List<TMP_Text> entries = new List<TMP_Text>(5);

    // Ruta de la base de datos (compatible con simpleDB)
    private string dbname = "URI=file:gamedb.db";

    private void Start()
    {
        // Recolectar todos los TMP en los hijos (incluso inactivos)
        var found = GetComponentsInChildren<TMP_Text>(true);
        entries = new List<TMP_Text>(5);
        for (int i = 0; i < found.Length && entries.Count < 5; i++)
            entries.Add(found[i]);

        // Si hay menos de 5 entradas, rellenar con nulls para mantener el tamaño
        while (entries.Count < 5) entries.Add(null);
    }

    //Consulta los 5 mejores registros de la tabla playerdata y escribe los resultados
    //en los TextMeshProUGUI recogidos en las entradas.
    public void ShowTopScores(int top = 5)
    {
        // Si las entradas no se han recogido (objeto inactivo o Start no llamado), recoger ahora
        if (entries == null || entries.Count == 0 || entries.TrueForAll(e => e == null))
        {
            var found = GetComponentsInChildren<TMP_Text>(true);
            entries = new List<TMP_Text>(5);
            for (int i = 0; i < found.Length && entries.Count < top; i++)
                entries.Add(found[i]);

            while (entries.Count < top) entries.Add(null);
        }

        try
        {
            using (var connection = new SqliteConnection(dbname))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT name, score FROM playerdata ORDER BY score DESC LIMIT {top}";
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        int i = 0;
                        while (reader.Read() && i < entries.Count)
                        {
                            var name = reader["name"]?.ToString() ?? "--";
                            var score = reader["score"]?.ToString() ?? "0";
                            if (entries[i] != null)
                                entries[i].SetText($"{i + 1}. {name}: {score}");
                            i++;
                        }

                        // Si hay menos filas que entradas, limpiar las restantes
                        for (; i < entries.Count; i++)
                        {
                            if (entries[i] != null)
                                entries[i].SetText(string.Empty);
                        }
                    }
                }
                connection.Close();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"scoreboard.ShowTopScores: error al leer la BBDD: {ex.Message}");
        }
    }

    private void OnEnable()
    {
        // Cuando el GameObject se activa (por ejemplo al abrir Game Over UI), mostrar los TOP
        ShowTopScores(5);
    }
}
