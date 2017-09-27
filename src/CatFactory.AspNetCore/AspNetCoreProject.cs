using System;

namespace CatFactory.AspNetCore
{
    public class AspNetCoreProject : Project
    {
        private AspNetCoreProjectSettings m_settings;

        public AspNetCoreProjectSettings Settings
            => m_settings ?? (m_settings = new AspNetCoreProjectSettings());
    }
}
