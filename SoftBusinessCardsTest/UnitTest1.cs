using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BusinessCardAPI.Controllers;  
using SoftBusinessCards.Models;  
using BusinessCardAPI.Controllers;
using SoftBusinessCards.Data;
using SoftBusinessCards.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

[TestFixture]
public class ExportBusinessCardTests
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
    public async Task ExportBusinessCard_ReturnsCsvFile_WhenFormatIsCsv()
    {
         var result = await _controller.ExportBusinessCard(1, "csv");

         Assert.IsInstanceOf<FileContentResult>(result);
        var fileResult = result as FileContentResult;
        Assert.AreEqual("text/csv", fileResult.ContentType);
    }

    [Test]
    public async Task ExportBusinessCard_ReturnsXmlFile_WhenFormatIsXml()
    {
         var result = await _controller.ExportBusinessCard(1, "xml");

         Assert.IsInstanceOf<FileContentResult>(result);
        var fileResult = result as FileContentResult;
        Assert.AreEqual("application/xml", fileResult.ContentType);
    }

    [Test]
    public async Task ExportBusinessCard_ReturnsNotFound_WhenNoBusinessCard()
    {
         var result = await _controller.ExportBusinessCard(999, "csv");

         Assert.IsInstanceOf<NotFoundObjectResult>(result);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
