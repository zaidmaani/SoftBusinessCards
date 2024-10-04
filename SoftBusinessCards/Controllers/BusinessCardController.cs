using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SoftBusinessCards.Models;
using SoftBusinessCards.Data;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SoftBusinessCards.Data;
using SoftBusinessCards.Models;
using Microsoft.EntityFrameworkCore;
using System.Formats.Asn1;
using System.Globalization;
using System.Xml.Linq;
using CsvHelper;

namespace BusinessCardAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessCardController : ControllerBase
    {
        private readonly BusinessCardContext _context;
        private readonly ILogger<BusinessCardController> _logger;

        public BusinessCardController(BusinessCardContext context, ILogger<BusinessCardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Method to create a new business card 
        [HttpPost("create")]
        public async Task<IActionResult> CreateBusinessCard([FromBody] BusinessCard card)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.BusinessCards.Add(card);
                    await _context.SaveChangesAsync();
                    return CreatedAtAction(nameof(ViewBusinessCards), new { id = card.Id }, card);
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating business card.");
                return StatusCode(500, "Internal server error");
            }
        }
        //============================================================================//
        // Method to view all business cards
        [HttpGet("view")]
        public async Task<IActionResult> ViewBusinessCards()
        {
            try
            {
                var cards = await _context.BusinessCards.ToListAsync();
                return Ok(cards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching business cards.");
                return StatusCode(500, "Internal server error");
            }
        }
        //============================================================================//
        // Method to delete a business card
        [HttpDelete("DeleteBusinessCards")]
        public async Task<IActionResult> DeleteBusinessCard(int id)
        {
            try
            {
                var card = await _context.BusinessCards.FindAsync(id);
                if (card == null) return NotFound();

                _context.BusinessCards.Remove(card);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting the business card.");
                return StatusCode(500, "Internal server error");
            }
        }
        //============================================================================//
        // Method to create business card via file import (XML or CSV)
        [HttpPost("createFromFile")]
        public async Task<IActionResult> CreateBusinessCardFromFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("File is empty.");

                string extension = Path.GetExtension(file.FileName);
                if (extension != ".xml" && extension != ".csv")
                    return BadRequest("Unsupported file format.");

                List<BusinessCard> businessCards = new List<BusinessCard>();

                if (extension == ".csv")
                {
                    using (var reader = new StreamReader(file.OpenReadStream()))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csv.Context.TypeConverterOptionsCache.GetOptions<DateTime>()
                   .Formats = new[] { "yyyy-MM-ddTHH:mm:ss", "MM/dd/yyyy", "M/d/yyyy" };

                        var records = csv.GetRecords<BusinessCard>();
                        businessCards.AddRange(records);
                    }
                }
                else if (extension == ".xml")
                {
                    XDocument doc = XDocument.Load(file.OpenReadStream());
                    var records = doc.Descendants("BusinessCard")
                        .Select(x => new BusinessCard
                        {
                            Id = (int)x.Element("Id"),
                            Name = (string)x.Element("Name"),
                            Gender = (string)x.Element("Gender"),
                            DateOfBirth = (DateTime)x.Element("DateOfBirth"),
                            Email = (string)x.Element("Email"),
                            Phone = (string)x.Element("Phone"),
                            Address = (string)x.Element("Address"),
                            Photo = (string)x.Element("Photo")
                        }).ToList();
                    businessCards.AddRange(records);
                }              
                _context.BusinessCards.AddRange(businessCards);
                await _context.SaveChangesAsync();

                return Ok($"Successfully processed and saved {businessCards.Count} business card(s) from the file.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the file.");
                return StatusCode(500, "Internal server error");
            }
        }
        //============================================================================//      
    }
}
