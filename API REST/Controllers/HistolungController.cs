using BLL.Interfaces;
using DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_REST.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class HistolungController : ControllerBase
	{
		// A T T R I B U T E S
		private readonly IHistolungManager _histolungManager;

		// C O N S T R U C T O R
		public HistolungController(IHistolungManager histolungManager)
		{
			_histolungManager = histolungManager;
		}

		// M E T H O D S
		// TODO
		// 1) Methode pour donner un nom depuis swagger, cela va jusqu'à l'IA et commande l'IA de lancer l'analyse pour X image. Ensuite on récupère le résultat dans le output folder.
		// Avant chaque création de output, il faut clean le folder.

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

	}
}
