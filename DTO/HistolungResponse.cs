using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
	/// <summary>
	/// Response object for the Histolung analysis operation
	/// </summary>
	public class HistolungResponse 
	{
		// For the ANALYSIS RESPONSE
		/// <summary>
		/// The cancer prediction of the analysis 
		/// </summary>
		public string Prediction { get; set; }

		/// <summary>
		/// The heatmap of the analysis as a base64 string 
		/// </summary>
		public string Heatmap { get; set; }
	}
}
