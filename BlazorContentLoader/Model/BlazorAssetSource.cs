using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorContentLoader.Model
{
    public class BlazorAssetSource
    {
        public string Path { get; set; }  // nuget folder file path
        public string BasePath { get; set; }  // blazor content path

        public BlazorAssetSource(string path, string basePath)
        {
            this.Path = path;
            this.BasePath = basePath;
        }
    }
}
