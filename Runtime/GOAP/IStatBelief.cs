using System;

namespace Kickstarter.GOAP
{
    public interface IStatBelief
    {
        public string Name { get; }
        public Func<bool> Evaluate { get; }
    }
}
