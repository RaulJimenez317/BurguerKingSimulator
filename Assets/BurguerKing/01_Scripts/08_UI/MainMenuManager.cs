using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject rankingPanel;
    [SerializeField] private TMP_Text rankingText;
    [SerializeField] private Button loadGameButton;

    private const string GameSceneName = "BurguerKingScene";


    private void Start()
    {
        Time.timeScale = 1f;

        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
        }

        if (rankingPanel != null)
        {
            rankingPanel.SetActive(false);
        }

        RefreshLoadButton();
    }


    public void NewGame()
    {
        // Nueva partida:
        // nos aseguramos de NO cargar ningún guardado anterior.
        SaveManager.ClearLoadRequest();

        SceneManager.LoadScene(
            GameSceneName
        );
    }


    public void LoadGame()
    {
        if (!SaveManager.HasSavedGame())
        {
            Debug.LogWarning(
                "No existe una partida guardada."
            );

            RefreshLoadButton();

            return;
        }

        // Marcamos que BurguerKingScene debe
        // restaurar la partida guardada.
        SaveManager.RequestLoadGame();

        SceneManager.LoadScene(
            GameSceneName
        );
    }


    public void OpenRanking()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }

        if (rankingPanel != null)
        {
            rankingPanel.SetActive(true);
        }

        RefreshRanking();
    }


    public void CloseRanking()
    {
        if (rankingPanel != null)
        {
            rankingPanel.SetActive(false);
        }

        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
        }
    }


    public void RefreshRanking()
    {
        if (rankingText == null)
        {
            return;
        }

        List<SaveManager.RankingEntry> ranking =
            SaveManager.GetRanking();

        StringBuilder builder =
            new StringBuilder();

        for (int i = 0; i < 10; i++)
        {
            if (i < ranking.Count)
            {
                SaveManager.RankingEntry entry =
                    ranking[i];

                builder.Append(
                    (i + 1) +
                    ".  " +
                    entry.score +
                    " PTS"
                );
            }
            else
            {
                builder.Append(
                    (i + 1) +
                    ".  ---"
                );
            }

            if (i < 9)
            {
                builder.AppendLine();
            }
        }

        rankingText.text =
            builder.ToString();
    }


    public void RefreshLoadButton()
    {
        if (loadGameButton != null)
        {
            loadGameButton.interactable =
                SaveManager.HasSavedGame();
        }
    }


    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying =
            false;
#else
        Application.Quit();
#endif
    }
}