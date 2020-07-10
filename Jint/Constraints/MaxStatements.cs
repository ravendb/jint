using Jint.Runtime;

namespace Jint.Constraints
{
    public sealed class MaxStatements : IConstraint
    {
        public int Max => _maxStatements;

        private int _maxStatements;

        private int _statementsCount;

        public MaxStatements(int maxStatements)
        {
            Change(maxStatements);
        }

        public void Change(int maxStatements)
        {
            _maxStatements = maxStatements;
        }

        public void Check()
        {
            if (_maxStatements > 0 && _statementsCount++ > _maxStatements)
            {
                ExceptionHelper.ThrowStatementsCountOverflowException();
            }
        }

        public void Reset()
        {
            _statementsCount = 0;
        }
    }
}