using FluentValidation;
using ModuloReserva.Strategies.StrategyContract;
using ModuloReservaContract;
using ModuloReservaContract.Dtos;
using ValidationException = FluentValidation.ValidationException;

namespace ModuloReserva.ImplementationUseCase
{
    internal class ConfirmarReservaUseCase(IEnumerable<IStrategyReserva> strategies,
                                           IValidator<RequestEstadoReserva> validator) : IConfirmarReservaUseCase
    {
        /// <summary>
        /// Ejecuta la estrategia correspondiente para cambiar el estado de una reserva.
        /// </summary>
        /// <param name="requestEstadoReserva">Datos con el nuevo estado de la reserva.</param>
        /// <returns></returns>
        public async Task<string> ExecuteAsync(RequestEstadoReserva requestEstadoReserva)
        {
            var validate = await validator.ValidateAsync(requestEstadoReserva);
            if (validate.IsValid is false)
            {
                throw new ValidationException(validate.Errors);
            }

           var strategy = strategies.FirstOrDefault(s => s.StrategyId == (int)requestEstadoReserva.EstadoReservaEnum) 
                          ?? throw new InvalidOperationException($"No strategy found for EstadoReservaEnum: {requestEstadoReserva.EstadoReservaEnum}");

           var result = await strategy.ExecuteAsync(requestEstadoReserva.ReservaId);
            return result;
        }
    }

    internal class ValidationConfirmarReservaUseCase : AbstractValidator<RequestEstadoReserva>
    {
        public ValidationConfirmarReservaUseCase()
        {
            RuleFor(x => x.ReservaId).NotNull().NotEmpty().GreaterThan(0);
            RuleFor(x => x.EstadoReservaEnum).IsInEnum();
        }
    }
}
