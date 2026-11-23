using AppForSEII.Web.API;

namespace AppForSEII.Web
{
    public class ResenaStateContainer
    {
        //se crea una reseña vacía cuando se crea una instancia de ResenaStateContainer
        public ResenaForCreateDTO Resena { get; private set; } = new ResenaForCreateDTO()
        {
            ResenaItems = new List<ResenaItemDTO>()
        };

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();

        //añade un libro a la lista de libros a reseñar
        public void AddLibroToResena(LibroParaResenarDTO libro)
        {
            //antes de añadir el libro comprobamos que no se haya añadido ya
            if (!Resena.ResenaItems.Any(ri => ri.LibroId == libro.Id))
                Resena.ResenaItems.Add(new ResenaItemDTO()
                {
                    LibroId = libro.Id,
                    Titulo = libro.Titulo,
                    TipoLibro = libro.TipoLibro,
                    Editorial = libro.Editorial,
                });

            NotifyStateChanged();
        }

        //elimina un libro de la lista de libros a reseñar
        public void RemoveResenaItem(ResenaItemDTO item)
        {
            Resena.ResenaItems.Remove(item);
            NotifyStateChanged();
        }

        //elimina todos los libros de la lista
        public void ClearResenaCart()
        {
            Resena.ResenaItems.Clear();
            NotifyStateChanged();
        }

        //cuando la reseña ya se ha publicado, se crea una reseña nueva sin datos
        public void ResenaProcessed()
        {
            Resena = new ResenaForCreateDTO()
            {
                ResenaItems = new List<ResenaItemDTO>()
            };
        }
    }
}