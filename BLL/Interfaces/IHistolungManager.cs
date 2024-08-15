using DTO;

namespace BLL.Interfaces
{
	/// <summary>
	/// Interface defining the methods for managing histological image analysis and retrieving associated data.
	/// </summary>
	public interface IHistolungManager
	{

		/// <summary>
		/// Analyzes a histological image based on the parameters specified in the request.
		/// </summary>
		/// <param name="request">The request containing the necessary information for image analysis.</param>
		/// <returns>A <see cref="HistolungResponse"/> object containing the results of the analysis.</returns>
		HistolungResponse AnalyseImage(global::DTO.HistolungRequest request);

		/// <summary>
		/// Retrieves the heatmap of the last image analysis.
		/// </summary>
		/// <returns>A byte array representing the heatmap image.</returns>
		byte[] GetHeatmap();

		/// <summary>
		/// Retrieves the list of available image names for analysis.
		/// </summary>
		/// <returns>An array of strings containing the names of available images.</returns>
		string[] GetImagesList();

		/// <summary>
		/// Retrieves the results of the last analysis performed.
		/// </summary>
		/// <returns>A <see cref="HistolungResponse"/> object containing the results of the last analysis.</returns>
		HistolungResponse GetLastAnalysis();
	}
}