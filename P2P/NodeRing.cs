using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace P2P
{

    class NodeRing
    {
        /*private Node[] myRing;
        NodeRing()
        {
            
        }

        public void addNode(string id)
        {

        }

        public void removeNode(string id)
        {

        }

        public void getNode(string key)
        {
            UInt32 top = (UInt32)myRing.Length;
            UInt32 high = top;
            UInt32 low = 0;
            var val = Hashing(key);

            /*
            while (true)
            {
                UInt32 mid = (high - low) / 2 + low;
                var pt = myRing[mid];
                if (val > pt.key)
                    low = mid;
                else if (val < pt.key)
                    high = mid;
                if (mid == top)
                    return myRing[0].value;
                else if (high - low <= 1)
                    return myRing[mid].value;
            }
            
        

    */
    }
}
