using Microsoft.AspNetCore.Mvc;
using SoftBusinessCards.Models;
using SoftBusinessCards.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Xml.Linq;
using CsvHelper;
using System.Drawing;
using Aspose.BarCode.BarCodeRecognition;

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
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                _logger.LogError(ex, "Error occurred while creating business card. IP: {IpAddress}, Exception: {Exception}", ipAddress, ex.Message);
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

                    using (var tempStream = new MemoryStream())
                    {
                        bitmap.Save(tempStream, System.Drawing.Imaging.ImageFormat.Png);
                        tempStream.Position = 0;

                        using (BarCodeReader reader = new BarCodeReader(tempStream, DecodeType.QR))
                        {
                            var qrCodes = reader.ReadBarCodes();
                            if (qrCodes.Length == 0)
                                return BadRequest("Unable to decode the QR code");

                            List<BusinessCard> businessCards = new List<BusinessCard>();

                            foreach (var qrCode in qrCodes)
                            {
                                var data = qrCode.CodeText.Split(';');
                                if (data.Length != 8)
                                    return BadRequest("Invalid data format in QR code");

                                var businessCard = new BusinessCard
                                {
                                    Id = int.Parse(data[0]),
                                    Name = data[1],
                                    Gender = data[2],
                                    DateOfBirth = DateTime.Parse(data[3]),
                                    Email = data[4],
                                    Phone = data[5],
                                    Address = data[6],
                                    Photo = data[7],
                                    CreatedAt = DateTime.Now
                                };

                                businessCards.Add(businessCard);
                            }

                            _context.BusinessCards.AddRange(businessCards);
                            await _context.SaveChangesAsync();

                            return Ok(businessCards);
                        }
                    }
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
