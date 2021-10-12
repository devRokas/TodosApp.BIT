using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Persistence.Models.ReadModels;
using Persistence.Repositories;
using RestAPI.Options;
using RestAPI.Services;
using TestHelpers.Attributes;
using Xunit;

namespace RestAPI.UnitTests.Services
{
    public class ApikeyService_Should
    {
        [Theory, AutoMoqData]
        public async Task CreateApiKey_ReturnsBadHttpException_When_UserIsNull(
            string username,
            string password,
            [Frozen] Mock<IUserRepository> userRepositoryMock,
            ApikeyService sut)
        {
            // Arrange
            userRepositoryMock
                .Setup(mock => mock.GetAsync(It.IsAny<string>()))
                .ReturnsAsync((UserReadModel) null);
            
            // Act & Assert 
            var result = await sut
                .Invoking(sut => sut.CreateApiKey(username, password))
                .Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage($"User with Username: '{username}' does not exists!");

            result.Which.StatusCode.Should().Be(404);
            
            userRepositoryMock.Verify(userRepository => userRepository.GetAsync(username), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task CreateApiKey_ReturnsBadHttpException_When_WrongPassword(
            string username,
            string password,
            UserReadModel userReadModel,
            [Frozen] Mock<IUserRepository> userRepositoryMock,
            ApikeyService sut)
        {
            // Arrange
            userRepositoryMock
                .Setup(mock => mock.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(userReadModel);
            
            // Act & Assert 
            var result = await sut
                .Invoking(sut => sut.CreateApiKey(username, password))
                .Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage($"Wrong password for user: '{userReadModel.Username}'");

            result.Which.StatusCode.Should().Be(400);
            
            userRepositoryMock.Verify(userRepository => userRepository.GetAsync(username), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task CreateApiKey_ReturnsBadHttpException_When_ApiKeyLimit_Is_Reached(
            UserReadModel userReadModel,
            ApiKeySettings apiKeySettings,
            IEnumerable<ApiKeyReadModel> apiKeys,
            [Frozen] Mock<IUserRepository> userRepositoryMock,
            [Frozen] Mock<IApiKeysRepository> apiKeyRepositoryMock,
            [Frozen] Mock<IOptions<ApiKeySettings>> apiKeySettingsMock,
            ApikeyService sut)
        {
            // Arrange
            userRepositoryMock
                .Setup(mock => mock.GetAsync(userReadModel.Username))
                .ReturnsAsync(userReadModel);

            apiKeyRepositoryMock
                .Setup(mock => mock.GetByUserIdAsync(userReadModel.Id))
                .ReturnsAsync(apiKeys);

            apiKeySettings.ApiKeyLimit = apiKeys.Count();
            
            apiKeySettingsMock
                .SetupGet(mock => mock.Value)
                .Returns(apiKeySettings);
            
            // var sut = fixture.Create<ApikeyService>();
            
            // Act & Assert
            var result = await sut
                .Invoking(sut => sut.CreateApiKey(userReadModel.Username, userReadModel.Password))
                .Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage($"Api key limit is reached");

            result.Which.StatusCode.Should().Be(400);
            
            userRepositoryMock.Verify(userRepository => userRepository.GetAsync(It.IsAny<string>()), Times.Once);

            apiKeyRepositoryMock.Verify(mock => mock.GetByUserIdAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task CreateApiKey_When_AllChecksPass(
            UserReadModel userReadModel,
            ApiKeySettings apiKeySettings,
            IEnumerable<ApiKeyReadModel> apiKeys,
            [Frozen] Mock<IUserRepository> userRepositoryMock,
            [Frozen] Mock<IApiKeysRepository> apiKeyRepositoryMock,
            [Frozen] Mock<IOptions<ApiKeySettings>> apiKeySettingsMock,
            ApikeyService sut)
        {
            // Arrange
            userRepositoryMock
                .Setup(mock => mock.GetAsync(userReadModel.Username))
                .ReturnsAsync(userReadModel);

            apiKeyRepositoryMock
                .Setup(mock => mock.GetByUserIdAsync(userReadModel.Id))
                .ReturnsAsync(apiKeys);

            apiKeySettings.ApiKeyLimit = apiKeys.Count() + 1;
            
            apiKeySettingsMock
                .SetupGet(mock => mock.Value)
                .Returns(apiKeySettings);
            
            // Act
            var result = await sut.CreateApiKey(userReadModel.Username, userReadModel.Password);
            
            // Assert
            userRepositoryMock.Verify(userRepository => userRepository.GetAsync(It.IsAny<string>()), Times.Once);
            
            apiKeyRepositoryMock.Verify(mock => mock.GetByUserIdAsync(It.IsAny<Guid>()), Times.Once);
            
            apiKeyRepositoryMock
                .Verify(mock => mock.SaveAsync(It.Is<ApiKeyReadModel>(model => 
                    model.UserId.Equals(userReadModel.Id) &&
                    model.IsActive)));

            result.UserId.Should().Be(userReadModel.Id);
            result.IsActive.Should().BeTrue();
        }

        
        
        
        
        // public async Task CreateApiKey_ReturnsBadHttpException_When_WrongPassword()
        // {
        //     var userRepositoryMock = new Mock<IUserRepository>();
        //
        //     userRepositoryMock
        //         .Setup(mock => mock.GetAsync(It.IsAny<string>()))
        //         .ReturnsAsync((UserReadModel) null);
        //     
        //     // Act
        //     // var result = 
        //         
        //     // Assert 
        //         
        // }
        
        // public async Task TestCreate()
        // {
        //     var apiKeySettingsMock = new Mock<IOptions<ApiKeySettings>>();
        //
        //     var apiKeySettings = new ApiKeySettings
        //     {
        //         ExpirationTimeInMinutes = 455,
        //         ApiKeyLimit = 54
        //     };
        //
        //     apiKeySettingsMock
        //         .SetupGet(apiKeySettings => apiKeySettings.Value)
        //         .Returns(apiKeySettings);
        //
        //     var apiKeyService = new ApikeyService(new UsersRepository(), new ApiKeysRepository(), apiKeySettingsMock.Object);
        // }
    }
}