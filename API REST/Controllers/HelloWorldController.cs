using BLL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_REST.Controllers
{
    [Route("api/[controller]")]	// Defines the route to access the controller (api/HelloWorld)
	[ApiController] 
	public class HelloWorldController : ControllerBase
	{
		// * * * D E P E N D E N C I E S   I N J E C T I O N * * *
		private readonly IHelloWorldManager _helloWorldManager;

		// Constructor to inject the manager. Parameter is an interface to respect the dependency inversion principle
		public HelloWorldController(IHelloWorldManager helloWorldManager)
		{
			_helloWorldManager = helloWorldManager;
		}

		// * * * M E T H O D S * * *
		
		// POST: api/HelloWorld
		[HttpPost]
		public IActionResult SendHelloWorld()
		{
			bool isSaved = _helloWorldManager.SendHelloWorld();

			if (isSaved)
				return Ok("Hello World sent.");
			else
				return BadRequest("Hello World not sent.");
		}

		// GET: api/HelloWorld
		[HttpGet]
		public IActionResult GetHelloWorld()
		{
			string returnValue = _helloWorldManager.GetHelloWorld();
			return Ok(returnValue);
		}

		// POST: api/HelloWorld/SaveImage
		[HttpPost("SaveImage")]
		public IActionResult SaveImage(IHelloWorldManager _helloWorldManager)
		{
			bool isSaved = _helloWorldManager.GeneratAndSaveImage();
			
			if (isSaved)
				return Ok("Image saved.");
			else
				return BadRequest("Image not saved.");
		}

		// GET: api/HelloWorld/GetImage
		[HttpGet("GetImage")]
		public IActionResult GetImage()
		{
			byte[] imageToReturn = _helloWorldManager.GetImage();
			return File(imageToReturn, "image/png", "image.png");
		}


	}
}
