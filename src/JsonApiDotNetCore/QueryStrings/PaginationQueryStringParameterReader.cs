using System.Collections.Immutable;
using System.Collections.ObjectModel;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.QueryStrings;

/// <inheritdoc cref="IPaginationQueryStringParameterReader" />
[PublicAPI]
public class PaginationQueryStringParameterReader : QueryStringParameterReader, IPaginationQueryStringParameterReader
{
    private const string PageSizeParameterName = "page[size]";
    private const string PageNumberParameterName = "page[number]";

    private readonly IJsonApiOptions _options;
    private readonly IPaginationParser _paginationParser;

    private PaginationQueryStringValueExpression? _pageSizeConstraint;
    private PaginationQueryStringValueExpression? _pageNumberConstraint;

    public bool AllowEmptyValue => false;

    public PaginationQueryStringParameterReader(IPaginationParser paginationParser, IJsonApiRequest request, IResourceGraph resourceGraph,
        IJsonApiOptions options)
        : base(request, resourceGraph)
    {
        ArgumentGuard.NotNull(paginationParser);
        ArgumentGuard.NotNull(options);

        _options = options;
        _paginationParser = paginationParser;
    }

    /// <inheritdoc />
    public virtual bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute)
    {
        ArgumentGuard.NotNull(disableQueryStringAttribute);

        return !IsAtomicOperationsRequest && !disableQueryStringAttribute.ContainsParameter(JsonApiQueryStringParameters.Page);
    }

    /// <inheritdoc />
    public virtual bool CanRead(string parameterName)
    {
        return parameterName is PageSizeParameterName or PageNumberParameterName;
    }

    /// <inheritdoc />
    public virtual void Read(string parameterName, StringValues parameterValue)
    {
        bool isParameterNameValid = true;

        try
        {
            PaginationQueryStringValueExpression constraint = GetPageConstraint(parameterValue.ToString());

            if (constraint.Elements.Any(element => element.Scope == null))
            {
                isParameterNameValid = false;
                AssertIsCollectionRequest();
                isParameterNameValid = true;
            }

            if (parameterName == PageSizeParameterName)
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
            string specificMessage = exception.GetMessageWithPosition(isParameterNameValid ? parameterValue.ToString() : parameterName);
            throw new InvalidQueryStringParameterException(parameterName, "The specified pagination is invalid.", specificMessage, exception);
        }
    }

    private PaginationQueryStringValueExpression GetPageConstraint(string parameterValue)
    {
        return _paginationParser.Parse(parameterValue, RequestResourceType);
    }

    protected virtual void ValidatePageSize(PaginationQueryStringValueExpression constraint)
    {
        ArgumentGuard.NotNull(constraint);

        foreach (PaginationElementQueryStringValueExpression element in constraint.Elements)
        {
            if (_options.MaximumPageSize != null)
            {
                if (element.Value > _options.MaximumPageSize.Value)
                {
                    throw new QueryParseException($"Page size cannot be higher than {_options.MaximumPageSize}.", element.Position);
                }

                if (element.Value == 0)
                {
                    throw new QueryParseException("Page size cannot be unconstrained.", element.Position);
                }
            }

            if (element.Value < 0)
            {
                throw new QueryParseException("Page size cannot be negative.", element.Position);
            }
        }
    }

    protected virtual void ValidatePageNumber(PaginationQueryStringValueExpression constraint)
    {
        ArgumentGuard.NotNull(constraint);

        foreach (PaginationElementQueryStringValueExpression element in constraint.Elements)
        {
            if (_options.MaximumPageNumber != null)
            {
                if (element.Value > _options.MaximumPageNumber.OneBasedValue)
                {
                    throw new QueryParseException($"Page number cannot be higher than {_options.MaximumPageNumber}.", element.Position);
                }
            }

            if (element.Value < 1)
            {
                throw new QueryParseException("Page number cannot be negative or zero.", element.Position);
            }
        }
    }

    /// <inheritdoc />
    public virtual IReadOnlyCollection<ExpressionInScope> GetConstraints()
    {
        var paginationState = new PaginationState();

        foreach (PaginationElementQueryStringValueExpression element in _pageSizeConstraint?.Elements ??
            ImmutableArray<PaginationElementQueryStringValueExpression>.Empty)
        {
            MutablePaginationEntry entry = paginationState.ResolveEntryInScope(element.Scope);
            entry.PageSize = element.Value == 0 ? null : new PageSize(element.Value);
            entry.HasSetPageSize = true;
        }

        foreach (PaginationElementQueryStringValueExpression element in _pageNumberConstraint?.Elements ??
            ImmutableArray<PaginationElementQueryStringValueExpression>.Empty)
        {
            MutablePaginationEntry entry = paginationState.ResolveEntryInScope(element.Scope);
            entry.PageNumber = new PageNumber(element.Value);
        }

        paginationState.ApplyOptions(_options);

        return paginationState.GetExpressionsInScope();
    }

    private sealed class PaginationState
    {
        private readonly MutablePaginationEntry _globalScope = new();
        private readonly Dictionary<ResourceFieldChainExpression, MutablePaginationEntry> _nestedScopes = [];

        public MutablePaginationEntry ResolveEntryInScope(ResourceFieldChainExpression? scope)
        {
            if (scope == null)
            {
                return _globalScope;
            }

            _nestedScopes.TryAdd(scope, new MutablePaginationEntry());
            return _nestedScopes[scope];
        }

        public void ApplyOptions(IJsonApiOptions options)
        {
            ApplyOptionsInEntry(_globalScope, options);

            foreach ((_, MutablePaginationEntry entry) in _nestedScopes)
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

        public ReadOnlyCollection<ExpressionInScope> GetExpressionsInScope()
        {
            return EnumerateExpressionsInScope().ToArray().AsReadOnly();
        }

        private IEnumerable<ExpressionInScope> EnumerateExpressionsInScope()
        {
            yield return new ExpressionInScope(null, new PaginationExpression(_globalScope.PageNumber!, _globalScope.PageSize));

            foreach ((ResourceFieldChainExpression scope, MutablePaginationEntry entry) in _nestedScopes)
            {
                yield return new ExpressionInScope(scope, new PaginationExpression(entry.PageNumber!, entry.PageSize));
            }
        }
    }

    private sealed class MutablePaginationEntry
    {
        public PageSize? PageSize { get; set; }
        public bool HasSetPageSize { get; set; }

        public PageNumber? PageNumber { get; set; }
    }
}
