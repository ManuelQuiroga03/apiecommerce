using System.ComponentModel.DataAnnotations;

namespace ApiEcommerce.Models.Dtos
{
    public class CreateCategoryDTO
    {
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El nombre de la categoría no puede exceder los 50 caracteres.")]
        [MinLength(3, ErrorMessage = "El nombre de la categoría debe tener al menos 3 caracteres.")]
        public string Name { get; set; } = string.Empty;
    }
}
