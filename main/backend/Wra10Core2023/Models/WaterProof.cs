namespace Wra10Core2023.Models
{
    public class WaterProofMain
    {
        public string? id { get; set; }
        public string? fileName { get; set; }
    }

    public class WaterProofTaget
    {
        public string? target { get; set; }
        public int waterproofid { get; set; }

        public class WaterProof
        {
            public int waterProofId { get; set; }
            public string? serialNo { get; set; }
            public string? projectName { get; set; }
            public int projectId { get; set; }
            public string? target { get; set; }
            public string? year { get; set; }
            public List<WaterProofGps> lstGps { get; set; } = new List<WaterProofGps>();
            public List<string> lstImg { get; set; } = new List<string>();
        }

        public class WaterProofProject
        {
            public int waterProofId { get; set; }
            public string? projectName { get; set; }
            public int projectId { get; set; }
            public string? year { get; set; }
        }

        public class WaterProofGps
        {
            public double x { get; set; }
            public double y { get; set; }

            public string? gpsName { get; set; }
        }

        public class CompareParameter
        {
            public string? id { get; set; }
            //public string? sn { get; set; }
        }

        public class CompareData
        {
            public string? sequence { get; set; }
            public string? projectname { get; set; }
        }
    }
}
