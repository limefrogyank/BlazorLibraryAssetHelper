using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorContentLoader.ToolWindow
{
    public class AssetWindowState
    {
        //public EnvDTE80.DTE2 DTE { get; set; }

        public IVsSolution Solution { get; set; }
    }
}
