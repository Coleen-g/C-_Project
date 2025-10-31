using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

public class LoginModel : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; }

    public string ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string SchoolId { get; set; }
    }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        // Replace with your user validation logic (e.g., database lookup)
        if (Input.Username == "testuser" && Input.SchoolId == "12345")
        {
            // TODO: Set authentication cookie/session
            return RedirectToPage("/Index");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return Page();
    }
}