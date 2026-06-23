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
        public async Task ExecuteAsync(RequestEstadoReserva requestEstadoReserva)
        {
            var validate = await validator.ValidateAsync(requestEstadoReserva);
            if (validate.IsValid is false)
            {
                throw new ValidationException(validate.Errors);
            }

           var strategy = strategies.FirstOrDefault(s => s.StrategyId == (int)requestEstadoReserva.EstadoReservaEnum) 
                          ?? throw new InvalidOperationException($"No strategy found for EstadoReservaEnum: {requestEstadoReserva.EstadoReservaEnum}");

            await strategy.ExecuteAsync(requestEstadoReserva.ReservaId);
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
