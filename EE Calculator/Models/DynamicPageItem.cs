using System;

namespace EE_Calculator.Models
{
    public class DynamicPageItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Type PageType { get; set; }
        public bool IsClosable { get; set; }
    }
}
