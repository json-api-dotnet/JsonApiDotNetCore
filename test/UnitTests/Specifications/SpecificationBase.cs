using System;
using System.Threading.Tasks;

namespace UnitTests.Specifications
{
    public abstract class SpecificationBase: IDisposable
    {
        public SpecificationBase()
        {
            Given().Wait();
            When().Wait();
        }

        public void Dispose()
        {
            CleanUp();
        }

        protected virtual Task Given()
        {
            return Task.CompletedTask;
        }

        protected virtual Task When()
        {
            return Task.CompletedTask;
        }

        protected virtual void CleanUp() { }
    }
}
