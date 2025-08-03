using Xunit;
using Moq;
using ParkLite.Api.Services;
using ParkLite.Api.Interfaces;
using ParkLite.Api.Models;

namespace ParkLite.Tests.Services
{
	public class AccountServiceTests
	{
		[Fact]
		public void GetById_ReturnsAccount_WhenAccountExists()
		{
			// Arrange
			var mockRepo = new Mock<IAccountRepository>();
			var expectedAccount = new Account(1, "Test Account", true)
			{
				Contacts = [],
				Vehicles = []
			};

			mockRepo.Setup(r => r.GetById(1)).Returns(expectedAccount);

			var service = new AccountService(mockRepo.Object);

			// Act
			var result = service.GetById(1);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(1, result.Id);
			Assert.Equal("Test Account", result.Name);
			Assert.True(result.IsActive);
		}

		[Fact]
		public void GetById_ReturnsNull_WhenAccountDoesNotExist()
		{
			// Arrange
			var mockRepo = new Mock<IAccountRepository>();
			mockRepo.Setup(r => r.GetById(It.IsAny<int>())).Returns((Account?)null);

			var service = new AccountService(mockRepo.Object);

			// Act
			var result = service.GetById(999);

			// Assert
			Assert.Null(result);
		}
	}
}