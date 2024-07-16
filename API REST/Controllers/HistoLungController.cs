using Microsoft.AspNetCore.Mvc;
using BLL;
using DTO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API_REST.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class HistoLungController : ControllerBase
	{
		// Injection of dependencies in the constructor to use the manager
		private readonly IHistoLungManager _histoLungManager;

		public HistoLungController(IHistoLungManager histoLungManager)
		{
			_histoLungManager = histoLungManager;
		}


		// GET: api/<HistoLungController>
		[HttpGet]
		public ActionResult<IEnumerable<string>> GetImages()
		{
			IEnumerable<string> images = _histoLungManager.ListImageFiles(); 
			return Ok(images);
		}

		// POST: api/<HistoLungController>
		[HttpPost]
		public ActionResult PostImage([FromBody] HistoImage imageHisto)
		{
			if (string.IsNullOrEmpty(imageHisto.FileName))
				return BadRequest("File name is required.");

			var result = _histoLungManager.ProcessImage(imageHisto.FileName);
			if (!result)
				return NotFound("Image not found.");

			return Ok("Image processing started.");
		}

		// GET api/<HistoLungController>/5
		[HttpGet("{id}")]
		public string Get(int id)
		{
			return "value";
		}

		

		// PUT api/<HistoLungController>/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody] string value)
		{
		}

		// DELETE api/<HistoLungController>/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
	}
}
