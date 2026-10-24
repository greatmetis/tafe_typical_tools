using TypicalTechTools.Models.Data;
using TypicalTechTools.Models.DTOs;

namespace TypicalTechTools.Models.Repositories;

public class AuthenticationRepository:IAuthenticationRepository
{
    private readonly TypicalTechToolDbContext _context;

    public AuthenticationRepository(TypicalTechToolDbContext context)
    {
        _context = context;
    }
    
    public AppUser Authenticate(LoginDTO loginDTO)
    {
        //Search the AppUsers table to try to find a record matching the username
        //that was provided in the login DTO
        var userDetails = _context.AppUsers.Where(u => u.UserName.Equals(loginDTO.UserName))
            .FirstOrDefault();
        //If no user was found, return null to indicate a login failure
        if (userDetails == null) 
        {
            return null;
        }
        //Run Bcrypts Enhavced Verify method and pass it the password the user gave in the DTO
        //as well as the hash from the retrived user account. It will run the required hash on the
        //provided password and then compare it to the Hash t see if it matches.
        if (BCrypt.Net.BCrypt.EnhancedVerify(loginDTO.Password, userDetails.PasswordHash)) 
        {
            return userDetails;
        }

        return null;
    }

    public AppUser CreateUser(CreateUserDTO userDTO)
    {
        //Search the AppUsers table to try to find a record matchin the username
        //that was provided in the login DTO
        var userDetails = _context.AppUsers.Where(u => u.UserName.Equals(userDTO.UserName))
            .FirstOrDefault();
        //If we did find a user, don't go any further and retrun null. We dont want to create 
        //another account with the same name 
        if (userDetails != null) 
        {
            return null;
        }
        //Create a new user using the details provide in the DTO. This includes hashing the 
        //password using BCrypt and defaulting the user role to GUEST.
        AppUser user = new AppUser
        {
            UserName = userDTO.UserName,
            PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(userDTO.Password),
            Role = "GUEST"
        };
        //Add the user to the DB set then save it into the database
        _context.AppUsers.Add(user);
        _context.SaveChanges();
        //Return the user to confirm the process is completed.
        return user;
    }
}