using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace JsonApiDotNetCore.Managers.Contracts
{
    public interface IPageManager
    {
        /// <summary>
        /// What the total records are for this output
        /// </summary>
        int? TotalRecords { get; set; }
        /// <summary>
        /// How many records per page should be shown
        /// </summary>
        int PageSize { get; set; }
        /// <summary>
        /// What is the default page size
        /// </summary>
        int DefaultPageSize { get; set; }
        /// <summary>
        /// What page are we currently on
        /// </summary>
        int CurrentPage { get; set; }
        /// <summary>
        /// Are we even paginating
        /// </summary>
        bool IsPaginated { get; }

        RootLinks GetPageLinks();
    }
}
