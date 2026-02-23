namespace PT.PlanetTogetherAPI.Importing
{
    internal class ImportStatusUpdater
    {
        private double m_currentProgressPoints;  
        private double m_totalProgressPoints;
        private double m_allowedProgressPoints;

        public void SetInitialProgressPoints(double a_currentProgressPoints, double a_totalPoints, double a_allowedProgressPoints)
        {
            m_currentProgressPoints = a_currentProgressPoints;
            m_totalProgressPoints = a_totalPoints;
            m_allowedProgressPoints = a_allowedProgressPoints;
        }

        /// <summary>
        /// Updates the current progress and returns the % of the way to the total.
        /// </summary>
        /// <param name="a_pointIncrease"></param>
        /// <returns></returns>
        public double UpdateProgressPoints(double a_pointIncrease)
        {
            double progressBeforeStep = m_currentProgressPoints / m_totalProgressPoints;
            m_currentProgressPoints += m_allowedProgressPoints * a_pointIncrease;
            return progressBeforeStep;
        }
    }
}
