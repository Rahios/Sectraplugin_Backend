using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Interfaces;
using DTO;

namespace DAL.FileSystem
{
	public class HistolungFS : IHistolungFS
	{
		// A T T R I B U T E S
		private string backendWorkDirPath; // Path to the working directory ("/home/user/appBackend/")
		private string wsiFolder = "/home/user/appHistolung/data/tcga/wsi";
		private string outputFolder = "/home/user/appHistolung/data/outputs";
		private string histolungWorkDirPath = "/home/user/appHistolung";

		// C O N S T R U C T O R
		public HistolungFS(string basePath)
		{
			backendWorkDirPath = basePath;
		}


		// M E T H O D S
		// Update the .env file with the new values of the image to analyse
		// and then re-run the Histolung service
		public HistolungResponse AnalyzeImage(HistolungRequest request)
		{
			// Initialize the response
			HistolungResponse response = new HistolungResponse();
			response.Prediction = "Failure. ";
			response.Heatmap = Array.Empty<byte>();

			// Verify the paths and folders
			try
			{
				// Check if the wsi and output folders exist
				if (!Directory.Exists(wsiFolder) || !Directory.Exists(outputFolder))
				{
					//Directory.CreateDirectory(wsiFolder);
					throw new Exception("WSI or Output folder does not exist");
				}
			}
			catch (Exception e)
			{
				string message = "Error verifying the paths and folders for /appHistolung/data - OUTPUTS and TCGI/WSI";
				Console.WriteLine(message + e);
				response.Prediction += message;
				return response;
			}


			// Update the .env file with the new file name
			try
			{
				string envContent = $"WSI_NAME={request.ImageName}\nSIGMA=8\nTAG=v1.0\nGPU_DEVICE_IDS=0";
				// Write the content to the .env file, overwriting the previous content if it exists
				File.WriteAllText($"{histolungWorkDirPath}/.env", envContent);
			}
			catch (Exception e)
			{
				string message = "Error updating the .env file";
				Console.WriteLine(message + e);
				response.Prediction += message;
				return response;
			}

			// Run the Histolung service with docker compose up
			try
			{
				// Prepare the command to run
				ProcessStartInfo processStartInfo = new ProcessStartInfo
				{
					FileName = "docker",                            // Command to run
					Arguments = "compose run hlung",    // Arguments to pass to the command 
					RedirectStandardOutput = true,          // Redirect the output to the console window
					RedirectStandardError = true,               // Redirect the error to the console window
					UseShellExecute = false,                        // Do not use the shell to execute the command
					CreateNoWindow = true                       // Do not create a window for the command
				};

				Console.WriteLine("Running the Histolung service");
				Process process = Process.Start(processStartInfo);
				process.WaitForExit(); // Wait for the process to finish before continuing
				Console.WriteLine("Histolung service finished running");

				Console.WriteLine("Reading the output of the Histolung service");
				Console.WriteLine(process.StandardOutput.ReadToEnd());
				Console.WriteLine(process.StandardError.ReadToEnd());

				// Check if the process exited with an error
				if (process.ExitCode != 0)
				{
					throw new Exception("Error running the Histolung service");
				}


			}
			catch (Exception e)
			{
				string message = "Error running the Histolung service";
				Console.WriteLine(message + e);
				response.Prediction += message;
				return response;
			}

			// Recover the prediction and heatmap from the output folder
			try
			{
				response.Prediction = File.ReadAllText($"{outputFolder}/predictions.csv");
				response.Heatmap = File.ReadAllBytes($"{outputFolder}/heatmap_{request.ImageName}.png");
			}
			catch (Exception e)
			{
				string message = "Error recovering the prediction and heatmap";
				Console.WriteLine(message + e);
				response.Prediction += message;
				return response;
			}

			// Return the response, wethever it is a success or a failure
			return response;
		}
	}
}
