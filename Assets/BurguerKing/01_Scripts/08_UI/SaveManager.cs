using System;
using System.Collections.Generic;
using UnityEngine;

public static class SaveManager
{
    // =========================================================
    // CLAVES DE PLAYER PREFS
    // =========================================================

    private const string SaveKey =
        "BurgerKingVR_SaveData_v1";

    private const string GameSaveKey =
        "BurgerKingVR_GameSave";

    private const string LoadRequestKey =
        "BurgerKingVR_LoadGameRequested";


    // =========================================================
    // CONFIGURACIÓN
    // =========================================================

    private const int MaxRankingEntries = 10;

    private const int CurrentGameSaveVersion = 2;


    // =========================================================
    // RANKING
    // =========================================================

    [Serializable]
    public class RankingEntry
    {
        public int score;
        public int correctOrders;
        public int incorrectOrders;
        public string date;


        public RankingEntry()
        {
        }


        public RankingEntry(
            int score,
            int correctOrders,
            int incorrectOrders)
        {
            this.score = score;
            this.correctOrders = correctOrders;
            this.incorrectOrders = incorrectOrders;

            date = DateTime.Now.ToString(
                "yyyy-MM-dd HH:mm"
            );
        }
    }


    // =========================================================
    // VECTOR 3 SERIALIZABLE
    // =========================================================

    [Serializable]
    public class Vector3Data
    {
        public float x;
        public float y;
        public float z;


        public Vector3Data()
        {
            x = 0f;
            y = 0f;
            z = 0f;
        }


        public Vector3Data(Vector3 value)
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }


