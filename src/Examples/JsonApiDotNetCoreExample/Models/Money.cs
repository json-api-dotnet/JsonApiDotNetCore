using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JsonApiDotNetCoreExample.Models
{
    public class Money
    {
        private decimal _value;
        private string _currency;

        public Money(decimal value, string currency)
        {
            _value = value;
            _currency = currency;
        }

        public decimal Value
        {
            get
            {
                return _value;
            }
            protected set
            {
                _value = value;
            }
        }

        public string Currency
        {
            get
            {
                return _currency;
            }
            protected set
            {
                _currency = value;
            }
        }
    }
}
