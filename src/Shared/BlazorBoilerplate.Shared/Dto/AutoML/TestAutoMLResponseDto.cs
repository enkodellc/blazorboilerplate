using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Dto.AutoML
{
    public class TestAutoMLResponseDto
    {
        public List<string> Predictions { get; set; }
        public double Score { get; set; }
        public double Predictiontime { get; set; }
        public TestAutoMLResponseDto()
        {
            Predictions = new List<string>();
        }
    }
}
