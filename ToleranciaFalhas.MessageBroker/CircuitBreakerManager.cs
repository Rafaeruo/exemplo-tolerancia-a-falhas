using System;
using System.Collections.Concurrent;

namespace ToleranciaFalhas.MessageBroker
{
    public class CircuitBreakerManager
    {
        private readonly ConcurrentDictionary<string, CircuitBreakerState> _circuitStates = new();
        private readonly int _failureThreshold;
        private readonly TimeSpan _openToHalfOpenWaitTime;

        public CircuitBreakerManager(int failureThreshold = 10, TimeSpan? waitTime = null) // TODO: maybe make these configurable per service?
        {
            waitTime ??= TimeSpan.FromSeconds(30);

            _failureThreshold = failureThreshold;
            _openToHalfOpenWaitTime = waitTime.Value;
        }

        public bool CanProceed(string key)
        {
            var state = _circuitStates.GetOrAdd(key, _ => new CircuitBreakerState());

            if (state.State == CircuitState.Open)
            {
                if (DateTime.UtcNow >= state.LastFailureTime.Add(_openToHalfOpenWaitTime))
                {
                    state.State = CircuitState.HalfOpen;
                    return true;
                }

                return false;
            }

            return true;
        }

        public void ReportSuccess(string key)
        {
            if (_circuitStates.TryGetValue(key, out var state))
            {
                if (state.State == CircuitState.HalfOpen || state.State == CircuitState.Open)
                {
                    state.Reset();
                }
            }
        }

        public void ReportFailure(string key)
        {
            var state = _circuitStates.GetOrAdd(key, _ => new CircuitBreakerState());
            state.FailureCount++;
            state.LastFailureTime = DateTime.UtcNow;

            if (state.State == CircuitState.HalfOpen || state.FailureCount >= _failureThreshold)
            {
                state.State = CircuitState.Open;
            }
        }

        private class CircuitBreakerState
        {
            public CircuitState State { get; set; } = CircuitState.Closed;
            public int FailureCount { get; set; } = 0;
            public DateTime LastFailureTime { get; set; } = DateTime.MinValue;

            public void Reset()
            {
                State = CircuitState.Closed;
                FailureCount = 0;
                LastFailureTime = DateTime.MinValue;
            }
        }

        private enum CircuitState
        {
            Closed,
            Open,
            HalfOpen
        }
    }
}
