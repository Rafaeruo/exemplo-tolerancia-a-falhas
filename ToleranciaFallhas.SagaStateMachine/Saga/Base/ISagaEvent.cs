namespace ToleranciaFalhas.Shared.Saga.Base
{
    public interface ISagaEvent<TState, TStep, TKey> 
        where TState : SagaState<TStep, TKey>
        where TStep : Enum
    {
        TKey GetKey();
        TState Apply(TState? currentState = null);
    }
}