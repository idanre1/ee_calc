using System;
using System.Collections.Generic;

namespace EE_Calculator.Models
{
    public class TabState
    {
        public int Index { get; set; }
        public string Header { get; set; }
        public string Input { get; set; }
    }

    public class PageState
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public List<TabState> Tabs { get; set; }
    }

    public class DynamicPagesState
    {
        public List<PageState> Pages { get; set; } = new List<PageState>();
    }
}
