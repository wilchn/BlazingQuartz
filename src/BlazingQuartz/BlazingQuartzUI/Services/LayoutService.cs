using System;
namespace BlazingQuartzUI.Services
{
    public class LayoutService
    {
        public LayoutService()
        {
        }

        public BasePage GetDocsBasePage(string uri)
        {
            if (uri.Contains("/overview"))
            {
                return BasePage.Overview;
            }
            else if (uri.Contains("/schedules"))
            {
                return BasePage.Schedules;
            }
            else if (uri.Contains("/triggers"))
            {
                return BasePage.Triggers;
            }
            else if (uri.Contains("/history"))
            {
                return BasePage.History;
            }
            else if (uri.Contains("/calendars"))
            {
                return BasePage.Calendars;
            }
            else
            {
                return BasePage.None;
            }
        }
    }
}

