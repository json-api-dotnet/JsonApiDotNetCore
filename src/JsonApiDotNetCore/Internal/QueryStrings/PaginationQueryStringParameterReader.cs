using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Queries;
using JsonApiDotNetCore.Internal.Queries.Expressions;
using JsonApiDotNetCore.Internal.Queries.Parsing;
using JsonApiDotNetCore.RequestServices.Contracts;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Internal.QueryStrings
{
    /// <summary>
    /// Reads the 'page' query string parameter and produces a set of query constraints from it.
    /// </summary>
    public interface IPaginationQueryStringParameterReader : IQueryStringParameterReader, IQueryConstraintProvider
    {
    }

    public class PaginationQueryStringParameterReader : QueryStringParameterReader, IPaginationQueryStringParameterReader
    {
        private const string _pageSizeParameterName = "page[size]";
        private const string _pageNumberParameterName = "page[number]";

        private readonly IJsonApiOptions _options;

        private PaginationQueryStringValueExpression _pageSizeConstraint;
        private PaginationQueryStringValueExpression _pageNumberConstraint;

        public PaginationQueryStringParameterReader(ICurrentRequest currentRequest, IResourceContextProvider resourceContextProvider, IJsonApiOptions options)
            : base(currentRequest, resourceContextProvider)
        {
            _options = options;
        }

        public bool IsEnabled(DisableQueryAttribute disableQueryAttribute)
        {
            return !disableQueryAttribute.ContainsParameter(StandardQueryStringParameters.Page);
        }

        public bool CanRead(string parameterName)
        {
            return parameterName == _pageSizeParameterName || parameterName == _pageNumberParameterName;
        }

        public void Read(string parameterName, StringValues parameterValue)
        {
            try
            {
                var constraint = GetPageConstraint(parameterValue);

                if (constraint.Elements.Any(element => element.Scope == null))
                {
                    AssertIsCollectionRequest();
                }

                if (parameterName == _pageSizeParameterName)
                {
                    ValidatePageSize(constraint);
                    _pageSizeConstraint = constraint;
                }
                else
                {
                    ValidatePageNumber(constraint);
                    _pageNumberConstraint = constraint;
                }
            }
            catch (QueryParseException exception)
            {
                throw new InvalidQueryStringParameterException(parameterName, "The specified paging is invalid.", exception.Message, exception);
            }
        }

        private PaginationQueryStringValueExpression GetPageConstraint(string parameterValue)
        {
            var parser = new PaginationParser(parameterValue,
                (path, _) => ChainResolver.ResolveToManyChain(RequestResource, path));

            return parser.Parse();
        }

        private void ValidatePageSize(PaginationQueryStringValueExpression constraint)
        {
            if (_options.MaximumPageSize != null)
            {
                if (constraint.Elements.Any(element => element.Value > _options.MaximumPageSize.Value))
                {
                    throw new QueryParseException($"Page size cannot be higher than {_options.MaximumPageSize}.");
                }

                if (constraint.Elements.Any(element => element.Value == 0))
                {
                    throw new QueryParseException("Page size cannot be unconstrained.");
                }
            }

            if (constraint.Elements.Any(element => element.Value < 0))
            {
                throw new QueryParseException("Page size cannot be negative.");
            }
        }

        private void ValidatePageNumber(PaginationQueryStringValueExpression constraint)
        {
            if (_options.MaximumPageNumber != null &&
                constraint.Elements.Any(element => element.Value > _options.MaximumPageNumber.OneBasedValue))
            {
                throw new QueryParseException($"Page number cannot be higher than {_options.MaximumPageNumber}.");
            }

            if (constraint.Elements.Any(element => element.Value < 1))
            {
                throw new QueryParseException("Page number cannot be negative or zero.");
            }
        }

        public IReadOnlyCollection<ExpressionInScope> GetConstraints()
        {
            var context = new PaginationContext();

            foreach (var element in _pageSizeConstraint?.Elements ?? Array.Empty<PaginationElementQueryStringValueExpression>())
            {
                var entry = context.ResolveEntryInScope(element.Scope);
                entry.PageSize = element.Value == 0 ? null : new PageSize(element.Value);
                entry.HasSetPageSize = true;
            }

            foreach (var element in _pageNumberConstraint?.Elements ?? Array.Empty<PaginationElementQueryStringValueExpression>())
            {
                var entry = context.ResolveEntryInScope(element.Scope);
                entry.PageNumber = new PageNumber(element.Value);
            }

            context.ApplyOptions(_options);

            return context.GetExpressionsInScope();
        }

        private sealed class PaginationContext
        {
            private readonly MutablePaginationEntry _globalScope = new MutablePaginationEntry();
            private readonly Dictionary<ResourceFieldChainExpression, MutablePaginationEntry> _nestedScopes = new Dictionary<ResourceFieldChainExpression, MutablePaginationEntry>();

            public MutablePaginationEntry ResolveEntryInScope(ResourceFieldChainExpression scope)
            {
                if (scope == null)
                {
                    return _globalScope;
                }

                if (!_nestedScopes.ContainsKey(scope))
                {
                    _nestedScopes.Add(scope, new MutablePaginationEntry());
                }

                return _nestedScopes[scope];
            }

            public void ApplyOptions(IJsonApiOptions options)
            {
                ApplyOptionsInEntry(_globalScope, options);

                foreach (var (_, entry) in _nestedScopes)
                {
                    ApplyOptionsInEntry(entry, options);
                }
            }

            private void ApplyOptionsInEntry(MutablePaginationEntry entry, IJsonApiOptions options)
            {
                if (!entry.HasSetPageSize)
                {
                    entry.PageSize = options.DefaultPageSize;
                }

                entry.PageNumber ??= PageNumber.ValueOne;
            }

            public IReadOnlyCollection<ExpressionInScope> GetExpressionsInScope()
            {
                return EnumerateExpressionsInScope().ToList();
            }

            private IEnumerable<ExpressionInScope> EnumerateExpressionsInScope()
            {
                yield return new ExpressionInScope(null, new PaginationExpression(_globalScope.PageNumber, _globalScope.PageSize));

                foreach (var (scope, entry) in _nestedScopes)
                {
                    yield return new ExpressionInScope(scope, new PaginationExpression(entry.PageNumber, entry.PageSize));
                }
            }
        }

        private sealed class MutablePaginationEntry
        {
            public PageSize PageSize { get; set; }
            public bool HasSetPageSize { get; set; }

            public PageNumber PageNumber { get; set; }
        }
    }
}
