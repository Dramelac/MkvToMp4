using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MkvToMp4.Config
{
    public class ToolConfig
    {
        public bool HideBanner { get; set; }
        public string Preset { get; set; }
        public string Profile { get; set; }
        public string VCodec { get; set; }
        public string ACodec { get; set; }
        public string Format { get; set; }
        public string OutputFile { get; set; }
        public string InputFile { get; set; }
        public string AdvanceArgs { get; set; }


        public ToolConfig()
        {
            HideBanner = true;
            Format = "mp4";
            Preset = "Default";
            VCodec = null;
            Profile = "Default";
            ACodec = null;
            AdvanceArgs = "";
        }

        public ToolConfig(ToolConfig src)
        {
            HideBanner = src.HideBanner;
            Format = src.Format;
            Preset = src.Preset;
            VCodec = src.VCodec;
            Profile = src.Profile;
            ACodec = src.ACodec;
            OutputFile = src.OutputFile;
            InputFile = src.InputFile;
            AdvanceArgs = src.AdvanceArgs;
        }

        public string GenerateArguments()
        {
            string result = string.Format("-i \"{0}\" -f {1} ", InputFile, Format);
            
            if (HideBanner) result += "-hide_banner ";
            if (Preset != "Default") result += "-preset " + Preset.ToString().ToLower() + " ";
            if (Profile != "Default") result += "-profile:v " + Profile.ToString().ToLower() + " ";
            if (!String.IsNullOrEmpty(VCodec)) result += "-vcodec " + VCodec + " ";
            if (!String.IsNullOrEmpty(ACodec)) result += "-acodec " + ACodec + " ";
            
            if (!String.IsNullOrEmpty(AdvanceArgs)) result += AdvanceArgs + " ";

            result += string.Format("\"{0}.{1}\"",OutputFile, Format);
            
            return result;
        }
    }
    

}