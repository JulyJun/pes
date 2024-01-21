using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pAlgorithms
{
    public class Algorithms
    {
        public double calculatedLine(double x)
        {
            return -1 / 20 * (x - 75);
        }
        public double weightedData(double humid)
        {
            if (humid < 55)
            {
                //essensial
                return 2;
            }
            else if (humid >= 60 && humid < 70)
            {
                return 1;
            }
            else if (humid >= 80)
            {
                return 0;
            }
            else
            {
                // TODO
                // do some magic Algorithm here
                return calculatedLine(humid);
            }
        }
    }
}
