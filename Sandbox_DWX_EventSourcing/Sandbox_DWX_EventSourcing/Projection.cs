using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox_DWX_EventSourcing
{

    /// <summary>
    /// Definiert ein Konzept ('Offener Betrag') als Projektion auf den Ereignissen
    /// </summary>
    class Projection<TState>
    {
        private readonly Dictionary<Type, Func<TState, object, TState>> _handlers
            = new Dictionary<Type, Func<TState, object, TState>>();

        public Projection<TState> Fuer<TEvent>(Func<TState, TEvent, TState> handler)
        {
            _handlers.Add(
                typeof (TEvent),
                (TState current_state, object @event) => handler(current_state, (TEvent) @event));
            return this;
        }

        public TState Eval(TState initial_value, object @event)
        {
            var t = @event.GetType();

            return !_handlers.ContainsKey(t)
                ? initial_value
                : _handlers[t](initial_value, @event);
        }

        public TState Eval(TState initial_value, IEnumerable<object> history)
        {
            return history.Aggregate(initial_value, Eval);
        }
    }
}