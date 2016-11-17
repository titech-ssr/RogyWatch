using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogyWatchCommon
{
    public partial class Config : IConfig
    {
        [JsonProperty("attei_config")]
        public AtteiConfig AtteiConfig{ get; set; } = new AtteiConfig();
    }
   
    public class AtteiConfig
    {
        [DefaultValue(@"C:\Users\Maquinista\Desktop\hisui-1_13_0_0-20110927\bin\V1\")]
        public string MakeAscDir1 { get; set; } = @"C:\Users\Maquinista\Desktop\hisui-1_13_0_0-20110927\bin\V1";

        [DefaultValue(@"C:\Users\Maquinista\Desktop\hisui-1_13_0_0-20110927\bin\V2\")]
        public string MakeAscDir2 { get; set; } = @"C:\Users\Maquinista\Desktop\hisui-1_13_0_0-20110927\bin\V2";

        [DefaultValue(@"C:\template_V1.bmp")]
        public string Template1 { get; set; } = @"C:\template_V1.bmp";

        [DefaultValue(@"C:\template_V2.bmp")]
        public string Template2 { get; set; } = @"C:\template_V2.bmp";

        [DefaultValue(@"C:\Users\Public\share\V1")]
        public string OutDir1 { get; set; } = @"C:\Users\Public\share\V1";

        [DefaultValue(@"C:\Users\Public\share\V2")]
        public string OutDir2 { get; set; } = @"C:\Users\Public\share\V2";
    }
}
