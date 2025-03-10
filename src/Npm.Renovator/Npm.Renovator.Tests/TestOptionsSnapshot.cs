using Microsoft.Extensions.Options;
using Moq;

namespace AiTrainer.Web.TestBase
{
    public class TestOptionsSnapshot<T>: Mock<IOptionsSnapshot<T>> where T : class
    {
        public TestOptionsSnapshot(T optionsVal)
        {
            Setup(x => x.Value).Returns(optionsVal);
        }
    }
}
