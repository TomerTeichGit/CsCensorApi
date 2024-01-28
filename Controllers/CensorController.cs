using Microsoft.AspNetCore.Mvc;
using System.IO;
using NsfwSpyNS;
using System.Text.Json;
using System.Diagnostics;

namespace CsCensorApi.Controllers;
[ApiController]
[Route("[controller]")]
public class CensorshipController : ControllerBase
{
    public NsfwSpy spy = new();
    public string outDir = Path.Combine(Directory.GetCurrentDirectory(), "outDir");
    public CensorshipController()
    {

    }
    static void EmptyDirectory(string directoryPath)
    {
        // Check if the directory exists
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
        }

        // Get all files in the directory
        string[] files = Directory.GetFiles(directoryPath);

        // Delete each file
        foreach (string file in files)
        {
            System.IO.File.Delete(file);
        }

        // Get all subdirectories in the directory
        string[] subdirectories = Directory.GetDirectories(directoryPath);

        // Delete each subdirectory (and its contents) recursively
        foreach (string subdirectory in subdirectories)
        {
            Directory.Delete(subdirectory, true);
        }
    }
    private static List<string> SplitVideoToFrames(string videoFilePath, string outputFolderPath)
    {
        //* Empty the outDir
        try
        {
            EmptyDirectory(outputFolderPath);
            Console.WriteLine($"Directory '{outputFolderPath}' emptied successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        List<string> frameFilePaths = [];

        // Specify the FFmpeg command
        // FFmpeg command to split the video into frames
        string ffmpegCommand = $"ffmpeg -i {videoFilePath} -vf fps=1 {outputFolderPath}/frame_%04d.png";

        // Create process start info
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "bash", // Use bash to run the FFmpeg command
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true, // Specify the directory where ffmpeg is located
        };

        // Start the process
        using (Process process = new Process { StartInfo = psi })
        {
            process.Start();

            // Pass the FFmpeg command to the process
            process.StandardInput.WriteLine(ffmpegCommand);
            process.StandardInput.Close();

            // Read the output and error streams (optional)
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            // Wait for the process to finish
            process.WaitForExit();

            string[] files = Directory.GetFiles(outputFolderPath, "frame_*.png");
            frameFilePaths.AddRange(files);


            // Print output and error messages (optional)
            Console.WriteLine("Output: " + output);
            Console.WriteLine("Error: " + error);
        }

        return frameFilePaths;

    }


    [HttpPost()]
    public ActionResult<string> Post(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        // Create the directory if it doesn't exist
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        // Generate a unique file name
        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

        string fileType = file.ContentType.Split('/')[0];
        switch (fileType)
        {
            case "video":
                // Combine the directory and file name to get the full path
                string filePath = Path.Combine(uploadPath, fileName);

                // Use CopyToAsync to copy the contents of the file to the specified path
                using (FileStream stream = new(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                

                List<string> filePaths = SplitVideoToFrames(filePath, outDir);

                List<NsfwSpyValue> res = spy.ClassifyImages(filePaths);
                string jsonString = JsonSerializer.Serialize(res);

                System.IO.File.Delete(filePath);

                return Ok(jsonString);

            case "image":
                // Combine the directory and file name to get the full path
                string filePathImg = Path.Combine(uploadPath, fileName);

                // Use CopyToAsync to copy the contents of the file to the specified path
                using (FileStream stream = new(filePathImg, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                NsfwSpyResult resImg = spy.ClassifyImage(filePathImg);
                string jsonStringImg = JsonSerializer.Serialize(resImg);
                
                System.IO.File.Delete(filePathImg);

                return Ok(jsonStringImg);
            default:
                return BadRequest("Bad File Type.");
        }
    }
}