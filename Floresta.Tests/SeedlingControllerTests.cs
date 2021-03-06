﻿using Floresta.Controllers;
using Floresta.Interfaces;
using Floresta.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Floresta.Tests
{
    public class SeedlingsControllerTests
    {
        [Fact]
        public void IndexReturnsAViewResultWithAListOfTests()
        {

            // Arrange
            var mock = new Mock<IRepository<Seedling>>();
            mock.Setup(repo => repo.GetAll()).Returns(GetTestSeedlings());
            var controller = new SeedlingsController(mock.Object);

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Seedling>>(viewResult.Model);
            Assert.Equal(GetTestSeedlings().Count, model.Count());
        }
        private List<Seedling> GetTestSeedlings()
        {
            var seedlings = new List<Seedling>
            {
                new Seedling { Id=1, Name="pear", Amount = 200, Price = 100},
                new Seedling { Id=2, Name="apple", Amount = 100, Price = 100},
                new Seedling { Id=3, Name="oak", Amount = 20, Price = 300},
            };
            return seedlings;
        }
        [Fact]
        public async void AddSeedlingReturnsViewResultWithSeedlingModel()
        {
            // Arrange
            var mock = new Mock<IRepository<Seedling>>();
            var controller = new SeedlingsController(mock.Object);
            controller.ModelState.AddModelError("Name", "Required");
            Seedling newSeedling = new Seedling();

            // Act
            var result = await controller.Create(newSeedling);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(newSeedling, viewResult?.Model);
        }

        [Fact]
        public async void AddSeedlingReturnsARedirectAndAddsSeedling()
        {
            // Arrange
            var mock = new Mock<IRepository<Seedling>>();
            var controller = new SeedlingsController(mock.Object);
            var newSeedling = new Seedling()
            {
                Name = "pear",
                Price = 100,
                Amount = 100 
            };

            // Act
            var result = await controller.Create(newSeedling);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Null(redirectToActionResult.ControllerName);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            mock.Verify(r => r.AddAsync(newSeedling));
        }

        [Fact]
        public void GetSeedlingReturnsNotFoundResultWhenSeedlingNotFound()
        {
            // Arrange
            int testSeedlingId = 1;
            var mock = new Mock<IRepository<Seedling>>();
            mock.Setup(repo => repo.GetById(testSeedlingId))
                .Returns(null as Seedling);
            var controller = new SeedlingsController(mock.Object);

            // Act
            var result = controller.Edit(testSeedlingId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void GetSeedlingReturnsViewResultWithSeedling()
        {
            // Arrange
            int testSeedlingId = 1;
            var mock = new Mock<IRepository<Seedling>>();
            mock.Setup(repo => repo.GetById(testSeedlingId))
                .Returns(GetTestSeedlings().FirstOrDefault(p => p.Id == testSeedlingId));
            var controller = new SeedlingsController(mock.Object);

            // Act
            var result = controller.Edit(testSeedlingId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Seedling>(viewResult.ViewData.Model);
            Assert.Equal("pear", model.Name);
            Assert.Equal(200, model.Amount);
            Assert.Equal(testSeedlingId, model.Id);
        }
        [Fact]
        public async void CreatePostAction_SaveModel()
        {
            // arrange
            var mock = new Mock<IRepository<Seedling>>();
            var seedling = new Seedling();
            var controller = new SeedlingsController(mock.Object);
            // act
            RedirectToRouteResult result = await controller.Create(seedling) as RedirectToRouteResult;
            // assert
            mock.Verify(a => a.AddAsync(seedling));
        }
        [Fact]
        public async void DeletePostAction_SaveModel()
        {
            // arrange
            var mock = new Mock<IRepository<Seedling>>();
            int newsId = 1;
            var controller = new SeedlingsController(mock.Object);
            // act
            RedirectToRouteResult result = await controller.Delete(newsId) as RedirectToRouteResult;
            // assert
            mock.Verify(a => a.DeleteAsync(newsId));
        }
    }
}