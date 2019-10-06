using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorContentLoader.Model
{
    public class BlazorAssetFile
    {
        public BlazorAssetSource Source { get; set; } 
        public string Path { get; set; }  // name of file

        public string ProjectPath { get; set; }

        public BlazorAssetFile(BlazorAssetSource source, string path, string projectPath)
        {
            this.Source = source;
            this.Path = path;

            this.ProjectPath = projectPath;
        }
    }
}
