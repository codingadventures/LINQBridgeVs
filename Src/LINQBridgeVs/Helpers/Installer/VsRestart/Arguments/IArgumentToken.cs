namespace MidnightDevelopers.VisualStudio.VsRestart.Arguments
{
    internal interface IArgumentToken
    {
        string Argument { get; }
    }

    internal abstract class BaseArgumentToken : IArgumentToken
    {
        private readonly string _argument;

        protected BaseArgumentToken(string argument)
        {
            _argument = argument;
        }

        public string Argument
        {
            get { return _argument; }
        }

        public override string ToString()
        {
            return _argument;
        }
    }

    internal class GenericArgumentToken : BaseArgumentToken
    {
        public GenericArgumentToken(string argument)
            : base(argument)
        { }
    }

    internal class SolutionArgumentToken : BaseArgumentToken
    {
        public SolutionArgumentToken(string argument)
            : base(argument)
        { }
    }

    internal class ProjectArgumentToken : BaseArgumentToken
    {
        public ProjectArgumentToken(string argument)
            : base(argument)
        { }
    }
}