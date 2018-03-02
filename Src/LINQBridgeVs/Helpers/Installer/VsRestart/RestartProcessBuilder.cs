using MidnightDevelopers.VisualStudio.VsRestart.Arguments;
using System.Diagnostics;
using System.Linq;

namespace FirstTimeConfigurator.VsRestart
{
    internal class RestartProcessBuilder
    {
        private string _solutionFile;
        private string _devenv;
        private ArgumentTokenCollection _arguments;
        private string _projectFile;
        private string _verb = null;

        public RestartProcessBuilder WithSolution(string solutionFile)
        {
            _solutionFile = solutionFile;
            return this;
        }

        public RestartProcessBuilder WithArguments(ArgumentTokenCollection arguments)
        {
            _arguments = arguments;
            return this;
        }

        public RestartProcessBuilder WithDevenv(string devenv)
        {
            _devenv = devenv;
            return this;
        }

        public RestartProcessBuilder WithProject(string projectFile)
        {
            _projectFile = projectFile;
            return this;
        }

        public RestartProcessBuilder WithElevatedPermission()
        {
            _verb = "runas";
            return this;
        }

        public ProcessStartInfo Build()
        {
            return new ProcessStartInfo
            {
                FileName = _devenv,
                ErrorDialog = true,
                UseShellExecute = true,
                Verb = _verb,
                Arguments = BuildArguments(),
            };
        }

        private string BuildArguments()
        {
            if (!string.IsNullOrEmpty(_solutionFile))
            {
                if (_arguments.OfType<SolutionArgumentToken>().Any())
                {
                    _arguments.Replace<SolutionArgumentToken>(new SolutionArgumentToken(Quote(_solutionFile)));
                }
                else if (_arguments.OfType<ProjectArgumentToken>().Any())
                {
                    _arguments.Replace<ProjectArgumentToken>(new SolutionArgumentToken(Quote(_solutionFile)));
                }
                else
                {
                    _arguments.Add(new SolutionArgumentToken(Quote(_solutionFile)));
                }
            }

            if (!string.IsNullOrEmpty(_projectFile))
            {
                if (_arguments.OfType<SolutionArgumentToken>().Any())
                {
                    _arguments.Replace<SolutionArgumentToken>(new ProjectArgumentToken(Quote(_projectFile)));
                }
                else if (_arguments.OfType<ProjectArgumentToken>().Any())
                {
                    _arguments.Replace<ProjectArgumentToken>(new ProjectArgumentToken(Quote(_projectFile)));
                }
                else
                {
                    _arguments.Add(new ProjectArgumentToken(Quote(_projectFile)));
                }
            }

            string escapedArguments = _arguments.ToString().Replace(Quote(_devenv), string.Empty);

            return escapedArguments;
        }

        private string Quote(string input)
        {
            return string.Format("\"{0}\"", input);
        }
    }
}
