using System;
using System.Collections.Generic;
using System.Text;

namespace JsonApiDotNetCore.Builders
{
    public interface IDocumentBuilderOptionsProvider
    {
        DocumentBuilderOptions GetDocumentBuilderOptions(); 
    }
}
