using Microsoft.AspNetCore.Mvc;
using SoftBusinessCards.Models;
using SoftBusinessCards.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Xml.Linq;
using CsvHelper;
using System.Drawing;
using ZXing;
using ZXing.Common;
using System.Drawing;
using ZXing.QrCode;
using System.Text;

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


        [HttpPost("Create")]
        public async Task<IActionResult> CreateBusinessCard([FromBody] BusinessCard card)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingCard = await _context.BusinessCards.FindAsync(card.Id);

                if (existingCard != null)
                {
                     existingCard.Name = card.Name;
                    existingCard.Gender = card.Gender;
                    existingCard.DateOfBirth = card.DateOfBirth;
                    existingCard.Email = card.Email;
                    existingCard.Phone = card.Phone;
                    existingCard.Address = card.Address;
                    existingCard.Photo = card.Photo;

                    _context.BusinessCards.Update(existingCard);
                }
                else
                {
                     _context.BusinessCards.Add(card);
                }

                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(ViewBusinessCards), new { id = card.Id }, card);
            }
            catch (Exception ex)
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                _logger.LogError(ex, "Error occurred while creating or updating business card. IP: {IpAddress}, Exception: {Exception}", ipAddress, ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
        //============================================================================//

        [HttpGet("View")]
        public async Task<IActionResult> ViewBusinessCards()
        {
            try
            {
                var cards = await _context.BusinessCards.ToListAsync();
                return Ok(cards);
            }
            catch (Exception ex)
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                _logger.LogError(ex, "Error occurred while creating business card. IP: {IpAddress}, Exception: {Exception}", ipAddress, ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
        //============================================================================//

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
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                _logger.LogError(ex, "Error occurred while creating business card. IP: {IpAddress}, Exception: {Exception}", ipAddress, ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
        //============================================================================//

        [HttpPost("CreateFromFile")]
        public async Task<IActionResult> CreateBusinessCardFromFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("File is empty");

                string extension = Path.GetExtension(file.FileName);
                if (extension != ".xml" && extension != ".csv")
                    return BadRequest("Unsupported file format");

                List<BusinessCard> businessCards = new List<BusinessCard>();

                if (extension == ".csv")
                {
                    using (var reader = new StreamReader(file.OpenReadStream()))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csv.Context.TypeConverterOptionsCache.GetOptions<DateTime>()
                           .Formats = new[] { "yyyy-MM-ddTHH:mm:ss", "MM/dd/yyyy", "M/d/yyyy" };

                        var records = csv.GetRecords<BusinessCard>().ToList();

                        foreach (var card in records)
                        {
                            var existingCard = await _context.BusinessCards
                                .FirstOrDefaultAsync(b => b.Id == card.Id);

                            if (existingCard != null)
                            {
                                existingCard.Name = card.Name;
                                existingCard.Gender = card.Gender;
                                existingCard.DateOfBirth = card.DateOfBirth;
                                existingCard.Email = card.Email;
                                existingCard.Phone = card.Phone;
                                existingCard.Address = card.Address;
                                existingCard.Photo = card.Photo;
                            }
                            else
                            {
                                _context.BusinessCards.Add(card);
                            }

                            businessCards.Add(card);  
                        }
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

                    foreach (var card in records)
                    {
                        var existingCard = await _context.BusinessCards
                            .FirstOrDefaultAsync(b => b.Id == card.Id);

                        if (existingCard != null)
                        {
                            existingCard.Name = card.Name;
                            existingCard.Gender = card.Gender;
                            existingCard.DateOfBirth = card.DateOfBirth;
                            existingCard.Email = card.Email;
                            existingCard.Phone = card.Phone;
                            existingCard.Address = card.Address;
                            existingCard.Photo = card.Photo;
                        }
                        else
                        {
                            _context.BusinessCards.Add(card);
                        }

                        businessCards.Add(card);  
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(businessCards);
            }
            catch (Exception ex)
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                _logger.LogError(ex, "Error occurred while creating business card. IP: {IpAddress}, Exception: {Exception}", ipAddress, ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
        //============================================================================//

        [HttpGet("ExportBusinessCard")]
        public async Task<IActionResult> ExportBusinessCard(int id, string format)
        {
            try
            {
                 var businessCards = await _context.BusinessCards
                    .Where(bc => bc.Id == id)
                    .ToListAsync();

                if (businessCards == null || businessCards.Count == 0)
                    return NotFound($"No business card found with ID: {id}");

                 if (format.ToLower() == "csv")
                {
                     var csvContent = new StringWriter();
                    using (var csvWriter = new CsvWriter(csvContent, CultureInfo.InvariantCulture))
                    {
                        csvWriter.WriteRecords(businessCards);
                    }

                     var bytes = Encoding.UTF8.GetBytes(csvContent.ToString());
                    return File(bytes, "text/csv", $"BusinessCard_{id}.csv");
                }
                else if (format.ToLower() == "xml")
                {
                     var xmlDocument = new XDocument(
                        new XElement("BusinessCards",
                            businessCards.Select(bc => new XElement("BusinessCard",
                                new XElement("Id", bc.Id),
                                new XElement("Name", bc.Name),
                                new XElement("Gender", bc.Gender),
                                new XElement("DateOfBirth", bc.DateOfBirth.ToString("yyyy-MM-ddTHH:mm:ss")),
                                new XElement("Email", bc.Email),
                                new XElement("Phone", bc.Phone),
                                new XElement("Address", bc.Address),
                                new XElement("Photo", bc.Photo)
                            ))
                        )
                    );

                     var xmlBytes = Encoding.UTF8.GetBytes(xmlDocument.ToString());
                    return File(xmlBytes, "application/xml", $"BusinessCard_{id}.xml");
                }
                else
                {
                    return BadRequest("Unsupported format. Please use 'csv' or 'xml'.");
                }
            }
            catch (Exception ex)
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                _logger.LogError(ex, "Error occurred while exporting business card. IP: {IpAddress}, Exception: {Exception}", ipAddress, ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        //============================================================================//
        [HttpPost("ExtractFromQRCode")]
        public async Task<IActionResult> ExtractBusinessCardFromQRCode(IFormFile qrCodeImage)
        {
            try
            {
                if (qrCodeImage == null || qrCodeImage.Length == 0)
                    return BadRequest("Please upload a valid QR code image");

                using (var stream = new MemoryStream())
                {
                    await qrCodeImage.CopyToAsync(stream);

                    var bitmap = new Bitmap(stream);
                    var barcodeReader = new ZXing.Windows.Compatibility.BarcodeReader();
                    var result = barcodeReader.Decode(bitmap);

                    if (result == null)
                        return BadRequest("Unable to decode the QR code");

                    if (string.IsNullOrEmpty(result.Text))
                        return BadRequest("QR code does not contain valid data");

                    var data = result.Text.Split(';');
                    if (data.Length != 7)
                        return BadRequest("Invalid data format in QR code");

                    var cardId = int.Parse(data[0]);
                    var businessCard = await _context.BusinessCards.FindAsync(cardId);

                    if (businessCard != null)
                    {
                         businessCard.Name = data[1];
                        businessCard.Gender = data[2];
                        businessCard.DateOfBirth = DateTime.Parse(data[3]);
                        businessCard.Email = data[4];
                        businessCard.Phone = data[5];
                        businessCard.Address = data[6];
                        businessCard.CreatedAt = DateTime.Now;

                        _context.BusinessCards.Update(businessCard);
                    }
                    else
                    {
                         businessCard = new BusinessCard
                        {
                            Id = cardId,
                            Name = data[1],
                            Gender = data[2],
                            DateOfBirth = DateTime.Parse(data[3]),
                            Email = data[4],
                            Phone = data[5],
                            Address = data[6],
                            CreatedAt = DateTime.Now
                        };

                        _context.BusinessCards.Add(businessCard);
                    }

                    await _context.SaveChangesAsync();

                    return Ok(new[] { businessCard });
                }
            }
            catch (Exception ex)
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                _logger.LogError(ex, "Error occurred while extracting business card from QR code. IP: {IpAddress}, Exception: {Exception}", ipAddress, ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        //==============================================================================//

        [HttpGet("Filter")]
        public async Task<IActionResult> FilterBusinessCards([FromQuery] BusinessCardFilter filter)
        {
            try
            {
                var query = _context.BusinessCards.AsQueryable();

                if (!string.IsNullOrEmpty(filter.Name))
                {
                    query = query.Where(c => c.Name.ToLower().Contains(filter.Name.ToLower()));
                }

                if (!string.IsNullOrEmpty(filter.Gender))
                {
                    query = query.Where(c => c.Gender.ToLower().Contains(filter.Gender.ToLower()));
                }

                if (filter.DateOfBirth.HasValue)
                {
                    query = query.Where(c => c.DateOfBirth.Date == filter.DateOfBirth.Value.Date);
                }

                if (!string.IsNullOrEmpty(filter.Email))
                {
                    query = query.Where(c => c.Email.ToLower().Contains(filter.Email.ToLower()));
                }

                if (!string.IsNullOrEmpty(filter.Phone))
                {
                    query = query.Where(c => c.Phone.Contains(filter.Phone));
                }

                if (!string.IsNullOrEmpty(filter.Address))
                {
                    query = query.Where(c => c.Address.ToLower().Contains(filter.Address.ToLower()));
                }

                if (filter.CreatedAt.HasValue)
                {
                    query = query.Where(c => c.CreatedAt.Date == filter.CreatedAt.Value.Date);
                }

                var filteredCards = await query.ToListAsync();
                return Ok(filteredCards);
            }
            catch (Exception ex)
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                _logger.LogError(ex, "Error occurred while filtering business cards. IP: {IpAddress}, Exception: {Exception}", ipAddress, ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }


    }
}
