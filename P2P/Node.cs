using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace P2P
{
    class Node
    {
        private string key = null;
        private string value = null;
        public Node(string k, string v)
        {
            key = k;
            value = v;
        }

        public void Set(string k, string v)
        {
            key = k;
            value = v;
        }

        public string getKey()
        {
            return key;
        }

        public string getValue()
        {
            return value;
        }

    }
}
