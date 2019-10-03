using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.QueryServices.Contracts
{

    /// <summary>
    /// Query service to access the inclusion chains.
    /// </summary>
    public interface IIncludedQueryService
    {
        /// <summary>
        /// Gets the list of included relationships chains for the current request.
        /// </summary>
        List<List<RelationshipAttribute>> Get();
    }

    /// <summary>
    /// Internal interface to register inclusion chains when parsing query params internally.
    /// This is to prevent the registering method from being exposed to the developer.
    /// </summary>
    public interface IInternalIncludedQueryService
    {
        void Register(List<RelationshipAttribute> inclusionChain);
    }
}