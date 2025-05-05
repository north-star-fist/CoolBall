using Sergei.Safonov.StateMachine;
using VContainer;

namespace Coolball.Flow {

    /// <summary>
    /// Application Flow state machine.
    /// </summary>
    public class AppFlow : DiStateMachine, IAppFlow {
        public AppFlow(IObjectResolver resolver) : base(resolver) { }
    }
}
