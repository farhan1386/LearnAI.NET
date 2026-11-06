namespace LearnAI.Web.Models
{
    public class WeekData
    {
        public string Title { get; set; } = string.Empty;
        public List<DayData> Days { get; set; } = new();
    }

    public class DayData
    {
        public string DayNum { get; set; } = string.Empty;
        public string DayTitle { get; set; } = string.Empty;
        public List<SectionData> Sections { get; set; } = new();
    }

    public class SectionData
    {
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string TextColor { get; set; } = string.Empty;
        public List<ItemData> Items { get; set; } = new();
    }

    public class ItemData
    {
        public string Label { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
    }
}
