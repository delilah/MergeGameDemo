using NUnit.Framework;

public class SimpleMathTests
{
    [Test]
    public void SumOfTwoIntegers_IsCorrect()
    {
        // Arrange
        int a = 2;
        int b = 3;

        // Act
        int result = a + b;

        // Assert
        Assert.AreEqual(5, result);
    }
}
