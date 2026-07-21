namespace Aura.Models
{
    public class ResultadoAsistencia
    {
        public double PorcentajeActual { get; set; }
        public int FaltasPermitidasRestantes { get; set; }
        public EstadoSemaforo Semaforo { get; set; }
    }
}
