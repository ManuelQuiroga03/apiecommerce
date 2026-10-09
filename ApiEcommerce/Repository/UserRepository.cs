using ApiEcommerce.Data;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiEcommerce.Repository
{
    public class UserRepository : IUserRepository
    {
        public readonly ApplicationDbContext _db;
        private string? secretKey;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public UserRepository(ApplicationDbContext db, IConfiguration config,
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager, 
            IMapper mapper)
        {
            _db = db;
            secretKey = config.GetValue<string>("ApiSettings:SecretKey");
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public ApplicationUser? GetUser(string id)
        {
            return _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
        }

        public ICollection<ApplicationUser> GetUsers()
        {
            return _db.ApplicationUsers.OrderBy(u => u.UserName).ToList();
        }

        public bool IsUniqueUser(string username)
        {
            return !_db.Users.Any(u => u.Username.ToLower().Trim() == username.ToLower().Trim());
        }

        public async Task<UserLoginResponseDTO> Login(UserLoginDTO userLoginDTO)
        {
            if(string.IsNullOrEmpty(userLoginDTO.Username))
            {
                return new UserLoginResponseDTO()
                {
                    User = null,
                    Token = "",
                    Message = "El Username es requerido"
                };
            }

            var user = await _db.ApplicationUsers.FirstOrDefaultAsync<ApplicationUser>(u => 
                u.UserName != null && u.UserName.ToLower().Trim() == userLoginDTO.Username.ToLower().Trim());
            if (user == null)
            {
                return new UserLoginResponseDTO()
                {
                    User = null,
                    Token = "",
                    Message = "Username no encontrado"
                };
            }
            if (userLoginDTO.Password == null)
            {
                return new UserLoginResponseDTO()
                {
                    User = null,
                    Token = "",
                    Message = "Password requerido"
                };
            }
            bool isValid = await _userManager.CheckPasswordAsync(user, userLoginDTO.Password);
            if (!isValid)
            {
                return new UserLoginResponseDTO()
                {
                    User = null,
                    Token = "",
                    Message = "Password incorrecto"
                };
            }
            //JWT Token Generation
            var handlerToken = new JwtSecurityTokenHandler();
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException("Secretkey no está configurada.");
            }
            var roles = await _userManager.GetRolesAsync(user);
            var key = Encoding.UTF8.GetBytes(secretKey);
            var tokenDescripttor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim("username", user.UserName ?? string.Empty),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? string.Empty),
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = handlerToken.CreateToken(tokenDescripttor);
            return new UserLoginResponseDTO()
            {
                Token = handlerToken.WriteToken(token),
                User = _mapper.Map<UserDataDTO>(user),
                Message = "Usuario logueado correctamente."
            };
        }

        async Task<UserDataDTO> IUserRepository.Register(CreateUserDTO userDTO)
        {
            if (string.IsNullOrEmpty(userDTO.Username))
            {
                throw new ArgumentNullException("El username es requerido.");
            }
            if (userDTO.Password == null)
            {
                throw new ArgumentNullException("El password es requerido.");
            }

            var user = new ApplicationUser()
            {
                UserName = userDTO.Username,
                Email = userDTO.Username,
                NormalizedEmail = userDTO.Username.ToUpper(),
                Name = userDTO.Name
            };
            var result = await _userManager.CreateAsync(user, userDTO.Password);
            if (result.Succeeded)
            {
                var userRole = userDTO.Role ?? "User";
                var roleExists = await _roleManager.RoleExistsAsync(userRole);
                if (!roleExists)
                {
                    var identityRole = new IdentityRole(userRole);
                    await _roleManager.CreateAsync(identityRole);
                }
                await _userManager.AddToRoleAsync(user, userRole);
                var createdUser = _db.ApplicationUsers
                    .FirstOrDefaultAsync(u => u.UserName == userDTO.Username);
                return _mapper.Map<UserDataDTO>(await createdUser);
            }
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ApplicationException($"Error al crear el usuario: {errors}");
        }

        public bool Save()
        {
            return _db.SaveChanges() >= 0 ? true : false;
        }

    }
}
