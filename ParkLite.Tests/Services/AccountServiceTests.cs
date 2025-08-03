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
		public void GetAllAccounts_ReturnsAllAccounts()
		{
			// Arrange
			var mockRepo = new Mock<IAccountRepository>();
			var accounts = new List<Account>
			{
				new(1, "Account1", true),
				new(2, "Account2", false)
			};
			mockRepo.Setup(r => r.GetAll()).Returns(accounts);
			var service = new AccountService(mockRepo.Object);

			// Act
			var result = service.GetAllAccounts();

			// Assert
			Assert.NotNull(result);
			Assert.Equal(2, ((List<Account>)result).Count);
		}

		[Fact]
		public void Add_CallsRepositoryAdd()
		{
			// Arrange
			var mockRepo = new Mock<IAccountRepository>();
			var account = new Account(0, "New Account", true);
			var service = new AccountService(mockRepo.Object);

			// Act
			service.Add(account);

			// Assert
			mockRepo.Verify(r => r.Add(account), Times.Once);
		}

		[Fact]
		public void Update_CallsRepositoryUpdate()
		{
			// Arrange
			var mockRepo = new Mock<IAccountRepository>();
			var account = new Account(1, "Updated Account", false);
			var service = new AccountService(mockRepo.Object);

			// Act
			service.Update(account);

			// Assert
			mockRepo.Verify(r => r.Update(account), Times.Once);
		}

		[Fact]
		public void Delete_CallsRepositoryDelete()
		{
			// Arrange
			var mockRepo = new Mock<IAccountRepository>();
			var service = new AccountService(mockRepo.Object);

			// Act
			service.Delete(1);

			// Assert
			mockRepo.Verify(r => r.Delete(1), Times.Once);
		}
	}
}
