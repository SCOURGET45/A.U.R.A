using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Models;

namespace Aura.Services
{
    public class AsistenciaService : IAsistenciaService
    {
        public ResultadoAsistencia CalcularAsistenciaUnidad(List<RegistroAsistencia> historial, int totalClasesUnidad)
        {
            if (totalClasesUnidad <= 0)
                throw new ArgumentException("El total de clases debe ser mayor a cero.");

            int faltasDirectas = historial.Count(x => x.Estado == TipoAsistencia.Falta);
            int retardos = historial.Count(x => x.Estado == TipoAsistencia.Retardo);

            int faltasPorRetardo = retardos / 3;
            int faltasTotales = faltasDirectas + faltasPorRetardo;

            int clasesAsistidas = totalClasesUnidad - faltasTotales;
            double porcentaje = Math.Round(((double)clasesAsistidas / totalClasesUnidad) * 100, 2);

            int limiteFaltas = (int)Math.Floor(totalClasesUnidad * 0.20);
            int faltasRestantes = limiteFaltas - faltasTotales;

            faltasRestantes = faltasRestantes < 0 ? 0 : faltasRestantes;

            EstadoSemaforo estadoActual;
            if (porcentaje >= 90)
            {
                estadoActual = EstadoSemaforo.Verde;
            }
            else if (porcentaje >= 80)
            {
                estadoActual = EstadoSemaforo.Amarillo;
            }
            else
            {
                estadoActual = EstadoSemaforo.Rojo;
            }

            return new ResultadoAsistencia
            {
                PorcentajeActual = porcentaje,
                FaltasPermitidasRestantes = faltasRestantes,
                Semaforo = estadoActual
            };
        }
    }
}
