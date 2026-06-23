using ModuloEventoContract.Dtos;

namespace ModuloEventoContract
{
    public interface ICrearEventoUseCase
    {
        /// <summary>
        /// Crea un nuevo evento en el sistema.
        /// </summary>
        /// <param name="requestCrearEvento">Datos del evento a crear.</param>
        /// <returns></returns>
        public Task ExecuteAsync(RequestCrearEvento requestCrearEvento);
    }
}
