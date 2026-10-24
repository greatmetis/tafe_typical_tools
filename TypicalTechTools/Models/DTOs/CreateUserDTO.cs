namespace TypicalTechTools.Models.DTOs;

public class CreateUserDTO
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string PasswordConfirmation { get; set; } = "";
}