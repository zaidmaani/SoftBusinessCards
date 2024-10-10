using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BusinessCardAPI.Controllers;  
using SoftBusinessCards.Models;  
using ZXing;
using BusinessCardAPI.Controllers;
using SoftBusinessCards.Data;
using SoftBusinessCards.Models;
using Microsoft.Extensions.Logging;
using System.Drawing;

[TestFixture]
public class ExtractBusinessCardFromQRCodeTests
{
    private BusinessCardContext _context;
    private BusinessCardController _controller;

    [SetUp]
    public void Setup()
    {
         
        var options = new DbContextOptionsBuilder<BusinessCardContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new BusinessCardContext(options);

         var loggerMock = new Mock<ILogger<BusinessCardController>>();

         _controller = new BusinessCardController(_context, loggerMock.Object);

         _context.BusinessCards.Add(new BusinessCard
        {
            Id = 1,
            Name = "John Doe",
            Gender = "Male",
            Email = "john@example.com",
            Phone = "1234567890",  
            Address = "123 Main St",  
            DateOfBirth = new DateTime(1990, 1, 1)  
        });
        _context.SaveChanges();
    }

    [Test]
    public async Task ExtractBusinessCardFromQRCode_ReturnsOk_WhenQRCodeIsValid()
    {
         var physicalFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "qrcode.png");

         var fileMock = new Mock<IFormFile>();

         var memoryStream = new MemoryStream();
        using (var fileStream = new FileStream(physicalFilePath, FileMode.Open))
        {
            await fileStream.CopyToAsync(memoryStream);
        }
        memoryStream.Position = 0;

         fileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);
        fileMock.Setup(f => f.FileName).Returns(physicalFilePath);
        fileMock.Setup(f => f.Length).Returns(memoryStream.Length);

         var result = await _controller.ExtractBusinessCardFromQRCode(fileMock.Object);

         Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task ExtractBusinessCardFromQRCode_ReturnsBadRequest_WhenQRCodeIsInvalid()
    {
         var fileMock = new Mock<IFormFile>();
        var stream = new MemoryStream();
        fileMock.Setup(x => x.OpenReadStream()).Returns(stream);
        fileMock.Setup(x => x.Length).Returns(0);

         var result = await _controller.ExtractBusinessCardFromQRCode(fileMock.Object);

         Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
