using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Interfaces;



namespace DAL.FileSystem
{

    public class HelloWorldFS : IHelloWorldFS
	{
		// * * * A T T R I B U T E S * * *
		private string PATH;
		private string TxtFilePath   => Path.Combine( PATH, "HelloWorld.txt");
		private string ImgFilePath => Path.Combine( PATH, "RedBlue.png");

		// * * * C O N S T R U C T O R * * *
		// Get the path of the file system, stored in the Program.cs file
		public HelloWorldFS(string basePath)
		{
			PATH = basePath;
		}

		// * * * M E T H O D S * * *
		public bool SaveToTextFile(string message)
		{
			try
			{
				// Check if the directory exists, if not, create it
				if (!Directory.Exists(PATH))
				{
					Directory.CreateDirectory(PATH);
				}
				File.WriteAllText(TxtFilePath, message);
				return true;
			} catch (Exception e){
				Console.WriteLine("Error saving the file" + e);
				return false;
			}
		}

		public string ReadtextFile()
		{
			try
			{
				// Check if the file exists
				if (!File.Exists( TxtFilePath))
				{
					return "ERROR: File not found";
				}

				return File.ReadAllText(TxtFilePath);
			}
			catch (Exception e)
			{
				Console.WriteLine("Error reading the file" + e);
				return "ERROR: File not found";
			}
		}
		
		public bool SaveToImage(byte[] image)
		{
			try
			{
				// Check if the directory exists, if not, create it
				Console.WriteLine("Saving the image on the FileSystem");
				Console.WriteLine("verify the existence of the PATH: " + PATH);
				if (!Directory.Exists(PATH))
				{
					Directory.CreateDirectory(PATH);
				}

				// Save the image to the file system in PNG format
				Console.WriteLine("Saving the image on the FileSystem");
				File.WriteAllBytes(ImgFilePath, image );

				return true;
			} 
			catch (Exception e)
			{
				Console.WriteLine("Error saving the image" + e);
				return false;
			}

		

	}

		public byte[] ReadImage()
		{
			try
			{
				if (!File.Exists(ImgFilePath))
				{
					// si je veux retourner un message d'erreur
					throw new Exception("ERROR: File not found");
				}

				// Read the image from the file system
				return File.ReadAllBytes( ImgFilePath);
			}
			catch (Exception e)
			{
				Console.WriteLine("Error reading the image" + e);
				return Array.Empty<byte>();
			}
		}

	 }
}
