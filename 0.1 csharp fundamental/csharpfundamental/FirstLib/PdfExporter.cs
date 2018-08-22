using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstLib
{
    public class BangExporter : IPlugin
    {
        public void Hide()
        {
            throw new NotImplementedException();
        }

        public void Init(string connectionString)
        {
            throw new NotImplementedException();
        }

        public void Show()
        {
            throw new NotImplementedException();
        }
    }

    public class PdfExporter : IPlugin
    {
        public void Hide()
        {
            throw new NotImplementedException();
        }

        public void Init(string connectionString)
        {
            throw new NotImplementedException();
        }

        public void Show()
        {
            throw new NotImplementedException();
        }

        public void Du()
        {

        }
    }

    public class StringHelper
    {
        public string CreatePassword(string src)
        {
            return src.GetHashCode().ToString();
        }
    }

    public interface IPlugin
    {
        void Init(string connectionString);
        void Show();
        void Hide();
    }
}
