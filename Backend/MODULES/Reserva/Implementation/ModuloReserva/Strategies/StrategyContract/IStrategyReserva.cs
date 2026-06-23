namespace ModuloReserva.Strategies.StrategyContract
{
    internal interface IStrategyReserva
    {
        int StrategyId { get; }

        /// <summary>
        /// Ejecuta la estrategia de cambio de estado para una reserva.
        /// </summary>
        /// <param name="ReservaId">Identificador de la reserva.</param>
        /// <returns></returns>
        Task ExecuteAsync(int ReservaId);
    }
}
