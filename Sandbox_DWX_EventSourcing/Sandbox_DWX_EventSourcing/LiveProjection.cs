using System;
using System.Collections.Generic;

namespace Sandbox_DWX_EventSourcing
{
    class LiveProjection<TData, TIntermediateState>
    {
        private TIntermediateState _value;
        private readonly Projection<TIntermediateState> _projection;
        private readonly Func<TIntermediateState, TData> _finalEvaluation;

        public LiveProjection(
            TIntermediateState value,
            Projection<TIntermediateState> projection,
            Func<TIntermediateState, TData> final_evaluation)
        {
            _value = value;
            _projection = projection;
            _finalEvaluation = final_evaluation;
        }

        public event Action<TData> Next;
        public TData Value { get { return _finalEvaluation(_value); } }

        protected void OnNext(TData data)
        {
            var handler = Next;
            if (handler != null) handler(data);
        }

        public void Handle(IEnumerable<object> events)
        {
            _value = _projection.Eval(_value, events);
            OnNext(_finalEvaluation(_value));
        }
    }
}