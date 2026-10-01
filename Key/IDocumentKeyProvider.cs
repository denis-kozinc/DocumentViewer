using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentViewer.Key
{
    /// <summary>
    /// Interface for providing document keys.
    /// </summary>
    public interface IDocumentKeyProvider
    {
        event EventHandler<string> DocumentKeyChanged;
    }
}
