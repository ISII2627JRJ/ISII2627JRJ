using AppForSEII.Web.API;

namespace AppForSEII.Web
{
    // Guarda el carrito de reposición mientras el usuario pasa de una página a otra
    public class ReposicionStateContainer
    {
        // Lo que se enviará al POST CrearReposicion de la API
        public ReposicionForCreateDTO Reposicion { get; private set; } = new ReposicionForCreateDTO();

        // Los libros elegidos, con su título, autor y precio, para mostrarlos en el carrito
        public List<LibroParaReponerDTO> LibrosSeleccionados { get; private set; } = new List<LibroParaReponerDTO>();

        // Suma de (precio de reposición x cantidad) de todos los libros del carrito
        public double PrecioTotal { get; private set; }

        // Añade un libro al carrito si no estaba ya
        public void AddLibroToReposicion(LibroParaReponerDTO libro)
        {
            if (!Reposicion.Libros.Any(l => l.LibroId == libro.Id))
            {
                // Cantidad inicial 2: el caso de uso exige que sea mayor que 1
                Reposicion.Libros.Add(new ItemDTO { LibroId = libro.Id, Cantidad = 2 });
                LibrosSeleccionados.Add(libro);
                ComputeTotalPrice();
            }
        }

        // Quita un libro del carrito
        public void RemoveLibroFromReposicion(LibroParaReponerDTO libro)
        {
            var item = Reposicion.Libros.FirstOrDefault(l => l.LibroId == libro.Id);
            if (item != null)
                Reposicion.Libros.Remove(item);

            LibrosSeleccionados.RemoveAll(l => l.Id == libro.Id);
            ComputeTotalPrice();
        }

        // Cambia la cantidad de un libro (se usará en la página de crear la reposición)
        public void SetCantidad(int libroId, int cantidad)
        {
            var item = Reposicion.Libros.FirstOrDefault(l => l.LibroId == libroId);
            if (item != null)
            {
                item.Cantidad = cantidad;
                ComputeTotalPrice();
            }
        }

        // Vacía el carrito
        public void ClearReposicionCart()
        {
            Reposicion.Libros.Clear();
            LibrosSeleccionados.Clear();
            PrecioTotal = 0;
        }

        private void ComputeTotalPrice()
        {
            PrecioTotal = LibrosSeleccionados.Sum(libro =>
                libro.PrecioReposicion * Reposicion.Libros.First(l => l.LibroId == libro.Id).Cantidad);
        }
    }
}