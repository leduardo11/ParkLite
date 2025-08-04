using Xunit;
using Moq;
using ParkLite.Api.Services;
using ParkLite.Api.Interfaces;
using ParkLite.Api.Models;
using ParkLite.Api.Dtos;

namespace ParkLite.Tests.Services
{
	public class AccountServiceTests
	{
		[Fact]
		public async Task GetPaginatedAccountsAsync_ReturnsMappedResult()
		{
			// Arrange
			var mockRepo = new Mock<IAccountRepository>();
			var mockData = new PaginatedResult<Account>
			{
				Total = 2,
				Result =
				[
					new(1, "Test 1", true),
					new(2, "Test 2", false)
				]
			};

			mockRepo.Setup(r => r.GetPaginatedAccountsAsync(10, 0, null)).ReturnsAsync(mockData);
			var service = new AccountService(mockRepo.Object);

			// Act
			var result = await service.GetPaginatedAccountsAsync(10, 0);

			// Assert
			Assert.Equal(2, result.Total);
			Assert.Collection(result.Result,
				item => Assert.Equal("Test 1", item.Name),
				item => Assert.Equal("Test 2", item.Name));
		}

		[Fact]
		public async Task GetByIdAsync_ReturnsMappedDTO()
		{
			// Arrange
			var mockRepo = new Mock<IAccountRepository>();
			var account = new Account(1, "Sample", true);
			mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(account);
			var service = new AccountService(mockRepo.Object);

			// Act
			var result = await service.GetByIdAsync(1);

			// Assert
			Assert.NotNull(result);
			Assert.Equal("Sample", result!.Name);
			Assert.True(result.IsActive);
		}

		[Fact]
		public async Task AddAsync_CallsRepositoryWithMappedAccount()
		{
			// Arrange
			var mockRepo = new Mock<IAccountRepository>();
			var service = new AccountService(mockRepo.Object);
			var dto = new AccountDTO
			{
				Name = "New Account",
				IsActive = true,
				Contacts = [],
				Vehicles = []
			};

			// Act
			await service.AddAsync(dto);

			// Assert
			mockRepo.Verify(r => r.AddAsync(It.Is<Account>(a =>
				a.Name == dto.Name &&
				a.IsActive == dto.IsActive &&
				a.Contacts.Count == 0 &&
				a.Vehicles.Count == 0
			)), Times.Once);
		}

		[Fact]
		public async Task UpdateAsync_CallsRepositoryWithMappedAccount()
		{
			// Arrange
			var mockRepo = new Mock<IAccountRepository>();
			var service = new AccountService(mockRepo.Object);
			var dto = new AccountDTO
			{
				Id = 42,
				Name = "Updated Account",
				IsActive = false,
				Contacts = [],
				Vehicles = []
			};

			// Act
			await service.UpdateAsync(dto);

			// Assert
			mockRepo.Verify(r => r.UpdateAsync(It.Is<Account>(a =>
				a.Id == dto.Id &&
				a.Name == dto.Name &&
				a.IsActive == dto.IsActive
			)), Times.Once);
		}

		[Fact]
		public async Task DeleteAsync_CallsRepositoryDelete()
		{
			// Arrange
			var mockRepo = new Mock<IAccountRepository>();
			var service = new AccountService(mockRepo.Object);

			// Act
			await service.DeleteAsync(1);

			// Assert
			mockRepo.Verify(r => r.DeleteAsync(1), Times.Once);
		}

		[Fact]
		public async Task BatchDeactivateInactiveAccountsAsync_CallsRepository()
		{
			// Arrange
			var mockRepo = new Mock<IAccountRepository>();
			var service = new AccountService(mockRepo.Object);

			// Act
			await service.BatchDeactivateInactiveAccountsAsync(batchSize: 25, delayMs: 500);

			// Assert
			mockRepo.Verify(r => r.BatchDeactivateInactiveAccountsAsync(25, 500), Times.Once);
		}
	}
}
