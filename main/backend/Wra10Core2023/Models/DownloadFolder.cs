namespace Wra10Core2023.Models
{

    public class FileNode
    {
        public string Name { get; set; }
        public bool IsFolder { get; set; }
        public List<FileNode> SubFolder { get; set; }
        public string? IconType { get; set; }
        public string? IconPath { get; set; }
        public string? DownloadLink { get; set; }    
    }

    

}
