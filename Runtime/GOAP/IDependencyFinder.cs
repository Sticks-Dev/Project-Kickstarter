namespace Kickstarter.GOAP
{
    public interface IDependencyFinder
    {

    }

    public interface IDependencyFinder<T> : IDependencyFinder
    {
        public T[] GetDependencies()
        {
            return default;
        }
        public T GetDependency()
        {
            return default;
        }
    }
}
