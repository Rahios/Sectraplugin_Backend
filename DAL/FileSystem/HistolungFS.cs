using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Interfaces;
using Docker.DotNet.Models;
using Docker.DotNet;
using DTO;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.AccessControl;
using System.Security.Principal;

namespace DAL.FileSystem
{
	public class HistolungFS : IHistolungFS
	{
		// A T T R I B U T E S
		private string backendWorkDirPath; // Path to the working directory ("/home/user/appBackend/")
		private string wsiFolder = "/home/user/appBackend/data/tcga/wsi";
		private string outputFolder = "/home/user/appBackend/data/outputs";
		private string envFilePath = "/home/user/appBackend/.env";
		private string projectDirectory = "/home/user/appBackend/histo_lung";
		private int delayHistolungService					= 90000; // 90 seconds
		private int delayCounterLoopOutputFolder	= 11; // 10 seconds each loop

		// C O N S T R U C T O R
		public HistolungFS(string basePath)
		{
			backendWorkDirPath = basePath;
		}


		// M E T H O D S
		// Update the .env file with the new values of the image to analyse
		// and then re-run the Histolung service
		// Takes 90 seconds to run the Histolung service and 110 seconds to wait for the output files
		public HistolungResponse AnalyzeImage(HistolungRequest request)
		{
			Console.WriteLine("\nMETHOD AnalyzeImage - Analyzing the image");
			Console.WriteLine("Image Name to analyze : " + request.ImageName);

			// 1) Initialize the response
			HistolungResponse response = new HistolungResponse();
			response.Prediction = "Failure. ";
			response.Heatmap = "";

			// 2) Verify the paths and folders
			try
			{
				// Check if the wsi and output folders exist
				if (!Directory.Exists(wsiFolder))
				{
					//Directory.CreateDirectory(wsiFolder);
					string error = "WSI folder does not exist.\n";
					response.Prediction += error;
					throw new Exception(error);
				}
				if (!Directory.Exists(outputFolder))
				{
					string error = "Output folder does not exist.\n";
					response.Prediction += error;
					throw new Exception(error);
				}
				// Check if the .env file exists in the Histolung folder
				if (!File.Exists(envFilePath))
				{
					string error = ".env file does not exist.\n";
					response.Prediction += error;
					throw new Exception(error);
				}

				// Log permissions and file availability
				FileInfo fileInfo = new FileInfo(envFilePath);
				Console.WriteLine($"File .env exists: {fileInfo.Exists}");
				Console.WriteLine($"File .env is read-only: {fileInfo.IsReadOnly}");
				Console.WriteLine($"File .env length: {fileInfo.Length}");
				Console.WriteLine($"File .env directory: {fileInfo.DirectoryName}");

				using (FileStream fs = fileInfo.Open(FileMode.Open, FileAccess.ReadWrite))
				{
					Console.WriteLine($"File can be opened for read/write: {fs.CanWrite}");
				}

			}
			catch (Exception e)
			{
				string message = "Error verifying the paths and folders in the /appBackend/ folder and subfolders." +
											 "Be sure to run de code in on the server to match the docker-compose volume share and binds.";
				Console.WriteLine(message + e);
				response.Prediction += message;
				return response;
			}


			// 3) Update the .env file with the new file name
			// WARNING ! The .env file ressources must be free and not opened by another process
			try
			{
				Console.WriteLine("Updating the .env file with the new image name");
				string envContent = $"WSI_NAME={request.ImageName}\nSIGMA=8\nTAG=v1.0\nGPU_DEVICE_IDS=0";
				File.WriteAllText(envFilePath, envContent); // Écriture du contenu dans le fichier .env
															// Write the content to the .env file, overwriting the previous content if it exists
				Console.WriteLine("What SHOULD BE written in the .env file :\n" + envContent + "\n");

				// Read back the content to verify
				string verifyContent = File.ReadAllText(envFilePath);
				Console.WriteLine("\nACTUAL Updated .env file content :\n" + verifyContent + "\n");
			}
			catch (Exception e)
			{
				string message = "Error updating the .env file";
				Console.WriteLine(message + e);
				response.Prediction += message;
				return response;
			}

			// 4) Clean the output folder before running the Histolung service
			try
			{
				// Verify if my access rights are correct to delete files in the output folder
				if (!HasDirectoryAccessRights(outputFolder, "w"))
				{
					string message = "Error verifying the access rights to the output folder.";
					Console.WriteLine(message);
					response.Prediction += message;
					return response;
				}


				// Delete all the files in the output folder
				Console.WriteLine("Cleaning the output folder");
				DirectoryInfo di = new DirectoryInfo(outputFolder);
				FileInfo[] files = di.GetFiles();
				DirectoryInfo[] directories = di.GetDirectories();

				Console.WriteLine("Number of files in the output folder: " + files.Length);
				Console.WriteLine("Number of directories in the output folder: " + directories.Length);

				// Delete all the files in the output folder
				foreach (FileInfo file in files)
				{
					try
					{
						Console.WriteLine($"Deleting file: {file.Name}");
						file.Delete();
					}
					catch (Exception fileEx)
					{
						Console.WriteLine($"Error deleting file {file.Name}: {fileEx.Message}");
						throw;
					}
				}

				// Delete all the directories in the output folder
				foreach (DirectoryInfo dir in directories)
				{
					try
					{
						Console.WriteLine($"Deleting directory: {dir.Name}");
						dir.Delete(true);
					}
					catch (Exception dirEx)
					{
						Console.WriteLine($"Error deleting directory {dir.Name}: {dirEx.Message}");
						throw;
					}
				}
			}
			catch (Exception e)
			{
				string message = "Error cleaning the output folder";
				Console.WriteLine(message + e);
				response.Prediction += message;
				return response;
			}



			// 5) Run the Histolung service with docker compose up
			try
			{
				// Restart the Histolung service with the Docker.DotNet API and wait for it to finish before continuing
				Task task = DockerComposeUpDownAsync(projectDirectory,  delayHistolungService);


				task.Wait(); // Wait for the task to complete before continuing
				if (task.IsCompleted)
				{
					// Means that the task completed, faulted, or was canceled
					Console.WriteLine("DockerComposeUpDownAsync completed");
				}
				else if (task.IsCompletedSuccessfully)
				{
					// Means that the task completed successfully without throwing an exception
					Console.WriteLine("DockerComposeUpDownAsync completed successfully");
				}
				else if (task.IsCanceled)
				{
					// Means that the task was canceled before it completed
					Console.WriteLine("DockerComposeUpDownAsync canceled");
				}
				else if (task.IsFaulted)
				{
					// Means that an exception was thrown during the execution of the task
					Console.WriteLine("DockerComposeUpDownAsync faulted");
					if (task.Exception != null)
					{
						Console.WriteLine("Exception: " + task.Exception.GetBaseException().Message);
					}
				}
			}
			catch (Exception e)
			{
				string message = "Error running the Histolung service";
				Console.WriteLine(message + e);
				response.Prediction += message;
				return response;
			}


			// 6)  Recover the prediction and heatmap from the output folder
			try
			{
				// recover image name without extension
				string imageName = request.ImageName.Split('.')[0];
				string heatmapPath = $"{outputFolder}/heatmap_{imageName}.png";
				string predictionPath = $"{outputFolder}/predictions.csv";

				Console.WriteLine("Image Name: " + imageName);
				Console.WriteLine("Full path for heatmap: " + heatmapPath);

				// Verify the filenames of output folder. If there are no files in the folder, wait a bit and try again. 
				Console.WriteLine("Files in the output folder:");
				int counter = 0;
				if (Directory.GetFiles(outputFolder).Length == 0 ||
					counter < delayCounterLoopOutputFolder)
				{
					Console.WriteLine("No files found in the output folder. Waiting 10 seconds and trying again.");
					Task.Delay(10000).Wait(); // Wait 10 seconds
					counter++;
				}
				else
				{
					Console.WriteLine("No files found in the output folder after 11 tries. Exiting.");
					response.Prediction += "No files found in the output folder after 11 tries.";
					throw new Exception("No files found in the output folder after 11 tries.");
				}
				foreach (string file in Directory.GetFiles(outputFolder))
				{
					Console.WriteLine(file);
				}

				// Verify if the prediction file exists
				if (!File.Exists(predictionPath))
				{
					response.Prediction += "Prediction file not found at path : " + predictionPath;
					throw new FileNotFoundException("Prediction file not found.", predictionPath);
				}

				// Verify if the heatmap file exists
				if (!File.Exists(heatmapPath))
				{
					response.Prediction += "Heatmap file not found at path : " + heatmapPath;
					throw new FileNotFoundException("Heatmap file not found.", heatmapPath);
				}

				// Read the prediction and heatmap files
				response.Prediction = File.ReadAllText(predictionPath);
				response.Heatmap = Convert.ToBase64String(File.ReadAllBytes(heatmapPath));
				//response.Heatmap = File.ReadAllBytes(heatmapPath);


				Console.WriteLine("Prediction and heatmap successfully recovered.");
			}
			catch (Exception e)
			{
				string message = "Error recovering the prediction and heatmap.\n";
				Console.WriteLine(message + e);
				response.Prediction += message;
				return response;
			}

			// 7) Return the response, wethever it is a success or a failure
			return response;
		}

