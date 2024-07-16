using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
	public class HistoLungFS : IHistoLungFS
	{
		private readonly string _pathDirectory;

		// C O N S T R U C T O R S
		public HistoLungFS(string pathDirectory)
		{
			_pathDirectory = pathDirectory;
		}

		// M E T H O D S
		// Get all images in the directory specified in the constructor
		public IEnumerable<string> GetImages()
		{
			return Directory.EnumerateFiles(_pathDirectory, "*.tif")
							.Select(Path.GetFileName);
		}

		// Check if the image exists in the directory specified in the constructor
		public bool ImageExists(string fileName)
		{
			return File.Exists(Path.Combine(_pathDirectory, fileName));
		}

		// Send a command to the AI to analyze the image
		public void CommandAIToAnalyze(string fileName)
		{

		}



	}
}
