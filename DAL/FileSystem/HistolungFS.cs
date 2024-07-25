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

namespace DAL.FileSystem
{
	public class HistolungFS : IHistolungFS
	{
		// A T T R I B U T E S
		private string backendWorkDirPath; // Path to the working directory ("/home/user/appBackend/")
		private string wsiFolder = "/home/user/appBackend/data/tcga/wsi";
		private string outputFolder = "/home/user/appBackend/data/outputs";
		//private string histolungWorkDirPath = "/home/user/appHistolung"; // NE sert a rien, car on n'y a pas accès
		private string envFilePath = "/home/user/appBackend/.env";

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
			Console.WriteLine("METHOD AnalyzeImage - Analyzing the image");
			// 1) Initialize the response
			HistolungResponse response = new HistolungResponse();
			response.Prediction = "Failure. ";
			response.Heatmap = Array.Empty<byte>();

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
				if(!Directory.Exists(outputFolder))
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
				// Write the content to the .env file, overwriting the previous content if it exists

				// Read back the content to verify
				string verifyContent = File.ReadAllText(envFilePath);
				Console.WriteLine("Updated .env file content :\n" + verifyContent +"\n");
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
				DockerRestartAsync("hlung").Wait();

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
				string imageName		= request.ImageName.Split('.')[0];
				string heatmapPath		= $"{outputFolder}/heatmap_{imageName}.png";
				string predictionPath	= $"{outputFolder}/predictions.csv";

				Console.WriteLine("Image Name: " + imageName);
				Console.WriteLine("Full path for heatmap: " + heatmapPath);

				// Verify if the prediction file exists
				if (!File.Exists(predictionPath))
				{
					throw new FileNotFoundException("Prediction file not found.", predictionPath);
				}

				// Verify if the heatmap file exists
				if (!File.Exists(heatmapPath))
				{
					throw new FileNotFoundException("Heatmap file not found.", heatmapPath);
				}

				// Read the prediction and heatmap files
				response.Prediction = File.ReadAllText(predictionPath);
				response.Heatmap = File.ReadAllBytes(heatmapPath);

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
			Console.WriteLine("METHOD GetHeatmap - Getting the heatmap image");
			try
			{
				// recover image name with the extension name ".png" 
				// Scan the folder for the heatmap image and return the name of it
				string imageName = Directory.GetFiles(outputFolder, "heatmap_*.png").FirstOrDefault();
				Console.WriteLine("Image Name : "+imageName);

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

		// Run the Histolung service with Docker.DotNet API that allows to interact with the Docker daemon outside of the container
		private async Task DockerRestartAsync(string containerName)
		{
			Console.WriteLine("METHOD DockerRestartAsync - Restarting Histolung service");

				// Create a Docker client to interact with the Docker daemon on the host machine
				using (var client = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient())
				{
					Console.WriteLine("Docker client created");

					// List all the containers on the host machine and find the Histolung container by its name
					var containers = await client.Containers.ListContainersAsync(new ContainersListParameters() { All = true });
					var container = containers.FirstOrDefault(c => c.Names.Any(n => n.EndsWith(containerName)));

					Console.WriteLine("Histolung container found");

					// Restart the Histolung container if it exists on the host machine
					if (container != null)
					{
						Console.WriteLine("Restarting Histolung service");
						var restartParameters = new ContainerRestartParameters(); 
						await client.Containers.RestartContainerAsync(container.ID, restartParameters);

						Console.WriteLine("Histolung service restarted");
						Console.WriteLine("Waiting for the container to complete its task");

						// Wait for the container to complete its task
						await Task.Delay(50000); // Adjust delay as needed, those are milliseconds (50 seconds)

						Console.WriteLine("Container finished its task");
				}
					else
					{
						throw new Exception("Histolung container not found");
					}
				}
			}
			
		}

}
