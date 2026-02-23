namespace PT.Scheduler.CoPilot.RuleSeek;

/// <summary>
/// Class for storing RuleSeekScores. Used to synchronize multiple simulation score results.
/// Has functionality for storing only the best scores.
/// </summary>
public class RuleSeekScoreSet
{
    public RuleSeekScoreSet(int a_scoresToKeep)
    {
        m_numScoresToKeep = a_scoresToKeep;
        m_scoreList = new List<RuleSeekScore>();
    }

    private readonly int m_numScoresToKeep;
    private readonly List<RuleSeekScore> m_scoreList;

    public List<RuleSeekScore> Scores => m_scoreList;

    /// <summary>
    /// Adds a new score to the list if high enough. Returns if the score was kept
    /// </summary>
    /// <param name="a_ruleSeekScore">Score being added</param>
    /// <returns>[bool] if the score was kept</returns>
    public bool Add(RuleSeekScore a_ruleSeekScore)
    {
        m_scoreList.Add(a_ruleSeekScore);

        if (m_scoreList.Count > m_numScoresToKeep)
        {
            //Only keep the best.
            Sort(a_ruleSeekScore.LowerIsBetter);
            m_scoreList.RemoveAt(m_scoreList.Count - 1);
        }

        return m_scoreList.Contains(a_ruleSeekScore);
    }

    //public void Merge(RuleSeekScoreSet a_setToAdd)
    //{
    //    m_scoreList.AddRange(a_setToAdd.Scores);
    //    Sort();
    //    m_scoreList.RemoveRange(m_numScoresToKeep, m_scoreList.Count - m_numScoresToKeep);
    //}

    /// <summary>
    /// The worst score in the set.
    /// </summary>
    public RuleSeekScore WorstScore => m_scoreList[m_scoreList.Count - 1];

    /// <summary>
    /// Sorts the list descending from best score
    /// </summary>
    /// <param name="a_lowerIsBetter">Lower scores will be considered better</param>
    private void Sort(bool a_lowerIsBetter)
    {
        if (a_lowerIsBetter)
        {
            m_scoreList.Sort((score1, score2) => score1.Score.CompareTo(score2.Score));
        }
        else
        {
            m_scoreList.Sort((score1, score2) => score2.Score.CompareTo(score1.Score));
        }
    }
}