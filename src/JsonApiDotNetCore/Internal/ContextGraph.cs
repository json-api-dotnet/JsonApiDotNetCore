using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Internal
{
    public class ContextGraph<T> where T : DbContext
    {
        public List<ContextEntity> Entities { get; set; }
    }
}
