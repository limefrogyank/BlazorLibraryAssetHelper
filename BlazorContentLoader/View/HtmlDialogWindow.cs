using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorContentLoader.View
{
    public class HtmlDialogWindow : DialogWindow
    {
        public HtmlDialogWindow()
        {
            this.HasMaximizeButton = true;
            this.HasMinimizeButton = true;
        }
    }
}
