namespace Wra10Core2023.Models
{
    

    public class Feature
    {
        public string type { get; set; }
        public Properties properties { get; set; }
        public Geometry geometry { get; set; }
    }

    public class Geometry
    {
        public string type { get; set; }
        public List<List<double>> coordinates { get; set; }
    }

    public class Properties
    {
        public string Fibr_NO { get; set; }
        public string NAME { get; set; }
    }

    public class ChainGPS
    {
        public string type { get; set; }
        public List<Feature> features { get; set; }
    }

}
