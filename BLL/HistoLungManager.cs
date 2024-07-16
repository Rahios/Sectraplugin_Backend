using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;

namespace BLL
{
	public class HistoLungManager : IHistoLungManager
	{
		private IHistoLungFS HistoLungFS { get; }

		// C O N S T R U C T O R S
		public HistoLungManager(IHistoLungFS histoLungFS)
		{
			HistoLungFS = histoLungFS;
		}

		// M E T H O D S
		// List all images in the directory
		public IEnumerable<string> ListImageFiles()
		{
			return HistoLungFS.GetImages();
		}

		// Execute the AI to analyze the image
		public bool ProcessImage(string fileName)
		{
			if (!HistoLungFS.ImageExists(fileName))
				return false;

			HistoLungFS.CommandAIToAnalyze(fileName);
			return true;
		}

	}
}
