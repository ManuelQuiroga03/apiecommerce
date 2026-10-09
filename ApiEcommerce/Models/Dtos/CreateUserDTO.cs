using System.ComponentModel.DataAnnotations;

namespace ApiEcommerce.Models.Dtos
{
    public class CreateUserDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        public string? Username { get; set; }
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string? Password { get; set; }
        [Required(ErrorMessage = "El rol es obligatorio")]
        public string? Role { get; set; }
    }
}
