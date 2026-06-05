using System.Data;
using UnityEngine;

public class simpleDB : MonoBehaviour
{
    private string dbname = "URI=file:gamedb.db";
    public int score;
    public string name;
    void Start()
    {
        createDB();
        var gm = GameManager.instance ?? FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.onGameOver.AddListener(OnGameOver);
        }
        else
        {
            Debug.LogWarning("simpleDB: GameManager no encontrado en Start; no se suscribirá al evento onGameOver.");
        }
    }

    private void OnDestroy()
    {
        var gm = GameManager.instance ?? FindObjectOfType<GameManager>();
        if (gm != null)
        {
            try { gm.onGameOver.RemoveListener(OnGameOver); } catch { }
        }
    }

    void createDB()
    {
        using (var connection = new Mono.Data.Sqlite.SqliteConnection(dbname))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS playerdata (id INTEGER PRIMARY KEY, name TEXT, score INTEGER)";
                command.ExecuteNonQuery();
            }
        }
    }

    // Handler llamado cuando ocurre Game Over: obtiene usuario y score y añade entry
    private void OnGameOver()
    {
        try
        {
            name = System.Environment.UserName ?? "Unknown";
        }
        catch
        {
            name = "Unknown";
        }

        if (GameManager.instance != null)
        {
            score = Mathf.RoundToInt(GameManager.instance.score);
        }
        else
        {
            score = 0;
        }

        addentry(name, score);

        // Opcional: mostrar en consola los entries actuales
        Displayentry();
    }        

    void addentry(string name, int score)
    {
        using (var connection = new Mono.Data.Sqlite.SqliteConnection(dbname))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                // Usar parámetros para evitar problemas con comillas en el nombre
                command.CommandText = "INSERT INTO playerdata (name, score) VALUES (@name, @score);";

                var pName = command.CreateParameter();
                pName.ParameterName = "@name";
                pName.Value = name;
                command.Parameters.Add(pName);

                var pScore = command.CreateParameter();
                pScore.ParameterName = "@score";
                pScore.Value = score;
                command.Parameters.Add(pScore);

                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }


void Displayentry()
    {
        using (var connection = new Mono.Data.Sqlite.SqliteConnection(dbname))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM playerdata";
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Debug.Log("ID: " + reader["id"] + ", Name: " + reader["name"] + ", Score: " + reader["score"]);
                    }
                }
            }
            connection.Close();
        }
    }

    void screenwrite(string name, int score)
    {
        // Aquí podrías implementar la lógica para mostrar el nombre y el puntaje en la pantalla del juego.
        // Esto podría ser a través de un TextMeshProUGUI, un Canvas, o cualquier otro método de UI que estés utilizando.
        // Por ejemplo:
        // myTextMeshProUGUI.SetText($"Player: {name}, Score: {score}");
        Debug.Log($"Player: {name}, Score: {score}");
    }
}