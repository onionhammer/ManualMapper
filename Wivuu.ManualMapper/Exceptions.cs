using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wivuu.ManualMapper
{
    public class DestinationNotMapped : Exception
    {
        public override string Message { get; }

        public DestinationNotMapped(string dest)
        {
            Message = $"{dest} has no defined mapping";
        }
    }
}
