using System;
using System.Collections.Generic;

namespace EE_Calculator.Models
{
    // Model for persisting tab data
    public class TabData
    {
        public int Index { get; set; }
        public string Header { get; set; }
        public string MathInputText { get; set; }
    }

    // Model for persisting dynamic page data
    public class DynamicPageData
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public List<TabData> Tabs { get; set; } = new List<TabData>();
    }

    // Model for persisting the entire session
    public class SessionData
    {
        public List<TabData> MainPageTabs { get; set; } = new List<TabData>();
        public List<DynamicPageData> DynamicPages { get; set; } = new List<DynamicPageData>();
        public int PageCounter { get; set; } = 1;
    }
}
