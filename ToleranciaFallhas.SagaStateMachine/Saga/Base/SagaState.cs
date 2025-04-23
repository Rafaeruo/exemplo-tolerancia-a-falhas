namespace ToleranciaFalhas.Shared.Saga.Base
{
    public abstract record SagaState<TStep, TKey>
        where TStep : Enum
    {
        public required TStep Step { get; init; }

        public SagaState<TStep, TKey> WithStep(TStep newStep)
        {
            return this with { Step = newStep };
        }

        public abstract TKey GetKey();
    }
}