using System;
using System.Collections.Generic;

namespace DatasetProcessing.Datas
{
    public class V0CategoriesRoot
    {
        public List<V0Category> categories { get; set; } = new();
        public object info { get; set; }
    }

    public class V0Category
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class V0Mask
    {
        public string V0Name { get; set; }
        public string V0Format { get; set; }
        public double[][] V0Values { get; set; }
    }
}