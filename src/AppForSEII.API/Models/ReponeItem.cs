using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.Models
{
    public class ReponeItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        public DateTime FechaReposicion { get; set; }
    }
}