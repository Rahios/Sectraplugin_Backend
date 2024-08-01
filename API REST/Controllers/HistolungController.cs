using BLL.Interfaces;
using DTO;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_REST.Controllers
{
	/// <inheritdoc/>
	[Route("api/[controller]")]
	[ApiController]
	[EnableCors("AllowSpecificOrigin")]         // Enable CORS (Cross-Origin Resource Sharing) for the controller (for all the methods in the controller) 
	public class HistolungController : ControllerBase, IHistolungController
	{
		// A T T R I B U T E S
		private readonly IHistolungManager _histolungManager;

		// C O N S T R U C T O R
		/// <summary>
		/// Constructor of the Histolung controller to inject the Histolung manager
		/// </summary>
		/// <param name="histolungManager"></param>
		public HistolungController(IHistolungManager histolungManager)
		{
			_histolungManager = histolungManager;
		}

		// M E T H O D S
		// TODO
		// 1) Methode pour donner un nom depuis swagger, cela va jusqu'à l'IA et commande l'IA de lancer l'analyse pour X image. Ensuite on récupère le résultat dans le output folder.
		// Avant chaque création de output, il faut clean le folder.
		/// <inheritdoc/>
		[HttpPost("AnalyseImage")]
		public IActionResult AnalyseImage([FromBody] HistolungRequest request)
		{
			// FromBody : Allows you to bind the request body to a parameter in a controller action.
			// Example : [FromBody] string name -> The name parameter is bound to the request body.
			// To bind to the request body allows you to send complex types to the API (like objects).

			// 1) Call the manager to analyse the image
			HistolungResponse response = _histolungManager.AnalyseImage(request);

			// 2) Verify if the result of the analysis succeeded
			if (response == null ||
				response.Prediction.Contains("Failure"))
			{
				return BadRequest("Analysis failed.\n" + response.Prediction);
			}
			else
			{
				return Ok(response);
				// Return a complex type (object) as a response to the client. 
				// The object is serialized into JSON format before sending it to the client. 
			}
		}

		// 2) Methode pour récupérer l'image heatmap de l'analyse
		/// <inheritdoc/>
		[HttpGet("GetHeatmap")]
		public IActionResult GetHeatmap()
		{
			// 1) Call the manager to get the heatmap
			byte[] heatmap = _histolungManager.GetHeatmap();

			// 2) Verify if the heatmap was found
			if (heatmap == null)
			{
				return BadRequest("Heatmap not found.");
			}
			else
			{
				// Return the heatmap as a file to the client (image/png format)
				return File(heatmap, "image/png", "image.png");
			}
		}

		// 3) Methode pour récuperer le dernier résultat d'analyse dans le dossier output
		/// <inheritdoc/>
		[HttpGet("GetLatestAnalysis")]
		public IActionResult GetLastAnalysis()
		{
			// 1) Call the manager to analyse the image
			HistolungResponse response = _histolungManager.GetLastAnalysis();

			// 2) Verify if the result of the analysis succeeded
			if (response == null ||
				response.Prediction.Contains("Failure"))
			{
				return BadRequest("Analysis failed.\n" + response.Prediction);
			}
			else
			{
				return Ok(response);
				// Return a complex type (object) as a response to the client. 
				// The object is serialized into JSON format before sending it to the client. 
			}
		}

		// 4) Methode pour récuperer la liste des images disponibles à l'analyse
		/// <inheritdoc/>
		[HttpGet("GetImagesList")]
		public IActionResult GetImagesList()
		{
			// 1) Call the manager to get the list of images
			string[] imagesList = _histolungManager.GetImagesList();

			// 2) Verify if the list of images was found
			if (imagesList == null)
			{
				return BadRequest("Images list not found.");
			}
			else
			{
				return Ok(imagesList);
				// Return a complex type (object) as a response to the client. 
				// The object is serialized into JSON format before sending it to the client. 
			}
		}

	}
}
