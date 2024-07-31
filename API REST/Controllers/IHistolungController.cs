using DTO;
using Microsoft.AspNetCore.Mvc;

namespace API_REST.Controllers
{
	// INTERFACE - ONLY FOR DOCUMENTATION PURPOSES (SWAGGER) - NOT USED IN THE CODE

	/// <summary>
	/// Interface for the Histolung controller operations API
	/// </summary>
	public interface IHistolungController
	{
		/// <summary>
		/// Starts the analysis of the image and returns the prediction and the heatmap of the analysis.
		/// Can take a while to process. Around 1 min 30 sec minimum to 3 min 30 sec maximum.
		/// </summary>
		/// <param name="request">The request containing the image name as a ".tif" file to analyze</param>
		/// <returns>The analysis of the result containing the prediction and the heatmap of the analysis</returns>
		IActionResult AnalyseImage([FromBody] HistolungRequest request);

		/// <summary>
		/// Gets the heatmap of the analysis as a .png image
		/// </summary>
		/// <returns>The last image of any alaysis</returns>
		IActionResult GetHeatmap();
	}
}