        public Vector3 ToVector3()
        {
            return new Vector3(
                x,
                y,
                z
            );
        }
    }


    // =========================================================
    // QUATERNION SERIALIZABLE
    // =========================================================

    [Serializable]
    public class QuaternionData
    {
        public float x;
        public float y;
        public float z;
        public float w;


        public QuaternionData()
        {
            x = 0f;
            y = 0f;
            z = 0f;
            w = 1f;
        }


        public QuaternionData(Quaternion value)
        {
            x = value.x;
            y = value.y;
            z = value.z;
            w = value.w;
        }


        public Quaternion ToQuaternion()
        {
            return new Quaternion(
                x,
                y,
                z,
                w
            );
        }
    }


    // =========================================================
    // ESTADO DEL PEDIDO
    // =========================================================

    [Serializable]
    public class OrderSaveData
    {
        public bool hasOrder;

        public string recipeName = "";

        public int recipeIndex = -1;

        public float remainingTime;

        public float currentOrderTimeLimit;

        public bool orderActive;

        public int orderNumber;

        public int difficulty;


        public OrderSaveData()
        {
        }
    }


    // =========================================================
    // ESTADO DE CARNE
    // =========================================================

    [Serializable]
    public class MeatSaveData
    {
        public bool hasMeatCooking;

        // Se guardará como int para no depender
        // directamente del enum MeatCooking.
        //
        // 0 = Raw
        // 1 = Cooking
        // 2 = Ready
        // 3 = Burned

        public int cookingState = 0;

        public float cookingProgress;

        public bool onGrill;


        public MeatSaveData()
        {
        }
    }


    // =========================================================
    // INGREDIENTE DE LA HAMBURGUESA
    // =========================================================

    [Serializable]
    public class AssemblyIngredientSaveData
    {
        public string ingredientName = "";

        public string objectName = "";

        public Vector3Data position =
            new Vector3Data();

        public QuaternionData rotation =
            new QuaternionData();

        public Vector3Data scale =
            new Vector3Data(
                Vector3.one
            );

        public MeatSaveData meatState =
            new MeatSaveData();


        public AssemblyIngredientSaveData()
        {
        }
    }


    // =========================================================
    // ESTADO DE LA HAMBURGUESA
    // =========================================================

    [Serializable]
    public class AssemblySaveData
    {
        public bool hasAssemblyData;

        public bool burgerCompleted;

        public bool hasMeat;

        public bool meatCookedCorrectly;

        public int meatState;

        public bool finishedBurgerActive;

        public Vector3Data finishedBurgerPosition =
            new Vector3Data();

        public QuaternionData finishedBurgerRotation =
            new QuaternionData();

        public List<AssemblyIngredientSaveData>
            placedIngredients =
                new List<AssemblyIngredientSaveData>();


        public AssemblySaveData()
        {
        }
    }


    // =========================================================
    // ESTADO DEL CLIENTE
    // =========================================================

    [Serializable]
    public class CustomerSaveData
    {
        public bool exists;

        public Vector3Data position =
            new Vector3Data();

        public QuaternionData rotation =
            new QuaternionData();

        public bool arrived;

        public bool finishedLooking;

        public bool leaving;

        public bool finishedTurning;

        public bool orderPanelActive;


        public CustomerSaveData()
        {
        }
    }


    // =========================================================
    // OBJETOS DEL MUNDO
    // PAPAS / REFRESCO / INGREDIENTES / CARNE
    // =========================================================

    [Serializable]
    public class WorldItemSaveData
    {
        public string itemType = "";

        public string prefabName = "";

        public string objectName = "";

        public bool active = true;


        public Vector3Data position =
            new Vector3Data();

        public QuaternionData rotation =
            new QuaternionData();

        public Vector3Data scale =
            new Vector3Data(
                Vector3.one
            );


        // Papas / refresco
        public bool isFilled;


        // Carne
        public bool hasMeatCooking;

        public int cookingState;

        public float cookingProgress;

        public bool onGrill;


        public WorldItemSaveData()
        {
        }
    }


    // =========================================================
    // GUARDADO DE PARTIDA
    // =========================================================

    [Serializable]
    public class GameSaveData
    {
        // -----------------------------------------------------
        // VERSIÓN DEL GUARDADO
        // -----------------------------------------------------

        public int saveVersion =
            CurrentGameSaveVersion;


        // -----------------------------------------------------
        // ESTADÍSTICAS
        // -----------------------------------------------------

        public int score;

        public int highestScore;

        public int ordersCompleted;

        public int correctOrders;

        public int incorrectOrders;


        // -----------------------------------------------------
        // INDICA SI EXISTE ESTADO COMPLETO
        // -----------------------------------------------------

        public bool hasFullGameState;


        // -----------------------------------------------------
        // PEDIDO
        // -----------------------------------------------------

        public OrderSaveData order =
            new OrderSaveData();


        // -----------------------------------------------------
        // HAMBURGUESA
        // -----------------------------------------------------

        public AssemblySaveData assembly =
            new AssemblySaveData();


        // -----------------------------------------------------
        // CLIENTE
        // -----------------------------------------------------

        public CustomerSaveData customer =
            new CustomerSaveData();


        // -----------------------------------------------------
        // OBJETOS ACTIVOS
        // -----------------------------------------------------

        public List<WorldItemSaveData> worldItems =
            new List<WorldItemSaveData>();


        // -----------------------------------------------------
        // FECHA
        // -----------------------------------------------------

        public string saveDate;


        public GameSaveData()
        {
            saveVersion =
                CurrentGameSaveVersion;

            saveDate =
                DateTime.Now.ToString(
                    "yyyy-MM-dd HH:mm"
                );
        }


        // -----------------------------------------------------
        // CONSTRUCTOR ANTIGUO
        //
        // IMPORTANTE:
        // Se mantiene para NO romper PauseMenuManager.
        // -----------------------------------------------------

        public GameSaveData(
            int score,
            int highestScore,
            int ordersCompleted,
            int correctOrders,
            int incorrectOrders)
        {
            this.score =
                score;

            this.highestScore =
                highestScore;

            this.ordersCompleted =
                ordersCompleted;

            this.correctOrders =
                correctOrders;

            this.incorrectOrders =
                incorrectOrders;


            saveVersion =
                CurrentGameSaveVersion;


            // Hasta que conectemos OrderManager,
            // BurgerAssembly, Customer, etc.,
            // este guardado seguirá siendo básico.
            hasFullGameState =
                false;


            order =
                new OrderSaveData();

            assembly =
                new AssemblySaveData();

            customer =
                new CustomerSaveData();

            worldItems =
                new List<WorldItemSaveData>();


            saveDate =
                DateTime.Now.ToString(
                    "yyyy-MM-dd HH:mm"
                );
        }
    }


    // =========================================================
    // DATOS GENERALES / RANKING
    // =========================================================

    [Serializable]
    private class SaveData
    {
        public int highScore;

        public int gamesPlayed;

        public int bestCorrectOrders;

        public int totalCorrectOrders;

        public int totalIncorrectOrders;


        public List<RankingEntry> ranking =
            new List<RankingEntry>();
    }


    // =========================================================
    // GUARDAR RESULTADO PARA RANKING
    // =========================================================

    public static void SaveGameResult(
        int score,
        int correctOrders,
        int incorrectOrders)
    {
        SaveData data =
            LoadData();


        data.gamesPlayed++;


        data.totalCorrectOrders +=
            Mathf.Max(
                0,
                correctOrders
            );


        data.totalIncorrectOrders +=
            Mathf.Max(
                0,
                incorrectOrders
            );


        if (score >
            data.highScore)
        {
            data.highScore =
                score;
        }


        if (correctOrders >
            data.bestCorrectOrders)
        {
            data.bestCorrectOrders =
                correctOrders;
        }


        RankingEntry newEntry =
            new RankingEntry(
                score,
                correctOrders,
                incorrectOrders
            );


        data.ranking.Add(
            newEntry
        );


        data.ranking.Sort(
            (a, b) =>
            {
                if (a == null &&
                    b == null)
                {
                    return 0;
                }


                if (a == null)
                {
                    return 1;
                }


                if (b == null)
                {
                    return -1;
                }


                int scoreComparison =
                    b.score.CompareTo(
                        a.score
                    );


                if (scoreComparison != 0)
                {
                    return scoreComparison;
                }


                return
                    b.correctOrders.CompareTo(
                        a.correctOrders
                    );
            }
        );


        data.ranking.RemoveAll(
            entry =>
                entry == null
        );


        if (data.ranking.Count >
            MaxRankingEntries)
        {
            data.ranking.RemoveRange(
                MaxRankingEntries,
                data.ranking.Count -
                MaxRankingEntries
            );
        }


        SaveDataToPlayerPrefs(
            data
        );
    }


    // =========================================================
    // GUARDADO ANTIGUO
    //
    // IMPORTANTE:
    // Se mantiene exactamente para que PauseMenuManager
    // siga funcionando sin cambios.
    // =========================================================

    public static void SaveCurrentGame(
        int score,
        int highestScore,
        int ordersCompleted,
        int correctOrders,
        int incorrectOrders)
    {
        GameSaveData gameData =
            new GameSaveData(
                score,
                highestScore,
                ordersCompleted,
                correctOrders,
                incorrectOrders
            );


        SaveCurrentGame(
            gameData
        );
    }


    // =========================================================
    // NUEVO GUARDADO COMPLETO
    //
    // ESTE ES EL QUE USAREMOS MÁS ADELANTE.
    // =========================================================

    public static void SaveCurrentGame(
        GameSaveData gameData)
    {
        if (gameData == null)
        {
            Debug.LogWarning(
                "No se puede guardar una partida nula."
            );

            return;
        }


        NormalizeGameSaveData(
            gameData
        );


        gameData.saveVersion =
            CurrentGameSaveVersion;


        gameData.saveDate =
            DateTime.Now.ToString(
                "yyyy-MM-dd HH:mm"
            );


        try
        {
            string json =
                JsonUtility.ToJson(
                    gameData
                );


            PlayerPrefs.SetString(
                GameSaveKey,
                json
            );


            PlayerPrefs.Save();


            Debug.Log(
                "💾 Partida guardada correctamente."
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "No se pudo guardar la partida: " +
                exception.Message
            );
        }
    }


    // =========================================================
    // CARGAR PARTIDA
    // =========================================================

    public static GameSaveData LoadCurrentGame()
    {
        if (!HasSavedGame())
        {
            Debug.LogWarning(
                "No existe una partida guardada."
            );

            return null;
        }


        string json =
            PlayerPrefs.GetString(
                GameSaveKey,
                ""
            );


        if (string.IsNullOrEmpty(
            json))
        {
            return null;
        }


        try
        {
            GameSaveData gameData =
                JsonUtility.FromJson<GameSaveData>(
                    json
                );


            if (gameData == null)
            {
                Debug.LogWarning(
                    "La partida guardada está vacía o dañada."
                );

                return null;
            }


            NormalizeGameSaveData(
                gameData
            );


            return gameData;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "No se pudo cargar la partida guardada: " +
                exception.Message
            );


            return null;
        }
    }


    // =========================================================
    // NORMALIZAR GUARDADO
    //
    // Esto permite que guardados antiguos sigan cargando.
    // =========================================================

    private static void NormalizeGameSaveData(
        GameSaveData gameData)
    {
        if (gameData == null)
        {
            return;
        }


        // Los guardados anteriores no tenían versión.
        if (gameData.saveVersion <= 0)
        {
            gameData.saveVersion = 1;
        }


        gameData.score =
            Mathf.Max(
                0,
                gameData.score
            );


        gameData.highestScore =
            Mathf.Max(
                gameData.score,
                gameData.highestScore
            );


        gameData.ordersCompleted =
            Mathf.Max(
                0,
                gameData.ordersCompleted
            );


        gameData.correctOrders =
            Mathf.Max(
                0,
                gameData.correctOrders
            );


        gameData.incorrectOrders =
            Mathf.Max(
                0,
                gameData.incorrectOrders
            );


        if (gameData.order == null)
        {
            gameData.order =
                new OrderSaveData();
        }


        if (gameData.assembly == null)
        {
            gameData.assembly =
                new AssemblySaveData();
        }


        if (gameData.customer == null)
        {
            gameData.customer =
                new CustomerSaveData();
        }


        if (gameData.worldItems == null)
        {
            gameData.worldItems =
                new List<WorldItemSaveData>();
        }


        if (gameData.assembly.placedIngredients ==
            null)
        {
            gameData.assembly.placedIngredients =
                new List<AssemblyIngredientSaveData>();
        }


        if (string.IsNullOrEmpty(
            gameData.saveDate))
        {
            gameData.saveDate =
                DateTime.Now.ToString(
                    "yyyy-MM-dd HH:mm"
                );
        }
    }


    // =========================================================
    // EXISTE PARTIDA GUARDADA
    // =========================================================

    public static bool HasSavedGame()
    {
        if (!PlayerPrefs.HasKey(
            GameSaveKey))
        {
            return false;
        }


        string json =
            PlayerPrefs.GetString(
                GameSaveKey,
                ""
            );


        return
            !string.IsNullOrEmpty(
                json
            );
    }


    // =========================================================
    // BORRAR PARTIDA GUARDADA
    // =========================================================

    public static void DeleteSavedGame()
    {
        PlayerPrefs.DeleteKey(
            GameSaveKey
        );


        PlayerPrefs.SetInt(
            LoadRequestKey,
            0
        );


        PlayerPrefs.Save();
    }


    // =========================================================
    // SOLICITAR CARGA
    // =========================================================

    public static void RequestLoadGame()
    {
        if (!HasSavedGame())
        {
            Debug.LogWarning(
                "No existe una partida para cargar."
            );

            return;
        }


        PlayerPrefs.SetInt(
            LoadRequestKey,
            1
        );


        PlayerPrefs.Save();
    }


    // =========================================================
    // CANCELAR SOLICITUD DE CARGA
    // =========================================================

    public static void ClearLoadRequest()
    {
        PlayerPrefs.SetInt(
            LoadRequestKey,
            0
        );


        PlayerPrefs.Save();
    }


    // =========================================================
    // COMPROBAR SOLICITUD DE CARGA
    // =========================================================

    public static bool IsLoadRequested()
    {
        return
            PlayerPrefs.GetInt(
                LoadRequestKey,
                0
            ) == 1;
    }


    // =========================================================
    // CONSUMIR SOLICITUD DE CARGA
    //
    // Devuelve true una sola vez y la resetea.
    // =========================================================

    public static bool ConsumeLoadRequest()
    {
        bool requested =
            IsLoadRequested();


        if (requested)
        {
            ClearLoadRequest();
        }


        return requested;
    }


    // =========================================================
    // ESTADÍSTICAS GENERALES
    // =========================================================

    public static int GetHighScore()
    {
        return
            LoadData().highScore;
    }


    public static int GetGamesPlayed()
    {
        return
            LoadData().gamesPlayed;
    }


    public static int GetBestCorrectOrders()
    {
        return
            LoadData().bestCorrectOrders;
    }


    public static int GetTotalCorrectOrders()
    {
        return
            LoadData().totalCorrectOrders;
    }


    public static int GetTotalIncorrectOrders()
    {
        return
            LoadData().totalIncorrectOrders;
    }


    // =========================================================
    // OBTENER RANKING
    // =========================================================

    public static List<RankingEntry> GetRanking()
    {
        SaveData data =
            LoadData();


        return
            new List<RankingEntry>(
                data.ranking
            );
    }


    // =========================================================
    // EXISTEN DATOS GENERALES
    // =========================================================

    public static bool HasSaveData()
    {
        return
            PlayerPrefs.HasKey(
                SaveKey
            );
    }


    // =========================================================
    // BORRAR TODO
    // =========================================================

    public static void ResetAllData()
    {
        PlayerPrefs.DeleteKey(
            SaveKey
        );


        PlayerPrefs.DeleteKey(
            GameSaveKey
        );


        PlayerPrefs.DeleteKey(
            LoadRequestKey
        );


        PlayerPrefs.Save();


        Debug.Log(
            "🗑 Todos los datos guardados fueron eliminados."
        );
    }


    // =========================================================
    // CARGAR DATOS GENERALES
    // =========================================================

    private static SaveData LoadData()
    {
        if (!PlayerPrefs.HasKey(
            SaveKey))
        {
            return
                new SaveData();
        }


        string json =
            PlayerPrefs.GetString(
                SaveKey,
                ""
            );


        if (string.IsNullOrEmpty(
            json))
        {
            return
                new SaveData();
        }


        try
        {
            SaveData data =
                JsonUtility.FromJson<SaveData>(
                    json
                );


            if (data == null)
            {
                return
                    new SaveData();
            }


            if (data.ranking == null)
            {
                data.ranking =
                    new List<RankingEntry>();
            }


            data.ranking.RemoveAll(
                entry =>
                    entry == null
            );


            return data;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "No se pudieron cargar los datos guardados: " +
                exception.Message
            );


            return
                new SaveData();
        }
    }


    // =========================================================
    // GUARDAR DATOS GENERALES
    // =========================================================

    private static void SaveDataToPlayerPrefs(
        SaveData data)
    {
        if (data == null)
        {
            return;
        }


        try
        {
            string json =
                JsonUtility.ToJson(
                    data
                );


            PlayerPrefs.SetString(
                SaveKey,
                json
            );


            PlayerPrefs.Save();
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "No se pudieron guardar los datos: " +
                exception.Message
            );
        }
    }
}