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
				Console.WriteLine($"File exists: {fileInfo.Exists}");
				Console.WriteLine($"File is read-only: {fileInfo.IsReadOnly}");
				Console.WriteLine($"File length: {fileInfo.Length}");
				Console.WriteLine($"File directory: {fileInfo.DirectoryName}");

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
			try
			{
				string envContent = $"WSI_NAME={request.ImageName}\nSIGMA=8\nTAG=v1.0\nGPU_DEVICE_IDS=0";
				// Write the content to the .env file, overwriting the previous content if it exists
				File.WriteAllText(envFilePath, envContent);
			}
			catch (Exception e)
			{
				string message = "Error updating the .env file";
				Console.WriteLine(message + e);
				response.Prediction += message;
				return response;
			}

			// TODO : Clean the output folder before running the Histolung service

			// 4) Run the Histolung service with docker compose up

			/*try
			{
				// Prepare the command to run
				ProcessStartInfo processStartInfo = new ProcessStartInfo
				{
					FileName = "docker",                           // Command to run
					Arguments = "compose run hlung",    // Arguments to pass to the command 
					RedirectStandardOutput = true,          // Redirect the output to the console window
					RedirectStandardError = true,              // Redirect the error to the console window
					UseShellExecute = false,                       // Do not use the shell to execute the command
					CreateNoWindow = true                      // Do not create a window for the command
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
			}*/
			try
			{
				// Restart the Histolung service with the Docker.DotNet API and wait for it to finish before continuing
				DockerRestartAsync("hlung").Wait();

				//TODO : Wait que le output sorte après le restart ? 
			}
			catch (Exception e)
			{
				string message = "Error running the Histolung service";
				Console.WriteLine(message + e);
				response.Prediction += message;
				return response;
			}


			//5)  Recover the prediction and heatmap from the output folder
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

			// 6) Return the response, wethever it is a success or a failure
			return response;
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
						await Task.Delay(15000); // Adjust delay as needed, those are milliseconds (15 seconds)

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
