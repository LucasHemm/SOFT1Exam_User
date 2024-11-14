namespace UserServiceTest;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        int one = 2;
        int two = 2;

        int three = one + two;
        
        Assert.Equal(4,three);
    }
}