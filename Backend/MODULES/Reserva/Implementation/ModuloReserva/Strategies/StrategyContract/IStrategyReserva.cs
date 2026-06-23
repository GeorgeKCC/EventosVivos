namespace ModuloReserva.Strategies.StrategyContract
{
    internal interface IStrategyReserva
    {
        int StrategyId { get; }

        Task ExecuteAsync(int ReservaId);
    }
}
