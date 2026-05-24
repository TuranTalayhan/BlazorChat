using System;
using System.Net;
using System.Threading.Tasks;
using BlazorChat.Client.Core;
using BlazorChat.Client.Features.Servers.Dialogs.CreateCategory;
using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Shared.DTO;
using NSubstitute;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.Servers.Dialogs.CreateCategory;

[TestFixture]
public class CreateCategoryViewModelTest
{
    private IChannelsApiService _mockApiService = null!;
    private CreateCategoryViewModel _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockApiService = Substitute.For<IChannelsApiService>();
        _sut = new CreateCategoryViewModel(_mockApiService);
    }

    [Test]
    public void IsValid_IsTrue_WhenNameIsNotEmpty()
    {
        _sut.CategoryName = "New Category";
        Assert.That(_sut.IsValid, Is.True);
    }

    [Test]
    public void IsValid_IsFalse_WhenNameIsEmpty()
    {
        _sut.CategoryName = "";
        Assert.That(_sut.IsValid, Is.False);
    }

    [Test]
    public async Task SubmitAsync_ReturnsNull_WhenInvalid()
    {
        _sut.CategoryName = "";
        var result = await _sut.SubmitAsync(1);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task SubmitAsync_SetsErrorMessage_OnApiFailure()
    {
        _sut.CategoryName = "New Category";
        _mockApiService.CreateCategoryAsync(1, Arg.Any<CreateCategoryDto>())
            .Returns(new ApiResponse<CategoryDto> { IsSuccess = false, ErrorMessage = "Failed" });

        var result = await _sut.SubmitAsync(1);

        Assert.That(result, Is.Null);
        Assert.That(_sut.ErrorMessage, Is.EqualTo("Failed"));
    }

    [Test]
    public async Task SubmitAsync_ReturnsData_OnSuccess()
    {
        _sut.CategoryName = "New Category";
        var expected = new CategoryDto { Id = 1, Name = "New Category" };
        _mockApiService.CreateCategoryAsync(1, Arg.Any<CreateCategoryDto>())
            .Returns(new ApiResponse<CategoryDto> { IsSuccess = true, Data = expected });

        var result = await _sut.SubmitAsync(1);

        Assert.That(result, Is.EqualTo(expected));
        Assert.That(_sut.ErrorMessage, Is.Null);
    }
}
