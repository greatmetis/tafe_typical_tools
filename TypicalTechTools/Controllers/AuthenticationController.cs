using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using TypicalTechTools.Models.DTOs;
using TypicalTechTools.Models.Repositories;

namespace TypicalTechTools.Controllers;

public class AuthenticationController : Controller
{
    private readonly IAuthenticationRepository _repository;
    public AuthenticationController(IAuthenticationRepository repository)
    {
        _repository = repository;
    }
    
   
    public IActionResult Login([FromQuery]string ReturnUrl)
    {
        //Create a login DTO and set the return URL inside it so the login will know where the
        //user needs to be sent once the login is finished
        LoginDTO login = new LoginDTO 
        {
            //If the return URl was blank, set it to the homepage
            ReturnUrl = string.IsNullOrWhiteSpace(ReturnUrl) ? "/" : ReturnUrl
        };
        //PAss the DTo to the login form as it is provided to the user.
        return View(login);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDTO loginDTO)
    {
        //Pass the DTo to the authenticate method in our repo. If it is a valid login we will
        //get the user details back.
        var user = _repository.Authenticate(loginDTO);
        //If the user returned null, it means the login failed
        if (user == null) 
        {
            ViewBag.LoginMessage = "Username or Password Incorrect!";
            return View();
        }
        //Create a list of claims that represent identifiebel fetaures about the user that will
        //normally be needed by other parts of the system to manage their access or any
        //customisation in the app such as heading and messages
        var claims = new List<Claim>
        {
            //Each claim is a key:value pair of strings, but some of the standard types can be found 
            //in the Claim types enum
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("Department","Marketing") //<=This is for example only
        };
        //Create a new claims principal which is just a wrapper to identify the user details and the 
        //authentication system being used to track them
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims,
            CookieAuthenticationDefaults.AuthenticationScheme));

        //Run the Sign in command and pass it the authentication type and the user's principal
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        //Redirect to the endpoint in our return URL. 
        return Redirect(loginDTO.ReturnUrl);
    }

}