using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetaPoco.SqlKata.Tests
{
    public class PercentCompiler : Compiler
    {
        public PercentCompiler()
        {
            OpeningIdentifier = ClosingIdentifier = "%%";
        }
    }


}
