using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentViewer
{
    public class EbaApiException(string message) : Exception(message);
}
