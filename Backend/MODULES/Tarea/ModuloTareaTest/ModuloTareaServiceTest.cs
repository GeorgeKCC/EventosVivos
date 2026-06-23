using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModuloTarea;
using Quartz;

namespace ModuloTareaTest
{
    public class ModuloTareaServiceTest
    {
        [Fact]
        public void ModuloTareaRegisterServices_DebeRegistrarQuartz()
        {
            var services = new ServiceCollection();

            services.ModuloTareaRegisterServices();

            services.Should().Contain(s => s.ServiceType == typeof(ISchedulerFactory));
        }

        [Fact]
        public void ModuloTareaRegisterServices_ConCronPersonalizado_NoDebeLanzarExcepcion()
        {
            var services = new ServiceCollection();

            var act = () => services.ModuloTareaRegisterServices("0 0/1 * * * ?");

            act.Should().NotThrow();
        }

        [Fact]
        public void ModuloTareaRegisterServices_DebeRetornarMismaInstanciaServiceCollection()
        {
            var services = new ServiceCollection();

            var result = services.ModuloTareaRegisterServices();

            result.Should().BeSameAs(services);
        }
    }
}
