﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using System.Linq;
using System.Threading.Tasks;
using UploadPB.Services.Interfaces.IPostBC23;
using UploadPB.Models;
using UploadPB.Models.BCTemp;
using UploadPB.Tools;
using UploadPB.DBAdapters;
using UploadPB.DBAdapters.Insert;
using UploadPB.ViewModels;
using UploadPB.SupporttDbContext;
using Microsoft.EntityFrameworkCore;

//using Com.Danliris.ETL.Service.DBAdapters.DyeingAdapters;
//using Com.Danliris.ETL.Service.ExcelModels.DashboardDyeingModels;

namespace UploadPB.Services.Class.PostBC23
{
    public class UploadExcelService : IUploadExcel
    {
        private readonly SupportDbContext context;
        private readonly DbSet<BeacukaiTemporaryModel> dbSet;

        //static string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings:SQLConnectionString", EnvironmentVariableTarget.Process);
        ConverterChecker converterChecker = new ConverterChecker();
        public IDokumenHeaderAdapter headerAdapter;
        public IBarangAdapter barangAdapter;
        public IDokumenPelengkapAdapter dokumenPelengkapAdapter;
        
        public UploadExcelService(IServiceProvider provider, SupportDbContext context)
        {
            headerAdapter = provider.GetService<IDokumenHeaderAdapter>();
            barangAdapter = provider.GetService<IBarangAdapter>();
            dokumenPelengkapAdapter = provider.GetService<IDokumenPelengkapAdapter>();
            this.context = context;
            this.dbSet = context.Set<BeacukaiTemporaryModel>();
        }

