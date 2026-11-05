namespace AppForSEII.API.DTOs
{
    public class ReposicionItemDTO
    {
        public int LibroId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public int Cantidad { get; set; }

        public ReposicionItemDTO() { }

        public ReposicionItemDTO(int libroId, string titulo, string autor, int cantidad)
        {
            LibroId = libroId;
            Titulo = titulo;
            Autor = autor;
            Cantidad = cantidad;
        }
                public override bool Equals(object? obj)
        {
            return obj is ReposicionItemDTO dTO &&
                   LibroId == dTO.LibroId &&
                   Titulo == dTO.Titulo &&
                   Autor == dTO.Autor &&
                   Cantidad == dTO.Cantidad;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LibroId, Titulo, Autor, Cantidad);
        }
    }
}