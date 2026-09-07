using Microsoft.AspNetCore.Mvc;
namespace TypicalTechTools.Controllers
{
    public class WarrantyController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public WarrantyController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public IActionResult Index()
        {
            //Retrieve a list of the currently uploaded files and put it in the Viewbag to be passed to the view when opened.
            ViewBag.FileList = GetUploadFileList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile? file)
        {
            if (file is null || file.Length == 0)
            {
                //generate an error message to infrom the user they have no file selected.
                ViewBag.WarrantyErrorMessage = "No File Selected";
                //Retrieve a list of the currently uploaded files and put it in the Viewbag to be passed to the view when opened.
                ViewBag.FileList = GetUploadFileList();
                return View(nameof(Index));
            }

            //Check to see if there already is a file in the system with the same name.
            string fileName = GenerateUniqueFileName(Path.GetFileName(file.FileName));
            string uploadDirectory = GetUploadDirectory();
            Directory.CreateDirectory(uploadDirectory);

            string targetFile = Path.Combine(uploadDirectory, fileName);
            await using var fileStream = new FileStream(targetFile, FileMode.CreateNew);
            await file.CopyToAsync(fileStream);

            return RedirectToAction("Index");
        }

        public IActionResult DownloadClaimForm()
        {
            //Get the file path of the warranty form.
            string filePath = Path.Combine(
                _environment.WebRootPath,
                "Forms",
                "TypicalTools_WarrantyForm.docx");

            //Read the File data into Byte Array.
            byte[] bytes = System.IO.File.ReadAllBytes(filePath);

            //Send the File to Download.
            return File(bytes, "application/octet-stream", Path.GetFileName(filePath));
        }

        public IActionResult DownloadFile(string filePath)
        {
            string safePath = GetUploadPath(filePath);

            if (!System.IO.File.Exists(safePath))
            {
                return NotFound();
            }

            //Read the File data into Byte Array.
            byte[] bytes = System.IO.File.ReadAllBytes(safePath);

            //Send the File to Download.
            return File(bytes, "application/octet-stream", Path.GetFileName(safePath));
        }

        public IActionResult Delete(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string safePath = GetUploadPath(fileName);

            if (!System.IO.File.Exists(safePath))
            {
                return NotFound();
            }

            //Creates an anonymous object list where each entry holds 2 values, the filename and the filepath.
            ViewBag.File = new { Name = fileName, Path = fileName };
            return View();
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string filePath)
        {
            //Try catch to handl if the file delete has any issues
            try
            {
                //Tell the system to delete the file based upon the provided filepath.
                System.IO.File.Delete(GetUploadPath(filePath));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return RedirectToAction("Index");
        }


        private IEnumerable<UploadedFileDetails> GetUploadFileList()
        {
            //Retrieves all the current files located in the Uploads folder of the wwwroot directory.
            string uploadDirectory = GetUploadDirectory();
            Directory.CreateDirectory(uploadDirectory);
            string[] filePaths = Directory.GetFiles(uploadDirectory);
            //Creates an list of UploadFileDetails objects to hold the filename and the filepath of each file in the folder.
            var files = filePaths.Select(file => new UploadedFileDetails
            {
                Name = Path.GetFileName(file),
                Path = Path.GetFileName(file)
            });
            //Return the list to the caller.
            return files;
        }

        /// <summary>
        /// Takes a provided file name and checks it against the current files in the uploads folder.
        /// If a file exists with the same name, the method will try to append the file name with a numeric
        /// value and then check if that updated file name exists. it will keep going until a unique name
        /// is determined.
        /// </summary>
        /// <param name="fileName">The starting file name of the file(Example: File.txt)</param>
        /// <returns>
        /// The original name if no match is found(File.txt) or a modified file name generated using the
        /// original file name(File(1).txt etc)
        /// </returns>
        private string GenerateUniqueFileName(string fileName)
        {
            //Takes the starting file name and separates the name and extension sections.
            string startingName = Path.GetFileNameWithoutExtension(fileName);
            string fileExtension = Path.GetExtension(fileName);
            //Sets the initial updatedFileName to be the same as the starting name. This will be changed if this name
            //is not found to be unique.
            string updatedFileName = fileName;
            //Sets a counter to be used for the numeric modifier for the file name if needed.
            int counter = 1;

            //Checks if any current files in the folder match the file name, if a match is found the loop will run to
            //modify the name and try again.
            while (System.IO.File.Exists(GetUploadPath(updatedFileName)))
            {
                //Change the updated name to be the starting name plus a numeric modifier based upon the counter value
                updatedFileName = $"{startingName}({counter}){fileExtension}";
                //Increase the counter for if it needs to be higher in the next loop.
                counter++;
            }

            return updatedFileName;
        }

        private string GetUploadDirectory()
        {
            return Path.Combine(_environment.WebRootPath, "Uploads");
        }

        private string GetUploadPath(string filePath)
        {
            return Path.Combine(GetUploadDirectory(), Path.GetFileName(filePath));
        }
    }

    public class UploadedFileDetails
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }
}
