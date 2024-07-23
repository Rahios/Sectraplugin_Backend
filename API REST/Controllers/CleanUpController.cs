using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;

namespace API_REST.Controllers
{
	[Route("api/[controller]")] //Route is (api/CleanUp)
	[ApiController]
	public class CleanUpController : ControllerBase
	{
		// *** D E P E N D E N C I E S   I N J E C T I O N * * *
		private readonly ICleanUpManager _cleanUpManager;

		// Constructor to inject the manager. Parameter is an interface to respect the dependency inversion principle
		public CleanUpController(ICleanUpManager cleanUpManager)
		{
			_cleanUpManager = cleanUpManager;
		}

		// * * * M E T H O D S * * *
		[HttpDelete("CleanUp")]
		public IActionResult CleanUp()
		{
			bool isCleaned = _cleanUpManager.CleanUp();

			if (isCleaned)
				return Ok("Cleaned up.");
			else
				return BadRequest("Not cleaned up.");
		}
		
	}
}
