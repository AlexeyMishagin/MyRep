using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIC_Test_work
{
    public interface IMessage
    {
        void ShowError(string message);
        void ShowMessage(string message);
    }
}
