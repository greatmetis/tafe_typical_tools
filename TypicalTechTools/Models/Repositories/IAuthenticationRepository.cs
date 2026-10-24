using TypicalTechTools.Models.DTOs;

namespace TypicalTechTools.Models.Repositories;

public interface IAuthenticationRepository
{
    AppUser Authenticate(LoginDTO loginDTO);
    AppUser CreateUser(CreateUserDTO userDTO);
}