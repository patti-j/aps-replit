namespace PT.ServerManagerSharedLib.DTOs.Entities
{
    public class ApiSettingsEntity
    {
        public ApiSettingsEntity() { }

        public ApiSettingsEntity(ApiSettingsEntity a_aCopyFromSettings)
        {
            m_enabled = a_aCopyFromSettings.m_enabled;
            m_port = a_aCopyFromSettings.m_port;
            m_diagnostics = a_aCopyFromSettings.m_diagnostics;
        }

        int m_port = 8080;

        public int Port
        {
            get { return m_port; }
            set { m_port = value; }
        }

        bool m_enabled;

        public bool Enabled
        {
            get { return m_enabled; }
            set { m_enabled = value; }
        }

        bool m_diagnostics;

        public bool Diagnostics
        {
            get { return m_diagnostics; }
            set { m_diagnostics = value; }
        }

        internal void Update(ApiSettingsEntity a_aUpdateSettings)
        {
            //These values are grabbed when needed so no need to restart the service when changed.

            if (Enabled != a_aUpdateSettings.Enabled)
            {
                Enabled = a_aUpdateSettings.Enabled;
            }


            if (Port != a_aUpdateSettings.Port)
            {
                Port = a_aUpdateSettings.Port;
            }


            if (Diagnostics != a_aUpdateSettings.Diagnostics)
            {
                Diagnostics = a_aUpdateSettings.Diagnostics;
            }
        }
    }
}
