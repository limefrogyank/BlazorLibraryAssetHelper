using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorContentLoader
{
    public class CustomModeDetector : Splat.IModeDetector
    {
        public bool? InUnitTestRunner()
        {
            return false;
        }
    }
}
