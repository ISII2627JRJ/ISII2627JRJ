namespace AppForSEII.API.DTOs
{
    //el detalle tiene los mismos datos que el formulario más el Id y la fecha (como RentalDetailDTO)
    public class ResenaDetailDTO : ResenaForCreateDTO
    {
        public ResenaDetailDTO(int id, DateTime fecha, string userName, string nombre, string apellidos,
            string direccion, string telefono, string tituloResena, IList<ResenaItemDTO> resenaItems)
            : base(userName, nombre, apellidos, direccion, telefono, tituloResena, resenaItems)
        {
            Id = id;
            Fecha = fecha;
        }

        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        //la fecha se genera con DateTime.Now, así que se comparan con un margen de un minuto
        protected bool CompareDate(DateTime date1, DateTime date2)
        {
            return Math.Abs((date1 - date2).TotalMinutes) < 1;
        }

        public override bool Equals(object? obj)
        {
            return obj is ResenaDetailDTO dTO &&
                   base.Equals(obj) &&
                   Id == dTO.Id &&
                   CompareDate(Fecha, dTO.Fecha);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Id, Fecha);
        }
    }
}