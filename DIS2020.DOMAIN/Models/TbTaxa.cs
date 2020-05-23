using System;
using System.Collections.Generic;
using System.Text;

namespace DIS2020.DOMAIN.Models
{
    public class TbTaxa
    {
        public int id { get; set; }
        public int taxa { get; set; }
        public DateTime dia_medicao { get; set; }
        public int variacao { get; set; }

    }
}
