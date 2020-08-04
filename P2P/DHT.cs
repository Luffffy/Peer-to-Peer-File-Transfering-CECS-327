using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace P2P
{

    
    public class DHT
    {
        private Node[] HT;
        private int size;
        private int capacity;
        public DHT(int c)
        {
            HT = new Node[c];
            size = 0;
            capacity = c;

            for (int i = 0; i < capacity; i++)
                HT[i] = null;
        }

        public int Size()
        {
            return size; 
        }

        public bool isEmpty()
        {
            return Size() == 0;
        }

        public int getIndex(string key)
        {

            BigInteger hashCode = Hashing(key);
            int mod = (int)(hashCode % int.MaxValue);
            int index = mod % capacity;
            return index;
        }

        private BigInteger Hashing(string input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("X2"));
                }
                string temp = builder.ToString();

                return BigInteger.Parse(temp, System.Globalization.NumberStyles.HexNumber);
            }
        }

        public string GetValue(string k)
        {
            return HT[getIndex(k)].getValue();
        }

        public bool Add(string k, string v)
        {
            if (HT[getIndex(k)] == null)
            { 
                HT[getIndex(k)] = new Node(k,v);
                size++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Remove(string k)
        {
            if (HT[getIndex(k)].getKey() == k)
            {
                HT[getIndex(k)].Set(null, null);
                size--;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool FindKey(string k)
        {
            if (HT[getIndex(k)].getKey() == null)
                return false;
            else
                return true;
        }

        public List<string> getKeys()
        {
            List<string> temp = new List<string>();
            foreach(var n in HT)
            {
                temp.Add(n.getKey());
            }
            return temp;
        }
    }

    
}
