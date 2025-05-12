using LabsAPI.Handlers;
using Microsoft.Extensions.Configuration;
using Moq;

namespace LabsTest;

[TestFixture]
public class ProcessHandlerPrivateMethodsTests
{
    private ProcessHandler _handler;
    private Mock<IHttpClientFactory> _httpClientFactoryMock;
    private Mock<IConfiguration> _configurationMock;

    [SetUp]
    public void Setup()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _configurationMock = new Mock<IConfiguration>();

        // 1. Настройка для простых значений (Url)
        var urlSectionMock = new Mock<IConfigurationSection>();
        urlSectionMock.Setup(x => x.Value).Returns("http://test.com");
        _configurationMock
            .Setup(x => x.GetSection("RandomNumberApi"))
            .Returns(urlSectionMock.Object);

        // 2. Настройка для массива (BlacklistedWords)
        var blacklist = new List<string> { "badword", "test" };
        var blacklistSectionMock = new Mock<IConfigurationSection>();
        blacklistSectionMock.Setup(x => x.Value).Returns(string.Join(",", blacklist));
        _configurationMock
            .Setup(x => x.GetSection("BlacklistedWords"))
            .Returns(blacklistSectionMock.Object);

        // 3. Настройка для числовых значений (ParallelLimit)
        var limitSectionMock = new Mock<IConfigurationSection>();
        limitSectionMock.Setup(x => x.Value).Returns("5");
        _configurationMock
            .Setup(x => x.GetSection("Settings:ParallelLimit"))
            .Returns(limitSectionMock.Object);

        _handler = new ProcessHandler(_httpClientFactoryMock.Object, _configurationMock.Object);
    }

    [Test]
    [TestCase("abc", "")]
    [TestCase("a1b", "Ошибка: в строке содержатся недопустимые символы: '1' ")]
    public void ValidateInputWithRegex_Test(string input, string expected)
    {
        var result = PrivateMethodAccessor.InvokePrivateMethod<ProcessHandler, string>(
            _handler,
            "ValidateInputWithRegex",
            input
        );

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("abcd", "badc")]
    [TestCase("abcde", "edcbaabcde")]
    public void ProcessValidString_Test(string input, string expected)
    {
        var result = PrivateMethodAccessor.InvokePrivateMethod<ProcessHandler, string>(
            _handler,
            "ProcessValidString",
            input
        );

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetCharacterCounts_Test()
    {
        var input = "aabbc";
        var result = PrivateMethodAccessor.InvokePrivateMethod<
            ProcessHandler,
            Dictionary<char, int>
        >(_handler, "GetCharacterCounts", input);

        Assert.Multiple(() =>
        {
            Assert.That(result['a'], Is.EqualTo(2));
            Assert.That(result['b'], Is.EqualTo(2));
            Assert.That(result['c'], Is.EqualTo(1));
        });
    }

    [Test]
    [TestCase("aeb", "ae")]
    [TestCase("xbacefgi", "acefgi")]
    public void FindMaxVowelSubstring_Test(string input, string expected)
    {
        var result = PrivateMethodAccessor.InvokePrivateMethod<ProcessHandler, string>(
            _handler,
            "FindMaxVowelSubstring",
            input
        );

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("cba", "abc")]
    public void QuickSortString_Test(string input, string expected)
    {
        var result = PrivateMethodAccessor.InvokePrivateMethod<ProcessHandler, string>(
            _handler,
            "QuickSortString",
            input
        );

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void RemoveCharAtPosition_Test()
    {
        var result = PrivateMethodAccessor.InvokePrivateMethod<ProcessHandler, string>(
            _handler,
            "RemoveCharAtPosition",
            "abcde",
            2
        );

        Assert.That(result, Is.EqualTo("abde"));
    }
    /*
        [Test]
        public async Task GetResultAsync_ShouldCallAllPrivateMethods()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            _httpClientFactoryMock
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClientMock.Object);

            // Act
            var result = await _handler.GetResultAsync("abcdef", true);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            // Проверяем что все методы отработали корректно через результат
            Assert.Multiple(() =>
            {
                Assert.That(result.ProcessedString, Is.EqualTo("cbafed")); // ProcessValidString
                Assert.That(result.CharacterCounts['a'], Is.EqualTo(1)); // GetCharacterCounts
                Assert.That(result.MaxVowelSubstring, Is.EqualTo("afe")); // FindMaxVowelSubstring
                Assert.That(result.SortedString, Is.EqualTo("abcdef")); // QuickSortString
                Assert.That(result.TrimmedString.Length, Is.EqualTo(5)); // RemoveCharAtPosition
            });
        }*/
}
