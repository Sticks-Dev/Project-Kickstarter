namespace Kickstarter.GOAP
{
    public interface IDependencyFinder
    {
        public T GetDependency<T>() where T : class;
    }
}
