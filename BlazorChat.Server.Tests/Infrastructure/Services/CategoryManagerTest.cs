using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Server.Infrastructure.Services;
using NSubstitute;

namespace BlazorChat.Tests.Server.Infrastructure.Services;

[TestFixture]
public class CategoryManagerTests
{
    private ICategoryRepository _mockRepository = null!;
    private CategoryManager _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = Substitute.For<ICategoryRepository>();

        _sut = new CategoryManager(_mockRepository);
    }

    [Test]
    public async Task ResolveCategoryAsync_WhenIdIsProvidedAndExists_ShouldReturnMatchingCategory()
    {
        var serverId = 1;
        var categoryId = 42;
        var expectedCategory = ChannelCategory.Create("Text Channels", serverId, 0);

        _mockRepository.GetByIdAndServerAsync(categoryId, serverId, Arg.Any<CancellationToken>())
            .Returns(expectedCategory);

        var result = await _sut.ResolveCategoryAsync(serverId, categoryId, categoryName: null);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.SameAs(expectedCategory), "Should return the exact tracked category object found by its unique ID.");
        });

        await _mockRepository.DidNotReceive().GetByNameAndServerAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ResolveCategoryAsync_WhenIdIsInvalidAndNameIsMissing_ShouldReturnNull()
    {
        var serverId = 1;
        int? invalidCategoryId = 0;

        var result = await _sut.ResolveCategoryAsync(serverId, invalidCategoryId, categoryName: "   ");

        Assert.That(result, Is.Null, "Should return null immediately if no valid ID or Name criteria can be verified.");
    }

    [Test]
    public async Task ResolveCategoryAsync_WhenIdMissingButNameMatchesExisting_ShouldReturnExistingCategory()
    {
        var serverId = 2;
        var searchName = "  Voice Chat  ";
        var expectedCategory = ChannelCategory.Create("Voice Chat", serverId, 0);

        _mockRepository.GetByNameAndServerAsync("Voice Chat", serverId, Arg.Any<CancellationToken>())
            .Returns(expectedCategory);

        var result = await _sut.ResolveCategoryAsync(serverId, categoryId: null, searchName);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Voice Chat"));
            Assert.That(result, Is.SameAs(expectedCategory), "Should fallback to string-name lookups and return the found category reference.");
        });

        await _mockRepository.DidNotReceive().AddAsync(Arg.Any<ChannelCategory>(), Arg.Any<CancellationToken>());
        await _mockRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ResolveCategoryAsync_WhenCategoryNotFound_ShouldCreateAndPersistNewCategory()
    {
        var serverId = 3;
        var targetName = "Gaming Zone";

        _mockRepository.GetByNameAndServerAsync(targetName, serverId, Arg.Any<CancellationToken>())
            .Returns((ChannelCategory?)null);

        var result = await _sut.ResolveCategoryAsync(serverId, categoryId: null, targetName);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo(targetName), "The generated category name must match our search parameters.");
            Assert.That(result.ServerId, Is.EqualTo(serverId));
        });

        await _mockRepository.Received(1).AddAsync(Arg.Is<ChannelCategory>(c => c.Name == targetName && c.ServerId == serverId), Arg.Any<CancellationToken>());
        await _mockRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}