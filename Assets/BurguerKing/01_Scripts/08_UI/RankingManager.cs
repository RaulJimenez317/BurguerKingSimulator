using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class RankingManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text rankingText;

    [Header("CONFIGURACION")]
    [Min(1)]
    public int maxPositions = 10;

    private void OnEnable()
    {
        RefreshRanking();
    }

    public void RefreshRanking()
    {
        if (rankingText == null)
        {
            Debug.LogWarning(
                "No hay RankingText asignado en RankingManager."
            );
            return;
        }

        List<SaveManager.RankingEntry> ranking =
            SaveManager.GetRanking();

        StringBuilder builder =
            new StringBuilder();

        for (int i = 0; i < maxPositions; i++)
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

                builder.AppendLine();
            }
            else
            {
                builder.Append(
                    (i + 1) +
                    ".  ---"
                );

                builder.AppendLine();
            }
        }

        rankingText.text =
            builder.ToString().TrimEnd();
    }
}