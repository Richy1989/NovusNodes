namespace NovusNodoCore.SaveData
{
    public class PageSaveModel
    {
        public string PageId { get; set; }
        public string PageName { get; set; }
        public List<NodeSaveModel> Nodes { get; set; }
    }
}
