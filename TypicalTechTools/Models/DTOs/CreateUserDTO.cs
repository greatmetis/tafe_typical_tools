using System.ComponentModel.DataAnnotations;

namespace TypicalTechTools.Models.DTOs;

public class CreateUserDTO
{
    public string UserName { get; set; } = "";
    [RegularExpression("^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[*.!@$%^&,?_-])[A-Za-z0-9*.!@$%^&,?_-].{8,20}$", 
        ErrorMessage ="Passwords must have at least 1 upper case letter, 1 lower case letter, 1 number," +
                      " at least 1 of the following symbols *.!@$%^&,?_- and be between 8-20 characters long")]
    public string Password { get; set; } = "";
    public string PasswordConfirmation { get; set; } = "";
}