		// Get the heatmap image from the output folder and return it as a byte array
		public byte[] GetHeatmap()
		{
			Console.WriteLine("\nMETHOD GetHeatmap - Getting the heatmap image");
			try
			{
				// recover image name with the extension name ".png" 
				// Scan the folder for the heatmap image and return the name of it
				string imageName = Directory.GetFiles(outputFolder, "heatmap_*.png").FirstOrDefault();
				Console.WriteLine("Image Name : " + imageName);

				// Check if a file was found
				if (imageName == null)
				{
					string message = "No heatmap file found in the output folder.";
					Console.WriteLine(message);
					throw new FileNotFoundException(message);
				}

				// Read the heatmap image as a byte array and return it
				//byte[] heatmap = File.ReadAllBytes($"{outputFolder}/{imageName}");
				byte[] heatmap = File.ReadAllBytes(imageName);

				return heatmap;
			}
			catch (Exception e)
			{
				string message = "Error recovering the prediction and heatmap";
				Console.WriteLine(message + e);
				return Array.Empty<byte>();
			}
		}

		// Get the last analysis result from the output folder and return it as a HistolungResponse object
		public HistolungResponse GetLastAnalysis()
		{
			Console.WriteLine("\nMETHOD GetLastAnalysis - Getting the last analysis done in the output folder");

			// 1) Initialize the response
			HistolungResponse response = new HistolungResponse();
			response.Prediction = "Failure. ";
			response.Heatmap = "";

			try
			{
				// 1) Recover heatmap in the output folder
				// recover image name with the extension name ".png" 
				// Scan the folder for the heatmap image and return the name of it
				string imageName = Directory.GetFiles(outputFolder, "heatmap_*.png").FirstOrDefault();
				Console.WriteLine("Image Name : " + imageName);

				// Check if a file was found
				if (imageName == null)
				{
					string message = "No heatmap file found in the output folder.";
					Console.WriteLine(message);
					throw new FileNotFoundException(message);
				}

				// Read the heatmap image as a byte array and return it
				//byte[] heatmap = File.ReadAllBytes($"{outputFolder}/{imageName}");
				byte[] heatmap = File.ReadAllBytes(imageName);
				// Convert byte array to base64 string
				string heatmapBase64 = Convert.ToBase64String(heatmap);
				response.Heatmap = heatmapBase64;

				// 2) Recover prediction in the output folder
			    string predictionPath = Directory.GetFiles(outputFolder, "predictions.csv").FirstOrDefault();
				Console.WriteLine("Prediction Path : " + predictionPath);

				// Check if a file was found
				if (predictionPath == null)
				{
					string message = "No prediction file found in the output folder.";
					Console.WriteLine(message);
					throw new FileNotFoundException(message);
				}

				// Read the prediction file 
				response.Prediction = File.ReadAllText(predictionPath);

				Console.WriteLine("Heatmap and prediction successfully recovered.");
				return response;

			}
			catch (Exception e)
			{
				string message = "Error recovering the prediction and heatmap";
				response.Prediction += message;
				Console.WriteLine(message + e);
				return response;
			}
		}

