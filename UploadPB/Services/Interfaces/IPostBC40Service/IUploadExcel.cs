﻿using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UploadPB.ViewModels;
using UploadPB.Services.Class.Post40;

namespace UploadPB.Services.Interfaces.IPostBC40Service
{
    public interface IUploadExcel40 : IBaseService
    {
        Task<int> Upload(ExcelWorksheets sheet);
    }
}
