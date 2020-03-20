using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace zCarape.Core
{
    public static class GlobalniKod
    {
        public static string BazaPath = ConfigurationManager.ConnectionStrings["BazaPath"].ConnectionString;
        public static string SlikeDir = ConfigurationManager.AppSettings["SlikeDir"];

        public static string ConnectionString => "Data Source=" + BazaPath + ";Version=3;ForeignKeys=True";

        public static DezenParam DezenParam { get; set; } = new DezenParam();

    }
}
