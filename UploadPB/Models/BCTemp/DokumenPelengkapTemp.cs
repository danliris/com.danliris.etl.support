﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UploadPB.Models.BCTemp
{
    public class DokumenPelengkapTemp
    {
        public DokumenPelengkapTemp(string noaju,string jenisdokumen, string nomordokumen, DateTimeOffset? tanggaldokumen)
        {
            NoAju = noaju;
            JenisDokumen = jenisdokumen;
            NomorDokumen = nomordokumen;
            TanggalDokumen = tanggaldokumen;
        }
        public string NoAju { get; set; }
        public string JenisDokumen { get; set; }
        public string NomorDokumen { get; set; }
        public DateTimeOffset? TanggalDokumen { get; set; }
    }
}
