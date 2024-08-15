using DTO;

namespace DAL.Interfaces
{
	/// <summary>
	/// Interface defining methods for interacting with the file system to perform histological image analysis and retrieve related data.
	/// </summary>
	public interface IHistolungFS
	{
		/// <summary>
		/// Analyzes a histological image based on the provided request.
		/// </summary>
		/// <param name="request">The request containing details of the image to be analyzed.</param>
		/// <returns>A <see cref="HistolungResponse"/> containing the analysis results, including predictions and heatmap data.</returns>
		HistolungResponse AnalyzeImage(HistolungRequest request);

		/// <summary>
		/// Retrieves the heatmap image from the last analysis as a byte array.
		/// </summary>
		/// <returns>A byte array representing the heatmap image.</returns>
		byte[] GetHeatmap();

		/// <summary>
		/// Retrieves the list of available images to be analyzed from the file system.
		/// </summary>
		/// <returns>An array of strings containing the names of available images.</returns>
		string[] GetImagesList();

		/// <summary>
		/// Retrieves the results of the last analysis performed.
		/// </summary>
		/// <returns>A <see cref="HistolungResponse"/> containing the results of the last analysis.</returns>
		HistolungResponse GetLastAnalysis();
	}
}