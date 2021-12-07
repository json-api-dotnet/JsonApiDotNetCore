using Xunit;

// Puts all test classes in the same collection, meaning they won't be run concurrently.
// There is no way in xUnit to throttle the number of async tests, which is needed to
// prevent PostgreSQL failing due to too many open connections at the same time.
// https://github.com/xunit/xunit/issues/2003
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]