		// Get the list of images in the /wsi folder to analyze and return it as a string array
		public string[] GetImagesList()
		{
			Console.WriteLine("\nMETHOD GetImagesList - Getting the list of images to analyze in the input folder");
			// 1) Scan the WSI folder
			string[] imageList = Directory.GetFiles(wsiFolder);
			Console.WriteLine("Number of images in the WSI folder: " + imageList.Length);

			// 2) Only return the file names without the full path
			for (int i = 0; i < imageList.Length; i++)
			{
				Console.WriteLine("Image full path: " + imageList[i]);
				imageList[i] = Path.GetFileName(imageList[i]); // Keep only the file name
				Console.WriteLine("Image name: " + imageList[i]);
			}

			// 3) sort the list of images alphabetically
			Array.Sort(imageList);

			return imageList;
		}

		// Run the Histolung service with Docker.DotNet API that allows to interact with the Docker daemon outside of the container
		private async Task DockerComposeUpDownAsync(string projectDirectory, int msDelay)
		{
			Console.WriteLine("\nMETHOD DockerComposeUpDownAsync - Starting and stopping Histolung service with docker compose up and down");

			Console.WriteLine($"Project directory to execute docker compose commands: {projectDirectory}");
			Console.WriteLine($"Delay in milliseconds before stopping the service: {msDelay}");

			// Docker compose down
			//string downOutput1 = await RunCommand("docker", "compose down", projectDirectory);
			//Console.WriteLine($"Docker compose down output: {downOutput1}");

			// Docker compose up
			string upOutput = await RunCommand("docker", "compose up -d", projectDirectory);
			Console.WriteLine($"Docker compose up output: {upOutput}");

			// Wait for the container to finish its task
			await Task.Delay(msDelay);

			// Docker compose down
			//string downOutput2 = await RunCommand("docker", "compose down", projectDirectory);
			//Console.WriteLine($"Docker compose down output: {downOutput2}");
		}


