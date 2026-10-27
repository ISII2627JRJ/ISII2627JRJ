namespace AppForSEII.API.DTOs
{
    public class ResenaItemDTO
    {
        public int LibroId { get; set; }
        public string TituloLibro { get; set; }
        public int Puntuacion { get; set; }
        public string Comentario { get; set; }

        public ResenaItemDTO(int libroId, string tituloLibro, int puntuacion, string comentario)
        {
            LibroId = libroId;
            TituloLibro = tituloLibro;
            Puntuacion = puntuacion;
            Comentario = comentario;
        }
    }
}