        public async Task<int> Upload(ExcelWorksheets sheet)
            //public async Task<List<TemporaryViewModel>> Upload(ExcelWorksheets sheet)
        {
            int Upload = 0;

            using (var transaction = this.context.Database.BeginTransaction())
            {
                try
                {
                    var data = 0;
                    //foreach (var item in sheet)
                    //{
                    //    switch (item.Name.ToUpper())
                    //    {
                    //        case "HEADER DOKUMEN":
                    //            await UploadHeader(sheet, data);
                    //            break;
                    //        case "BARANG":
                    //            await UploadBarang(sheet, data);
                    //            break;
                    //        case "DOKUMEN PELENGKAP":
                    //            await UploadDokumenPelengkap(sheet, data);
                    //            break;




                    //        default:
                    //            throw new Exception(item.Name + " tidak valid");
                    //    }
                    //    data++;
                    //}
                    var ListHeader = new List<HeaderDokumenTempModel>();
                    var ListBarang = new List<BarangTemp>();
                    var ListDokument = new List<DokumenPelengkapTemp>();

                    var count = sheet.Count();
                    for (var i = 0; i < count; i++)
                    {
                        if (sheet[i].Name.ToUpper() == "HEADER DOKUMEN")
                        {
                            var listdata = UploadHeader(sheet, data);
                            ListHeader = listdata;
                        }

                        if (sheet[i].Name.ToUpper() == "BARANG")
                        {
                            var listdata = UploadBarang(sheet, data);
                            ListBarang = listdata;
                        }
                        if (sheet[i].Name.ToUpper() == "DOKUMEN PELENGKAP")
                        {
                            var listdata = UploadDokumenPelengkap(sheet, data);
                            ListDokument = listdata;
                        }
                        data++;
                    }

                    var queryheader = (from a in ListHeader
                                       join b in ListBarang on a.NoAju equals b.NoAju
                                       select new TemporaryViewModel
                                       {
                                           ID = 0,
                                           BCNo = a.BCNo,
                                           Barang = b.Barang,
                                           Bruto = a.Bruto,
                                           CIF = a.CIF,
                                           CIF_Rupiah = a.CIF_Rupiah,
                                           JumlahSatBarang = b.JumlahSatBarang,
                                           KodeBarang = b.KodeBarang,
                                           Netto = a.Netto,
                                           NoAju = b.NoAju,
                                           NamaSupplier = a.NamaSupplier,
                                           Vendor = a.Vendor,
                                           TglBCNO = a.TglBCNO,
                                           Valuta = a.Valuta,
                                           JenisBC = a.JenisBC,
                                           JumlahBarang = a.JumlahBarang,
                                           Sat = b.Sat,
                                           KodeSupplier = a.KodeSupplier,
                                       }).ToList();

                    var querydokumen = (from a in ListHeader
                                        join b in ListDokument on a.NoAju equals b.NoAju
                                        select new TemporaryViewModel
                                        {
                                            ID = 0,
                                            BCNo = a.BCNo,
                                            Bruto = a.Bruto,
                                            NoAju = b.NoAju,
                                            NamaSupplier = a.NamaSupplier,
                                            Vendor = a.Vendor,
                                            TglBCNO = a.TglBCNO,
                                            Valuta = a.Valuta,
                                            JenisBC = a.JenisBC,
                                            JenisDokumen = b.JenisDokumen,
                                            NomorDokumen = b.NomorDokumen,
                                            TanggalDokumen = b.TanggalDokumen,
                                            JumlahBarang = a.JumlahBarang
                                        }).ToList();

                    foreach (var item in querydokumen)
                    {
                        queryheader.Add(item);
                    }

                    //delete all temporaray data
                    var itemtoDelete = context.Set<BeacukaiTemporaryModel>();
                    context.BeacukaiTemporaries.RemoveRange(itemtoDelete);
                    context.SaveChanges();
                    transaction.Commit();

                    //UploadTemporaraydData
                    Upload = await InsertToTemporary(queryheader);

                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }
            return Upload;
        }

        public async Task<int> InsertToTemporary(List<TemporaryViewModel> data)
        {
            int Created = 0;
            using (var transaction = this.context.Database.BeginTransaction())
            {
                try
                {
                    long index = 1;
                    foreach(var a in data)
                    {
                        BeacukaiTemporaryModel beacukaiTemporaryModel = new BeacukaiTemporaryModel
                        {
                            ID = index++,
                            BCNo = a.BCNo,
                            Barang = a.Barang,
                            Bruto = a.Bruto,
                            CIF = a.CIF,
                            CIF_Rupiah = a.CIF_Rupiah,
                            JumlahSatBarang = a.JumlahSatBarang,
                            KodeBarang = a.KodeBarang,
                            Netto = a.Netto,
                            NoAju = a.NoAju,
                            NamaSupplier = a.NamaSupplier,
                            Vendor = a.Vendor,
                            TglBCNO = a.TglBCNO,
                            Valutta = a.Valuta,
                            JenisBC = a.JenisBC,
                            JumlahBarang = a.JumlahBarang,
                            Sat = a.Sat,
                            KodeSupplier = a.KodeSupplier,
                            JenisDokumen = a.JenisDokumen,
                            NomorDokumen = a.NomorDokumen,
                            TanggalDokumen = a.TanggalDokumen,

                        };

                        this.dbSet.Add(beacukaiTemporaryModel);
                    }

                    Created = await context.SaveChangesAsync();
                    transaction.Commit();

                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }
            return Created;
        }

        public List<DokumenPelengkapTemp> UploadDokumenPelengkap(ExcelWorksheets excel, int data)
        {
            var sheet = excel[data];
            var totalRow = sheet.Dimension.Rows;
            var listData = new List<DokumenPelengkapTemp>();
            int rowIndex = 0;
            try{
                for (rowIndex = 2; rowIndex <= totalRow; rowIndex++){
                    if (sheet.Cells[rowIndex, 2].Value != null)
                    {
                        listData.Add(new DokumenPelengkapTemp(
                              converterChecker.GenerateValueString(sheet.Cells[rowIndex, 2]),
                              converterChecker.GenerateValueString(sheet.Cells[rowIndex, 3]),
                              converterChecker.GenerateValueString(sheet.Cells[rowIndex, 4]),
                              converterChecker.GenerateValueDate(sheet.Cells[rowIndex, 5])
                      
                            ));
                    }
                }

                return listData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Gagal memproses Sheet Dokumen Pelengkap pada baris ke-{rowIndex} - {ex.Message}");
            }

            //try
            //{
            //    if (listData.Count() > 0)
            //    {
            //        await dokumenPelengkapAdapter.DeleteBulk();
            //        await dokumenPelengkapAdapter.Insert(listData);

            //    }
            //}catch (Exception ex)
            //{
            //    throw new Exception($"Gagal menyimpan Sheet Dokumen Pelengkap - " + ex.Message);
            //}
            

        }


        public List<HeaderDokumenTempModel> UploadHeader(ExcelWorksheets excel, int data)
        {
            var sheet = excel[data];
            var totalRow = sheet.Dimension.Rows;
            var listData = new List<HeaderDokumenTempModel>();
            int rowIndex = 0;
            try
            {
                for (rowIndex = 2; rowIndex <= totalRow; rowIndex++)
                {
                    if (sheet.Cells[rowIndex, 3].Value != null)
                    {
                        listData.Add(new HeaderDokumenTempModel(
                            converterChecker.GenerateValueString(sheet.Cells[rowIndex, 3]),
                            converterChecker.GenerateValueDecimal(sheet.Cells[rowIndex, 33]),
                            converterChecker.GenerateValueDecimal(sheet.Cells[rowIndex, 31]),
                            converterChecker.GenerateValueDecimal(sheet.Cells[rowIndex, 32]),
                            converterChecker.GenerateValueDecimal(sheet.Cells[rowIndex, 34]),
                            converterChecker.GenerateValueString(sheet.Cells[rowIndex, 2]),
                            converterChecker.GenerateValueString(sheet.Cells[rowIndex, 15]),
                            converterChecker.GenerateValueDate(sheet.Cells[rowIndex, 4]),
                            converterChecker.GenerateValueString(sheet.Cells[rowIndex, 28]),
                            converterChecker.GenerateValueStringBC(sheet.Cells[rowIndex, 6]),
                            converterChecker.GenerateValueInt(sheet.Cells[rowIndex, 35]),
                            converterChecker.GenerateValueString(sheet.Cells[rowIndex, 14]),
                            converterChecker.GenerateValueString(sheet.Cells[rowIndex, 40])
                            ));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Gagal memproses Sheet Header Dokumen pada baris ke-{rowIndex} - {ex.Message}");
            }
            //try
            //{
            //    if (listData.Count() > 0)
            //    {
            //        await headerAdapter.DeleteBulk();
            //        await headerAdapter.Insert(listData);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception($"Gagal menyimpan Sheet Header Dokumen - " + ex.Message);
            //}
            return listData;
        }

        public List<BarangTemp> UploadBarang(ExcelWorksheets excel, int data)
        {
            var sheet = excel[data];
            var totalRow = sheet.Dimension.Rows;
            var listData = new List<BarangTemp>();
            int rowIndex = 0;
            try
            {
                for (rowIndex = 2; rowIndex <= totalRow; rowIndex++)
                {
                    if (sheet.Cells[rowIndex, 2].Value != null)
                    {
                        listData.Add(new BarangTemp(
                              converterChecker.GenerateValueString(sheet.Cells[rowIndex, 2]),
                              converterChecker.GenerateValueString(sheet.Cells[rowIndex, 5]),
                              converterChecker.GenerateValueDecimal(sheet.Cells[rowIndex, 9]),
                              converterChecker.GenerateValueString(sheet.Cells[rowIndex, 8]),
                              converterChecker.GenerateValueString(sheet.Cells[rowIndex, 10]),
                              0,"",0
                            ));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Gagal memproses Sheet Barang pada baris ke-{rowIndex} - {ex.Message}");
            }
            //try
            //{
            //    if (listData.Count() > 0)
            //    {
            //        await barangAdapter.DeleteBulk();
            //        await barangAdapter.Insert(listData);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception($"Gagal menyimpan Sheet Barang - " + ex.Message);
            //}
            return listData;

        }
    }
}

