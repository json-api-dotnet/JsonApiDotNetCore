using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Remotion.Linq.Parsing.Structure;
using Database = Microsoft.EntityFrameworkCore.Storage.Database;

namespace JsonApiDotNetCoreExampleTests.Helpers.Extensions
{
    public static class IQueryableExtensions
    {
        public static string ToSql<TEntity>(this IQueryable<TEntity> query) where TEntity : class
        {
            var efCoreVersion = FileVersionInfo.GetVersionInfo(typeof(QueryCompiler).Assembly.Location);
            var majorMinor = $"{efCoreVersion.ProductMajorPart}.{efCoreVersion.ProductMinorPart}";

            return majorMinor == "2.1"
                ? QueryGenerator_2_1_0.ToSql(query)
                : QueryGenerator_2_0_0.ToSql(query);
        }
    }

    public static class QueryGenerator_2_0_0
    {
        private static readonly TypeInfo QueryCompilerTypeInfo = typeof(QueryCompiler).GetTypeInfo();

        private static readonly FieldInfo QueryCompilerField = typeof(EntityQueryProvider).GetTypeInfo().DeclaredFields.First(x => x.Name == "_queryCompiler");

        private static readonly PropertyInfo NodeTypeProviderField = QueryCompilerTypeInfo.DeclaredProperties.Single(x => x.Name == "NodeTypeProvider");

        private static readonly MethodInfo CreateQueryParserMethod = QueryCompilerTypeInfo.DeclaredMethods.First(x => x.Name == "CreateQueryParser");

        private static readonly FieldInfo DataBaseField = QueryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_database");

        private static readonly PropertyInfo DatabaseDependenciesField
            = typeof(Database).GetTypeInfo().DeclaredProperties.Single(x => x.Name == "Dependencies");

        public static string ToSql<TEntity>(IQueryable<TEntity> query) where TEntity : class
        {
            if (!(query is EntityQueryable<TEntity>) && !(query is InternalDbSet<TEntity>))
            {
                throw new ArgumentException("Invalid query");
            }

            var queryCompiler = (IQueryCompiler)QueryCompilerField.GetValue(query.Provider);
            var nodeTypeProvider = (INodeTypeProvider)NodeTypeProviderField.GetValue(queryCompiler);
            var parser = (IQueryParser)CreateQueryParserMethod.Invoke(queryCompiler, new object[] { nodeTypeProvider });
            var queryModel = parser.GetParsedQuery(query.Expression);
            var database = DataBaseField.GetValue(queryCompiler);
            var queryCompilationContextFactory = ((DatabaseDependencies)DatabaseDependenciesField.GetValue(database)).QueryCompilationContextFactory;
            var queryCompilationContext = queryCompilationContextFactory.Create(false);
            var modelVisitor = (RelationalQueryModelVisitor)queryCompilationContext.CreateQueryModelVisitor();
            modelVisitor.CreateQueryExecutor<TEntity>(queryModel);
            var sql = modelVisitor.Queries.First().ToString();

            return sql;
        }
    }

    public static class QueryGenerator_2_1_0
    {
        private static readonly FieldInfo QueryCompilerField = typeof(EntityQueryProvider).GetTypeInfo().DeclaredFields.Single(x => x.Name == "_queryCompiler");

        private static readonly TypeInfo QueryCompilerTypeInfo = typeof(QueryCompiler).GetTypeInfo();

        private static readonly FieldInfo QueryModelGeneratorField = QueryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_queryModelGenerator");

        private static readonly FieldInfo DatabaseField = QueryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_database");

        private static readonly PropertyInfo DependenciesProperty = typeof(Database).GetTypeInfo().DeclaredProperties.Single(x => x.Name == "Dependencies");

        public static string ToSql<TEntity>(IQueryable<TEntity> queryable)
            where TEntity : class
        {
            if (!(queryable is EntityQueryable<TEntity>) && !(queryable is InternalDbSet<TEntity>))
                throw new ArgumentException();

            var queryCompiler = (IQueryCompiler)QueryCompilerField.GetValue(queryable.Provider);
            var queryModelGenerator = (IQueryModelGenerator)QueryModelGeneratorField.GetValue(queryCompiler);
            var queryModel = queryModelGenerator.ParseQuery(queryable.Expression);
            var database = DatabaseField.GetValue(queryCompiler);
            var queryCompilationContextFactory = ((DatabaseDependencies)DependenciesProperty.GetValue(database)).QueryCompilationContextFactory;
            var queryCompilationContext = queryCompilationContextFactory.Create(false);
            var modelVisitor = (RelationalQueryModelVisitor)queryCompilationContext.CreateQueryModelVisitor();
            modelVisitor.CreateQueryExecutor<TEntity>(queryModel);
            return modelVisitor.Queries.Join(Environment.NewLine + Environment.NewLine);
        }
    }
}
