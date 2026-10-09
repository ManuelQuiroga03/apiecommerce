using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersionNeutral]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProducts()
        {
            var users = _userRepository.GetUsers();
            var usersDTO = _mapper.Map<List<UserDTO>>(users);

            return Ok(usersDTO);
        }

        [HttpGet("{id}", Name = "GetUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUser(string id)
        {
            var user = _userRepository.GetUser(id);
            if (user == null)
            {
                return NotFound($"El usuario con el id {id} no existe");
            }
            var userDto = _mapper.Map<UserDTO>(user);
            return Ok(userDto);
        }

        [AllowAnonymous]
        [HttpPost(Name="RegisterUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterUser([FromBody] 
            CreateUserDTO createUserDTO)
        {
            // Implementation for registering a new user
            if (createUserDTO == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (string.IsNullOrWhiteSpace(createUserDTO.Username))
            {
                return BadRequest("Username es requerido.");
            }
            if (!_userRepository.IsUniqueUser(createUserDTO.Username))
            {
                return BadRequest("El usuario ya existe.");
            }
            var result = await _userRepository.Register(createUserDTO);
            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al registrar el usuario.");
            }
            return CreatedAtRoute("GetUser", new { id = result.Id }, result);
        }

        [AllowAnonymous]
        [HttpPost("Login", Name = "LoginUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LoginUser([FromBody]
            UserLoginDTO userLoginDTO)
        {
            // Implementation for logging in a user
            if (userLoginDTO == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userRepository.Login(userLoginDTO);
            if (user == null)
            {
                return Unauthorized();
            }
            return Ok(user);
        }
    }
}