		// Check if the current user has the specified access rights to the specified directory path (unix / linux)
		private static bool HasDirectoryAccessRights(string folderPath, string permission)
		{
			Console.WriteLine($"\nMETHOD HasDirectoryAccessRights - Checking access rights to the output folder {folderPath} with the rights {permission}");
			try
			{
				// Construct the command to check directory permissions
				var command = $"test -{permission} {folderPath} && echo 'yes' || echo 'no'";

				// Create a process to run the command
				var process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "sh",
						Arguments = $"-c \"{command}\"",
						RedirectStandardOutput = true,
						UseShellExecute = false,
						CreateNoWindow = true
					}
				};

				// Start the process and read the output
				process.Start();
				string result = process.StandardOutput.ReadToEnd().Trim();
				process.WaitForExit();

				// Return true if the command output is 'yes', otherwise false
				Console.WriteLine($"Access rights to the output folder {folderPath} with the rights {permission} : {result}");
				return result == "yes";
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error checking access rights: {ex.Message}");
				return false;
			}
		}


		// Run a command in the specified working directory and return the output
		private async Task<string> RunCommand(string fileName, string arguments, string workingDirectory)
		{
			Console.WriteLine($"\nMETHOD RunCommand - Running command: {fileName} {arguments} in {workingDirectory}");
			var processStartInfo = new ProcessStartInfo
			{
				FileName = fileName,
				Arguments = arguments,
				WorkingDirectory = workingDirectory,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			using (var process = new Process { StartInfo = processStartInfo })
			{
				Console.WriteLine($"Running command: {fileName} {arguments} in {workingDirectory}");
				process.Start();

				string output = await process.StandardOutput.ReadToEndAsync();
				string error = await process.StandardError.ReadToEndAsync();
				process.WaitForExit();

				Console.Write("Command output: " + output);
				if (!string.IsNullOrEmpty(error))
				{
					Console.WriteLine($"\nError running command: {error}");
				}
				
				return output;
			}
		}

		
	}

}
