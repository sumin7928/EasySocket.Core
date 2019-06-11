using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace EasySocket.Core.Tests.Helper
{
    public class OutputHelper
    {
        protected readonly ITestOutputHelper _output;

        public OutputHelper(ITestOutputHelper output)
        {
            _output = output;
        }

    }
}
